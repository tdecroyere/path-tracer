using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using ImGuiNET;
using PathTracer.Platform.Inputs;
using Veldrid;

namespace PathTracer
{
    public class TextureRenderer : IDisposable
    {
        private GraphicsDevice _gd;

        // Veldrid objects
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private ResourceLayout _layout;
        private ResourceLayout _textureLayout;
        private Pipeline _pipeline;

        private int _windowWidth;
        private int _windowHeight;

        // Texture Surface Draw
        private DeviceBuffer _surfaceVertexBuffer;
        private DeviceBuffer _surfaceIndexBuffer;
        private DeviceBuffer _surfaceProjMatrixBuffer;
        private ResourceSet _mainSurfaceResourceSet;
        private ResourceSet? _surfaceTextureResourceSet;
        private bool _surfaceInitialized;

        public TextureRenderer(GraphicsDevice gd, OutputDescription outputDescription, int width, int height, float uiScale)
        {
            _gd = gd;
            _windowWidth = width;
            _windowHeight = height;

            CreateDeviceResources(outputDescription);
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

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
            
            _surfaceTextureResourceSet = null;
        }

        private void CreateDeviceResources(OutputDescription outputDescription)
        {
            ResourceFactory factory = _gd.ResourceFactory;
        
            _surfaceProjMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            _surfaceProjMatrixBuffer.Name = "Surface Projection Buffer";

            byte[] vertexShaderBytes = LoadEmbeddedShaderCode(_gd.ResourceFactory, "imgui-vertex");
            byte[] fragmentShaderBytes = LoadEmbeddedShaderCode(_gd.ResourceFactory, "imgui-frag");
            _vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, _gd.BackendType == GraphicsBackend.Metal ? "VS" : "main"));
            _fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, _gd.BackendType == GraphicsBackend.Metal ? "FS" : "main"));

            var vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                    new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4_Norm))
            };

            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            _textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

            var pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                new DepthStencilStateDescription(false, false, ComparisonKind.Always),
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, false, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(vertexLayouts, new[] { _vertexShader, _fragmentShader }),
                new ResourceLayout[] { _layout, _textureLayout },
                outputDescription,
                ResourceBindingModel.Default);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            _mainSurfaceResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_layout, _surfaceProjMatrixBuffer, _gd.PointSampler));

            uint totalVBSize = (uint)(4 * Unsafe.SizeOf<ImDrawVert>());
            _surfaceVertexBuffer = _gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalVBSize), BufferUsage.VertexBuffer));

            uint totalIBSize = (uint)(6 * sizeof(ushort));
            _surfaceIndexBuffer = _gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalIBSize), BufferUsage.IndexBuffer));
        }

        private static byte[] LoadEmbeddedShaderCode(ResourceFactory factory, string name)
        {
            switch (factory.BackendType)
            {
                case GraphicsBackend.Vulkan:
                {
                    string resourceName = name + ".spv";
                    return GetEmbeddedResourceBytes(resourceName);
                }
                case GraphicsBackend.Metal:
                {
                    string resourceName = name + ".metallib";
                    return GetEmbeddedResourceBytes(resourceName);
                }
                default:
                    throw new NotImplementedException();
            }
        }

        private static byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            var assembly = typeof(ImGuiBackend).Assembly;
            using Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream is null)
            {
                return Array.Empty<byte>();
            }

            byte[] ret = new byte[resourceStream.Length];
            resourceStream.Read(ret, 0, (int)resourceStream.Length);
            return ret;
        }

        public void RenderTexture(GraphicsDevice graphicsDevice, CommandList commandList, TextureView texture)
        {
            // TODO
            if (!_surfaceInitialized)
            {
                var vertices = new ImDrawVert[]
                {
                    new() { pos = new Vector2(0.0f, 0.0f), uv = new Vector2(0.0f, 0.0f), col = uint.MaxValue },
                    new() { pos = new Vector2(0.0f, 1.0f), uv = new Vector2(0.0f, 1.0f), col = uint.MaxValue },
                    new() { pos = new Vector2(1.0f, 0.0f), uv = new Vector2(1.0f, 0.0f), col = uint.MaxValue },
                    new() { pos = new Vector2(1.0f, 1.0f), uv = new Vector2(1.0f, 1.0f), col = uint.MaxValue }
                };

                var indices = new ushort[]
                {
                    0, 1, 2, 2, 1, 3
                };

                commandList.UpdateBuffer(
                    _surfaceVertexBuffer,
                    0,
                    vertices);

                commandList.UpdateBuffer(
                    _surfaceIndexBuffer,
                    0,
                    indices);

                _surfaceInitialized = true;
            }

            _surfaceTextureResourceSet ??= graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, texture));

            Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                0f,
                1,
                1,
                0.0f,
                -1.0f,
                1.0f);

            _gd.UpdateBuffer(_surfaceProjMatrixBuffer, 0, ref mvp);

            commandList.SetVertexBuffer(0, _surfaceVertexBuffer);
            commandList.SetIndexBuffer(_surfaceIndexBuffer, IndexFormat.UInt16);
            commandList.SetPipeline(_pipeline);
            
            commandList.SetGraphicsResourceSet(0, _mainSurfaceResourceSet);
            commandList.SetGraphicsResourceSet(1, _surfaceTextureResourceSet);
            
            commandList.DrawIndexed(6, 1, 0, 0, 0);
        }
    }
}