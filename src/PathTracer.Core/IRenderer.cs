namespace PathTracer.Core;

public interface IRenderer<TImage> where TImage : IImage
{
    // TODO: Do we need ValueTask here?
    Task RenderAsync(TImage image, Camera camera);
}