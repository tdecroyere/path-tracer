namespace PathTracer.Core;

public class Scene
{
    public Scene()
    {
        Spheres = new List<Sphere>();
        Materials = new List<Material>();
    }

    public IList<Sphere> Spheres { get; }
    public IList<Material> Materials { get; }

    // TODO: Temporary
    public bool HasChanged { get; set; }
}