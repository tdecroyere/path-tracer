namespace PathTracer.Platform.Inputs;

[PlatformService]
public interface INativeInputService
{
    void GetInputState(NativeApplication application, out NativeInputState inputState);
}