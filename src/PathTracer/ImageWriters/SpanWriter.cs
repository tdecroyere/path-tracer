using System.Buffers;
using System.Text;

namespace PathTracer.ImageWriters;

public class SpanWriter
{
    private readonly IBufferWriter<byte> _buffer;
    private readonly ArrayBufferWriter<byte>? _arrayBufferWriter;

    public SpanWriter()
    {
        _arrayBufferWriter = new ArrayBufferWriter<byte>();
        _buffer = _arrayBufferWriter;
    }

    public SpanWriter(int capacity)
    {
        _arrayBufferWriter = new ArrayBufferWriter<byte>(capacity);
        _buffer = _arrayBufferWriter;
    }

    public void WriteLine(ReadOnlySpan<char> value)
    {
        var encoding = UTF8Encoding.UTF8;

        var stringByteCount = encoding.GetByteCount(value) + 2;
        var span = this._buffer.GetSpan(stringByteCount);

        encoding.GetBytes(value, span);

        var endSlice = span[(stringByteCount - 2)..];

        endSlice[0] = (byte)'\r';
        endSlice[1] = (byte)'\n';

        _buffer.Advance(stringByteCount);
    }

    public ReadOnlySpan<byte> AsSpan()
    {
        if (_arrayBufferWriter is null)
        {
            throw new InvalidOperationException("Cannot retrieve a span when the writer was initialized with an IBufferWriter.");
        }

        return _arrayBufferWriter.WrittenSpan;
    }

    public ReadOnlyMemory<byte> AsMemory()
    {
        if (_arrayBufferWriter is null)
        {
            throw new InvalidOperationException("Cannot retrieve a span when the writer was initialized with an IBufferWriter.");
        }

        return _arrayBufferWriter.WrittenMemory;
    }
}