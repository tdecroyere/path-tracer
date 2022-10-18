namespace PathTracer.Core;

public readonly record struct Ray
{
    private readonly Vector3 _direction;

    public required Vector3 Origin { get; init; }
    public required Vector3 Direction 
    { 
        get
        {
            return _direction;
        } 
        
        init
        {
            if (value == Vector3.Zero)
            {
                throw new ArgumentException("Direction vector must be a valid unit vector.", nameof(Direction));
            }

            _direction = value;
        } 
    }

    public Vector3 GetPoint(float t)
    {
        return Origin + Direction * t;
    }
}