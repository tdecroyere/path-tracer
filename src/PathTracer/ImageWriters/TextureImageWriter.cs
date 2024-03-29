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
        var pixelRowIndex = (image.Height - 1 - y) * image.Width;

        pixel = GammaCorrect(pixel);
        pixel = Vector4.Clamp(pixel * 255.0f, Vector4.Zero, new Vector4(255.0f));

        image.ImageData.Span[pixelRowIndex + x] = (uint)pixel.W << 24 | (uint)pixel.Z << 16 | (uint)pixel.Y << 8 | (uint)pixel.X;
    }

    public void CommitImage(TextureImage image, CommandList commandList)
    {
        _graphicsService.UpdateTexture<uint>(image.CpuTexture, image.ImageData.Span);
        _graphicsService.CopyTexture(commandList, image.CpuTexture, image.GpuTexture);
    }

    private static Vector4 GammaCorrect(Vector4 pixel)
    {
        // TODO: Performance issue here
        // We need to to the pow with SIMD
        // TODO: Move that to MathUtils
        return new Vector4(MathF.Pow(pixel.X, _gammaCorrection), MathF.Pow(pixel.Y, _gammaCorrection), MathF.Pow(pixel.Z, _gammaCorrection), pixel.W);
    }
}