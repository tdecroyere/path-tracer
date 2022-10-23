using System.Buffers;
using System.Text;

namespace PathTracer.Core;

public class SpanWriter
{
    private IBufferWriter<byte> buffer;
    private ArrayBufferWriter<byte>? arrayBufferWriter;

    public SpanWriter()
    {
        this.arrayBufferWriter = new ArrayBufferWriter<byte>();
        this.buffer = arrayBufferWriter;
    }

    public SpanWriter(int capacity)
    {
        this.arrayBufferWriter = new ArrayBufferWriter<byte>(capacity);
        this.buffer = arrayBufferWriter;
    }

    public void WriteLine(ReadOnlySpan<char> value)
    {
        var encoding = UTF8Encoding.UTF8;

        var stringByteCount = encoding.GetByteCount(value) + 2;
        var span = this.buffer.GetSpan(stringByteCount);

        encoding.GetBytes(value, span);

        var endSlice = span.Slice(stringByteCount - 2);

        endSlice[0] = (byte)'\r';
        endSlice[1] = (byte)'\n';

        this.buffer.Advance(stringByteCount);
    }

    public ReadOnlySpan<byte> AsSpan()
    {
        if (this.arrayBufferWriter is null)
        {
            throw new InvalidOperationException("Cannot retrieve a span when the writer was initialized with an IBufferWriter.");
        }

        return this.arrayBufferWriter.WrittenSpan;
    }

    public ReadOnlyMemory<byte> AsMemory()
    {
        if (this.arrayBufferWriter is null)
        {
            throw new InvalidOperationException("Cannot retrieve a span when the writer was initialized with an IBufferWriter.");
        }

        return this.arrayBufferWriter.WrittenMemory;
    }
}