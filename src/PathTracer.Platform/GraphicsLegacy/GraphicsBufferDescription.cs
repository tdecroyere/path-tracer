namespace PathTracer.Platform.GraphicsLegacy;

public readonly record struct GraphicsBufferDescription
{
    public nuint SizeInBytes { get; init; }
    public GraphicsBufferUsage Usage { get; init; }
}