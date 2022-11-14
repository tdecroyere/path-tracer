using System.Runtime.InteropServices;

// TODO: This file could be potentially removed later if Source generators
// can be chained together. Check issue for follow up: https://github.com/dotnet/roslyn/issues/57239

namespace PathTracer.Platform
{
    internal static partial class NativeApplicationServiceInterop
    {
        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint CreateApplication(string applicationName);

        [LibraryImport("PathTracer.Platform.Native")]
        internal static partial NativeApplicationStatus ProcessSystemMessages(nint application);
    }
}

namespace PathTracer.Platform.Inputs
{
    internal static partial class NativeInputServiceInterop
    {
        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void GetInputState(nint application, ref NativeInputState state);
    }
}

namespace PathTracer.Platform.NativeUI
{
    internal static partial class NativeUIServiceInterop
    {
        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint CreateApplication(string applicationName);

        [LibraryImport("PathTracer.Platform.Native")]
        internal static partial NativeApplicationStatus ProcessSystemMessages(nint application);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint CreateWindow(nint application, string title, int width, int height, NativeWindowState windowState);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial NativeWindowSize GetWindowRenderSize(nint window); 

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void SetWindowTitle(nint window, string title);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint CreateImageSurface(nint window, int width, int height);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial NativeImageSurfaceInfo GetImageSurfaceInfo(nint imageSurface);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void UpdateImageSurface(nint imageSurface, ReadOnlySpan<byte> data);
    }
}