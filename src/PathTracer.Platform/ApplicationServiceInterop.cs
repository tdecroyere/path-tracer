using System.Runtime.InteropServices;

namespace PathTracer.Platform;

internal static partial class ApplicationServiceInterop
{
    [LibraryImport("PathTracer.Platform.Native", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint CreateApplication(string applicationName);

    [LibraryImport("PathTracer.Platform.Native")]
    internal static partial NativeApplicationStatus ProcessSystemMessages(nint application);
}