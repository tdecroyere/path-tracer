using System.Diagnostics;
using System.Numerics;
using PathTracer.Core;

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Ray Trace Console");

// Setup Image
var aspectRatio = 16.0f / 9.0f;
var outputWidth = 800;
var outputHeight = (int)(outputWidth / aspectRatio);
var outputPath = "./TestData/Output.ppm";
var outputData = new Vector3[outputWidth * outputHeight];

// Setup Camera
var viewportHeight = 2.0f;
var viewportWidth = viewportHeight * aspectRatio;
var focalLength = 1.0f;

var cameraPosition = new Vector3();
var horizontal = new Vector3(viewportWidth, 0, 0);
var vertical = new Vector3(0, viewportHeight, 0);
var lowerLeftCorner = cameraPosition - (horizontal * 0.5f) - (vertical * 0.5f) - new Vector3(0, 0, focalLength);

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
        var u = (float)j / (outputWidth - 1); //TODO: What is the effect if we don't include the -1?
        var v = (float)i / (outputHeight - 1); //TODO: What is the effect if we don't include the -1?

        var ray = new Ray(cameraPosition, Vector3.Normalize(lowerLeftCorner + u * horizontal + v * vertical - cameraPosition));
        var color = RayColor(ray);

        // TODO: Do something better here, we need to revert the pixel in y coordinate so that the viewport Y point UP
        outputData[(outputHeight - 1 - i) * outputWidth + j] = color;
    }
}

stopwatch.Stop();

Console.ResetColor();
Console.WriteLine();
Console.WriteLine($"Render done in {stopwatch.Elapsed.TotalSeconds}s");
Console.WriteLine($"Writing file: {outputPath}");

// Save image to disk
var directory = Path.GetDirectoryName(outputPath);

if (!Directory.Exists(directory))
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
}