namespace PathTracer.ImageWriters;

public class TextureImageWriter : IImageWriter<TextureImage, CommandList>
{
    private readonly IGraphicsService _graphicsService;
    private const float _gammaCorrection = 1.0f / 2.2f;

    public TextureImageWriter(IGraphicsService graphicsService)
    {
        _graphicsService = graphicsService;
    }

    public void StorePixel(TextureImage image, int x, int y, Vector4 pixel)
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

        pixel = GammaCorrect(pixel);
        pixel *= 255.0f;
        pixel = Vector4.Clamp(pixel, Vector4.Zero, new Vector4(255.0f));

        image.ImageData.Span[pixelRowIndex + x] = (uint)pixel.W << 24 | (uint)pixel.Z << 16 | (uint)pixel.Y << 8 | (uint)pixel.X;
    }

    public void CommitImage(TextureImage image, CommandList commandList)
    {
        _graphicsService.UpdateTexture<uint>(image.CpuTexture, image.ImageData.Span);
        _graphicsService.CopyTexture(commandList, image.CpuTexture, image.GpuTexture);
    }

    // TODO: Reset Frame Count

    private static Vector4 GammaCorrect(Vector4 pixel)
    {
        // TODO: Performance issue here
        // We need to to the pow with SIMD
        // TODO: Move that to MathUtils
        return new Vector4(MathF.Pow(pixel.X, _gammaCorrection), MathF.Pow(pixel.Y, _gammaCorrection), MathF.Pow(pixel.Z, _gammaCorrection), pixel.W);
    }
}