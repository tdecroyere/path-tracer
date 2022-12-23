namespace PathTracer.Core;

public class Scene
{
    public Scene()
    {
        Spheres = new List<Sphere>();
    }

    public IList<Sphere> Spheres { get; }

    // TODO: Temporary
    public bool HasChanged { get; set; }
}