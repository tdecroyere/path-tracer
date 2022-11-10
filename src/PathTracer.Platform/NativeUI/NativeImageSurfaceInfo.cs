namespace PathTracer.Platform.NativeUI;

public readonly record struct NativeImageSurfaceInfo
{
    public int RedShift { get; init; }
    public int GreenShift { get; init; }
    public int BlueShift { get; init; }
    public int AlphaShift { get; init; }
}