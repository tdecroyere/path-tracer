namespace PathTracer.Platform.GraphicsLegacy;

public record ResourceLayoutElement
{
    public required string Name { get; init; }
    public required ResourceLayoutKind ResourceKind { get; init; }
    public required ResourceLayoutShaderStages ShaderStages { get; init; }
}

public enum ResourceLayoutKind : byte
{
    UniformBuffer = 0,
    StructuredBufferReadOnly = 1,
    StructuredBufferReadWrite = 2,
    TextureReadOnly = 3,
    TextureReadWrite = 4,
    Sampler = 5
}

[Flags]
public enum ResourceLayoutShaderStages : byte
{
    None = 0,
    Vertex = 1,
    Geometry = 2,
    TessellationControl = 4,
    TessellationEvaluation = 8,
    Fragment = 16,
    Compute = 32
}