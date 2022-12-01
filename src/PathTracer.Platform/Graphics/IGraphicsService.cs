namespace PathTracer.Platform.Graphics;

public interface IGraphicsService
{
    GraphicsDevice CreateDevice(NativeWindow window);

    Shader CreateShader(GraphicsDevice graphicsDevice, ReadOnlySpan<byte> byteCode);
}