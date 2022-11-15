using System.Diagnostics;
using PathTracer.Platform;
using PathTracer.Platform.Inputs;
using PathTracer.Platform.NativeUI;

namespace PathTracer;

public class PathTracerApplication
{
    private readonly INativeApplicationService _applicationService;
    private readonly INativeUIService _nativeUIService;
    private readonly INativeInputService _nativeInputService;
    private readonly IRenderer<PlatformImage> _renderer;

    private readonly NativeApplication _nativeApplication;
    private readonly NativeWindow _nativeWindow;

    private readonly int _targetMS;

    private PlatformImage _platformImage;
    private NativeWindowSize _currentRenderSize;
    private readonly float _renderScaleRatio;

    public PathTracerApplication(INativeApplicationService applicationService,
                                 INativeUIService nativeUIService,
                                 INativeInputService nativeInputService,
                                 IRenderer<PlatformImage> renderer)
    {
        _applicationService = applicationService;
        _nativeUIService = nativeUIService;
        _nativeInputService = nativeInputService;
        _renderer = renderer;

        var windowWidth = 1280;
        var windowHeight = 720;

        _nativeApplication = applicationService.CreateApplication("Path Tracer");
        _nativeWindow = nativeUIService.CreateWindow(_nativeApplication, "Path Tracer", windowWidth, windowHeight, NativeWindowState.Normal);

        _targetMS = (int)(1.0f / 60.0f * 1000.0f);
        _renderScaleRatio = 0.25f;
    }

    public async Task RunAsync()
    {
        var stopwatch = new Stopwatch();
        var systemMessagesStopwatch = new Stopwatch();
        var renderingStopwatch = new Stopwatch();

        var appStatus = new NativeApplicationStatus();
        var inputState = new NativeInputState();
        var camera = new Camera();

        while (appStatus.IsRunning == 1)
        {
            // TODO: Compute real delta time
            var deltaTime = _targetMS * 0.001f;

            stopwatch.Restart();
            systemMessagesStopwatch.Restart();
            appStatus = _applicationService.ProcessSystemMessages(_nativeApplication);
            systemMessagesStopwatch.Stop();

            _nativeInputService.GetInputState(_nativeApplication, ref inputState);
            camera = UpdateCamera(camera, inputState, deltaTime);

            camera = CreateRenderSizeDependentResources(camera);
            var renderImage = _platformImage;

            renderingStopwatch.Restart();

            await _renderer.RenderAsync(renderImage, camera);
            renderingStopwatch.Stop();
            stopwatch.Stop();

            // TODO: Do better here
            var waitingMS = Math.Clamp(_targetMS - stopwatch.ElapsedMilliseconds, 0, _targetMS);

            _nativeUIService.SetWindowTitle(_nativeWindow, $"Path Tracer ({renderImage.Width}x{renderImage.Height}) - Frame: {stopwatch.Elapsed.Milliseconds:00}ms (System: {systemMessagesStopwatch.ElapsedMilliseconds:00}ms, Render: {renderingStopwatch.ElapsedMilliseconds:00}ms, Waiting: {waitingMS:00}ms)");
            Thread.Sleep((int)waitingMS);
        }
    }

    // TODO: To be converted to an ECS System
    private static Camera UpdateCamera(Camera camera, NativeInputState inputState, float deltaTime)
    {
        var forwardInput = inputState.Keyboard.KeyZ.Value - inputState.Keyboard.KeyS.Value;
        var sideInput = inputState.Keyboard.KeyD.Value - inputState.Keyboard.KeyQ.Value;
        var rotateYInput = inputState.Keyboard.ArrowRight.Value - inputState.Keyboard.ArrowLeft.Value;
        var rotateXInput = inputState.Keyboard.ArrowDown.Value - inputState.Keyboard.ArrowUp.Value;
        
        // TODO: No acceleration for the moment
        var movementSpeed = 1.0f;
        var rotationSpeed = 1.0f;

        // TODO: Put right direction vector to the Camera struct
        var cameraDirection = camera.Target - camera.Position;
        var rightDirection = Vector3.Cross(new Vector3(0, 1, 0), cameraDirection);

        var movementVector = rightDirection * sideInput * movementSpeed * deltaTime + cameraDirection * forwardInput * movementSpeed * deltaTime;
        var cameraPosition = camera.Position + movementVector;

        var rotateX = rotateXInput * rotationSpeed * deltaTime;
        var rotateY = rotateYInput * rotationSpeed * deltaTime;

        var quaternionX = Quaternion.CreateFromAxisAngle(rightDirection, rotateX);
        var quaternionY = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), rotateY);

        var rotationQuaternion = Quaternion.Normalize(quaternionX * quaternionY);
        cameraDirection = Vector3.Transform(cameraDirection, rotationQuaternion);

        return camera with
        {
            Position = cameraPosition,
            Target = cameraPosition + cameraDirection
        };
    }

    private Camera CreateRenderSizeDependentResources(Camera camera)
    {
        var renderSize = _nativeUIService.GetWindowRenderSize(_nativeWindow);

        if (renderSize != _currentRenderSize)
        {
            var aspectRatio = (float)renderSize.Width / renderSize.Height;
            var imageWidth = (int)(renderSize.Width * _renderScaleRatio);
            var imageHeight = (int)(imageWidth / aspectRatio);

            // TODO: Call a delete function
            _platformImage = CreatePlatformImage(_nativeUIService, _nativeWindow, imageWidth, imageHeight);
            _currentRenderSize = renderSize;

            return camera with
            {
                AspectRatio = aspectRatio
            };
        }

        return camera;
    }

    private static PlatformImage CreatePlatformImage(INativeUIService nativeUIService, NativeWindow window, int width, int height)
    {
        var nativeSurface = nativeUIService.CreateImageSurface(window, width, height);
        var nativeSurfaceInfo = nativeUIService.GetImageSurfaceInfo(nativeSurface);
        var imageData = new uint[width * height];

        return new PlatformImage
        {
            Width = width,
            Height = height,
            NativeSurface = nativeSurface,
            NativeSufaceInfo = nativeSurfaceInfo,
            ImageData = imageData
        };
    }
}