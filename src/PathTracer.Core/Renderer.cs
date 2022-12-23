namespace PathTracer.Core;

public class Renderer<TImage, TParameter> : IRenderer<TImage, TParameter> where TImage : IImage
{
    private readonly IImageWriter<TImage, TParameter> _imageWriter;

    public Renderer(IImageWriter<TImage, TParameter> imageWriter)
    {
        _imageWriter = imageWriter;
    }

    public void Render(TImage image, Scene scene, Camera camera)
    {
        if (image.Width == 0 || image.Height == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(image), "Image cannot have a width or height of 0.");
        }

        var imageWidth = image.Width;
        var imageHeight = image.Height;
        var rayGenerator = new RayGenerator(camera);

        //for (var i = 0; i < imageHeight; i++)
        Parallel.For(0, imageHeight, (i) =>
        {
            var pixelRowIndex = (imageHeight - 1 - i) * imageWidth;

            for (var j = 0; j < imageWidth; j++)
            {
                var u = (float)j / imageWidth;
                var v = (float)i / imageHeight;

                var pixelCoordinates = new Vector2(u, v);

                // Remap pixel coordinates to [-1, 1] range
                pixelCoordinates = pixelCoordinates * 2.0f - new Vector2(1.0f, 1.0f);

                var color = PixelShader(pixelCoordinates, rayGenerator, scene);
                color = Vector4.Clamp(color, Vector4.Zero, new Vector4(1.0f));

                _imageWriter.StorePixel(image, j, i, color);
            }
        });
    }

    public void CommitImage(TImage image, TParameter parameter)
    {
        _imageWriter.CommitImage(image, parameter);
    }

    private static Vector4 PixelShader(Vector2 pixelCoordinates, RayGenerator rayGenerator, Scene scene)
    {
        var ray = rayGenerator.GenerateRay(pixelCoordinates);
        return TraceRay(ray, scene);
    }

    private static Vector4 TraceRay(Ray ray, Scene scene)
    {
        if (scene.Spheres.Count == 0)
        {
            return new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        }

        Sphere? intersectSphere = null;
        var minimumHitDistance = float.MaxValue;

        for (var i = 0; i < scene.Spheres.Count; i++)
        {
            var sphere = scene.Spheres[i];

            var currentRay = ray with { Origin = ray.Origin - sphere.Position };

            // Construct quadratic function components
            var a = Vector3.Dot(currentRay.Direction, currentRay.Direction);
            var b = 2.0f * Vector3.Dot(currentRay.Origin, currentRay.Direction);
            var c = Vector3.Dot(currentRay.Origin, currentRay.Origin) - sphere.Radius * sphere.Radius;

            // Solve quadratic function
            var discriminant = b * b - 4.0f * a * c;

            if (discriminant < 0.0f)
            {
                continue;
            }

            var t = (-b + -MathF.Sqrt(discriminant)) / (2.0f * a);

            if (t < minimumHitDistance)
            {
                intersectSphere = sphere;
                minimumHitDistance = t;
            }
        }

        if (intersectSphere is null)
        {
            return new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        }
            
        var lightDirection = new Vector3(1.0f, -1.0f, 1.0f);

        // Compute normal
        ray = ray with { Origin = ray.Origin - intersectSphere.Value.Position };
        var intersectPoint = ray.GetPoint(minimumHitDistance);
        var normal = Vector3.Normalize(intersectPoint);

        // Remap the normal to color space
        //return new Vector4(0.5f * (normal + new Vector3(1, 1, 1)), 1.0f);

        // Compute light
        var light = MathF.Max(Vector3.Dot(normal, -lightDirection), 0.0f);
        return new Vector4(light * intersectSphere.Value.Albedo, 1.0f);
    }
}