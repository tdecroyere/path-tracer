namespace PathTracer.Console;

public readonly record struct FileImage : IImage
{
    public FileImage()
    {
        Width = 0;
        Height = 0;
        ImageData = Array.Empty<Vector4>();
    }

    public int Width { get; init; }
    public int Height { get; init; }
    public Memory<Vector4> ImageData { get; init; }
}