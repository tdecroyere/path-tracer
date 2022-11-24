namespace PathTracer.Platform.NativeUI;

[PlatformService]
public interface INativeUIService
{
    NativeWindow CreateWindow(NativeApplication application, string title, int width, int height, NativeWindowState windowState);
    NativeWindowSize GetWindowRenderSize(NativeWindow window);
    nint GetWindowSystemHandle(NativeWindow window);
    void SetWindowTitle(NativeWindow window, string title);

    [Obsolete]
    NativeImageSurface CreateImageSurface(NativeWindow nativeWindow, int width, int height);
    
    [Obsolete]
    NativeImageSurfaceInfo GetImageSurfaceInfo(NativeImageSurface imageSurface);

    [Obsolete]
    void UpdateImageSurface(NativeImageSurface imageSurface, ReadOnlySpan<byte> data);

    NativeControl CreatePanel(NativeWindow window);
    NativeControl CreateButton(NativeControl parent, string text);
}