namespace PathTracer.Platform.Graphics;

public readonly partial record struct GraphicsAllocationInfos
{
    public nuint SizeInBytes { get; init; }
    public nuint Alignment { get; init; }
}