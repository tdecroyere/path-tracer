using System.Diagnostics;
using System.IO.Pipelines;
using System.Numerics;
using PathTracer.Core;

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Ray Trace Console");

// Setup Image
// TODO: For the renderer, we need to have an abstraction for the image
// so that we can have multiple render targets.
var aspectRatio = 16.0f / 9.0f;
var outputWidth = 800;
var outputHeight = (int)(outputWidth / aspectRatio);
var outputPath = "./TestData/Output.ppm";
var outputData = new Vector3[outputWidth * outputHeight];

// Rendering
var stopwatch = new Stopwatch();
stopwatch.Start();

for (var i = 0; i < outputHeight; i++)
{
    Console.CursorLeft = 0;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"Rendering: {MathF.Round((float)i / outputHeight * 100.0f)}%");

    for (var j = 0; j < outputWidth; j++)
    {
        var u = (float)j / outputWidth;
        var v = (float)i / outputHeight;

        var pixelCoordinates = new Vector2(u, v);
        
        // Remap pixel coordinates to [-1, 1] range
        pixelCoordinates = pixelCoordinates * 2.0f - new Vector2(1.0f, 1.0f);

        pixelCoordinates.X *= aspectRatio;

        var color = PixelShader(pixelCoordinates);
        color = Vector3.Clamp(color, Vector3.Zero, new Vector3(1.0f));

        // TODO: Do something better here, we need to revert the pixel in y coordinate so that the viewport Y point UP
        outputData[(outputHeight - 1 - i) * outputWidth + j] = color;
    }
}

stopwatch.Stop();

Console.ResetColor();
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Render done in {stopwatch.Elapsed.TotalSeconds}s");
Console.ResetColor();
Console.WriteLine($"Writing file: {outputPath}");

// Save image to disk
//var pipe = new Pipe();

var directory = Path.GetDirectoryName(outputPath);

if (directory != null && !Directory.Exists(directory))
{
    Directory.CreateDirectory(directory);
}

using var writer = new StreamWriter(outputPath);

writer.WriteLine("P3");
writer.WriteLine($"{outputWidth} {outputHeight}");
writer.WriteLine("255");

for (var i = 0; i < outputHeight; i++)
{
    for (var j = 0; j < outputWidth; j++)
    {
        var color = outputData[i * outputWidth + j];

        var red = (int)(color.X * 255);
        var green = (int)(color.Y * 255);
        var blue = (int)(color.Z * 255);

        writer.WriteLine($"{red} {green} {blue}");
    }
}

// TODO: Change return type to Vector4
static Vector3 PixelShader(Vector2 pixelCoordinates)
{
    var cameraPosition = new Vector3(0.0f, 0.0f, -2.0f);
    var lightDirection = new Vector3(1.0f, -1.0f, 1.0f);

    var center = new Vector3(0.0f, 0.0f, 0.0f);
    var radius = 0.5f;

    var ray = new Ray
    {
        Origin = cameraPosition,
        Direction = new Vector3(pixelCoordinates.X, pixelCoordinates.Y, 1.0f)
    };

    // Construct quadratic function components
    var a = Vector3.Dot(ray.Direction, ray.Direction);
    var b = 2.0f * Vector3.Dot(ray.Origin, ray.Direction);
    var c = Vector3.Dot(ray.Origin, ray.Origin) - radius * radius;

    // Solve quadratic function
    var discriminant = b * b - 4.0f * a * c;

    if (discriminant < 0.0f)
    {
        return Vector3.Zero;
    }

    var t = (-b + -MathF.Sqrt(discriminant)) / (2.0f * a);

    if (t < 0.0f)
    {
        return Vector3.Zero;
    }

    // Compute normal
    var intersectPoint = ray.GetPoint(t);
    var normal = Vector3.Normalize(intersectPoint - center);

    // Remap the normal to color space
    //return 0.5f * (normal + new Vector3(1, 1, 1));

    // Compute light
    var light = MathF.Max(Vector3.Dot(normal, -lightDirection), 0);
    return light * new Vector3(1, 1, 0);
}

/*
static float HitSphere(Vector3 center, float radius, Ray ray)
{
    var oc = ray.Origin - center;

    var a = Vector3.Dot(ray.Direction, ray.Direction);
    var b = 2.0f * Vector3.Dot(oc, ray.Direction);
    var c = Vector3.Dot(oc, oc) - radius*radius;
    var discriminant = b*b - 4*a*c;

    if (discriminant < 0)
    {
        return -1.0f;
    }

    else
    {
        return (-b - MathF.Sqrt(discriminant) ) / (2.0f*a);
    }
}

static Vector3 RayColor(Ray ray)
{
    // TODO: Support left handed 
    var t = HitSphere(new Vector3(0, 0, -1), 0.5f, ray);

    if (t > 0.0f)
    {
        var normal = Vector3.Normalize(ray.GetPoint(t) - new Vector3(0, 0, -1));
        return 0.5f * (normal + new Vector3(1, 1, 1));
    }

    var color1 = new Vector3(1, 1, 1);
    var color2 = new Vector3(0.5f, 0.7f, 1);

    t = 0.5f * (ray.Direction.Y + 1.0f);

    return Vector3.Lerp(color1, color2, t);
}*/