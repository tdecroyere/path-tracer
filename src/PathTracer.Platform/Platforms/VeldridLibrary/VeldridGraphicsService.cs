using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PathTracer.Platform.GraphicsLegacy;
using Veldrid;

namespace PathTracer.Platform.Platforms.VeldridLibrary;

public class VeldridGraphicsService : IGraphicsService
{
    private readonly INativeUIService _nativeUIService;
    private readonly IList<Veldrid.GraphicsDevice> _graphicsDevices;
    private readonly IList<VeldridShader> _shaders;
    private readonly IList<VeldridBuffer> _buffers;
    private readonly IList<VeldridPipeline> _pipelines;

    public VeldridGraphicsService(INativeUIService nativeUIService)
    {
        _nativeUIService = nativeUIService;
        _graphicsDevices = new List<Veldrid.GraphicsDevice>();
        _shaders = new List<VeldridShader>();
        _buffers = new List<VeldridBuffer>();
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

    public void PresentSwapChain(GraphicsLegacy.GraphicsDevice graphicsDevice)
    {
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];

        // TEST CODE
        var commandList = veldridGraphicsDevice.ResourceFactory.CreateCommandList();
        
        commandList.Begin();
        commandList.SetFramebuffer(veldridGraphicsDevice.MainSwapchain.Framebuffer);
        commandList.ClearColorTarget(0, RgbaFloat.Yellow);
        commandList.End();

        veldridGraphicsDevice.SubmitCommands(commandList);
        // END TEST CODE
    
        veldridGraphicsDevice.SwapBuffers(veldridGraphicsDevice.MainSwapchain);
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

    private static int ToIndex(nint value)
    {
        return (int)value - 1;
    }

    public PipelineState CreatePipelineState(GraphicsLegacy.GraphicsDevice graphicsDevice, GraphicsLegacy.Shader shader)
    {
        // TODO: For the moment the shader parameters are hardcoded
        // This code is temporary
        var veldridGraphicsDevice = _graphicsDevices[ToIndex(graphicsDevice)];
        var veldridShader = _shaders[ToIndex(shader)];

        VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
        {
            new VertexLayoutDescription(
                new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4_Norm))
        };

        var layout = veldridGraphicsDevice.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        var textureLayout = veldridGraphicsDevice.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

        var pd = new GraphicsPipelineDescription(
            BlendStateDescription.SingleAlphaBlend,
            new DepthStencilStateDescription(false, false, ComparisonKind.Always),
            new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, false, true),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(vertexLayouts, new[] { veldridShader.VertexShader, veldridShader.FragmentShader }),
            new ResourceLayout[] { layout, textureLayout },
            veldridGraphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
            ResourceBindingModel.Default);

        var pipeline = veldridGraphicsDevice.ResourceFactory.CreateGraphicsPipeline(ref pd);

        var veldridPipeline = new VeldridPipeline
        {
            MainLayout = layout,
            TextureLayout = textureLayout,
            Pipeline = pipeline
        };

        _pipelines.Add(veldridPipeline);
        return _pipelines.Count;
    }
}