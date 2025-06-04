namespace PathTracer.ImageWriters;

public record struct FileImage : IImage
{
    public FileImage()
    {
        Width = 0;
        Height = 0;
        ImageData = Array.Empty<Vector4>();
        AccumulationData = Array.Empty<Vector4>();
    }

    public int Width { get; init; }
    public int Height { get; init; }
    public Memory<Vector4> ImageData { get; init; }
    public Memory<Vector4> AccumulationData { get; init; }
    public int FrameCount { get; set; }
}