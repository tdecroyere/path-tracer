namespace PathTracer.Platform;

[PlatformService]
public interface IApplicationService
{
    NativeApplication CreateApplication(string applicationName);
    NativeApplicationStatus ProcessSystemMessages(NativeApplication application);
}