namespace PathTracer.Core;

public readonly record struct RayHitPayload
{
    public float HitDistance { get; init; }
    public Vector3 WorldPosition { get; init; }
    public Vector3 WorldNormal { get; init; }
    public int ObjectIndex { get; init; }
}