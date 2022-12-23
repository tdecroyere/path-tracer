namespace PathTracer.Core;

// TODO: Convert that to ECS Component
public readonly record struct Camera
{
    public Camera()
    {
        Position = new Vector3(0.0f, 0.0f, -3.0f);
        Target = new Vector3(0.0f, 0.0f, 1.0f);

        VerticalFov = 45.0f;
        NearPlaneDistance = 0.1f;
        AspectRatio = 1.0f;
    }

    public Vector3 Position { get; init; }
    public Vector3 Target { get; init; }
    public float VerticalFov { get; init; }
    public float AspectRatio { get; init; }
    public float NearPlaneDistance { get; init; }
}