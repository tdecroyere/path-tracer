namespace PathTracer.Platform.NativeUI;

public readonly record struct NativeWindowSize
{
    public int Width { get; init; }
    public int Height { get; init; }
    public float UIScale { get; init; }
}

