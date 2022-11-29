namespace PathTracer.Platform.Graphics;

public interface IGraphicsService
{
    GraphicsDevice CreateGraphicsDevice(NativeWindow window);
}