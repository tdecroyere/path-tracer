using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace PathTracer.ImageWriters;

public class FileImageWriter : IImageWriter<FileImage>
{
    public FileImageWriter()
    {
    }

    public void StorePixel(FileImage image, int x, int y, Vector4 pixel)
    {
        var pixelRowIndex = (image.Height - 1 - y) * image.Width;
        image.ImageData.Span[pixelRowIndex + x] = pixel;
    }

    public void CommitImage(FileImage image)
    {
        var outputImage = new Image<Rgb24>(image.Width, image.Height);

        for (var i = 0; i < image.Height; i++)
        {
            for (var j = 0; j < image.Width; j++)
            {
                var color = image.ImageData.Span[i * image.Width + j];

                var red = (byte)(color.X * 255);
                var green = (byte)(color.Y * 255);
                var blue = (byte)(color.Z * 255);
              
                outputImage[j, i] = new Rgb24(red, green, blue);
            }
        }

        using var fileStream = new FileStream(image.OutputPath, FileMode.Create);
        var encoder = new PngEncoder();
        encoder.Encode(outputImage, fileStream); 
    }
}