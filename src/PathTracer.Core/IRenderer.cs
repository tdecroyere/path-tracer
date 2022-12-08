namespace PathTracer.Core;

public interface IRenderer<TImage> where TImage : IImage
{
    void Render(TImage image, Camera camera);
    void CommitImage(TImage image);
}