namespace PathTracer.ImageWriters;

public record struct FileImage : IImage
{
    public FileImage()
    {
        Width = 0;
        Height = 0;
        OutputPath = string.Empty;
        ImageData = Array.Empty<Vector4>();
    }

    public int Width { get; init; }
    public int Height { get; init; }
    public string OutputPath { get; init; }
    public Memory<Vector4> ImageData { get; init; }
}