namespace PathTracer.Core;

public record struct Material
{
    public Material()
    {
        Albedo = Vector3.One;
        Roughness = 1.0f;
        Metallic = 0.0f;
    }
    
    public Vector3 Albedo { get; set; }
    public float Roughness { get; set; }
    public float Metallic { get; set; }
}