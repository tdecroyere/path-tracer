namespace PathTracer.Core;

public interface IRenderer<TImage, TParameter> where TImage : IImage
{
    void Render(TImage image, Camera camera);
    void CommitImage(TImage image, TParameter parameter);
}