namespace PathTracer.Platform.Inputs;

[PlatformService]
public interface INativeInputService
{
    [PlatformMethodOverride]
    void UpdateInputState(NativeApplication application, ref NativeInputState inputState);
}