namespace PathTracer.Platform.NativeUI;

public interface INativeUIService
{
    NativeWindow CreateWindow(NativeApplication application, string title, int width, int height, NativeWindowState windowState);
    void SetWindowTitle(NativeWindow window, string title);

    NativeImageSurface CreateImageSurface(NativeWindow nativeWindow, int width, int height);
    NativeImageSurfaceInfo GetImageSurfaceInfo(NativeImageSurface imageSurface);
    void UpdateImageSurface(NativeImageSurface imageSurface, ReadOnlySpan<byte> data);
}