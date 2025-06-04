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

    private readonly int _windowWidth;
    private readonly int _windowHeight;

    private readonly NativeApplication _nativeApplication;
    private readonly NativeWindow _nativeWindow;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CommandList _commandList;
    private readonly FrameTimer _frameTimer;

    private RenderStatistics _renderStatistics;
    private NativeApplicationStatus _appStatus;
    private InputState _inputState;
    private NativeWindowSize _currentWindowSize;
    private Vector2 _currentRenderSize;

    private readonly Scene _scene;
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
        _windowWidth = 1280;
        _windowHeight = 720;
        
        _renderStatistics = new RenderStatistics();
        _frameTimer = new FrameTimer();
        _appStatus = new NativeApplicationStatus();
        _inputState = new InputState();

        _currentWindowSize = new NativeWindowSize();
        
        _nativeApplication = _applicationService.CreateApplication("Path Tracer");
        _nativeWindow = _nativeUIService.CreateWindow(_nativeApplication, "Path Tracer", _windowWidth, _windowHeight, NativeWindowState.Maximized);
        _graphicsDevice = _graphicsService.CreateDevice(_nativeWindow);

        _uiManager.Init(_nativeWindow, _graphicsDevice);
        
        _commandList = _graphicsService.CreateCommandList(_graphicsDevice);

        _camera = new Camera()
        {
            Position = new Vector3(0.0f, 0.0f, -6.0f)
        };

        _scene = new Scene();

        _scene.Materials.Add(new Material()
        {
            Albedo = new Vector3(1.0f, 1.0f, 0.0f),
            Roughness = 0.0f
        });
        
        _scene.Materials.Add(new Material()
        {
            Albedo = new Vector3(0.0f, 0.2f, 1.0f),
            Roughness = 0.1f
        });

        _scene.Spheres.Add(new Sphere()
        {
            Position = new Vector3(0.0f, 0.0f, 0.0f),
            Radius = 1.0f,
            MaterialIndex = 0 
        });
        
        _scene.Spheres.Add(new Sphere()
        {
            Position = new Vector3(0.0f, -101.0f, 0.0f),
            Radius = 100.0f,
            MaterialIndex = 1
        });

        _commandManager.RegisterCommandHandler<RenderCommand>((renderCommand) => _renderManager.RenderToImage(renderCommand.RenderSettings, _scene, _camera));
    }

    public void Run()
    {
        while (_appStatus.IsRunning == 1)
        {
            _appStatus = _applicationService.ProcessSystemMessages(_nativeApplication);

            var windowSize = _nativeUIService.GetWindowRenderSize(_nativeWindow);
            ResizeIfNeeded(windowSize);

            _inputService.UpdateInputState(_nativeApplication, ref _inputState);
            _camera = UpdateCamera(_camera, _inputState, _frameTimer.DeltaTime);

            // TODO: Temporary
            _scene.HasChanged = false;

            var availableViewportSize = _uiManager.Update(_frameTimer.DeltaTime, _inputState, _renderManager.CurrentTextureImage, _renderStatistics, _scene);
            _commandManager.Update();

            CreateRenderTexturesIfNeeded(windowSize, availableViewportSize);

            _renderManager.RenderScene(_commandList, _scene, _camera);
            _uiManager.Render();
            _graphicsService.PresentSwapChain(_graphicsDevice);

            // TODO: Change that: if we have an exception the Completed flag will be true anyway
            if (_renderManager.FileRenderingProgression < 100)
            {
                _renderManager.CheckRenderToImageErrors();
            }

            _frameTimer.Update();
            UpdateStatistics();
        }
    }

    private void ResizeIfNeeded(NativeWindowSize windowSize)
    {
        if (_currentWindowSize != windowSize)
        {
            _graphicsService.ResizeSwapChain(_graphicsDevice, windowSize.Width, windowSize.Height);
            _uiManager.Resize(windowSize);

            _currentWindowSize = windowSize;
        }
    }

    private void CreateRenderTexturesIfNeeded(NativeWindowSize windowSize, Vector2 availableViewportSize)
    {
        if (availableViewportSize != _currentRenderSize)
        {
            // TODO: Should we use a more functional approach and don't mutate private fields and try to have
            // more "pure" functions?
            _camera = _camera with
            {
                AspectRatio = availableViewportSize.X / availableViewportSize.Y
            };

            var scaledRenderSize = availableViewportSize * windowSize.UIScale;
            _renderManager.CreateRenderTextures(_graphicsDevice, (int)scaledRenderSize.X, (int)scaledRenderSize.Y);
            _currentRenderSize = availableViewportSize;
        }
    }

    private void UpdateStatistics()
    {
        _renderStatistics.RenderDuration = _renderManager.RenderDuration;
        _renderStatistics.LastRenderTime = _renderManager.LastRenderTime;
        _renderStatistics.CurrentFrameTime = (long)(_frameTimer.DeltaTime * 1000.0f);
        _renderStatistics.FramesPerSeconds = _frameTimer.FramesPerSeconds;
        _renderStatistics.FileRenderingProgression = _renderManager.FileRenderingProgression;
        _renderStatistics.RenderWidth = _renderManager.CurrentTextureImage.Width;
        _renderStatistics.RenderHeight = _renderManager.CurrentTextureImage.Height;
        _renderStatistics.AllocatedManagedMemory = GC.GetTotalMemory(false);
        _renderStatistics.CpuUsage = _frameTimer.CpuUsage;
        _renderStatistics.GCGen0Count = GC.CollectionCount(0);
        _renderStatistics.GCGen1Count = GC.CollectionCount(1);
        _renderStatistics.GCGen0Count = GC.CollectionCount(2);
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