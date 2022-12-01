using System.Runtime.InteropServices;
using PathTracer.Platform.Graphics;
using Veldrid;

namespace PathTracer.Platform.Platforms.VeldridLibrary;

public class VeldridGraphicsService : IGraphicsService
{
    private readonly INativeUIService _nativeUIService;
    private readonly IList<Veldrid.GraphicsDevice> _graphicsDevices;
    private readonly IList<VeldridShader> _shaders;

    public VeldridGraphicsService(INativeUIService nativeUIService)
    {
        _nativeUIService = nativeUIService;
        _graphicsDevices = new List<Veldrid.GraphicsDevice>();
        _shaders = new List<VeldridShader>();
    }

    public Graphics.GraphicsDevice CreateDevice(NativeWindow window)
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

    public Graphics.Shader CreateShader(Graphics.GraphicsDevice graphicsDevice, ReadOnlySpan<byte> byteCode)
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
}