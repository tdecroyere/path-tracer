using PathTracer.Platform;

var nativeUIService = new NativeUIService();

var nativeApplication = nativeUIService.CreateApplication("Path Tracer");
var nativeWindow = nativeUIService.CreateWindow(nativeApplication, "Path Tracer", 1280, 720, NativeWindowState.Normal);

var imageWidth = 640;
var imageHeight = 480;
var nativeSurface = nativeUIService.CreateImageSurface(nativeWindow, imageWidth, imageHeight);

var appStatus = new NativeAppStatus
{
    IsActive = 1,
    IsRunning = 1
};

var counter = 0;

while (appStatus.IsRunning == 1)
{
    appStatus = nativeUIService.ProcessSystemMessages(nativeApplication);
    
    var imageData = new Span<byte>(new byte[imageWidth * imageHeight * 4]);

    for (var i = 0; i < imageData.Length; i++)
    {
        imageData[i] = (byte)(counter % 255);
    }

    nativeUIService.UpdateImageSurface(nativeSurface, imageData);

    // TODO: Compute correct timesptep
    counter++;
    Thread.Sleep(10);
}