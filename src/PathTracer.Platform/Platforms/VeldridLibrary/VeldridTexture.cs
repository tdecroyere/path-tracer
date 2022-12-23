using Veldrid;

namespace PathTracer.Platform.Platforms.VeldridLibrary;

public record VeldridTexture
{
    public required Texture Texture { get; init; }
    public TextureView? TextureView { get; init; }
    public required GraphicsDevice GraphicsDevice { get; init; }
}