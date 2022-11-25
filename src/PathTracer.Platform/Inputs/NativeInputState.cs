using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PathTracer.Platform.Inputs;

// TODO: Do we really need this?
public enum NativeInputObjectType
{
    Digital,
    Analog,
    Relative
}

public record struct NativeInputObject
{
    public float Value { get; init; }
    public float PreviousValue;
    public short Repeatcount { get; init; }
    public short PreviousRepeatCount;

    public bool IsPressed => Value == 1.0f;
    public bool IsReleased => Value == 0.0f && Value != PreviousValue;
    public bool HasRepeatChanged => Repeatcount != PreviousRepeatCount && Repeatcount != 0;
    public float Delta => Value - PreviousValue;
}

public enum NativeInputObjectKey
{
    KeyA = 0,
    KeyB = 1,
    KeyC = 2,
    KeyD = 3,
    KeyE = 4,
    KeyF = 5,
    KeyG = 6,
    KeyH = 7,
    KeyI = 8,
    KeyJ = 9,
    KeyK = 10,
    KeyL = 11,
    KeyM = 12,
    KeyN = 13,
    KeyO = 14,
    KeyP = 15,
    KeyQ = 16,
    KeyR = 17,
    KeyS = 18,
    KeyT = 19,
    KeyU = 20,
    KeyV = 21,
    KeyW = 22,
    KeyX = 23,
    KeyY = 24,
    KeyZ = 25,

    Shift = 26,
    Control = 27,
    Menu = 28,
    Pause = 29,
    Capital = 30,
    Escape = 31,
    Space = 32,
    End = 33,
    Home = 34,
    Left = 35,
    Up = 36,
    Down = 37,
    Select = 38,
    Print = 39,
    Insert = 40,
    Delete = 41,
    Back = 42,
    Tab = 43,
    Clear = 44,
    Return = 45,
    LeftSystemButton = 46,
    RightSystemButton = 47,
    Numpad0 = 48,
    Numpad1 = 49,
    Numpad2 = 50,
    Numpad3 = 51,
    Numpad4 = 52,
    Numpad5 = 53,
    Numpad6 = 54,
    Numpad7 = 55,
    Numpad8 = 56,
    Numpad9 = 57,
    Multiply = 58,
    Add = 59,
    Separator = 60,
    Subtract = 61,
    Decimal = 62,
    Divide = 63,
    F1 = 64,
    F2 = 65,
    F3 = 66,
    F4 = 67,
    F5 = 68,
    F6 = 69,
    F7 = 70,
    F8 = 71,
    F9 = 72,
    F10 = 73,
    F11 = 74,
    F12 = 75,

    MouseAxisX = 76,
    MouseAxisY = 77,
    MouseLeftButton = 78,
    MaxValue = 79
}

public unsafe readonly record struct NativeInputState
{
    public NativeInputState()
    {
        InputObjectCount = (int)NativeInputObjectKey.MaxValue;
        InputObjectPointer = new nint(NativeMemory.AllocZeroed((nuint)(InputObjectCount * Unsafe.SizeOf<NativeInputObject>())));
    }

    internal nint InputObjectPointer { get; }
    internal int InputObjectCount { get; }

    public Span<NativeInputObject> InputObjects 
    { 
        get
        {
            return new Span<NativeInputObject>(InputObjectPointer.ToPointer(), InputObjectCount);
        }
    }
    
    public NativeKeyboard Keyboard => new(InputObjects);
    public NativeMouse Mouse => new(InputObjects);
}

