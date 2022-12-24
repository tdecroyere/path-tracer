using System.Diagnostics;

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Ray Trace Console");

// TODO: Add parameters

var aspectRatio = 16.0f / 9.0f;
var outputWidth = 800;
var outputHeight = (int)(outputWidth / aspectRatio);
var outputPath = "./TestData/OutputConsole.png";

var camera = new Camera
{
    AspectRatio = aspectRatio
};

var outputImage = new FileImage
{
    Width = outputWidth,
    Height = outputHeight,
    ImageData = new Vector4[outputWidth * outputHeight]
};

var imageWriter = new FileImageWriter();
var renderer = new Renderer<FileImage, string>(imageWriter);

var scene = new Scene();

scene.Spheres.Add(new Sphere()
{
    Position = new Vector3(0.0f, 0.0f, 0.0f),
    Radius = 0.5f,
    Albedo = new Vector3(1.0f, 1.0f, 0.0f)
});

scene.Spheres.Add(new Sphere()
{
    Position = new Vector3(1.0f, 0.0f, 5.0f),
    Radius = 1.5f,
    Albedo = new Vector3(0.0f, 0.2f, 1.0f)
});

// Rendering
var stopwatch = new Stopwatch();
stopwatch.Start();

renderer.Render(outputImage, scene, camera);
renderer.CommitImage(outputImage, outputPath);

stopwatch.Stop();

Console.ResetColor();
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Render done in {stopwatch.Elapsed.TotalSeconds}s");
Console.ResetColor();
