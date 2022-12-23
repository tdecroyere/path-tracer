namespace PathTracer;

public readonly record struct RenderResolutionItem
{
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}