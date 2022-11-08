namespace PathTracer.Platform;

public readonly record struct NativeApplication
{
    internal nint NativePointer { get; init; }
}

public readonly record struct NativeWindow
{
    internal nint NativePointer { get; init; }
}

public readonly record struct NativeImageSurface
{
    internal nint NativePointer { get; init; }
}

public enum NativeWindowState
{
    Normal,
    Maximized
}

public readonly record struct NativeAppStatus
{
    public NativeAppStatus()
    {
        IsRunning = 1;
        IsActive = 1;
    }

    public int IsRunning { get; init; }
    public int IsActive { get; init; }
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

public interface INativeUIService
{
    NativeApplication CreateApplication(string applicationName);
    NativeAppStatus ProcessSystemMessages(NativeApplication application);
    
    NativeWindow CreateWindow(NativeApplication application, string title, int width, int height, NativeWindowState windowState);
    
    NativeImageSurface CreateImageSurface(NativeWindow nativeWindow, int width, int height);
    NativeImageSurfaceInfo GetImageSurfaceInfo(NativeImageSurface imageSurface);
    void UpdateImageSurface(NativeImageSurface imageSurface, ReadOnlySpan<byte> data);
}

public class NativeUIService : INativeUIService
{
    public NativeApplication CreateApplication(string applicationName)
    {
        var nativePointer =  NativeUIServiceInterop.CreateApplication(applicationName);

        return new NativeApplication
        {
            NativePointer = nativePointer
        };
    }

    public NativeAppStatus ProcessSystemMessages(NativeApplication application)
    {
        return NativeUIServiceInterop.ProcessSystemMessages(application.NativePointer);
    }

    public NativeWindow CreateWindow(NativeApplication application, string title, int width, int height, NativeWindowState windowState)
    {
        var nativePointer = NativeUIServiceInterop.CreateNativeWindow(application.NativePointer, title, width, height, windowState);
        
        return new NativeWindow
        {
            NativePointer = nativePointer
        };
    }
    
    public NativeImageSurface CreateImageSurface(NativeWindow window, int width, int height)
    {
        var nativePointer = NativeUIServiceInterop.CreateImageSurface(window.NativePointer, width, height);
        
        return new NativeImageSurface
        {
            NativePointer = nativePointer
        };
    }

    public NativeImageSurfaceInfo GetImageSurfaceInfo(NativeImageSurface imageSurface)
    {
        return NativeUIServiceInterop.GetImageSurfaceInfo(imageSurface.NativePointer);
    }

    public void UpdateImageSurface(NativeImageSurface imageSurface, ReadOnlySpan<byte> data)
    {
        NativeUIServiceInterop.UpdateImageSurface(imageSurface.NativePointer, data);
    }
}