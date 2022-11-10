using System.Diagnostics;
using PathTracer.Platform;
using PathTracer.Platform.NativeUI;

namespace PathTracer;

public class PathTracerApplication
{
    private readonly IApplicationService _applicationService;
    private readonly INativeUIService _nativeUIService;
    private readonly IRenderer<PlatformImage> _renderer;

    private readonly int _targetMS;
    private readonly NativeApplication _nativeApplication;
    private readonly NativeWindow _nativeWindow;
    private readonly PlatformImage _platformImage;

    private Camera _camera;

    public PathTracerApplication(IApplicationService applicationService,
                                 INativeUIService nativeUIService,
                                 IRenderer<PlatformImage> renderer)
    {
        _applicationService = applicationService;
        _nativeUIService = nativeUIService;
        _renderer = renderer;

        var windowWidth = 1280;
        var windowHeight = 720;
        var aspectRatio = (float)windowWidth / windowHeight;

        _nativeApplication = applicationService.CreateApplication("Path Tracer");
        _nativeWindow = nativeUIService.CreateWindow(_nativeApplication, "Path Tracer", windowWidth, windowHeight, NativeWindowState.Normal);

        var imageWidth = 800;
        var imageHeight = (int)(imageWidth / aspectRatio);

        _platformImage = CreateImage(nativeUIService, _nativeWindow, imageWidth, imageHeight);

        _camera = new Camera
        {
            AspectRatio = aspectRatio
        };

        _targetMS = (int)(1.0f / 60.0f * 1000.0f);
    }

    public async Task RunAsync()
    {
        var stopwatch = new Stopwatch();
        var systemMessagesStopwatch = new Stopwatch();
        var renderingStopwatch = new Stopwatch();

        var appStatus = new NativeApplicationStatus();

        while (appStatus.IsRunning == 1)
        {
            stopwatch.Restart();
            systemMessagesStopwatch.Restart();
            // TODO: Investigate Process System Messages seems to take 2-3 ms
            // It seems it is the rendering of the calayer that's is done with an event
            appStatus = _applicationService.ProcessSystemMessages(_nativeApplication);
            systemMessagesStopwatch.Stop();

            renderingStopwatch.Restart();

            _camera = _camera with
            {
                Position = _camera.Position + new Vector3(0, 0, -0.01f)
            };

            await _renderer.RenderAsync(_platformImage, _camera);
            renderingStopwatch.Stop();
            stopwatch.Stop();

            // TODO: Do better here
            var waitingMS = Math.Clamp(_targetMS - stopwatch.ElapsedMilliseconds, 0, _targetMS);

            _nativeUIService.SetWindowTitle(_nativeWindow, $"Path Tracer - Frame: {stopwatch.Elapsed.Milliseconds.ToString("00")}ms (System: {systemMessagesStopwatch.ElapsedMilliseconds.ToString("00")}ms, Render: {renderingStopwatch.ElapsedMilliseconds.ToString("00")}ms, Waiting: {waitingMS.ToString("00")}ms)");
            Thread.Sleep((int)waitingMS);
        }
    }

    private static PlatformImage CreateImage(INativeUIService nativeUIService, NativeWindow window, int width, int height)
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