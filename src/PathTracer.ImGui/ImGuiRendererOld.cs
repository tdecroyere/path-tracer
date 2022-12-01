using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using Veldrid;

namespace PathTracer;

public unsafe class ImGuiRendererOld : BaseRendererOld, IDisposable
{
    private readonly DeviceBuffer _projMatrixBuffer;
    private readonly Texture _fontTexture;
    private readonly TextureView _fontTextureView;
    private readonly Shader _vertexShader;
    private readonly Shader _fragmentShader;
    private readonly ResourceLayout _layout;
    private readonly ResourceLayout _textureLayout;
    private readonly Pipeline _pipeline;
    private readonly ResourceSet _mainResourceSet;
    private readonly ResourceSet _fontTextureResourceSet;
    private readonly IList<ResourceSet> _textureResourceSets;

    private readonly nint _fontAtlasID = 1;
    private readonly uint _vertexSizeInBytes;

    private DeviceBuffer _vertexBuffer;
    private DeviceBuffer _indexBuffer;

    public ImGuiRendererOld(GraphicsDevice graphicsDevice, OutputDescription outputDescription, string? fontName) : base(graphicsDevice)
    {
        _fontAtlasID = 1;
        _vertexSizeInBytes = (uint)Unsafe.SizeOf<ImDrawVert>();
        _textureResourceSets = new List<ResourceSet>();

        ResourceFactory factory = GraphicsDevice.ResourceFactory;
        _vertexBuffer = factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        _vertexBuffer.Name = "ImGui Vertex Buffer";
        _indexBuffer = factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        _indexBuffer.Name = "ImGui Index Buffer";

        _projMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _projMatrixBuffer.Name = "ImGui Projection Buffer";

        _vertexShader = LoadShader("imgui-vertex", ShaderStages.Vertex);
        _fragmentShader = LoadShader("imgui-frag", ShaderStages.Fragment);

        VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
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

        _mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_layout, _projMatrixBuffer, GraphicsDevice.PointSampler));

        var io = ImGui.GetIO();

        if (fontName is not null)
        {
            LoadFont(io, fontName);
        }

        var fontAtlas = io.Fonts;
        _fontTexture = RecreateFontDeviceTexture(ref fontAtlas);
        _fontTextureView = GraphicsDevice.ResourceFactory.CreateTextureView(_fontTexture);
        _fontTextureResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _fontTextureView));
    }

    private static void LoadFont(ImGuiIOPtr io, string fontName)
    {
        var fontData = GetEmbeddedResourceBytes($"{fontName}.ttf");

        // create the object on the native side
        var nativeConfig = ImGuiNative.ImFontConfig_ImFontConfig();
        (*nativeConfig).OversampleH = 4;
        (*nativeConfig).OversampleV = 4;

        var handle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
        io.Fonts.AddFontFromMemoryTTF(handle.AddrOfPinnedObject(), fontData.Length, 13, nativeConfig);
        handle.Free();
    }

    public nint RegisterTexture(TextureView textureView)
    {
        var textureResourceSet = GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, textureView));
        _textureResourceSets.Add(textureResourceSet);
        return _textureResourceSets.Count + 1;
    }

    public void UpdateTexture(nint id, TextureView textureView)
    {
        var oldResourceSet = _textureResourceSets[(int)id - 2];
        GraphicsDevice.DisposeWhenIdle(oldResourceSet);

        var textureResourceSet = GraphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, textureView));
        _textureResourceSets[(int)id - 2] = textureResourceSet;
    }

    public void RenderImDrawData(CommandList commandList, ref ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0)
        {
            return;
        }

        CopyGpuData(commandList, ref drawData);

        commandList.SetVertexBuffer(0, _vertexBuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _mainResourceSet);

        drawData.ScaleClipRects(drawData.FramebufferScale);

        var vertexBufferOffset = 0;
        var indexBufferOffset = 0;

        for (var i = 0; i < drawData.CmdListsCount; i++)
        {
            var drawDataCommandList = drawData.CmdListsRange[i];
            RenderDrawDataCommandList(commandList, vertexBufferOffset, indexBufferOffset, ref drawDataCommandList);

            vertexBufferOffset += drawDataCommandList.VtxBuffer.Size;
            indexBufferOffset += drawDataCommandList.IdxBuffer.Size;
        }
    }

    private Texture RecreateFontDeviceTexture(ref ImFontAtlasPtr font)
    {
        font.GetTexDataAsRGBA32(out nint pixels, out var width, out var height, out var bytesPerPixel);
        font.SetTexID(_fontAtlasID);

        var texture = GraphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)width, (uint)height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
        texture.Name = "ImGui Font Texture";

        GraphicsDevice.UpdateTexture(
            texture,
            pixels,
            (uint)(bytesPerPixel * width * height),
            0,
            0,
            0,
            (uint)width,
            (uint)height,
            1,
            0,
            0);

        font.ClearTexData();

        return texture;
    }

    private void RenderDrawDataCommandList(CommandList commandList, int vertexBufferOffset, int indexBufferOffset, ref ImDrawListPtr drawDataCommandList)
    {
        for (var i = 0; i < drawDataCommandList.CmdBuffer.Size; i++)
        {
            var drawCommand = drawDataCommandList.CmdBuffer[i];

            if (drawCommand.UserCallback != nint.Zero)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (drawCommand.TextureId != nint.Zero)
                {
                    if (drawCommand.TextureId == _fontAtlasID)
                    {
                        commandList.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                    }
                    else
                    {
                        commandList.SetGraphicsResourceSet(1, _textureResourceSets[(int)drawCommand.TextureId - 2]);
                    }
                }

                var clipRectX = (uint)drawCommand.ClipRect.X;
                var clipRectY = (uint)drawCommand.ClipRect.Y;
                var clipRectWidth = (uint)drawCommand.ClipRect.Z - clipRectX;
                var clipRectHeight = (uint)drawCommand.ClipRect.W - clipRectY;

                commandList.SetScissorRect(0, clipRectX, clipRectY, clipRectWidth, clipRectHeight);
                commandList.DrawIndexed(drawCommand.ElemCount, 1, drawCommand.IdxOffset + (uint)indexBufferOffset, (int)drawCommand.VtxOffset + vertexBufferOffset, 0);
            }
        }
    }

    private void CopyGpuData(CommandList commandList, ref ImDrawDataPtr drawData)
    {
        var vertexOffsetInVertices = 0u;
        var indexOffsetInElements = 0u;

        var totalVBSize = (uint)(drawData.TotalVtxCount * _vertexSizeInBytes);
        _vertexBuffer = CheckSizeAndIncreaseBuffer(_vertexBuffer, totalVBSize);

        var totalIBSize = (uint)(drawData.TotalIdxCount * sizeof(ushort));
        _indexBuffer = CheckSizeAndIncreaseBuffer(_indexBuffer, totalIBSize);

        for (var i = 0; i < drawData.CmdListsCount; i++)
        {
            var drawDataCommandList = drawData.CmdListsRange[i];

            commandList.UpdateBuffer(
                _vertexBuffer,
                vertexOffsetInVertices * _vertexSizeInBytes,
                drawDataCommandList.VtxBuffer.Data,
                (uint)(drawDataCommandList.VtxBuffer.Size * _vertexSizeInBytes));

            commandList.UpdateBuffer(
                _indexBuffer,
                indexOffsetInElements * sizeof(ushort),
                drawDataCommandList.IdxBuffer.Data,
                (uint)(drawDataCommandList.IdxBuffer.Size * sizeof(ushort)));

            vertexOffsetInVertices += (uint)drawDataCommandList.VtxBuffer.Size;
            indexOffsetInElements += (uint)drawDataCommandList.IdxBuffer.Size;
        }

        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(left: 0.0f, right: drawData.DisplaySize.X, bottom: drawData.DisplaySize.Y, top: 0.0f, zNearPlane: -1.0f, zFarPlane: 1.0f);
        GraphicsDevice.UpdateBuffer(_projMatrixBuffer, 0, ref projectionMatrix);
    }

    /// <summary>
    /// Frees all graphics resources used by the renderer.
    /// </summary>
    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _projMatrixBuffer.Dispose();
        _fontTexture.Dispose();
        _fontTextureView.Dispose();
        _vertexShader.Dispose();
        _fragmentShader.Dispose();
        _layout.Dispose();
        _textureLayout.Dispose();
        _pipeline.Dispose();
        _mainResourceSet.Dispose();
    }
}