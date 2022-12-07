using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PathTracer.Platform.GraphicsLegacy;
using Veldrid;

namespace PathTracer.Platform.Platforms.VeldridLibrary;

public class VeldridGraphicsService : IGraphicsService
{
    private readonly INativeUIService _nativeUIService;
    private readonly IList<Veldrid.GraphicsDevice> _graphicsDevices;
    private readonly IList<VeldridCommandList> _commandLists;
    private readonly IList<VeldridShader> _shaders;
    private readonly IList<VeldridBuffer> _buffers;
    private readonly IList<VeldridTexture> _textures;
    private readonly IList<Veldrid.ResourceLayout> _layouts;
    private readonly IList<Veldrid.ResourceSet> _resourceSets;
    private readonly IList<VeldridPipeline> _pipelines;

    public VeldridGraphicsService(INativeUIService nativeUIService)
    {
        _nativeUIService = nativeUIService;
        _graphicsDevices = new List<Veldrid.GraphicsDevice>();
        _commandLists = new List<VeldridCommandList>();
        _shaders = new List<VeldridShader>();
        _buffers = new List<VeldridBuffer>();
        _textures = new List<VeldridTexture>();
        _layouts = new List<Veldrid.ResourceLayout>();
        _resourceSets = new List<Veldrid.ResourceSet>();
        _pipelines = new List<VeldridPipeline>();
    }

    public GraphicsLegacy.GraphicsDevice CreateDevice(NativeWindow window)
    {
        if (!(OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()))
        {
            throw new InvalidOperationException("Create Graphics device: Unsupported OS");
        }

        Veldrid.GraphicsDevice? graphicsDevice = null;

        var graphicsDeviceOptions = new GraphicsDeviceOptions(
                debug: true,
                swapchainDepthFormat: null,
                syncToVerticalBlank: true,
                ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true);

        var nativeWindowSystemHandle = _nativeUIService.GetWindowSystemHandle(window);
        var renderSize = _nativeUIService.GetWindowRenderSize(window);

        if (OperatingSystem.IsWindows())
        {
            var swapchainSource = SwapchainSource.CreateWin32(nativeWindowSystemHandle, 0);

            var swapchainDescription = new SwapchainDescription(
                            swapchainSource,
                            (uint)renderSize.Width,
                            (uint)renderSize.Height,
                            graphicsDeviceOptions.SwapchainDepthFormat,
                            graphicsDeviceOptions.SyncToVerticalBlank,
                            graphicsDeviceOptions.SwapchainSrgbFormat);

            graphicsDevice = Veldrid.GraphicsDevice.CreateVulkan(graphicsDeviceOptions, swapchainDescription);
        }

        else if (OperatingSystem.IsMacOS())
        {
            var swapchainSource = SwapchainSource.CreateNSWindow(nativeWindowSystemHandle);

            var swapchainDescription = new SwapchainDescription(
                            swapchainSource,
                            (uint)renderSize.Width,
                            (uint)renderSize.Height,
                            graphicsDeviceOptions.SwapchainDepthFormat,
                            graphicsDeviceOptions.SyncToVerticalBlank,
                            graphicsDeviceOptions.SwapchainSrgbFormat);

            graphicsDevice = Veldrid.GraphicsDevice.CreateMetal(graphicsDeviceOptions, swapchainDescription);
            graphicsDevice.MainSwapchain.Resize((uint)renderSize.Width, (uint)renderSize.Height);
        }

        if (graphicsDevice == null)
        {
            throw new InvalidOperationException("Error while creating graphics device.");
        }

        _graphicsDevices.Add(graphicsDevice);
        return _graphicsDevices.Count;
    }

