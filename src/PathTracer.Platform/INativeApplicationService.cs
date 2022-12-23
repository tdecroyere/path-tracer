namespace PathTracer.Platform;

// TODO: Create a Run method that process the messages and call an update callback?
[PlatformService]
public interface INativeApplicationService
{
    NativeApplication CreateApplication(string applicationName);
    NativeApplicationStatus ProcessSystemMessages(NativeApplication application);
}