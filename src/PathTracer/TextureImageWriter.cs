using System.Runtime.InteropServices;
using PathTracer.Platform.GraphicsLegacy;

namespace PathTracer;

public class TextureImageWriter : IImageWriter<TextureImage>
{
    private readonly IGraphicsService _graphicsService;

    public TextureImageWriter(IGraphicsService graphicsService)
    {
        _graphicsService = graphicsService;
    }

    public void StorePixel(TextureImage image, int x, int y, Vector4 pixel)
    {
        var pixelRowIndex = (image.Height - 1 - y) * image.Width;

        // TODO: Implement Gamma Correction
        pixel *= 255.0f;

        image.ImageData.Span[pixelRowIndex + x] = (uint)pixel.W << 24 | (uint)pixel.Z << 16 | (uint)pixel.Y << 8 | (uint)pixel.X;
    }

    public void CommitImage(TextureImage image)
    {
        _graphicsService.UpdateTexture<uint>(image.CpuTexture, image.ImageData.Span);
        _graphicsService.CopyTexture(image.CommandList, image.CpuTexture, image.GpuTexture);
    }
}