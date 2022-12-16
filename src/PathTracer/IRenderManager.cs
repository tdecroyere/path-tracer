namespace PathTracer;

public interface IRenderManager
{
    TextureImage CurrentTextureImage { get; }
    bool IsFileRenderingActive { get; }
    DateTime LastRenderTime { get; }
    long RenderDuration { get; }

    void CreateRenderTextures(GraphicsDevice graphicsDevice, int width, int height);
    void RenderScene(CommandList commandList, Camera camera);
    void RenderToImage(RenderSettings renderSettings, Camera camera);
    void CheckRenderToImageErrors();
}
