using PathTracer.Platform.NativeUI;

namespace PathTracer.Platform;

internal class ApplicationService : IApplicationService
{
    public NativeApplication CreateApplication(string applicationName)
    {
        var nativePointer = NativeUIServiceInterop.CreateApplication(applicationName);

        return new NativeApplication
        {
            NativePointer = nativePointer
        };
    }

    public NativeApplicationStatus ProcessSystemMessages(NativeApplication application)
    {
        return NativeUIServiceInterop.ProcessSystemMessages(application.NativePointer);
    }
}