using Veldrid;

namespace PathTracer.Platform.Platforms.VeldridLibrary;

public record VeldridPipeline
{
    public required Pipeline Pipeline { get; init; }
}