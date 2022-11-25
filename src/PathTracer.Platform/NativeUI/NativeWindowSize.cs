namespace PathTracer.Platform.NativeUI;

public readonly record struct NativeWindowSize
{
    public int Width { get; }
    public int Height { get; }
    public float UIScale { get; }
}

