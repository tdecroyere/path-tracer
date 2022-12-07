using PathTracer.Platform.GraphicsLegacy;

namespace PathTracer;

public record struct TextureImage : IImage
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Texture CpuTexture { get; init; }
    public required Texture GpuTexture { get; init; }
    public required CommandList CommandList { get; init; }
    public required Memory<uint> ImageData { get; init; }
}