namespace PathTracer.Core;

// TODO: Convert that to ECS Component
public record struct Sphere
{
    public Sphere()
    {
        Radius = 0.5f;
        Albedo = Vector3.One;
    }

    public Vector3 Position { get; set; }
    public float Radius { get; set; }

    // TODO: Should be part of Material Component
    public Vector3 Albedo { get; set; }
}