    public void ResizeSwapChain(GraphicsLegacy.GraphicsDevice graphicsDevice, int width, int height)
    {
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];
        veldridGraphicsDevice.MainSwapchain.Resize((uint)width, (uint)height);
    }

    public void PresentSwapChain(GraphicsLegacy.GraphicsDevice graphicsDevice)
    {
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];

    
    
        veldridGraphicsDevice.SwapBuffers(veldridGraphicsDevice.MainSwapchain);
    }

    public GraphicsLegacy.CommandList CreateCommandList(GraphicsLegacy.GraphicsDevice graphicsDevice)
    {
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];

        _commandLists.Add(new VeldridCommandList()
        {
            CommandList = veldridGraphicsDevice.ResourceFactory.CreateCommandList(),
            GraphicsDevice = veldridGraphicsDevice
        });

        return _commandLists.Count;
    }

    public void ResetCommandList(GraphicsLegacy.CommandList commandList)
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        veldridCommandList.CommandList.Begin();
        veldridCommandList.CommandList.SetFramebuffer(veldridCommandList.GraphicsDevice.MainSwapchain.Framebuffer);
    }

    public void SubmitCommandList(GraphicsLegacy.CommandList commandList)
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        veldridCommandList.CommandList.End();
        veldridCommandList.GraphicsDevice.SubmitCommands(veldridCommandList.CommandList);
    }

    public GraphicsBuffer CreateBuffer(GraphicsLegacy.GraphicsDevice graphicsDevice, nuint sizeInBytes, GraphicsBufferUsage usage)
    {
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];
        var buffer = veldridGraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeInBytes, (BufferUsage)(int)usage));

        _buffers.Add(new VeldridBuffer
        {
            Buffer = buffer,
            GraphicsDevice = veldridGraphicsDevice
        });
        
        return _buffers.Count;
    }

    public void DeleteBuffer(GraphicsBuffer buffer)
    {
        // TODO: For the moment we don't delete the struct in the list
        var veldridBuffer = _buffers[ToIndex(buffer)];
        veldridBuffer.GraphicsDevice.DisposeWhenIdle(veldridBuffer.Buffer);
    }

    public GraphicsBufferDescription GetBufferDescription(GraphicsBuffer buffer)
    {
        var veldridBuffer = _buffers[ToIndex(buffer)];

        return new GraphicsBufferDescription
        {
            SizeInBytes = veldridBuffer.Buffer.SizeInBytes,
            Usage = (GraphicsBufferUsage)(int)veldridBuffer.Buffer.Usage
        };
    }

    public void UpdateBuffer<T>(GraphicsBuffer buffer, nuint offset, ReadOnlySpan<T> data) where T : unmanaged
    {
        var veldridBuffer = _buffers[ToIndex(buffer)];
        veldridBuffer.GraphicsDevice.UpdateBuffer(veldridBuffer.Buffer, (uint)offset, data);
    }

    public GraphicsLegacy.Texture CreateTexture(GraphicsLegacy.GraphicsDevice graphicsDevice, int width, int height, int depth, int mipLevels, int arrayLayers, TextureFormat format, GraphicsLegacy.TextureUsage usage, GraphicsLegacy.TextureType type)
    {
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];

        var texture = veldridGraphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)width, (uint)height, (uint)depth, (uint)mipLevels, (uint)arrayLayers, (PixelFormat)(byte)format, (Veldrid.TextureUsage)(byte)usage, (Veldrid.TextureType)type));
        var textureView = veldridGraphicsDevice.ResourceFactory.CreateTextureView(texture);
        
        _textures.Add(new VeldridTexture
        {
            Texture = texture,
            TextureView = textureView,
            GraphicsDevice = veldridGraphicsDevice
        });

        return _textures.Count;
    }

    public void UpdateTexture<T>(GraphicsLegacy.Texture texture, ReadOnlySpan<T> data) where T : unmanaged
    {
        var veldridTexture = _textures[ToIndex(texture)];
        veldridTexture.GraphicsDevice.UpdateTexture(veldridTexture.Texture, data, 0, 0, 0, veldridTexture.Texture.Width, veldridTexture.Texture.Height, 1, 0, 0);
    }

    public GraphicsLegacy.Shader CreateShader(GraphicsLegacy.GraphicsDevice graphicsDevice, ReadOnlySpan<byte> byteCode)
    {
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];

        var vertexShaderSizeInBytes = MemoryMarshal.Read<int>(byteCode);
        byteCode = byteCode[sizeof(int)..];

        var fragmentShaderSizeInBytes = MemoryMarshal.Read<int>(byteCode);
        byteCode = byteCode[sizeof(int)..];

        var vertexShaderData = byteCode[0..vertexShaderSizeInBytes];
        byteCode = byteCode[vertexShaderSizeInBytes..];
        
        var fragmentShaderData = byteCode[0..fragmentShaderSizeInBytes];

        var vertexShader = veldridGraphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderData.ToArray(), veldridGraphicsDevice.BackendType == GraphicsBackend.Metal ? "VS" : "main"));
        var fragmentShader = veldridGraphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderData.ToArray(), veldridGraphicsDevice.BackendType == GraphicsBackend.Metal ? "FS" : "main"));

        var veldridShader = new VeldridShader
        {
            VertexShader = vertexShader,
            FragmentShader = fragmentShader
        };

        _shaders.Add(veldridShader);
        return _shaders.Count;
    }

    public GraphicsLegacy.ResourceLayout CreateResourceLayout(GraphicsLegacy.GraphicsDevice graphicsDevice, ReadOnlySpan<ResourceLayoutElement> elements)
    {
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];
        var layoutElements = new ResourceLayoutElementDescription[elements.Length];

        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            layoutElements[i] = new ResourceLayoutElementDescription(element.Name, (ResourceKind)(byte)element.ResourceKind, (ShaderStages)(byte)element.ShaderStages);
        }

        var description = new ResourceLayoutDescription(layoutElements);
        var layout = veldridGraphicsDevice.ResourceFactory.CreateResourceLayout(description);

        _layouts.Add(layout);
        return _layouts.Count;
    }

    public GraphicsLegacy.ResourceSet CreateResourceSet(GraphicsLegacy.ResourceLayout resourceLayout, GraphicsBuffer buffer)
    {
        var veldridBuffer = _buffers[ToIndex(buffer)];
        var layout = _layouts[ToIndex(resourceLayout)];
        var graphicsDevice = veldridBuffer.GraphicsDevice;

        // TODO: This is a hack, normally we should create a resource set from bindable resources
        // here we support only one resource per resource set. This is ok because resource layout will not exist anymore
        // with unbound resources.
        var resourceSet = graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout, veldridBuffer.Buffer, graphicsDevice.PointSampler));
        
        _resourceSets.Add(resourceSet);
        return _resourceSets.Count;
    }
    
    public GraphicsLegacy.ResourceSet CreateResourceSet(GraphicsLegacy.ResourceLayout resourceLayout, GraphicsLegacy.Texture texture)
    {
        var veldridTexture = _textures[ToIndex(texture)];
        var layout = _layouts[ToIndex(resourceLayout)];
        var graphicsDevice = veldridTexture.GraphicsDevice;

        // TODO: This is a hack, normally we should create a resource set from bindable resources
        // here we support only one resource per resource set. This is ok because resource layout will not exist anymore
        // with unbound resources.
        var resourceSet = graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(layout, veldridTexture.TextureView));
        
        _resourceSets.Add(resourceSet);
        return _resourceSets.Count;
    }

    public PipelineState CreatePipelineState(GraphicsLegacy.GraphicsDevice graphicsDevice, GraphicsLegacy.Shader shader, ReadOnlySpan<GraphicsLegacy.ResourceLayout> layouts)
    {
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];
        var veldridShader = _shaders[ToIndex(shader)];

        VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
        {
            new VertexLayoutDescription(
                new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4_Norm))
        };

        var veldridLayouts = new Veldrid.ResourceLayout[layouts.Length];

        for (var i = 0; i < layouts.Length; i++)
        {
            veldridLayouts[i] = _layouts[ToIndex(layouts[i])];
        }

        var pd = new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            new DepthStencilStateDescription(false, false, ComparisonKind.Always),
            new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, false, true),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(vertexLayouts, new[] { veldridShader.VertexShader, veldridShader.FragmentShader }),
            veldridLayouts,
            veldridGraphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
            ResourceBindingModel.Default);

        var pipeline = veldridGraphicsDevice.ResourceFactory.CreateGraphicsPipeline(ref pd);

        var veldridPipeline = new VeldridPipeline
        {
            Pipeline = pipeline
        };

        _pipelines.Add(veldridPipeline);
        return _pipelines.Count;
    }

    public void ClearColor(GraphicsLegacy.CommandList commandList, Vector4 color)
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        veldridCommandList.CommandList.ClearColorTarget(0, new RgbaFloat(color.X, color.Y, color.Z, color.W));
    }

    public void UpdateBuffer<T>(GraphicsLegacy.CommandList commandList, GraphicsBuffer buffer, nuint offset, ReadOnlySpan<T> data) where T : unmanaged
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        var veldridBuffer = _buffers[ToIndex(buffer)];

        veldridCommandList.CommandList.UpdateBuffer(veldridBuffer.Buffer, (uint)offset, data);
    }

    public void SetVertexBuffer(GraphicsLegacy.CommandList commandList, GraphicsBuffer buffer)
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        var veldridBuffer = _buffers[ToIndex(buffer)];
        
        veldridCommandList.CommandList.SetVertexBuffer(0, veldridBuffer.Buffer);
    }

    public void SetIndexBuffer(GraphicsLegacy.CommandList commandList, GraphicsBuffer buffer)
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        var veldridBuffer = _buffers[ToIndex(buffer)];

        veldridCommandList.CommandList.SetIndexBuffer(veldridBuffer.Buffer, IndexFormat.UInt16);
    }
    
    public void SetPipelineState(GraphicsLegacy.CommandList commandList, PipelineState pipelineState)
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        var veldridPipelineState = _pipelines[ToIndex(pipelineState)];

        veldridCommandList.CommandList.SetPipeline(veldridPipelineState.Pipeline);
    }
    
    public void SetResourceSet(GraphicsLegacy.CommandList commandList, int slot, GraphicsLegacy.ResourceSet resourceSet)
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        var veldridResourceSet = _resourceSets[ToIndex(resourceSet)];

        veldridCommandList.CommandList.SetGraphicsResourceSet((uint)slot, veldridResourceSet);
    }
    
    public void SetScissorRect(GraphicsLegacy.CommandList commandList, int x, int y, int width, int height)
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        veldridCommandList.CommandList.SetScissorRect(0, (uint)x, (uint)y, (uint)width, (uint)height);
    }
    
    public void DrawIndexed(GraphicsLegacy.CommandList commandList, uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
    {
        var veldridCommandList = _commandLists[ToIndex(commandList)];
        veldridCommandList.CommandList.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
    }

    private static int ToIndex(nint value)
    {
        return (int)value - 1;
    }
}