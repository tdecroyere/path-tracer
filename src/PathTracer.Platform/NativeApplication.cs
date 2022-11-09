namespace PathTracer.Platform;

public readonly record struct NativeApplication
{
    internal nint NativePointer { get; init; }

    public static implicit operator nint(NativeApplication src) => src.NativePointer;
    public static implicit operator NativeApplication(nint src) => new() { NativePointer = src };
}