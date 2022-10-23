namespace PathTracer.Core;

public interface IImageWriter
{
    Task WriteImageAsync(string key, int width, int height, ReadOnlyMemory<Vector3> data);
}