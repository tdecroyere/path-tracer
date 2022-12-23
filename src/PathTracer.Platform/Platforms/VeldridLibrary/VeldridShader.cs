using Veldrid;

namespace PathTracer.Platform.Platforms.VeldridLibrary;

public record VeldridShader
{
    public required Shader VertexShader { get; init; }
    public required Shader FragmentShader { get; init; }
}