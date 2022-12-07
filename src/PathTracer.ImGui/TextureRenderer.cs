using System.Numerics;
using PathTracer.Platform.GraphicsLegacy;

namespace PathTracer;

public class TextureRenderer : BaseRendererOld, IDisposable
{
    private int _width;
    private int _height;

    private Texture _cpuTexture;
    private Texture _gpuTexture;

    public TextureRenderer(IGraphicsService graphicsService, GraphicsDevice graphicsDevice, int width, int height) : base(graphicsService, graphicsDevice)
    {
        _width = width;
        _height = height;

        CreateTextures(_width, _height, out _cpuTexture, out _gpuTexture);
    }

    public Texture Texture => _gpuTexture;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // TODO: dispose managed state (managed objects)
        }
    }

    public void Resize(int width, int height)
    {
        _width = width;
        _height = height;
        
        /*GraphicsDevice.DisposeWhenIdle(_cpuTexture);
        GraphicsDevice.DisposeWhenIdle(_gpuTexture);*/
        
        CreateTextures(_width, _height, out _cpuTexture, out _gpuTexture);
    }

    public void UpdateTexture<T>(CommandList commandList, ReadOnlySpan<T> textureData) where T : unmanaged
    {
        GraphicsService.UpdateTexture(_cpuTexture, textureData);
        GraphicsService.CopyTexture(commandList, _cpuTexture, _gpuTexture);
    }

    private void CreateTextures(int width, int height, out Texture cpuTexture, out Texture gpuTexture)
    {
        cpuTexture = GraphicsService.CreateTexture(GraphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Staging, TextureType.Texture2D);
        gpuTexture = GraphicsService.CreateTexture(GraphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);
    }
}

internal readonly record struct TextureRendererVextex
{
    public TextureRendererVextex() {}
    public required Vector2 Position { get; init; }
    public required Vector2 TextureCoordinates { get; init; }
    public uint Reserved { get; init; } = uint.MaxValue;
}