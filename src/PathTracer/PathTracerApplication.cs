namespace PathTracer;

public class PathTracerApplication
{
    private readonly INativeApplicationService _applicationService;
    private readonly INativeUIService _nativeUIService;
    private readonly IInputService _inputService;
    private readonly IGraphicsService _graphicsService;
    private readonly IUIService _uiService;
    private readonly ICommandManager _commandManager;
    private readonly IRenderer<TextureImage> _renderer;
    private readonly IRenderer<PpmImage> _fileRenderer;

    private readonly NativeApplication _nativeApplication;
    private readonly NativeWindow _nativeWindow;
    private readonly GraphicsDevice _graphicsDevice;

    private readonly UIManager _uiManager;

    private readonly float _lowResolutionScaleRatio;

    private Camera _camera;

    public PathTracerApplication(INativeApplicationService applicationService,
                                 INativeUIService nativeUIService,
                                 IInputService inputService,
                                 IGraphicsService graphicsService,
                                 ICommandManager commandManager,
                                 IRenderer<TextureImage> renderer,
                                 IRenderer<PpmImage> fileRenderer)
    {
        _applicationService = applicationService;
        _nativeUIService = nativeUIService;
        _inputService = inputService;
        _graphicsService = graphicsService;
        _commandManager = commandManager;
        _renderer = renderer;
        _fileRenderer = fileRenderer;

        var windowWidth = 1280;
        var windowHeight = 720;

        _nativeApplication = applicationService.CreateApplication("Path Tracer");
        _nativeWindow = nativeUIService.CreateWindow(_nativeApplication, "Path Tracer", windowWidth, windowHeight, NativeWindowState.Maximized);
        _graphicsDevice = graphicsService.CreateDevice(_nativeWindow);

        // TODO: Refactor that !
        _uiService = new UI.ImGuiProvider.ImGuiUIService(_nativeUIService, _graphicsService, _graphicsDevice, _nativeWindow);
        _uiManager = new UIManager(_uiService, _commandManager);

        _lowResolutionScaleRatio = 0.25f;
        _camera = new Camera();

        _commandManager.RegisterCommandHandler(new Action<RenderCommand>(RenderToImage));
    }

    public void Run()
    {
        var stopwatch = new Stopwatch();
        var fpsCounter = new FpsCounter();

        var appStatus = new NativeApplicationStatus();
        var inputState = new InputState();
        var commandList = _graphicsService.CreateCommandList(_graphicsDevice);

        var _textureImage = new TextureImage();
        var _fullResolutionTextureImage = new TextureImage();

        var _currentWindowSize = new NativeWindowSize();
        var _currentRenderSize = Vector2.Zero;

        var _isFullResolutionRenderComplete = false;
        Task<bool>? _fullResolutionRenderingTask = null;

        var renderStatistics = new RenderStatistics();

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
                _isFullResolutionRenderComplete = false;
            }

            var previousCamera = _camera;
            _camera = UpdateCamera(_camera, inputState, deltaTime);

            _uiService.Update(deltaTime, inputState);

            var renderImage = _isFullResolutionRenderComplete ? _fullResolutionTextureImage : _textureImage;
            var availableViewportSize = _uiManager.BuildUI(renderImage, renderStatistics);

            var previousCameraSize = _camera;
            _camera = CreateRenderTexturesIfNeeded(_camera, commandList, availableViewportSize, windowSize.UIScale, ref _currentRenderSize, ref _textureImage, ref _fullResolutionTextureImage);
            
            RenderScene(_camera, commandList, previousCamera, previousCameraSize, _fullResolutionTextureImage, _textureImage, ref _isFullResolutionRenderComplete, ref _fullResolutionRenderingTask, ref renderStatistics);

            _graphicsService.ResetCommandList(commandList);
            _graphicsService.ClearColor(commandList, Vector4.Zero);
            _graphicsService.SubmitCommandList(commandList);

            _uiService.Render();
            _graphicsService.PresentSwapChain(_graphicsDevice);

            stopwatch.Stop();

            renderStatistics.CurrentFrameTime = stopwatch.ElapsedMilliseconds;
            renderStatistics.FramesPerSeconds = fpsCounter.FramesPerSeconds;

