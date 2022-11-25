using System.Numerics;
using Veldrid;

namespace PathTracer
{
    public class TextureRenderer : BaseRenderer, IDisposable
    {
        private readonly Shader _vertexShader;
        private readonly Shader _fragmentShader;
        private readonly DeviceBuffer _projectionMatrixBuffer;
        private readonly ResourceLayout _layout;
        private readonly ResourceLayout _textureLayout;
        private readonly Pipeline _pipeline;
        private readonly DeviceBuffer _surfaceVertexBuffer;
        private readonly DeviceBuffer _surfaceIndexBuffer;
        private readonly ResourceSet _mainSurfaceResourceSet;

        private int _width;
        private int _height;

        private ResourceSet? _surfaceTextureResourceSet;
        private Texture _cpuTexture;
        private Texture _gpuTexture;
        private TextureView _textureView;

        public TextureRenderer(GraphicsDevice graphicsDevice, OutputDescription outputDescription, int width, int height) : base(graphicsDevice)
        {
            _width = width;
            _height = height;

            _vertexShader = LoadShader("imgui-vertex", ShaderStages.Vertex);
            _fragmentShader = LoadShader("imgui-frag", ShaderStages.Fragment);
            
            var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(left: 0.0f, right: 1.0f, bottom: 1.0f, top: 0.0f, zNearPlane: -1.0f, zFarPlane: 1.0f);
            var indices = new ushort[] { 0, 1, 2, 2, 1, 3 };

            var vertices = new TextureRendererVextex[]
            {
                new() { Position = new Vector2(0.0f, 0.0f), TextureCoordinates = new Vector2(0.0f, 0.0f) },
                new() { Position = new Vector2(1.0f, 0.0f), TextureCoordinates = new Vector2(1.0f, 0.0f) },
                new() { Position = new Vector2(0.0f, 1.0f), TextureCoordinates = new Vector2(0.0f, 1.0f) },
                new() { Position = new Vector2(1.0f, 1.0f), TextureCoordinates = new Vector2(1.0f, 1.0f) }
            };

            _projectionMatrixBuffer = CreateBuffer(projectionMatrix, BufferUsage.UniformBuffer);
            _surfaceVertexBuffer = CreateBuffer<TextureRendererVextex>(vertices, BufferUsage.VertexBuffer);
            _surfaceIndexBuffer = CreateBuffer<ushort>(indices, BufferUsage.IndexBuffer);
            
            var vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                    new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4_Norm))
            };

            _layout = GraphicsDevice.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            _textureLayout = GraphicsDevice.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

            var pipelineDescription = new GraphicsPipelineDescription(
                BlendStateDescription.SingleDisabled,
                new DepthStencilStateDescription(false, false, ComparisonKind.Always),
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, false, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(vertexLayouts, new[] { _vertexShader, _fragmentShader }),
                new ResourceLayout[] { _layout, _textureLayout },
                outputDescription,
                ResourceBindingModel.Default);

            _pipeline = GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(ref pipelineDescription);

            _mainSurfaceResourceSet = GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_layout, _projectionMatrixBuffer, GraphicsDevice.PointSampler));
            
            CreateTextures(_width, _height, out _cpuTexture, out _gpuTexture, out _textureView);
        }

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

            GraphicsDevice.DisposeWhenIdle(_cpuTexture);
            GraphicsDevice.DisposeWhenIdle(_gpuTexture);
            GraphicsDevice.DisposeWhenIdle(_textureView);
            
            CreateTextures(_width, _height, out _cpuTexture, out _gpuTexture, out _textureView);
        }

        public void RenderTexture<T>(CommandList commandList, ReadOnlySpan<T> textureData) where T : unmanaged
        {
            GraphicsDevice.UpdateTexture(_cpuTexture, textureData, 0, 0, 0, (uint)_width, (uint)_height, 1, 0, 0);
            commandList.CopyTexture(_cpuTexture, _gpuTexture);
            
            _surfaceTextureResourceSet ??= GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _gpuTexture));

            commandList.SetVertexBuffer(0, _surfaceVertexBuffer);
            commandList.SetIndexBuffer(_surfaceIndexBuffer, IndexFormat.UInt16);
            commandList.SetPipeline(_pipeline);
            
            commandList.SetGraphicsResourceSet(0, _mainSurfaceResourceSet);
            commandList.SetGraphicsResourceSet(1, _surfaceTextureResourceSet);
            
            commandList.DrawIndexed(6, 1, 0, 0, 0);
        }

        private void CreateTextures(int width, int height, out Texture cpuTexture, out Texture gpuTexture, out TextureView textureView)
        {
            cpuTexture = GraphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)width, (uint)height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging, TextureType.Texture2D));
            gpuTexture = GraphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)width, (uint)height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
            textureView = GraphicsDevice.ResourceFactory.CreateTextureView(gpuTexture);
        }
    }
    
    internal readonly record struct TextureRendererVextex
    {
        public TextureRendererVextex() {}
        public required Vector2 Position { get; init; }
        public required Vector2 TextureCoordinates { get; init; }
        public uint Reserved { get; init; } = uint.MaxValue;
    }
}