namespace PathTracer.Platform.Inputs;

public record struct InputObject
{
    public float Value { get; init; }
    public float PreviousValue { get; internal set; }
    public short Repeatcount { get; init; }
    public short PreviousRepeatCount { get;  internal set; }

    public bool IsPressed => Value == 1.0f;
    public bool IsReleased => Value == 0.0f && Value != PreviousValue;
    public bool HasRepeatChanged => Repeatcount != PreviousRepeatCount && Repeatcount != 0;
    public float Delta => Value - PreviousValue;
}
