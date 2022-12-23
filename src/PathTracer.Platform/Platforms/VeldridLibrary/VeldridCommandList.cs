using Veldrid;

namespace PathTracer.Platform.Platforms.VeldridLibrary;

public record VeldridCommandList
{
    public required CommandList CommandList { get; init; }
    public required GraphicsDevice GraphicsDevice { get; init; }
}