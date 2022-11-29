namespace PathTracer.Platform.Inputs;

[PlatformService]
public interface IInputService
{
    [PlatformMethodOverride]
    void UpdateInputState(NativeApplication application, ref InputState inputState);
}