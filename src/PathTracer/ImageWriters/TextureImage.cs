namespace PathTracer.ImageWriters;

public record struct TextureImage : IImage
{
    public TextureImage()
    {
        Width = 0;
        Height = 0;
        CpuTexture = new Texture();
        GpuTexture = new Texture();
        ImageData = Array.Empty<uint>();
        TextureId = 0;
    }

    public int Width { get; init; }
    public int Height { get; init; }
    public Texture CpuTexture { get; init; }
    public Texture GpuTexture { get; init; }
    public nint TextureId { get; init; }
    public required CommandList CommandList { get; init; }
    public Memory<uint> ImageData { get; init; }
}