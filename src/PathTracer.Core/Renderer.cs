namespace PathTracer.Core;

public class Renderer<TImage, TParameter> : IRenderer<TImage, TParameter> where TImage : IImage
{
    private static readonly Random _random = new Random();
    private readonly IImageWriter<TImage, TParameter> _imageWriter;
    private readonly IRandomGenerator _randomGenerator;

    public Renderer(IImageWriter<TImage, TParameter> imageWriter, IRandomGenerator randomGenerator)
    {
        _imageWriter = imageWriter;
        _randomGenerator = randomGenerator;
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
            for (var j = 0; j < imageWidth; j++)
            {
                var u = (float)j / imageWidth;
                var v = (float)i / imageHeight;

                var pixelCoordinates = new Vector2(u, v);

                // Remap pixel coordinates to [-1, 1] range
                pixelCoordinates = pixelCoordinates * 2.0f - new Vector2(1.0f, 1.0f);

                var color = PixelShader(pixelCoordinates, _randomGenerator, rayGenerator, scene);
                _imageWriter.StorePixel(image, j, i, color);
            }
        });
        //}
    }

    public void CommitImage(TImage image, TParameter parameter)
    {
        _imageWriter.CommitImage(image, parameter);
    }

    private static Vector4 PixelShader(Vector2 pixelCoordinates, IRandomGenerator randomGenerator, RayGenerator rayGenerator, Scene scene)
    {
        var ray = rayGenerator.GenerateRay(pixelCoordinates);
        var color = Vector3.Zero;
        var multiplier = 1.0f;

        for (var i = 0; i < 5; i ++)
        {   
            var payload = TraceRay(scene, ray);

            if (payload.HitDistance < 0.0f)
            {
                var skyColor = new Vector3(0.6f, 0.7f, 0.9f);
                color += skyColor * multiplier;
                break;
            }

            // Remap the normal to color space
            //return new Vector4(0.5f * (normal + new Vector3(1, 1, 1)), 1.0f);

            // Compute light
            var lightDirection = new Vector3(1.0f, -1.0f, 1.0f);
            var light = MathF.Max(Vector3.Dot(payload.WorldNormal, -lightDirection), 0.0f);

            var sphere = scene.Spheres[payload.ObjectIndex];
            var material = scene.Materials[sphere.MaterialIndex];

            color += light * material.Albedo * multiplier;
            multiplier *= 0.4f;

            ray = ray with
            {
                Origin = payload.WorldPosition + payload.WorldNormal * 0.0001f,
                Direction = Vector3.Reflect(ray.Direction, payload.WorldNormal + material.Roughness * randomGenerator.GetVector3())
            };
        }

        return new Vector4(color, 1.0f); 
    }

    private static RayHitPayload TraceRay(Scene scene, Ray ray)
    {
        int intersectObjectIndex = -1;
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

            if (t > 0 && t < minimumHitDistance)
            {
                intersectObjectIndex = i;
                minimumHitDistance = t;
            }
        }

        if (intersectObjectIndex == -1)
        {
            return MissShader(ray);
        }

        return ClosestHitShader(scene, ray, minimumHitDistance, intersectObjectIndex);
    }

    private static RayHitPayload ClosestHitShader(Scene scene, Ray ray, float hitDistance, int objectIndex)
    {
        var sphere = scene.Spheres[objectIndex];

        ray = ray with { Origin = ray.Origin - sphere.Position };
        var intersectPoint = ray.GetPoint(hitDistance);
        var normal = Vector3.Normalize(intersectPoint);

        return new RayHitPayload
        {
            HitDistance = hitDistance,
            ObjectIndex = objectIndex,
            WorldPosition = intersectPoint + sphere.Position,
            WorldNormal = normal
        };
    }

    private static RayHitPayload MissShader(Ray ray)
    {
        return new RayHitPayload
        {
            HitDistance = -1.0f
        };
    }
}