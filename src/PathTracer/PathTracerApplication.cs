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
    private Camera _camera;
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
        _camera = new Camera();
    }

    public async Task RunAsync()
    {
        var stopwatch = new Stopwatch();
        var systemMessagesStopwatch = new Stopwatch();
        var renderingStopwatch = new Stopwatch();

        var appStatus = new NativeApplicationStatus();
        var inputState = new NativeInputState();

        while (appStatus.IsRunning == 1)
        {
            stopwatch.Restart();
            systemMessagesStopwatch.Restart();
            appStatus = _applicationService.ProcessSystemMessages(_nativeApplication);
            systemMessagesStopwatch.Stop();

            _nativeInputService.GetInputState(_nativeApplication, ref inputState);

            var movementSpeed = 0.01f;
            var forwardInput = inputState.Keyboard.KeyZ.Value - inputState.Keyboard.KeyS.Value;
            var sideInput = inputState.Keyboard.KeyD.Value - inputState.Keyboard.KeyQ.Value;

            var movementVector = new Vector3(sideInput * movementSpeed, 0.0f, forwardInput * movementSpeed);

            CreateRenderSizeDependentResources();
            var renderImage = _platformImage;
            
            renderingStopwatch.Restart();

            _camera = _camera with
            {
                Position = _camera.Position + movementVector
            };

            await _renderer.RenderAsync(renderImage, _camera);
            renderingStopwatch.Stop();
            stopwatch.Stop();

            // TODO: Do better here
            var waitingMS = Math.Clamp(_targetMS - stopwatch.ElapsedMilliseconds, 0, _targetMS);

            _nativeUIService.SetWindowTitle(_nativeWindow, $"Path Tracer ({renderImage.Width}x{renderImage.Height}) - Frame: {stopwatch.Elapsed.Milliseconds:00}ms (System: {systemMessagesStopwatch.ElapsedMilliseconds:00}ms, Render: {renderingStopwatch.ElapsedMilliseconds:00}ms, Waiting: {waitingMS:00}ms)");
            Thread.Sleep((int)waitingMS);
        }
    }

    private void CreateRenderSizeDependentResources()
    {
        var renderSize = _nativeUIService.GetWindowRenderSize(_nativeWindow);

        if (renderSize != _currentRenderSize)
        {
            var aspectRatio = (float)renderSize.Width / renderSize.Height;
            var imageWidth = (int)(renderSize.Width * _renderScaleRatio);
            var imageHeight = (int)(imageWidth / aspectRatio);

            // TODO: Call a delete function
            _platformImage = CreatePlatformImage(_nativeUIService, _nativeWindow, imageWidth, imageHeight);

            _camera = _camera with
            {
                AspectRatio = aspectRatio
            };

            _currentRenderSize = renderSize;
        }
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