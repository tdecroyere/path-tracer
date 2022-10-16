namespace PathTracer.Core;

public readonly record struct Ray
{
    public Ray(string test)
    {

    }
    // TODO: Remove the backing field by using the new C# 11 syntax
    private readonly Vector3 direction;

    public Vector3 Origin { get; init; }
    public Vector3 Direction 
    { 
        get
        {
            return this.direction;
        } 
        
        init
        {
            if (value == Vector3.Zero)
            {
                throw new ArgumentException(nameof(direction), "Direction vector must be a valid unit vector.");
            }

            this.direction = value;
        } 
    }

    public Vector3 GetPoint(float t)
    {
        return Origin + Direction * t;
    }
}