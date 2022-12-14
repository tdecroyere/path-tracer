namespace PathTracer;

public interface IUIManager
{
    void Init(NativeWindow window, GraphicsDevice graphicsDevice);
    void Resize(NativeWindowSize windowSize);
    Vector2 Update(float deltaTime, InputState inputState, TextureImage renderImage, RenderStatistics renderStatistics);
    void Render();
}
