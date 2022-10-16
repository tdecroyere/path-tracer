using System.Buffers.Text;
using System.Text;

namespace PathTracer.Core;

public ref struct SpanReader
{
    private ReadOnlySpan<byte> buffer;

    public SpanReader(ReadOnlySpan<byte> buffer)
    {
        this.buffer = buffer;
    }

    public string ReadString(char delimiter)
    {
        var part = ReadSpanData(delimiter);

        var encoding = UTF8Encoding.UTF8;
        return encoding.GetString(part);
    }

    public int ReadInt(char delimiter)
    {
        var part = ReadSpanData(delimiter);
        var result = Utf8Parser.TryParse(part, out int outputValue, out var _);

        if (!result)
        {
            throw new InvalidDataException();
        }

        return outputValue;
    }

    public float ReadFloat(char delimiter)
    {
        var part = ReadSpanData(delimiter);
        var result = Utf8Parser.TryParse(part, out float outputValue, out var _);

        if (!result)
        {
            throw new InvalidDataException();
        }

        return outputValue;
    }
    
    private ReadOnlySpan<byte> ReadSpanData(char delimiter)
    {
        var endOfLine = Convert.ToByte(delimiter);
        var end = this.buffer.IndexOf(endOfLine);
        var bytesRead = end + (delimiter == '\r' ? 2 : 1);

        if (end == -1)
        {
            end = this.buffer.Length;
            bytesRead = end;
        }

        var part = this.buffer.Slice(0, end);
        this.buffer = this.buffer.Slice(bytesRead, this.buffer.Length - bytesRead);

        return part;
    }
}