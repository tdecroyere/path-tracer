namespace PathTracer.Console;

public interface IImageStorage
{
    Task WriteDataAsync(string key, ReadOnlyMemory<byte> data);
}