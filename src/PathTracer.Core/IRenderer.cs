namespace PathTracer.Core;

public interface IRenderer<TImage, TParameter> where TImage : IImage
{
    void Render(TImage image, Scene scene, Camera camera);
    void CommitImage(TImage image, TParameter parameter);
}