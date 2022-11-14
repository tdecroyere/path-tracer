namespace PathTracer.Core;

public interface IRenderer<TImage> where TImage : IImage
{
    // TODO: Do we need ValueTask here?
    // TODO: Implement cancellation mechanism
    Task RenderAsync(TImage image, Camera camera);
}