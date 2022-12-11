using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace PathTracer.ImageWriters;

public class FileImageWriter : IImageWriter<FileImage>
{
    private const float _gammaCorrection = 1.0f / 2.2f;

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
                var pixel = image.ImageData.Span[i * image.Width + j];

                pixel = GammaCorrect(pixel);
                pixel = Vector4.Clamp(pixel * 255.0f, Vector4.Zero, new Vector4(255.0f));
            
                outputImage[j, i] = new Rgb24((byte)pixel.X, (byte)pixel.Y, (byte)pixel.Z);
            }
        }

        using var fileStream = new FileStream(image.OutputPath, FileMode.Create);
        var encoder = new PngEncoder();
        encoder.Encode(outputImage, fileStream); 
    }
    
    private static Vector4 GammaCorrect(Vector4 pixel)
    {
        return new Vector4(MathF.Pow(pixel.X, _gammaCorrection), MathF.Pow(pixel.Y, _gammaCorrection), MathF.Pow(pixel.Z, _gammaCorrection), pixel.W);
    }
}