namespace PathTracer;

public class PathTracerApplication
{
    private readonly INativeApplicationService _applicationService;
    private readonly INativeUIService _nativeUIService;
    private readonly IInputService _inputService;
    private readonly IGraphicsService _graphicsService;
    private readonly ICommandManager _commandManager;
    private readonly IUIManager _uiManager;
    private readonly IRenderManager _renderManager;

    private readonly NativeApplication _nativeApplication;
    private readonly NativeWindow _nativeWindow;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly RenderStatistics _renderStatistics;

    private Camera _camera;

    public PathTracerApplication(INativeApplicationService applicationService,
                                 INativeUIService nativeUIService,
                                 IInputService inputService,
                                 IGraphicsService graphicsService,
                                 ICommandManager commandManager,
                                 IUIManager uiManager,
                                 IRenderManager renderManager)
    {
        _applicationService = applicationService;
        _nativeUIService = nativeUIService;
        _inputService = inputService;
        _graphicsService = graphicsService;
        _commandManager = commandManager;
        _uiManager = uiManager;
        _renderManager = renderManager;

        // TODO: Pass settings with builders
        var windowWidth = 1280;
        var windowHeight = 720;

        _nativeApplication = applicationService.CreateApplication("Path Tracer");
        _nativeWindow = nativeUIService.CreateWindow(_nativeApplication, "Path Tracer", windowWidth, windowHeight, NativeWindowState.Maximized);
        _graphicsDevice = graphicsService.CreateDevice(_nativeWindow);

        _uiManager.Init(_nativeWindow, _graphicsDevice);

        _camera = new Camera();
        _renderStatistics = new RenderStatistics();

        _commandManager.RegisterCommandHandler<RenderCommand>((renderCommand) => _renderManager.RenderToImage(renderCommand.RenderSettings));
    }

    public void Run()
    {
        var stopwatch = new Stopwatch();
        var fpsCounter = new FpsCounter();

        var appStatus = new NativeApplicationStatus();
        var inputState = new InputState();
        var commandList = _graphicsService.CreateCommandList(_graphicsDevice);

        var _currentWindowSize = new NativeWindowSize();
        var _currentRenderSize = Vector2.Zero;

        while (appStatus.IsRunning == 1)
        {
            var deltaTime = stopwatch.ElapsedMilliseconds * 0.001f;
            stopwatch.Restart();

            appStatus = _applicationService.ProcessSystemMessages(_nativeApplication);
            _inputService.UpdateInputState(_nativeApplication, ref inputState);
            _commandManager.Update();
            
            var windowSize = _nativeUIService.GetWindowRenderSize(_nativeWindow);

            if (_currentWindowSize != windowSize)
            {
                _graphicsService.ResizeSwapChain(_graphicsDevice, windowSize.Width, windowSize.Height);
                _uiManager.Resize(windowSize);

                Console.WriteLine($"Resize: {windowSize}");

                _currentWindowSize = windowSize;
            }

            var availableViewportSize = _uiManager.Update(deltaTime, inputState, _renderManager.CurrentTextureImage, _renderStatistics);

            _camera = UpdateCamera(_camera, inputState, deltaTime);
            
            if (availableViewportSize != _currentRenderSize)
            {
                _camera = _camera with
                {
                    AspectRatio = availableViewportSize.X / availableViewportSize.Y
                };
            
                var scaledRenderSize = availableViewportSize * windowSize.UIScale;
                _renderManager.CreateRenderTextures(_graphicsDevice, (int)scaledRenderSize.X, (int)scaledRenderSize.Y);
                _currentRenderSize = availableViewportSize;
            }
            
            _renderManager.RenderScene(commandList, _camera);

            // TODO: Get rid of the clear color, for that we need to fix the UI 1px border padding
            _graphicsService.ResetCommandList(commandList);
            _graphicsService.ClearColor(commandList, Vector4.Zero);
            _graphicsService.SubmitCommandList(commandList);
            _uiManager.Render();

            _graphicsService.PresentSwapChain(_graphicsDevice);

            stopwatch.Stop();
            fpsCounter.Update();

            _renderStatistics.RenderDuration = _renderManager.RenderDuration;
            _renderStatistics.LastRenderTime = _renderManager.LastRenderTime;
            _renderStatistics.CurrentFrameTime = stopwatch.ElapsedMilliseconds;
            _renderStatistics.FramesPerSeconds = fpsCounter.FramesPerSeconds;
            _renderStatistics.IsFileRenderingActive = _renderManager.IsFileRenderingActive;
            _renderStatistics.RenderWidth = _renderManager.CurrentTextureImage.Width;
            _renderStatistics.RenderHeight = _renderManager.CurrentTextureImage.Height;

            // TODO: Change that: if we have an exception the Completed flag will be true anyway
            if (_renderManager.IsFileRenderingActive)
            {
                _renderManager.CheckRenderToImageErrors();
            }
        }
    }

    // TODO: To be converted to an ECS System
    private static Camera UpdateCamera(Camera camera, InputState inputState, float deltaTime)
    {
        var forwardInput = inputState.Keyboard.KeyZ.Value - inputState.Keyboard.KeyS.Value;
        var sideInput = inputState.Keyboard.KeyD.Value - inputState.Keyboard.KeyQ.Value;
        var rotateYInput = inputState.Keyboard.Right.Value - inputState.Keyboard.Left.Value;
        var rotateXInput = inputState.Keyboard.Down.Value - inputState.Keyboard.Up.Value;

        // TODO: No acceleration for the moment
        var movementSpeed = 0.5f;
        var rotationSpeed = 0.5f;

        // TODO: Put right direction vector to the Camera struct
        var forwardDirection = camera.Target - camera.Position;
        var rightDirection = Vector3.Cross(new Vector3(0, 1, 0), forwardDirection);

        var movementVector = rightDirection * sideInput * movementSpeed * deltaTime + forwardDirection * forwardInput * movementSpeed * deltaTime;
        var cameraPosition = camera.Position + movementVector;

        var rotateX = rotateXInput * rotationSpeed * deltaTime;
        var rotateY = rotateYInput * rotationSpeed * deltaTime;

        var quaternionX = Quaternion.CreateFromAxisAngle(rightDirection, rotateX);
        var quaternionY = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), rotateY);

        var rotationQuaternion = Quaternion.Normalize(quaternionX * quaternionY);
        forwardDirection = Vector3.Transform(forwardDirection, rotationQuaternion);

        return camera with
        {
            Position = cameraPosition,
            Target = cameraPosition + forwardDirection
        };
    }
}