            fpsCounter.Udpate();
        }
    }

    private void RenderToImage(RenderCommand renderCommand)
    {
        Console.WriteLine("Rendering...");
        
        var width = renderCommand.RenderSettings.Resolution.Width;
        var height = renderCommand.RenderSettings.Resolution.Height;

        var outputImage = new PpmImage
        {
            Width = width,
            Height = height,
            OutputPath = renderCommand.RenderSettings.OutputPath,
            ImageData = new Vector4[width * height]
        };

        var fileCamera = _camera with
        {
            AspectRatio = (float)width / height
        };

        _fileRenderer.Render(outputImage, fileCamera);
        _fileRenderer.CommitImage(outputImage);
    }

    private void RenderScene(Camera camera, 
                             CommandList commandList, 
                             Camera previousCamera, 
                             Camera previousCameraSize, 
                             TextureImage _fullResolutionTextureImage, 
                             TextureImage _textureImage,
                             ref bool _isFullResolutionRenderComplete,
                             ref Task<bool>? _fullResolutionRenderingTask,
                             ref RenderStatistics renderStatistics)
    {
        var renderStopwatch = renderStatistics.RenderStopwatch;
        
        // TODO: Do we need a global task, can we reuse task with a pool?
        if (camera != previousCamera || camera != previousCameraSize)
        {
            // TODO: Cancel task when possible
            _isFullResolutionRenderComplete = false;
            _fullResolutionRenderingTask = null;
        }
        else if (_fullResolutionRenderingTask == null && _isFullResolutionRenderComplete == false)
        {

            _fullResolutionRenderingTask = new Task<bool>(() =>
            {
                renderStopwatch.Restart();
                _renderer.Render(_fullResolutionTextureImage, camera);
                renderStopwatch.Stop();
                return true;
            });

            _fullResolutionRenderingTask.Start();
        }

        if (_fullResolutionRenderingTask != null && _fullResolutionRenderingTask.Status == TaskStatus.RanToCompletion)
        {
            _isFullResolutionRenderComplete = _fullResolutionRenderingTask.Result;
            _fullResolutionRenderingTask = null;
            renderStatistics.LastRenderTime = DateTime.Now;

            _graphicsService.ResetCommandList(commandList);
            _renderer.CommitImage(_fullResolutionTextureImage);
            _graphicsService.SubmitCommandList(commandList);
        }

        if (camera != previousCamera || camera != previousCameraSize)
        {
            renderStopwatch.Restart();
            _renderer.Render(_textureImage, camera);
            renderStopwatch.Stop();
            _graphicsService.ResetCommandList(commandList);
            _renderer.CommitImage(_textureImage);
            _graphicsService.SubmitCommandList(commandList);
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
        var movementSpeed = 1.0f;
        var rotationSpeed = 1.0f;

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

    private Camera CreateRenderTexturesIfNeeded(Camera camera, CommandList commandList, Vector2 renderSize, float uiScale, ref Vector2 _currentRenderSize, ref TextureImage _textureImage, ref TextureImage _fullResolutionTextureImage)
    {
        if (renderSize != _currentRenderSize)
        {
            var scaledRenderSize = renderSize * uiScale;

            var aspectRatio = scaledRenderSize.X / scaledRenderSize.Y;
            var imageWidth = (int)(scaledRenderSize.X * _lowResolutionScaleRatio);
            var imageHeight = (int)(imageWidth / aspectRatio);

            CreateOrUpdateTextureImage(commandList, imageWidth, imageHeight, _currentRenderSize, ref _textureImage);
            CreateOrUpdateTextureImage(commandList, (int)scaledRenderSize.X, (int)scaledRenderSize.Y, _currentRenderSize, ref _fullResolutionTextureImage);

            _currentRenderSize = renderSize;

            return camera with
            {
                AspectRatio = aspectRatio
            };
        }

        return camera;
    }

    private void CreateOrUpdateTextureImage(CommandList commandList, int width, int height, Vector2 _currentRenderSize, ref TextureImage textureImage)
    {
        // TODO: Call a delete function

        var cpuTexture = _graphicsService.CreateTexture(_graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Staging, TextureType.Texture2D);
        var gpuTexture = _graphicsService.CreateTexture(_graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);

        var imageData = new uint[width * height];
        var textureId = textureImage.TextureId;

        if (_currentRenderSize.X == 0 && _currentRenderSize.Y == 0)
        {
            textureId = _uiService.RegisterTexture(gpuTexture);
        }
        else
        {
            _uiService.UpdateTexture(textureId, gpuTexture);
        }

        textureImage = textureImage with
        {
            Width = width,
            Height = height,
            CpuTexture = cpuTexture,
            GpuTexture = gpuTexture,
            CommandList = commandList,
            ImageData = imageData,
            TextureId = textureId
        };
    }
}