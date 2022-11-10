/*
namespace PathTracer.Platform.NativeUI;


internal class NativeUIService : INativeUIService
{
    public NativeWindow CreateWindow(NativeApplication application, string title, int width, int height, NativeWindowState windowState)
    {
        var nativePointer = NativeUIServiceInterop.CreateWindow(application.NativePointer, title, width, height, windowState);
        
        return new NativeWindow
        {
            NativePointer = nativePointer
        };
    }

    public void SetWindowTitle(NativeWindow window, string title)
    {
        NativeUIServiceInterop.SetWindowTitle(window.NativePointer, title);
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
}*/