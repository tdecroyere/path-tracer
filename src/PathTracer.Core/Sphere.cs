namespace PathTracer.Core;

// TODO: Convert that to ECS Component
public record struct Sphere
{
    public Sphere()
    {
        Radius = 0.5f;
    }

    public Vector3 Position { get; set; }
    public float Radius { get; set; }

    public int MaterialIndex { get; set; }
}