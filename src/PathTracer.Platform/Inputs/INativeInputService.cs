namespace PathTracer.Platform.Inputs;

[PlatformService]
public interface INativeInputService
{
    [PlatformMethodOverride]
    void GetInputState(NativeApplication application, ref NativeInputState inputState);
}