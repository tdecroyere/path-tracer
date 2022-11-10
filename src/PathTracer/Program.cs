using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using PathTracer.Core;
using PathTracer.Platform;
using PathTracer.Platform.NativeUI;

var serviceCollection = new ServiceCollection();
serviceCollection.UsePathTracerPlatform();

var serviceProvider = serviceCollection.BuildServiceProvider();

var applicationService = serviceProvider.GetRequiredService<IApplicationService>();
var nativeUIService = serviceProvider.GetRequiredService<INativeUIService>();

var windowWidth = 1280;
var windowHeight = 720;
var aspectRatio = (float)windowWidth / windowHeight;

var nativeApplication = applicationService.CreateApplication("Path Tracer");
var nativeWindow = nativeUIService.CreateWindow(nativeApplication, "Path Tracer", windowWidth, windowHeight, NativeWindowState.Normal);

var imageWidth = 800;
var imageHeight = (int)(imageWidth / aspectRatio);
var nativeSurface = nativeUIService.CreateImageSurface(nativeWindow, imageWidth, imageHeight);
var nativeImageInfo = nativeUIService.GetImageSurfaceInfo(nativeSurface);

var appStatus = new NativeApplicationStatus();

var stopwatch = new Stopwatch();
var systemMessagesStopwatch = new Stopwatch();
var renderingStopwatch = new Stopwatch();

var imageData = new uint[imageWidth * imageHeight];

var camera = new Camera
{
    AspectRatio = aspectRatio
};

var targetMS = (int)(1.0f / 60.0f * 1000.0f);

while (appStatus.IsRunning == 1)
{
    stopwatch.Restart();
    systemMessagesStopwatch.Restart();
    // TODO: Investigate Process System Messages seems to take 2-3 ms
    // It seems it is the rendering of the calayer that's is done with an event
    appStatus = applicationService.ProcessSystemMessages(nativeApplication);
    systemMessagesStopwatch.Stop();

    renderingStopwatch.Restart();
    camera = camera with 
    { 
        Position = camera.Position + new Vector3(0, 0, -0.01f)
    };

    var rayGenerator = new RayGenerator(camera);
    
    //for (var i = 0; i < imageHeight; i++)
    Parallel.For(0, imageHeight, (i) =>
    {
        var pixelRowIndex = (imageHeight - 1 - i) * imageWidth;

        for (var j = 0; j < imageWidth; j++)
        {
            var u = (float)j / imageWidth;
            var v = (float)i / imageHeight;

            var pixelCoordinates = new Vector2(u, v);
            
            // Remap pixel coordinates to [-1, 1] range
            pixelCoordinates = pixelCoordinates * 2.0f - new Vector2(1.0f, 1.0f);

            var color = PixelShader(pixelCoordinates, rayGenerator);
            color = Vector4.Clamp(color, Vector4.Zero, new Vector4(1.0f)) * 255.0f;

            imageData[pixelRowIndex + j] = (uint)color.W << nativeImageInfo.AlphaShift | (uint)color.Z << nativeImageInfo.BlueShift | (uint)color.Y << nativeImageInfo.GreenShift | (uint)color.X << nativeImageInfo.RedShift; 
        }
    });
    
    renderingStopwatch.Stop();

    nativeUIService.UpdateImageSurface(nativeSurface, MemoryMarshal.Cast<uint, byte>(imageData));
    stopwatch.Stop();

    // TODO: Do better here
    var waitingMS = Math.Clamp(targetMS - stopwatch.ElapsedMilliseconds, 0, targetMS);

    nativeUIService.SetWindowTitle(nativeWindow, $"Path Tracer - Frame: {stopwatch.Elapsed.Milliseconds.ToString("00")}ms (System: {systemMessagesStopwatch.ElapsedMilliseconds.ToString("00")}ms, Render: {renderingStopwatch.ElapsedMilliseconds.ToString("00")}ms, Waiting: {waitingMS.ToString("00")}ms)");
    Thread.Sleep((int)waitingMS);
}

static Vector4 PixelShader(Vector2 pixelCoordinates, RayGenerator rayGenerator)
{
    var lightDirection = new Vector3(1.0f, -1.0f, 1.0f);
    var radius = 0.5f;

    var ray = rayGenerator.GenerateRay(pixelCoordinates);

    // Construct quadratic function components
    var a = Vector3.Dot(ray.Direction, ray.Direction);
    var b = 2.0f * Vector3.Dot(ray.Origin, ray.Direction);
    var c = Vector3.Dot(ray.Origin, ray.Origin) - radius * radius;

    // Solve quadratic function
    var discriminant = b * b - 4.0f * a * c;

    if (discriminant < 0.0f)
    {
        return Vector4.Zero;
    }

    var t = (-b + -MathF.Sqrt(discriminant)) / (2.0f * a);

    if (t < 0.0f)
    {
        return Vector4.Zero;
    }

    // Compute normal
    var intersectPoint = ray.GetPoint(t);
    var normal = Vector3.Normalize(intersectPoint);

    // Remap the normal to color space
    //return 0.5f * (normal + new Vector3(1, 1, 1));

    // Compute light
    var light = MathF.Max(Vector3.Dot(normal, -lightDirection), 0.0f);
    return new Vector4(light * new Vector3(1, 1, 0), 1.0f);
}