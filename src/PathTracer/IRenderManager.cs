namespace PathTracer;

public interface IRenderManager
{
    TextureImage CurrentTextureImage { get; }
    int FileRenderingProgression { get; }
    DateTime LastRenderTime { get; }
    long RenderDuration { get; }

    void CreateRenderTextures(GraphicsDevice graphicsDevice, int width, int height);
    void RenderScene(CommandList commandList, Scene scene, Camera camera);
    void RenderToImage(RenderSettings renderSettings, Scene scene, Camera camera);
    void CheckRenderToImageErrors();
}
