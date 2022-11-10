namespace PathTracer.Platform.NativeUI;

[PlatformNativePointer]
public readonly partial record struct NativeWindow
{
}

[PlatformNativePointer]
public readonly partial record struct NativeImageSurface
{
}

public enum NativeWindowState
{
    Normal,
    Maximized
}

public readonly record struct NativeWindowSize
{
    public int Width { get; init; }
    public int Height { get; init; }
}

public readonly record struct NativeImageSurfaceInfo
{
    public int RedShift { get; init; }
    public int GreenShift { get; init; }
    public int BlueShift { get; init; }
    public int AlphaShift { get; init; }
}

[PlatformService]
public interface INativeUIService
{
    NativeWindow CreateWindow(NativeApplication application, string title, int width, int height, NativeWindowState windowState);
    void SetWindowTitle(NativeWindow window, string title);

    NativeImageSurface CreateImageSurface(NativeWindow nativeWindow, int width, int height);
    NativeImageSurfaceInfo GetImageSurfaceInfo(NativeImageSurface imageSurface);
    void UpdateImageSurface(NativeImageSurface imageSurface, ReadOnlySpan<byte> data);
}