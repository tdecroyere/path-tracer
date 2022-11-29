namespace PathTracer.Platform.Inputs;

internal partial class InputService
{
    public void UpdateInputState(NativeApplication application, ref InputState inputState)
    {
        for (var i = 0; i < inputState.InputObjects.Length; i++)
        {
            ref var inputObject = ref inputState.InputObjects[i];
            inputObject.PreviousValue = inputObject.Value;
            inputObject.PreviousRepeatCount = inputObject.Repeatcount;
        }

        UpdateInputStateImplementation(application, ref inputState);
    }
}