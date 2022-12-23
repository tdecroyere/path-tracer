namespace PathTracer.Platform.Inputs;

public ref struct MouseInputState
{
    private readonly Span<InputObject> _inputObjects;

    public MouseInputState(Span<InputObject> inputObjects)
    {
        _inputObjects = inputObjects;
    }

    public ref InputObject AxisX => ref _inputObjects[(int)InputObjectKey.MouseAxisX];
    public ref InputObject AxisY => ref _inputObjects[(int)InputObjectKey.MouseAxisY];
    public ref InputObject MouseLeftButton => ref _inputObjects[(int)InputObjectKey.MouseLeftButton];
}