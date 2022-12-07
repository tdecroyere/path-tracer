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

    private TextureImage _textureImage;
    private nint _textureImageId;
    private NativeWindowSize _currentRenderSize;
    private readonly float _renderScaleRatio;

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
        _nativeWindow = nativeUIService.CreateWindow(_nativeApplication, "Path Tracer", windowWidth, windowHeight, NativeWindowState.Normal);
        _graphicsDevice = graphicsService.CreateDevice(_nativeWindow);

        // TODO: Refactor that !
        _uiService = new UI.ImGuiProvider.ImGuiUIService(_nativeUIService, _graphicsService, _graphicsDevice, _nativeWindow);

        _targetMS = (int)(1.0f / 60.0f * 1000.0f);
        _renderScaleRatio = 0.25f;
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
            // TODO: Compute real delta time
            var deltaTime = _targetMS * 0.001f;

            stopwatch.Restart();
            systemMessagesStopwatch.Restart();
            appStatus = _applicationService.ProcessSystemMessages(_nativeApplication);
            systemMessagesStopwatch.Stop();

            _inputService.UpdateInputState(_nativeApplication, ref inputState);
            camera = UpdateCamera(camera, inputState, deltaTime);

            // TODO: Compute the correct timing
            _uiService.Update(1.0f / 60.0f, inputState);

            camera = CreateRenderSizeDependentResources(camera, commandList);
            var renderImage = _textureImage;

            renderingStopwatch.Restart();

            _graphicsService.ResetCommandList(commandList);
            _graphicsService.ClearColor(commandList, new Vector4(1, 1, 0, 1));

            _renderer.Render(renderImage, camera);
            renderingStopwatch.Stop();
            stopwatch.Stop();

            _graphicsService.SubmitCommandList(commandList);

            _uiService.BeginPanel("Render", PanelStyles.NoTitle | PanelStyles.NoPadding);
            var renderSize = _uiService.GetPanelAvailableSize();
            _uiService.Image(_textureImageId, (int)renderSize.X, (int)renderSize.Y);
            _uiService.EndPanel();

            _uiService.BeginPanel("Inspector");
            _uiService.Text("Hellooooo");
            _uiService.EndPanel();

            _uiService.Render();

            // TODO: Do better here
            var waitingMS = Math.Clamp(_targetMS - stopwatch.ElapsedMilliseconds, 0, _targetMS);

            _nativeUIService.SetWindowTitle(_nativeWindow, $"Path Tracer ({renderImage.Width}x{renderImage.Height}) - Frame: {stopwatch.Elapsed.Milliseconds:00}ms (System: {systemMessagesStopwatch.ElapsedMilliseconds:00}ms, Render: {renderingStopwatch.ElapsedMilliseconds:00}ms, Waiting: {waitingMS:00}ms)");
            //Thread.Sleep((int)waitingMS);

            _graphicsService.PresentSwapChain(_graphicsDevice);
        }
    }

    // TODO: To be converted to an ECS System
    private static Camera UpdateCamera(Camera camera, InputState inputState, float deltaTime)
    {
        var forwardInput = inputState.Keyboard.KeyZ.Value - inputState.Keyboard.KeyS.Value;
        var sideInput = inputState.Keyboard.KeyD.Value - inputState.Keyboard.KeyQ.Value;
        var rotateYInput = 0;//inputState.Keyboard.ArrowRight.Value - inputState.Keyboard.ArrowLeft.Value;
        var rotateXInput = 0;//inputState.Keyboard.ArrowDown.Value - inputState.Keyboard.ArrowUp.Value;

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

    private Camera CreateRenderSizeDependentResources(Camera camera, CommandList commandList)
    {
        var renderSize = _nativeUIService.GetWindowRenderSize(_nativeWindow);

        if (renderSize != _currentRenderSize)
        {
            var aspectRatio = (float)renderSize.Width / renderSize.Height;
            var imageWidth = (int)(renderSize.Width * _renderScaleRatio);
            var imageHeight = (int)(imageWidth / aspectRatio);

            // TODO: Call a delete function
            _textureImage = CreatePlatformImage(commandList, imageWidth, imageHeight);

            if (_currentRenderSize.Width == 0)
            {
                _textureImageId = _uiService.RegisterTexture(_textureImage.GpuTexture);
            }
            else
            {
                _uiService.UpdateTexture(_textureImageId, _textureImage.GpuTexture);
            }

            _currentRenderSize = renderSize;

            return camera with
            {
                AspectRatio = aspectRatio
            };
        }

        return camera;
    }

    private TextureImage CreatePlatformImage(CommandList commandList, int width, int height)
    {
        var cpuTexture = _graphicsService.CreateTexture(_graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Staging, TextureType.Texture2D);
        var gpuTexture = _graphicsService.CreateTexture(_graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);

        var imageData = new uint[width * height];

        return new TextureImage
        {
            Width = width,
            Height = height,
            CpuTexture = cpuTexture,
            GpuTexture = gpuTexture,
            CommandList = commandList,
            ImageData = imageData
        };
    }
}