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
    private readonly IRenderer<TextureImage> _renderer;
    private readonly IRenderer<FileImage> _fileRenderer;

    private readonly NativeApplication _nativeApplication;
    private readonly NativeWindow _nativeWindow;
    private readonly GraphicsDevice _graphicsDevice;

    private const float _lowResolutionScaleRatio = 0.25f;

    private Camera _camera;
    private RenderStatistics _renderStatistics;
    private Task? _fileRenderingTask;

    public PathTracerApplication(INativeApplicationService applicationService,
                                 INativeUIService nativeUIService,
                                 IInputService inputService,
                                 IGraphicsService graphicsService,
                                 IUIService uiService,
                                 ICommandManager commandManager,
                                 UIManager uIManager,
                                 IRenderer<TextureImage> renderer,
                                 IRenderer<FileImage> fileRenderer)
    {
        _applicationService = applicationService;
        _nativeUIService = nativeUIService;
        _inputService = inputService;
        _graphicsService = graphicsService;
        _uiService = uiService;
        _commandManager = commandManager;
        _uiManager = uIManager;
        _renderer = renderer;
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

        var _textureImage = new TextureImage { CommandList = commandList };
        var _fullResolutionTextureImage = new TextureImage { CommandList = commandList };

        var _currentWindowSize = new NativeWindowSize();
        var _currentRenderSize = Vector2.Zero;

        var _isFullResolutionRenderComplete = false;
        Task<bool>? _fullResolutionRenderingTask = null;

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

            var renderImage = _isFullResolutionRenderComplete ? _fullResolutionTextureImage : _textureImage;
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
                CreateRenderTextures((int)scaledRenderSize.X, (int)scaledRenderSize.Y, ref _textureImage, ref _fullResolutionTextureImage);
                _currentRenderSize = availableViewportSize;
            }
            
            RenderScene(_camera, commandList, previousCamera, _fullResolutionTextureImage, _textureImage, ref _isFullResolutionRenderComplete, ref _fullResolutionRenderingTask, ref _renderStatistics);

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
            _fileRenderingTask = new Task(() => 
            {
                _renderStatistics.IsFileRenderingActive = true;

                var width = renderCommand.RenderSettings.Resolution.Width;
                var height = renderCommand.RenderSettings.Resolution.Height;

                var outputImage = new FileImage
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
                _renderStatistics.IsFileRenderingActive = false;
            });

            _fileRenderingTask.Start();
        }
    }

    private void RenderScene(Camera camera, 
                             CommandList commandList, 
                             Camera previousCamera, 
                             TextureImage _fullResolutionTextureImage, 
                             TextureImage _textureImage,
                             ref bool _isFullResolutionRenderComplete,
                             ref Task<bool>? _fullResolutionRenderingTask,
                             ref RenderStatistics renderStatistics)
    {
        var renderStopwatch = renderStatistics.RenderStopwatch;
        
        // TODO: Do we need a global task, can we reuse task with a pool?
        if (camera != previousCamera)
        {
            Console.WriteLine("Render Low Resolution");
            renderStopwatch.Restart();
            _renderer.Render(_textureImage, camera);
            renderStopwatch.Stop();
            _graphicsService.ResetCommandList(commandList);
            _renderer.CommitImage(_textureImage);
            _graphicsService.SubmitCommandList(commandList);

            // TODO: Cancel task when possible
            _isFullResolutionRenderComplete = false;
            _fullResolutionRenderingTask = null;
        }
        else if (_fullResolutionRenderingTask == null && _isFullResolutionRenderComplete == false)
        {
            _fullResolutionRenderingTask = new Task<bool>(() =>
            {
                Console.WriteLine("Render Full Resolution");
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

    private void CreateRenderTextures(int width, int height, ref TextureImage _textureImage, ref TextureImage _fullResolutionTextureImage)
    {
        var aspectRatio = (float)width / height;
        var lowResWidth = (int)(width * _lowResolutionScaleRatio);
        var lowResHeight = (int)(lowResWidth / aspectRatio);

        _textureImage = CreateOrUpdateTextureImage(_textureImage, lowResWidth, lowResHeight);
        _fullResolutionTextureImage = CreateOrUpdateTextureImage(_fullResolutionTextureImage, width, height);
    }

    private TextureImage CreateOrUpdateTextureImage(TextureImage textureImage, int width, int height)
    {
        // TODO: Call a delete function

        var cpuTexture = _graphicsService.CreateTexture(_graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Staging, TextureType.Texture2D);
        var gpuTexture = _graphicsService.CreateTexture(_graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);

        var imageData = new uint[width * height];
        var textureId = textureImage.TextureId;

        if (textureId == 0)
        {
            textureId = _uiService.RegisterTexture(gpuTexture);
        }
        else
        {
            _uiService.UpdateTexture(textureId, gpuTexture);
        }

        return textureImage with
        {
            Width = width,
            Height = height,
            CpuTexture = cpuTexture,
            GpuTexture = gpuTexture,
            ImageData = imageData,
            TextureId = textureId
        };
    }
}