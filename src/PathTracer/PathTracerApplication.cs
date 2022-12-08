using System.Diagnostics;
using PathTracer.Platform;
using PathTracer.Platform.GraphicsLegacy;
using PathTracer.Platform.Inputs;
using PathTracer.UI;

namespace PathTracer;

public class PathTracerApplication
{
    private readonly INativeApplicationService _applicationService;
    private readonly INativeUIService _nativeUIService;
    private readonly IInputService _inputService;
    private readonly IGraphicsService _graphicsService;
    private readonly IUIService _uiService;
    private readonly IRenderer<TextureImage> _renderer;

    private readonly NativeApplication _nativeApplication;
    private readonly NativeWindow _nativeWindow;
    private readonly GraphicsDevice _graphicsDevice;

    private readonly int _targetMS;
    private readonly float _renderScaleRatio;

    private TextureImage _textureImage;
    private TextureImage _fullResolutionTextureImage;
    private NativeWindowSize _currentWindowSize;
    private Vector2 _currentRenderSize;
    private DateTime _lastRenderTime = DateTime.Now;
    private Task<bool>? _fullResolutionRenderingTask = null;
    private bool _isFullResolutionRenderComplete = false;

    public PathTracerApplication(INativeApplicationService applicationService,
                                 INativeUIService nativeUIService,
                                 IInputService inputService,
                                 IGraphicsService graphicsService,
                                 IRenderer<TextureImage> renderer)
    {
        _applicationService = applicationService;
        _nativeUIService = nativeUIService;
        _inputService = inputService;
        _graphicsService = graphicsService;
        _renderer = renderer;

        var windowWidth = 1280;
        var windowHeight = 720;

        _nativeApplication = applicationService.CreateApplication("Path Tracer");
        _nativeWindow = nativeUIService.CreateWindow(_nativeApplication, "Path Tracer", windowWidth, windowHeight, NativeWindowState.Maximized);
        _graphicsDevice = graphicsService.CreateDevice(_nativeWindow);

        // TODO: Refactor that !
        _uiService = new UI.ImGuiProvider.ImGuiUIService(_nativeUIService, _graphicsService, _graphicsDevice, _nativeWindow);

        _targetMS = (int)(1.0f / 144.0f * 1000.0f);
        _renderScaleRatio = 0.5f;
    }

    public void Run()
    {
        var stopwatch = new Stopwatch();
        var systemMessagesStopwatch = new Stopwatch();
        var renderingStopwatch = new Stopwatch();

        var appStatus = new NativeApplicationStatus();
        var inputState = new InputState();
        var camera = new Camera();
        var commandList = _graphicsService.CreateCommandList(_graphicsDevice);

        while (appStatus.IsRunning == 1)
        {
            var deltaTime = stopwatch.ElapsedMilliseconds * 0.001f;

            stopwatch.Restart();
            systemMessagesStopwatch.Restart();
            appStatus = _applicationService.ProcessSystemMessages(_nativeApplication);
            systemMessagesStopwatch.Stop();
            
            var windowSize = _nativeUIService.GetWindowRenderSize(_nativeWindow);

            if (_currentWindowSize != windowSize)
            {
                _graphicsService.ResizeSwapChain(_graphicsDevice, windowSize.Width, windowSize.Height);
                _uiService.Resize(windowSize.Width, windowSize.Height, windowSize.UIScale);

                Console.WriteLine($"Resize: {windowSize}");

                _currentWindowSize = windowSize;
                _isFullResolutionRenderComplete = false;
            }

            _inputService.UpdateInputState(_nativeApplication, ref inputState);
            var previousCamera = camera;
            camera = UpdateCamera(camera, inputState, deltaTime);

            if (camera != previousCamera)
            {
                // TODO: Cancel task when possible
                _isFullResolutionRenderComplete = false;
            }
            else if (_fullResolutionRenderingTask == null && _isFullResolutionRenderComplete == false)
            {
                _fullResolutionRenderingTask = new Task<bool>(() =>
                {
                Console.WriteLine("Rendering high res");
                    _renderer.Render(_fullResolutionTextureImage, camera);
                    return true;
                });

                _fullResolutionRenderingTask.Start();
            }
            
            if (_fullResolutionRenderingTask != null && _fullResolutionRenderingTask.Status == TaskStatus.RanToCompletion)
            {
                _isFullResolutionRenderComplete = _fullResolutionRenderingTask.Result;
                _fullResolutionRenderingTask = null;
                _lastRenderTime = DateTime.Now;

                _graphicsService.ResetCommandList(commandList);
                _renderer.CommitImage(_fullResolutionTextureImage);
                _graphicsService.SubmitCommandList(commandList);
            }
            
            // TODO: Compute the correct timing
            _uiService.Update(_targetMS, inputState);
            
            _uiService.BeginPanel("Render", PanelStyles.NoTitle | PanelStyles.NoPadding);
            var renderSize = _uiService.GetPanelAvailableSize();
            
            var renderImage = _textureImage;

            if (_isFullResolutionRenderComplete)
            {
                renderImage = _fullResolutionTextureImage;
            }

            _uiService.Image(renderImage.TextureId, (int)renderSize.X, (int)renderSize.Y);
            _uiService.EndPanel();

            _uiService.BeginPanel("Inspector");
            _uiService.Text($"Current RenderSize: {renderImage.Width}x{renderImage.Height}");
            _uiService.Text($"Show full resolution image: {_isFullResolutionRenderComplete}");
            _uiService.Text($"Last render time: {_lastRenderTime}");
            _uiService.EndPanel();

            var previousCameraSize = camera;
            camera = CreateRenderSizeDependentResources(camera, commandList, renderSize);
            
            if (camera != previousCameraSize)
            {
                // TODO: Cancel task when possible
                _isFullResolutionRenderComplete = false;
                _fullResolutionRenderingTask = null;
            }

            _graphicsService.ResetCommandList(commandList);
            _graphicsService.ClearColor(commandList, Vector4.Zero);


            if (camera != previousCamera || camera != previousCameraSize)
            {
                Console.WriteLine("Rendering low res");
                renderingStopwatch.Restart();
                _renderer.Render(renderImage, camera);
                renderingStopwatch.Stop();
                _renderer.CommitImage(_textureImage);
            }

            _graphicsService.SubmitCommandList(commandList);

            _uiService.Render();
            _graphicsService.PresentSwapChain(_graphicsDevice);
            stopwatch.Stop();

            _nativeUIService.SetWindowTitle(_nativeWindow, $"Path Tracer - Frame: {stopwatch.Elapsed.Milliseconds:00}ms (System: {systemMessagesStopwatch.ElapsedMilliseconds:00}ms, Render: {renderingStopwatch.ElapsedMilliseconds:00}ms)");
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

    private Camera CreateRenderSizeDependentResources(Camera camera, CommandList commandList, Vector2 renderSize)
    {
        if (renderSize != _currentRenderSize)
        {
            var aspectRatio = renderSize.X / renderSize.Y;
            var imageWidth = (int)(renderSize.X * _renderScaleRatio);
            var imageHeight = (int)(imageWidth / aspectRatio);

            CreateOrUpdateTextureImage(commandList, imageWidth, imageHeight, ref _textureImage);
            CreateOrUpdateTextureImage(commandList, (int)renderSize.X, (int)renderSize.Y, ref _fullResolutionTextureImage);

            _currentRenderSize = renderSize;

            return camera with
            {
                AspectRatio = aspectRatio
            };
        }

        return camera;
    }

    private void CreateOrUpdateTextureImage(CommandList commandList, int width, int height, ref TextureImage textureImage)
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