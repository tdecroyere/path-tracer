using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PathTracer.Platform.Graphics;

namespace PathTracer;

public abstract class BaseRenderer
{
    private readonly GraphicsHeap _gpuGraphicsHeap;
    private nuint _currentGpuHeapOffset;

    protected BaseRenderer(IGraphicsService graphicsService, GraphicsDevice graphicsDevice)
    {
        GraphicsService = graphicsService;
        GraphicsDevice = graphicsDevice;

        _gpuGraphicsHeap = graphicsService.CreateGraphicsHeap(graphicsDevice, GraphicsHeapType.Gpu, 1024 * 1024 * 400);
    }

    protected IGraphicsService GraphicsService { get; init; }
    protected GraphicsDevice GraphicsDevice { get; init; }  

    protected GraphicsBuffer CreateBuffer(nuint sizeInBytes)
    {
        // TODO: Buffers needs to be dynamic so create one per frame in flights
        // For now we wait for the current frame to finish so it will work
        var bufferAllocation = GraphicsService.GetBufferAllocationInfos(_gpuGraphicsHeap, sizeInBytes);
        var buffer = GraphicsService.CreateBuffer(_gpuGraphicsHeap, GraphicsBufferUsage.Storage, _currentGpuHeapOffset, sizeInBytes);

        _currentGpuHeapOffset += bufferAllocation.SizeInBytes;
        return buffer;
    }
/*
    protected DeviceBuffer CreateBuffer<T>(T data, BufferUsage bufferUsage) where T : unmanaged
    {
        return CreateBuffer<T>(new T[] { data }, bufferUsage);
    }

    protected DeviceBuffer CreateBuffer<T>(ReadOnlySpan<T> data, BufferUsage bufferUsage) where T : unmanaged
    {
        var buffer = GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<T>() * data.Length), bufferUsage));
        GraphicsDevice.UpdateBuffer(buffer, 0, data);

        return buffer;
    }
    
    protected DeviceBuffer CheckSizeAndIncreaseBuffer(DeviceBuffer buffer, uint size, float increaseFactor = 1.5f)
    {
        if (size > buffer.SizeInBytes)
        {
            GraphicsDevice.DisposeWhenIdle(buffer);
            return GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription((uint)(size * increaseFactor), buffer.Usage));
        }

        return buffer;
    }*/
    
    protected Shader LoadShader(string name)
    {
        var shaderExtension = OperatingSystem.IsWindows() ? "spv" : "metallib";
        var vertexShaderName = $"{name}-vertex.{shaderExtension}";
        var pixelShaderName = $"{name}-frag.{shaderExtension}";

        var vertexShaderData = GetEmbeddedResourceBytes(vertexShaderName);
        var pixelShaderData = GetEmbeddedResourceBytes(pixelShaderName);
        var shaderData = BuildShaderData(vertexShaderData, pixelShaderData);

        return GraphicsService.CreateShader(GraphicsDevice, shaderData);
    }
    
    protected static byte[] GetEmbeddedResourceBytes(string resourceName)
    {
        var assembly = typeof(ImGuiBackend).Assembly;
        using var resourceStream = assembly.GetManifestResourceStream(resourceName);
        
        if (resourceStream is null)
        {
            return Array.Empty<byte>();
        }

        var result = new byte[resourceStream.Length];
        resourceStream.Read(result, 0, (int)resourceStream.Length);
        return result;
    }

    private static ReadOnlySpan<byte> BuildShaderData(byte[] vertexShaderData, byte[] pixelShaderData)
    {
        var vertexShaderDataLength = vertexShaderData.Length;
        var pixelShaderDataLength = pixelShaderData.Length;

        var shaderData = new byte[vertexShaderDataLength + pixelShaderDataLength + 2 * sizeof(int)].AsSpan();

        var copyShaderData = shaderData;
        MemoryMarshal.Write(copyShaderData, ref vertexShaderDataLength);
        copyShaderData = copyShaderData[sizeof(int)..];

        MemoryMarshal.Write(copyShaderData, ref pixelShaderDataLength);
        copyShaderData = copyShaderData[sizeof(int)..];

        vertexShaderData.CopyTo(copyShaderData);
        copyShaderData = copyShaderData[vertexShaderDataLength..];

        pixelShaderData.CopyTo(copyShaderData);
        return shaderData;
    }
}