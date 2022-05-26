namespace PathTracer.Core;

public readonly record struct Ray
{
    public Ray(Vector3 origin, Vector3 direction)
    {
        if (direction == Vector3.Zero)
        {
            throw new ArgumentException(nameof(direction), "Direction vector must be a valid unit vector.");
        }

        Origin = origin;
        Direction = direction;
    }
    
    public Vector3 Origin { get; init; }
    public Vector3 Direction { get; init; }

    public Vector3 GetPoint(float t)
    {
        return Origin + Direction * t;
    }
}