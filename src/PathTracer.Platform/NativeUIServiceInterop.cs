using System.Runtime.InteropServices;

namespace PathTracer.Platform;

internal static partial class NativeUIServiceInterop
{
    [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint CreateApplication(string applicationName);

    [LibraryImport("PathTracer.Platform.Native")]
    internal static partial NativeAppStatus ProcessSystemMessages(nint application);

    [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint CreateNativeWindow(nint application, string title, int width, int height, NativeWindowState windowState);

    [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void SetWindowTitle(nint window, string title);

    [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint CreateImageSurface(nint window, int width, int height);

    [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial NativeImageSurfaceInfo GetImageSurfaceInfo(nint imageSurface);

    [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void UpdateImageSurface(nint imageSurface, ReadOnlySpan<byte> data);

    /*[LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial NativeWindowSize GetRenderSize(nint window); 

    [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void UploadImageData(nint surface, ReadOnlySpan<byte> data);*/
}