using System.Buffers;
using System.Text;

namespace PathTracer.ImageWriters;

public class PpmImageWriter : IImageWriter<FileImage>
{
    public PpmImageWriter()
    {
    }

    public void StorePixel(FileImage image, int x, int y, Vector4 pixel)
    {
        var pixelRowIndex = (image.Height - 1 - y) * image.Width;
        image.ImageData.Span[pixelRowIndex + x] = pixel;
    }

    public void CommitImage(FileImage image)
    {
        var writer = new SpanWriter(image.Width * image.Height * 7 * sizeof(char));

        writer.WriteLine("P3");
        writer.WriteLine($"{image.Width} {image.Height}");
        writer.WriteLine("255");
                  
        var tempBuffer = (Span<char>)stackalloc char[11];

        for (var i = 0; i < image.Height; i++)
        {
            for (var j = 0; j < image.Width; j++)
            {
                var color = image.ImageData.Span[i * image.Width + j];

                var red = (int)(color.X * 255);
                var green = (int)(color.Y * 255);
                var blue = (int)(color.Z * 255);
                
                tempBuffer.TryWrite(provider: null, $"{red} {green} {blue}", out var charsWritten);
                writer.WriteLine(tempBuffer[..charsWritten]);
            }
        }

        var outputData = writer.AsMemory();

        // TODO: Do something better here
        File.WriteAllBytes(image.OutputPath, outputData.ToArray());
    }
}