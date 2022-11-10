namespace PathTracer.Platform;

[PlatformService]
public interface INativeApplicationService
{
    NativeApplication CreateApplication(string applicationName);
    NativeApplicationStatus ProcessSystemMessages(NativeApplication application);
}