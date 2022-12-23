namespace PathTracer.Platform.Inputs;

public ref struct KeyboardInputState
{
    private readonly Span<InputObject> _inputObjects;

    public KeyboardInputState(Span<InputObject> inputObjects)
    {
        _inputObjects = inputObjects;
    }

    public ref InputObject KeyA => ref _inputObjects[(int)InputObjectKey.KeyA];
    public ref InputObject KeyB => ref _inputObjects[(int)InputObjectKey.KeyB];
    public ref InputObject KeyC => ref _inputObjects[(int)InputObjectKey.KeyC];
    public ref InputObject KeyD => ref _inputObjects[(int)InputObjectKey.KeyD];
    public ref InputObject KeyE => ref _inputObjects[(int)InputObjectKey.KeyE];
    public ref InputObject KeyF => ref _inputObjects[(int)InputObjectKey.KeyF];
    public ref InputObject KeyG => ref _inputObjects[(int)InputObjectKey.KeyG];
    public ref InputObject KeyH => ref _inputObjects[(int)InputObjectKey.KeyH];
    public ref InputObject KeyI => ref _inputObjects[(int)InputObjectKey.KeyI];
    public ref InputObject KeyJ => ref _inputObjects[(int)InputObjectKey.KeyJ];
    public ref InputObject KeyK => ref _inputObjects[(int)InputObjectKey.KeyK];
    public ref InputObject KeyL => ref _inputObjects[(int)InputObjectKey.KeyL];
    public ref InputObject KeyM => ref _inputObjects[(int)InputObjectKey.KeyM];
    public ref InputObject KeyN => ref _inputObjects[(int)InputObjectKey.KeyN];
    public ref InputObject KeyO => ref _inputObjects[(int)InputObjectKey.KeyO];
    public ref InputObject KeyP => ref _inputObjects[(int)InputObjectKey.KeyP];
    public ref InputObject KeyQ => ref _inputObjects[(int)InputObjectKey.KeyQ];
    public ref InputObject KeyR => ref _inputObjects[(int)InputObjectKey.KeyR];
    public ref InputObject KeyS => ref _inputObjects[(int)InputObjectKey.KeyS];
    public ref InputObject KeyT => ref _inputObjects[(int)InputObjectKey.KeyT];
    public ref InputObject KeyU => ref _inputObjects[(int)InputObjectKey.KeyU];
    public ref InputObject KeyV => ref _inputObjects[(int)InputObjectKey.KeyV];
    public ref InputObject KeyW => ref _inputObjects[(int)InputObjectKey.KeyW];
    public ref InputObject KeyX => ref _inputObjects[(int)InputObjectKey.KeyX];
    public ref InputObject KeyY => ref _inputObjects[(int)InputObjectKey.KeyY];
    public ref InputObject KeyZ => ref _inputObjects[(int)InputObjectKey.KeyZ];

    public ref InputObject Shift => ref _inputObjects[(int)InputObjectKey.Shift];
    public ref InputObject Control => ref _inputObjects[(int)InputObjectKey.Control];
    public ref InputObject Menu => ref _inputObjects[(int)InputObjectKey.Menu];
    public ref InputObject Pause => ref _inputObjects[(int)InputObjectKey.Pause];
    public ref InputObject Capital => ref _inputObjects[(int)InputObjectKey.Capital];
    public ref InputObject Escape => ref _inputObjects[(int)InputObjectKey.Escape];
    public ref InputObject Space => ref _inputObjects[(int)InputObjectKey.Space];
    public ref InputObject End => ref _inputObjects[(int)InputObjectKey.End];
    public ref InputObject Home => ref _inputObjects[(int)InputObjectKey.Home];
    public ref InputObject Left => ref _inputObjects[(int)InputObjectKey.Left];
    public ref InputObject Right => ref _inputObjects[(int)InputObjectKey.Right];
    public ref InputObject Up => ref _inputObjects[(int)InputObjectKey.Up];
    public ref InputObject Down => ref _inputObjects[(int)InputObjectKey.Down];
    public ref InputObject Select => ref _inputObjects[(int)InputObjectKey.Select];
    public ref InputObject Print => ref _inputObjects[(int)InputObjectKey.Print];
    public ref InputObject Insert => ref _inputObjects[(int)InputObjectKey.Insert];
    public ref InputObject Delete => ref _inputObjects[(int)InputObjectKey.Delete];
    public ref InputObject Back => ref _inputObjects[(int)InputObjectKey.Back];
    public ref InputObject Tab => ref _inputObjects[(int)InputObjectKey.Tab];
    public ref InputObject Clear => ref _inputObjects[(int)InputObjectKey.Clear];
    public ref InputObject Return => ref _inputObjects[(int)InputObjectKey.Return];
    public ref InputObject LeftSystemButton => ref _inputObjects[(int)InputObjectKey.LeftSystemButton];
    public ref InputObject RightSystemButton => ref _inputObjects[(int)InputObjectKey.RightSystemButton];
    public ref InputObject Numpad0 => ref _inputObjects[(int)InputObjectKey.Numpad0];
    public ref InputObject Numpad1 => ref _inputObjects[(int)InputObjectKey.Numpad1];
    public ref InputObject Numpad2 => ref _inputObjects[(int)InputObjectKey.Numpad2];
    public ref InputObject Numpad3 => ref _inputObjects[(int)InputObjectKey.Numpad3];
    public ref InputObject Numpad4 => ref _inputObjects[(int)InputObjectKey.Numpad4];
    public ref InputObject Numpad5 => ref _inputObjects[(int)InputObjectKey.Numpad5];
    public ref InputObject Numpad6 => ref _inputObjects[(int)InputObjectKey.Numpad6];
    public ref InputObject Numpad7 => ref _inputObjects[(int)InputObjectKey.Numpad7];
    public ref InputObject Numpad8 => ref _inputObjects[(int)InputObjectKey.Numpad8];
    public ref InputObject Numpad9 => ref _inputObjects[(int)InputObjectKey.Numpad9];
    public ref InputObject Multiply => ref _inputObjects[(int)InputObjectKey.Multiply];
    public ref InputObject Add => ref _inputObjects[(int)InputObjectKey.Add];
    public ref InputObject Separator => ref _inputObjects[(int)InputObjectKey.Separator];
    public ref InputObject Subtract => ref _inputObjects[(int)InputObjectKey.Subtract];
    public ref InputObject Decimal => ref _inputObjects[(int)InputObjectKey.Decimal];
    public ref InputObject Divide => ref _inputObjects[(int)InputObjectKey.Divide];
    public ref InputObject F1 => ref _inputObjects[(int)InputObjectKey.F1];
    public ref InputObject F2 => ref _inputObjects[(int)InputObjectKey.F2];
    public ref InputObject F3 => ref _inputObjects[(int)InputObjectKey.F3];
    public ref InputObject F4 => ref _inputObjects[(int)InputObjectKey.F4];
    public ref InputObject F5 => ref _inputObjects[(int)InputObjectKey.F5];
    public ref InputObject F6 => ref _inputObjects[(int)InputObjectKey.F6];
    public ref InputObject F7 => ref _inputObjects[(int)InputObjectKey.F7];
    public ref InputObject F8 => ref _inputObjects[(int)InputObjectKey.F8];
    public ref InputObject F9 => ref _inputObjects[(int)InputObjectKey.F9];
    public ref InputObject F10 => ref _inputObjects[(int)InputObjectKey.F10];
    public ref InputObject F11 => ref _inputObjects[(int)InputObjectKey.F11];
    public ref InputObject F12 => ref _inputObjects[(int)InputObjectKey.F12];
}
