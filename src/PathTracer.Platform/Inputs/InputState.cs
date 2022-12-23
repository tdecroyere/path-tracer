using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PathTracer.Platform.Inputs;

public unsafe readonly record struct InputState
{
    public InputState()
    {
        InputObjectCount = (int)InputObjectKey.MaxValue;
        InputObjectPointer = new nint(NativeMemory.AllocZeroed((nuint)(InputObjectCount * Unsafe.SizeOf<InputObject>())));
    }

    internal nint InputObjectPointer { get; }
    internal int InputObjectCount { get; }

    public Span<InputObject> InputObjects 
    { 
        get
        {
            return new Span<InputObject>(InputObjectPointer.ToPointer(), InputObjectCount);
        }
    }
    
    public KeyboardInputState Keyboard => new(InputObjects);
    public MouseInputState Mouse => new(InputObjects);
}