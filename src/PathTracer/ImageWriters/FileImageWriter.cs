using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace PathTracer.ImageWriters;

public class FileImageWriter : IImageWriter<FileImage, string>
{
    public FileImageWriter()
    {
    }

    public void StorePixel(FileImage image, int x, int y, Vector4 pixel)
    {
        // TODO: Move the logic to accumulation in the renderer
        var pixelRowIndex = (image.Height - 1 - y) * image.Width;
        
        if (image.FrameCount == 1)
        {
            image.AccumulationData.Span[pixelRowIndex + x] = Vector4.Zero;
        }

        image.AccumulationData.Span[pixelRowIndex + x] += pixel;

        var accumulatedColor = image.AccumulationData.Span[pixelRowIndex + x];
        pixel = accumulatedColor / image.FrameCount;
        
        image.ImageData.Span[pixelRowIndex + x] = pixel;
    }

    public void CommitImage(FileImage image, string outputPath)
    {
        var outputImage = new Image<Rgb24>(image.Width, image.Height);

        for (var i = 0; i < image.Height; i++)
        {
            for (var j = 0; j < image.Width; j++)
            {
                var pixel = image.ImageData.Span[i * image.Width + j];
                pixel = Vector4.Clamp(pixel * 255.0f, Vector4.Zero, new Vector4(255.0f));
            
                outputImage[j, i] = new Rgb24((byte)pixel.X, (byte)pixel.Y, (byte)pixel.Z);
            }
        }

        using var fileStream = new FileStream(outputPath, FileMode.Create);
        var encoder = new PngEncoder();
        encoder.Encode(outputImage, fileStream); 
    }
}