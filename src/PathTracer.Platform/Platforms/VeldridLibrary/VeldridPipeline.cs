using Veldrid;

namespace PathTracer.Platform.Platforms.VeldridLibrary;

public record VeldridPipeline
{
    public required ResourceLayout MainLayout { get; init; }
    public required ResourceLayout TextureLayout { get; init; }
    public required Pipeline Pipeline { get; init; }
}