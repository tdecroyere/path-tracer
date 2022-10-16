using System.Buffers;
using System.Text;

namespace PathTracer.Core;

public class PpmImageWriter : IImageWriter
{
    private readonly IImageStorage storage;

    public PpmImageWriter(IImageStorage storage)
    {
        this.storage = storage;
    }

    public async Task WriteImageAsync(string key, int width, int height, ReadOnlyMemory<Vector3> data)
    {
        if (width == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }
        
        if (height == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        if (data.Length != width * height)
        {
            throw new ArgumentOutOfRangeException(nameof(data));
        }

        var outputData = ComputeData(width, height, data.Span);
        await this.storage.WriteDataAsync(key, outputData);
    }

    private static ReadOnlyMemory<byte> ComputeData(int width, int height, ReadOnlySpan<Vector3> data)
    {
        var writer = new SpanWriter(width * height * 7 * sizeof(char));

        writer.WriteLine("P3");
        writer.WriteLine($"{width} {height}");
        writer.WriteLine("255");
                  
        var tempBuffer = (Span<char>)stackalloc char[11];

        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                var color = data[i * width + j];

                var red = (int)(color.X * 255);
                var green = (int)(color.Y * 255);
                var blue = (int)(color.Z * 255);
                
                tempBuffer.TryWrite(provider: null, $"{red} {green} {blue}", out var charsWritten);
                writer.WriteLine(tempBuffer.Slice(0, charsWritten));
            }
        }

        return writer.AsMemory();
    }
}