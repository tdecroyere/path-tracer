using System.Runtime.InteropServices;

// TODO: This file could be potentially removed later if Source generators
// can be chained together. Check issue for follow up: https://github.com/dotnet/roslyn/issues/57239

namespace PathTracer.Platform
{
    internal static partial class NativeApplicationServiceInterop
    {
        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint PT_CreateApplication(string applicationName);

        [LibraryImport("PathTracer.Platform.Native")]
        internal static partial NativeApplicationStatus PT_ProcessSystemMessages(nint application);
    }
}

namespace PathTracer.Platform.Inputs
{
    internal static partial class NativeInputServiceInterop
    {
        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void PT_UpdateInputState(nint application, ref NativeInputState state);
    }
}

namespace PathTracer.Platform.NativeUI
{
    internal static partial class NativeUIServiceInterop
    {
        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint PT_CreateWindow(nint application, string title, int width, int height, NativeWindowState windowState);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint PT_GetWindowSystemHandle(nint window); 

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial NativeWindowSize PT_GetWindowRenderSize(nint window); 

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void PT_SetWindowTitle(nint window, string title);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint PT_CreateImageSurface(nint window, int width, int height);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial NativeImageSurfaceInfo PT_GetImageSurfaceInfo(nint imageSurface);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void PT_UpdateImageSurface(nint imageSurface, ReadOnlySpan<byte> data);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint PT_CreatePanel(nint window);

        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint PT_CreateButton(nint parent, string text);
    }
}