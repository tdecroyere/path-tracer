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
var outputData = new Vector4[outputWidth * outputHeight];

var camera = new Camera
{
    AspectRatio = aspectRatio,
    CameraTarget = new Vector3(0, 0.5f, 1.0f)
};

var rayGenerator = new RayGenerator(camera);

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

        var color = PixelShader(pixelCoordinates, rayGenerator);
        color = Vector4.Clamp(color, Vector4.Zero, new Vector4(1.0f));

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

static Vector4 PixelShader(Vector2 pixelCoordinates, RayGenerator rayGenerator)
{
    var lightDirection = new Vector3(1.0f, -1.0f, 1.0f);
    var radius = 0.5f;

    var ray = rayGenerator.GenerateRay(pixelCoordinates);

    // Construct quadratic function components
    var a = Vector3.Dot(ray.Direction, ray.Direction);
    var b = 2.0f * Vector3.Dot(ray.Origin, ray.Direction);
    var c = Vector3.Dot(ray.Origin, ray.Origin) - radius * radius;

    // Solve quadratic function
    var discriminant = b * b - 4.0f * a * c;

    if (discriminant < 0.0f)
    {
        return Vector4.Zero;
    }

    var t = (-b + -MathF.Sqrt(discriminant)) / (2.0f * a);

    if (t < 0.0f)
    {
        return Vector4.Zero;
    }

    // Compute normal
    var intersectPoint = ray.GetPoint(t);
    var normal = Vector3.Normalize(intersectPoint);

    // Remap the normal to color space
    //return 0.5f * (normal + new Vector3(1, 1, 1));

    // Compute light
    var light = MathF.Max(Vector3.Dot(normal, -lightDirection), 0.0f);
    return new Vector4(light * new Vector3(1, 1, 0), 1.0f);
}