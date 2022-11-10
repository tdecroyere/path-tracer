namespace PathTracer.Core;

public interface IImageWriter<T> where T : IImage
{
    void StorePixel(T image, int x, int y, Vector4 pixel);
    void CommitImage(T image);
}