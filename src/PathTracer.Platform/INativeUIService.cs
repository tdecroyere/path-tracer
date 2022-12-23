namespace PathTracer.Platform;

[PlatformService]
public interface INativeUIService
{
    NativeWindow CreateWindow(NativeApplication application, string title, int width, int height, NativeWindowState windowState);
    NativeWindowSize GetWindowRenderSize(NativeWindow window);
    nint GetWindowSystemHandle(NativeWindow window);
    void SetWindowTitle(NativeWindow window, string title);
}