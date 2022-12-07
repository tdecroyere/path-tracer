using System.Numerics;
using PathTracer.Platform.GraphicsLegacy;

namespace PathTracer;

public class TextureRenderer : BaseRenderer, IDisposable
{
    private readonly Shader _shader;
    private readonly Shader _fragmentShader;
    private readonly GraphicsBuffer _projectionMatrixBuffer;
    private readonly ResourceLayout _layout;
    private readonly ResourceLayout _textureLayout;
    private readonly PipelineState _pipeline;
    private readonly GraphicsBuffer _surfaceVertexBuffer;
    private readonly GraphicsBuffer _surfaceIndexBuffer;
    private readonly ResourceSet _mainSurfaceResourceSet;

    private int _width;
    private int _height;

    private ResourceSet? _surfaceTextureResourceSet;
    private Texture _cpuTexture;
    private Texture _gpuTexture;

    public TextureRenderer(IGraphicsService graphicsService, GraphicsDevice graphicsDevice, int width, int height) : base(graphicsService, graphicsDevice)
    {
        _width = width;
        _height = height;

        _shader = LoadShader("imgui");
        
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(left: 0.0f, right: 1.0f, bottom: 1.0f, top: 0.0f, zNearPlane: -1.0f, zFarPlane: 1.0f);
        var indices = new ushort[] { 0, 1, 2, 2, 1, 3 };

        var vertices = new TextureRendererVextex[]
        {
            new() { Position = new Vector2(0.0f, 0.0f), TextureCoordinates = new Vector2(0.0f, 0.0f) },
            new() { Position = new Vector2(1.0f, 0.0f), TextureCoordinates = new Vector2(1.0f, 0.0f) },
            new() { Position = new Vector2(0.0f, 1.0f), TextureCoordinates = new Vector2(0.0f, 1.0f) },
            new() { Position = new Vector2(1.0f, 1.0f), TextureCoordinates = new Vector2(1.0f, 1.0f) }
        };

        _projectionMatrixBuffer = CreateBuffer(projectionMatrix, GraphicsBufferUsage.UniformBuffer);
        _surfaceVertexBuffer = CreateBuffer<TextureRendererVextex>(vertices, GraphicsBufferUsage.VertexBuffer);
        _surfaceIndexBuffer = CreateBuffer<ushort>(indices, GraphicsBufferUsage.IndexBuffer);

        _layout = GraphicsService.CreateResourceLayout(GraphicsDevice, new ResourceLayoutElement[]
        {
            new ResourceLayoutElement() { Name = "ProjectionMatrixBuffer", ResourceKind = ResourceLayoutKind.UniformBuffer, ShaderStages = ResourceLayoutShaderStages.Vertex },
            new ResourceLayoutElement() { Name = "MainSampler", ResourceKind = ResourceLayoutKind.Sampler, ShaderStages = ResourceLayoutShaderStages.Fragment }
        });
        
        _textureLayout = GraphicsService.CreateResourceLayout(GraphicsDevice, new ResourceLayoutElement[]
        {
            new ResourceLayoutElement() { Name = "MainTexture", ResourceKind = ResourceLayoutKind.TextureReadOnly, ShaderStages = ResourceLayoutShaderStages.Fragment },
        });
    
        _pipeline = GraphicsService.CreatePipelineState(GraphicsDevice, _shader, new ResourceLayout[] { _layout, _textureLayout });
        _mainSurfaceResourceSet = GraphicsService.CreateResourceSet(_layout, _projectionMatrixBuffer);

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
        
        _surfaceTextureResourceSet = null;

        /*GraphicsDevice.DisposeWhenIdle(_cpuTexture);
        GraphicsDevice.DisposeWhenIdle(_gpuTexture);*/
        
        CreateTextures(_width, _height, out _cpuTexture, out _gpuTexture);
    }

    public void UpdateTexture<T>(CommandList commandList, ReadOnlySpan<T> textureData) where T : unmanaged
    {
        GraphicsService.UpdateTexture(_cpuTexture, textureData);
        GraphicsService.CopyTexture(commandList, _cpuTexture, _gpuTexture);
    }

    public void RenderTexture(CommandList commandList)
    {
        /*_surfaceTextureResourceSet ??= GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _gpuTexture));

        commandList.SetVertexBuffer(0, _surfaceVertexBuffer);
        commandList.SetIndexBuffer(_surfaceIndexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(_pipeline);
        
        commandList.SetGraphicsResourceSet(0, _mainSurfaceResourceSet);
        commandList.SetGraphicsResourceSet(1, _surfaceTextureResourceSet);
        
        commandList.DrawIndexed(6, 1, 0, 0, 0);*/
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