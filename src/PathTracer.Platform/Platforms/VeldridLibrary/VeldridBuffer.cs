using Veldrid;

namespace PathTracer.Platform.Platforms.VeldridLibrary;

public record VeldridBuffer
{
    public required DeviceBuffer Buffer { get; init; }
    public required GraphicsDevice GraphicsDevice { get; init; }
}