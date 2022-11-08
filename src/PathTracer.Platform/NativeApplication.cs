namespace PathTracer.Platform;

public readonly record struct NativeApplication
{
    internal nint NativePointer { get; init; }
}