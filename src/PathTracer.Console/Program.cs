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
var renderer = new Renderer<FileImage, string>(imageWriter, new RandomGenerator());

var scene = new Scene();

scene.Materials.Add(new Material()
{
    Albedo = new Vector3(1.0f, 1.0f, 0.0f),
    Roughness = 0.0f
});

scene.Materials.Add(new Material()
{
    Albedo = new Vector3(0.0f, 0.2f, 1.0f),
    Roughness = 0.1f
});

scene.Spheres.Add(new Sphere()
{
    Position = new Vector3(0.0f, 0.0f, 0.0f),
    Radius = 1.0f,
    MaterialIndex = 0 
});

scene.Spheres.Add(new Sphere()
{
    Position = new Vector3(0.0f, -101.0f, 0.0f),
    Radius = 100.0f,
    MaterialIndex = 1
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
