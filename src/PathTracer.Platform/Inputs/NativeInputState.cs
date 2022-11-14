namespace PathTracer.Platform.Inputs;

public enum NativeInputObjectType
{
    Digital,
    Analog,
    Relative
}

public record struct NativeInputObject
{
    public NativeInputObjectType ObjectType { get; init; }
    public float Value { get; init; }
    public float PreviousValue;

    // TODO: Switch to extension methods
    public bool IsPressed()
    {
        return Value == 1.0f;
    }

    public bool IsReleased()
    {
        return Value == 0.0f && Value != PreviousValue;
    }
}

public record struct NativeKeyboard
{
    public NativeInputObject KeyA;
    public NativeInputObject KeyD;
    public NativeInputObject KeyQ;
    public NativeInputObject KeyS;
    public NativeInputObject KeyZ;
    public NativeInputObject ArrowUp;
    public NativeInputObject ArrowDown;
    public NativeInputObject ArrowLeft;
    public NativeInputObject ArrowRight;
}

public record struct NativeInputState
{
    public NativeKeyboard Keyboard;
}