namespace PathTracer.Platform.Inputs;

internal partial class NativeInputService
{
    public void GetInputState(NativeApplication application, ref NativeInputState inputState)
    {
        UpdatePreviousValue(ref inputState.Keyboard.KeyA);
        UpdatePreviousValue(ref inputState.Keyboard.KeyD);
        UpdatePreviousValue(ref inputState.Keyboard.KeyQ);
        UpdatePreviousValue(ref inputState.Keyboard.KeyS);
        UpdatePreviousValue(ref inputState.Keyboard.KeyZ);
        UpdatePreviousValue(ref inputState.Keyboard.ArrowUp);
        UpdatePreviousValue(ref inputState.Keyboard.ArrowDown);
        UpdatePreviousValue(ref inputState.Keyboard.ArrowLeft);
        UpdatePreviousValue(ref inputState.Keyboard.ArrowRight);

        GetInputStateImplementation(application, ref inputState);
    }

    private static void UpdatePreviousValue(ref NativeInputObject inputObject)
    {
        inputObject.PreviousValue = inputObject.Value;
    }
}