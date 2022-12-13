namespace PathTracer;

public class PathTracerApplication
{
    private readonly INativeApplicationService _applicationService;
    private readonly INativeUIService _nativeUIService;
    private readonly IInputService _inputService;
    private readonly IGraphicsService _graphicsService;
    private readonly IUIService _uiService;
    private readonly ICommandManager _commandManager;
    private readonly UIManager _uiManager;
    private readonly RenderManager _renderManager;
    private readonly IRenderer<FileImage, string> _fileRenderer;

    private readonly NativeApplication _nativeApplication;
    private readonly NativeWindow _nativeWindow;
    private readonly GraphicsDevice _graphicsDevice;

    private Camera _camera;
    private RenderStatistics _renderStatistics;
    private Task? _fileRenderingTask;

    public PathTracerApplication(INativeApplicationService applicationService,
                                 INativeUIService nativeUIService,
                                 IInputService inputService,
                                 IGraphicsService graphicsService,
                                 IUIService uiService,
                                 ICommandManager commandManager,
                                 UIManager uiManager,
                                 RenderManager renderManager,
                                 IRenderer<FileImage, string> fileRenderer)
    {
        _applicationService = applicationService;
        _nativeUIService = nativeUIService;
        _inputService = inputService;
        _graphicsService = graphicsService;
        _uiService = uiService;
        _commandManager = commandManager;
        _uiManager = uiManager;
        _renderManager = renderManager;
        _fileRenderer = fileRenderer;

        var windowWidth = 1280;
        var windowHeight = 720;

        _nativeApplication = applicationService.CreateApplication("Path Tracer");
        _nativeWindow = nativeUIService.CreateWindow(_nativeApplication, "Path Tracer", windowWidth, windowHeight, NativeWindowState.Maximized);
        _graphicsDevice = graphicsService.CreateDevice(_nativeWindow);

        _uiService.Init(_nativeWindow, _graphicsDevice);

        _camera = new Camera();
        _renderStatistics = new RenderStatistics();

        _commandManager.RegisterCommandHandler(new Action<RenderCommand>(RenderToImage));
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
                _uiService.Resize(windowSize.Width, windowSize.Height, windowSize.UIScale);

                Console.WriteLine($"Resize: {windowSize}");

                _currentWindowSize = windowSize;
            }

            _uiService.Update(deltaTime, inputState);

            var renderImage = _renderManager.CurrentTextureImage;
            var availableViewportSize = _uiManager.BuildUI(renderImage, _renderStatistics);

            var previousCamera = _camera;
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
            
            _renderManager.RenderScene(_camera, commandList, previousCamera, ref _renderStatistics);

            // TODO: Get rid of the clear color, for that we need to fix the UI 1px border padding
            _graphicsService.ResetCommandList(commandList);
            _graphicsService.ClearColor(commandList, Vector4.Zero);
            _graphicsService.SubmitCommandList(commandList);

            _uiService.Render();
            _graphicsService.PresentSwapChain(_graphicsDevice);

            stopwatch.Stop();

            _renderStatistics.CurrentFrameTime = stopwatch.ElapsedMilliseconds;
            _renderStatistics.FramesPerSeconds = fpsCounter.FramesPerSeconds;

            fpsCounter.Update();

            if (_fileRenderingTask != null && _fileRenderingTask.Exception != null)
            {
                Console.WriteLine(_fileRenderingTask.Exception);
                _fileRenderingTask = null;
            }
        }
    }

    private void RenderToImage(RenderCommand renderCommand)
    {
        if (_fileRenderingTask == null || _fileRenderingTask.IsCompleted)
        {
            _renderStatistics.IsFileRenderingActive = true;
            
            _fileRenderingTask = new Task(() => 
            {
                _renderStatistics.IsFileRenderingActive = true;

                var width = renderCommand.RenderSettings.Resolution.Width;
                var height = renderCommand.RenderSettings.Resolution.Height;

                var outputImage = new FileImage
                {
                    Width = width,
                    Height = height,
                    ImageData = new Vector4[width * height]
                };

                var fileCamera = _camera with
                {
                    AspectRatio = (float)width / height
                };

                _fileRenderer.Render(outputImage, fileCamera);
                _fileRenderer.CommitImage(outputImage, renderCommand.RenderSettings.OutputPath);
                _renderStatistics.IsFileRenderingActive = false;
            });

            _fileRenderingTask.Start();
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