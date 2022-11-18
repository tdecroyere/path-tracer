namespace PathTracer.Core;

public interface IImageWriter<T> where T : IImage
{
    void StorePixel(T image, int x, int y, Vector4 pixel);

    // TODO: Change this method to an async method
    void CommitImage(T image);
}