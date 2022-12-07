namespace PathTracer.Platform.GraphicsLegacy;

[Flags]
public enum TextureUsage : byte
{
    Sampled = 1,
    Staging = 32
}
