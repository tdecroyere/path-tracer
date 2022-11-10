using PathTracer.Platform.NativeUI;

namespace PathTracer;

public record struct PlatformImage : IImage
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required NativeImageSurface NativeSurface { get; init; }
    public required NativeImageSurfaceInfo NativeSufaceInfo { get; init; }
    public required Memory<uint> ImageData { get; init; }
}