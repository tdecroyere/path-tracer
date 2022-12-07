using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using PathTracer.Platform.GraphicsLegacy;

namespace PathTracer;

public unsafe class ImGuiRenderer : BaseRenderer, IDisposable
{
    private readonly Shader _shader;
    private readonly ResourceLayout _mainLayout;
    private readonly ResourceLayout _textureLayout;
    private readonly PipelineState _pipelineState;
    private readonly GraphicsBuffer _projectionMatrixBuffer;
    private readonly Texture _fontTexture;

    private readonly ResourceSet _mainResourceSet;
    private readonly ResourceSet _fontTextureResourceSet;

    private readonly nint _fontAtlasID;
    private readonly uint _vertexSizeInBytes;

    private GraphicsBuffer _vertexBuffer;
    private GraphicsBuffer _indexBuffer;
    
    public ImGuiRenderer(IGraphicsService graphicsService, GraphicsDevice graphicsDevice, string? fontName) : base(graphicsService, graphicsDevice)
    {
        _shader = LoadShader("imgui");

        _fontAtlasID = 1;
        _vertexSizeInBytes = (uint)Unsafe.SizeOf<ImDrawVert>();
        //_textureResourceSets = new List<ResourceSet>();

        _vertexBuffer = GraphicsService.CreateBuffer(GraphicsDevice, 10000, GraphicsBufferUsage.VertexBuffer | GraphicsBufferUsage.Dynamic);
        _indexBuffer = GraphicsService.CreateBuffer(GraphicsDevice, 2000, GraphicsBufferUsage.IndexBuffer | GraphicsBufferUsage.Dynamic);
        _projectionMatrixBuffer = GraphicsService.CreateBuffer(GraphicsDevice, 64, GraphicsBufferUsage.UniformBuffer | GraphicsBufferUsage.Dynamic);

        _mainLayout = GraphicsService.CreateResourceLayout(GraphicsDevice, new ResourceLayoutElement[]
        {
            new ResourceLayoutElement() { Name = "ProjectionMatrixBuffer", ResourceKind = ResourceLayoutKind.UniformBuffer, ShaderStages = ResourceLayoutShaderStages.Vertex },
            new ResourceLayoutElement() { Name = "MainSampler", ResourceKind = ResourceLayoutKind.Sampler, ShaderStages = ResourceLayoutShaderStages.Fragment }
        });
        
        _textureLayout = GraphicsService.CreateResourceLayout(GraphicsDevice, new ResourceLayoutElement[]
        {
            new ResourceLayoutElement() { Name = "MainTexture", ResourceKind = ResourceLayoutKind.TextureReadOnly, ShaderStages = ResourceLayoutShaderStages.Fragment },
        });

        _pipelineState = GraphicsService.CreatePipelineState(GraphicsDevice, _shader, new ResourceLayout[] { _mainLayout, _textureLayout });
        _mainResourceSet = GraphicsService.CreateResourceSet(_mainLayout, _projectionMatrixBuffer);

        var io = ImGui.GetIO();

        if (fontName is not null)
        {
            LoadFont(io, fontName);
        }

        var fontAtlas = io.Fonts;
        _fontTexture = RecreateFontDeviceTexture(ref fontAtlas);
        _fontTextureResourceSet = GraphicsService.CreateResourceSet(_textureLayout, _fontTexture);
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
            // TODO
        }
    }

    /*
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
   }*/

    public void RenderImDrawData(CommandList commandList, ref ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0)
        {
            return;
        }

        CopyGpuData(commandList, ref drawData);

        GraphicsService.SetVertexBuffer(commandList, _vertexBuffer);
        GraphicsService.SetIndexBuffer(commandList, _indexBuffer);
        GraphicsService.SetPipelineState(commandList, _pipelineState);
        GraphicsService.SetResourceSet(commandList, 0, _mainResourceSet);

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

        var texture = GraphicsService.CreateTexture(GraphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);
        GraphicsService.UpdateTexture(texture, new ReadOnlySpan<byte>(pixels.ToPointer(), width * height * bytesPerPixel));

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
                        GraphicsService.SetResourceSet(commandList, 1, _fontTextureResourceSet);
                    }
                    else
                    {
                        //commandList.SetGraphicsResourceSet(1, _textureResourceSets[(int)drawCommand.TextureId - 2]);
                    }
                }

                var clipRectX = (int)drawCommand.ClipRect.X;
                var clipRectY = (int)drawCommand.ClipRect.Y;
                var clipRectWidth = (int)drawCommand.ClipRect.Z - clipRectX;
                var clipRectHeight = (int)drawCommand.ClipRect.W - clipRectY;

                GraphicsService.SetScissorRect(commandList, clipRectX, clipRectY, clipRectWidth, clipRectHeight);
                GraphicsService.DrawIndexed(commandList, drawCommand.ElemCount, 1, drawCommand.IdxOffset + (uint)indexBufferOffset, (int)drawCommand.VtxOffset + vertexBufferOffset, 0);
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

            var vertexBufferData = new ReadOnlySpan<byte>(drawDataCommandList.VtxBuffer.Data.ToPointer(), (int)(drawDataCommandList.VtxBuffer.Size * _vertexSizeInBytes));
            GraphicsService.UpdateBuffer(commandList, _vertexBuffer, vertexOffsetInVertices * _vertexSizeInBytes, vertexBufferData);

            var indexBufferData = new ReadOnlySpan<byte>(drawDataCommandList.IdxBuffer.Data.ToPointer(), drawDataCommandList.IdxBuffer.Size * sizeof(ushort));
            GraphicsService.UpdateBuffer(commandList, _indexBuffer, indexOffsetInElements * sizeof(ushort), indexBufferData);

            vertexOffsetInVertices += (uint)drawDataCommandList.VtxBuffer.Size;
            indexOffsetInElements += (uint)drawDataCommandList.IdxBuffer.Size;
        }

        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(left: 0.0f, right: drawData.DisplaySize.X, bottom: drawData.DisplaySize.Y, top: 0.0f, zNearPlane: -1.0f, zFarPlane: 1.0f);
        GraphicsService.UpdateBuffer(commandList, _projectionMatrixBuffer, 0, MemoryMarshal.CreateReadOnlySpan(ref projectionMatrix, 1));
    }

    private static void LoadFont(ImGuiIOPtr io, string fontName)
    {
        var fontData = GetEmbeddedResourceBytes($"{fontName}.ttf");

        // create the object on the native side
        /*var nativeConfig = ImGuiNative.ImFontConfig_ImFontConfig();
        (*nativeConfig).OversampleH = 4;
        (*nativeConfig).OversampleV = 4;

        var handle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
        io.Fonts.AddFontFromMemoryTTF(handle.AddrOfPinnedObject(), fontData.Length, 13, nativeConfig);
        handle.Free();*/
    }
}