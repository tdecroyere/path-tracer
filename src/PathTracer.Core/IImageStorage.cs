namespace PathTracer.Core;

public interface IImageStorage
{
    Task WriteDataAsync(string key, ReadOnlyMemory<byte> data);
}