public ref struct NativeKeyboard
{
    private readonly Span<NativeInputObject> _inputObjects;

    public NativeKeyboard(Span<NativeInputObject> inputObjects)
    {
        _inputObjects = inputObjects;
    }

    public ref NativeInputObject KeyA => ref _inputObjects[(int)NativeInputObjectKey.KeyA];
    public ref NativeInputObject KeyB => ref _inputObjects[(int)NativeInputObjectKey.KeyB];
    public ref NativeInputObject KeyC => ref _inputObjects[(int)NativeInputObjectKey.KeyC];
    public ref NativeInputObject KeyD => ref _inputObjects[(int)NativeInputObjectKey.KeyD];
    public ref NativeInputObject KeyE => ref _inputObjects[(int)NativeInputObjectKey.KeyE];
    public ref NativeInputObject KeyF => ref _inputObjects[(int)NativeInputObjectKey.KeyF];
    public ref NativeInputObject KeyG => ref _inputObjects[(int)NativeInputObjectKey.KeyG];
    public ref NativeInputObject KeyH => ref _inputObjects[(int)NativeInputObjectKey.KeyH];
    public ref NativeInputObject KeyI => ref _inputObjects[(int)NativeInputObjectKey.KeyI];
    public ref NativeInputObject KeyJ => ref _inputObjects[(int)NativeInputObjectKey.KeyJ];
    public ref NativeInputObject KeyK => ref _inputObjects[(int)NativeInputObjectKey.KeyK];
    public ref NativeInputObject KeyL => ref _inputObjects[(int)NativeInputObjectKey.KeyL];
    public ref NativeInputObject KeyM => ref _inputObjects[(int)NativeInputObjectKey.KeyM];
    public ref NativeInputObject KeyN => ref _inputObjects[(int)NativeInputObjectKey.KeyN];
    public ref NativeInputObject KeyO => ref _inputObjects[(int)NativeInputObjectKey.KeyO];
    public ref NativeInputObject KeyP => ref _inputObjects[(int)NativeInputObjectKey.KeyP];
    public ref NativeInputObject KeyQ => ref _inputObjects[(int)NativeInputObjectKey.KeyQ];
    public ref NativeInputObject KeyR => ref _inputObjects[(int)NativeInputObjectKey.KeyR];
    public ref NativeInputObject KeyS => ref _inputObjects[(int)NativeInputObjectKey.KeyS];
    public ref NativeInputObject KeyT => ref _inputObjects[(int)NativeInputObjectKey.KeyT];
    public ref NativeInputObject KeyU => ref _inputObjects[(int)NativeInputObjectKey.KeyU];
    public ref NativeInputObject KeyV => ref _inputObjects[(int)NativeInputObjectKey.KeyV];
    public ref NativeInputObject KeyW => ref _inputObjects[(int)NativeInputObjectKey.KeyW];
    public ref NativeInputObject KeyX => ref _inputObjects[(int)NativeInputObjectKey.KeyX];
    public ref NativeInputObject KeyY => ref _inputObjects[(int)NativeInputObjectKey.KeyY];
    public ref NativeInputObject KeyZ => ref _inputObjects[(int)NativeInputObjectKey.KeyZ];

    public ref NativeInputObject Shift => ref _inputObjects[(int)NativeInputObjectKey.Shift];
    public ref NativeInputObject Control => ref _inputObjects[(int)NativeInputObjectKey.Control];
    public ref NativeInputObject Menu => ref _inputObjects[(int)NativeInputObjectKey.Menu];
    public ref NativeInputObject Pause => ref _inputObjects[(int)NativeInputObjectKey.Pause];
    public ref NativeInputObject Capital => ref _inputObjects[(int)NativeInputObjectKey.Capital];
    public ref NativeInputObject Escape => ref _inputObjects[(int)NativeInputObjectKey.Escape];
    public ref NativeInputObject Space => ref _inputObjects[(int)NativeInputObjectKey.Space];
    public ref NativeInputObject End => ref _inputObjects[(int)NativeInputObjectKey.End];
    public ref NativeInputObject Home => ref _inputObjects[(int)NativeInputObjectKey.Home];
    public ref NativeInputObject Left => ref _inputObjects[(int)NativeInputObjectKey.Left];
    public ref NativeInputObject Up => ref _inputObjects[(int)NativeInputObjectKey.Up];
    public ref NativeInputObject Down => ref _inputObjects[(int)NativeInputObjectKey.Down];
    public ref NativeInputObject Select => ref _inputObjects[(int)NativeInputObjectKey.Select];
    public ref NativeInputObject Print => ref _inputObjects[(int)NativeInputObjectKey.Print];
    public ref NativeInputObject Insert => ref _inputObjects[(int)NativeInputObjectKey.Insert];
    public ref NativeInputObject Delete => ref _inputObjects[(int)NativeInputObjectKey.Delete];
    public ref NativeInputObject Back => ref _inputObjects[(int)NativeInputObjectKey.Back];
    public ref NativeInputObject Tab => ref _inputObjects[(int)NativeInputObjectKey.Tab];
    public ref NativeInputObject Clear => ref _inputObjects[(int)NativeInputObjectKey.Clear];
    public ref NativeInputObject Return => ref _inputObjects[(int)NativeInputObjectKey.Return];
    public ref NativeInputObject LeftSystemButton => ref _inputObjects[(int)NativeInputObjectKey.LeftSystemButton];
    public ref NativeInputObject RightSystemButton => ref _inputObjects[(int)NativeInputObjectKey.RightSystemButton];
    public ref NativeInputObject Numpad0 => ref _inputObjects[(int)NativeInputObjectKey.Numpad0];
    public ref NativeInputObject Numpad1 => ref _inputObjects[(int)NativeInputObjectKey.Numpad1];
    public ref NativeInputObject Numpad2 => ref _inputObjects[(int)NativeInputObjectKey.Numpad2];
    public ref NativeInputObject Numpad3 => ref _inputObjects[(int)NativeInputObjectKey.Numpad3];
    public ref NativeInputObject Numpad4 => ref _inputObjects[(int)NativeInputObjectKey.Numpad4];
    public ref NativeInputObject Numpad5 => ref _inputObjects[(int)NativeInputObjectKey.Numpad5];
    public ref NativeInputObject Numpad6 => ref _inputObjects[(int)NativeInputObjectKey.Numpad6];
    public ref NativeInputObject Numpad7 => ref _inputObjects[(int)NativeInputObjectKey.Numpad7];
    public ref NativeInputObject Numpad8 => ref _inputObjects[(int)NativeInputObjectKey.Numpad8];
    public ref NativeInputObject Numpad9 => ref _inputObjects[(int)NativeInputObjectKey.Numpad9];
    public ref NativeInputObject Multiply => ref _inputObjects[(int)NativeInputObjectKey.Multiply];
    public ref NativeInputObject Add => ref _inputObjects[(int)NativeInputObjectKey.Add];
    public ref NativeInputObject Separator => ref _inputObjects[(int)NativeInputObjectKey.Separator];
    public ref NativeInputObject Subtract => ref _inputObjects[(int)NativeInputObjectKey.Subtract];
    public ref NativeInputObject Decimal => ref _inputObjects[(int)NativeInputObjectKey.Decimal];
    public ref NativeInputObject Divide => ref _inputObjects[(int)NativeInputObjectKey.Divide];
    public ref NativeInputObject F1 => ref _inputObjects[(int)NativeInputObjectKey.F1];
    public ref NativeInputObject F2 => ref _inputObjects[(int)NativeInputObjectKey.F2];
    public ref NativeInputObject F3 => ref _inputObjects[(int)NativeInputObjectKey.F3];
    public ref NativeInputObject F4 => ref _inputObjects[(int)NativeInputObjectKey.F4];
    public ref NativeInputObject F5 => ref _inputObjects[(int)NativeInputObjectKey.F5];
    public ref NativeInputObject F6 => ref _inputObjects[(int)NativeInputObjectKey.F6];
    public ref NativeInputObject F7 => ref _inputObjects[(int)NativeInputObjectKey.F7];
    public ref NativeInputObject F8 => ref _inputObjects[(int)NativeInputObjectKey.F8];
    public ref NativeInputObject F9 => ref _inputObjects[(int)NativeInputObjectKey.F9];
    public ref NativeInputObject F10 => ref _inputObjects[(int)NativeInputObjectKey.F10];
    public ref NativeInputObject F11 => ref _inputObjects[(int)NativeInputObjectKey.F11];
    public ref NativeInputObject F12 => ref _inputObjects[(int)NativeInputObjectKey.F12];
}

public ref struct NativeMouse
{
    private readonly Span<NativeInputObject> _inputObjects;

    public NativeMouse(Span<NativeInputObject> inputObjects)
    {
        _inputObjects = inputObjects;
    }

    public ref NativeInputObject AxisX => ref _inputObjects[(int)NativeInputObjectKey.MouseAxisX];
    public ref NativeInputObject AxisY => ref _inputObjects[(int)NativeInputObjectKey.MouseAxisY];
    public ref NativeInputObject MouseLeftButton => ref _inputObjects[(int)NativeInputObjectKey.MouseLeftButton];
}