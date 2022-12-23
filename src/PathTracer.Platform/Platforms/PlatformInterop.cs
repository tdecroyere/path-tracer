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
    internal static partial class InputServiceInterop
    {
        [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void PT_UpdateInputState(nint application, ref InputState state);
    }
}

namespace PathTracer.Platform
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
    }
}