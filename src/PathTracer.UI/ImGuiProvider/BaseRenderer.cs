using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PathTracer.Platform.GraphicsLegacy;

namespace PathTracer.UI.ImGuiProvider;

internal abstract class BaseRenderer
{
    protected BaseRenderer(IGraphicsService graphicsService, GraphicsDevice graphicsDevice)
    {
        GraphicsService = graphicsService;
        GraphicsDevice = graphicsDevice;
    }

    protected IGraphicsService GraphicsService { get; init; }
    protected GraphicsDevice GraphicsDevice { get; init; }  

    protected GraphicsBuffer CreateBuffer<T>(T data, GraphicsBufferUsage bufferUsage) where T : unmanaged
    {
        return CreateBuffer<T>(MemoryMarshal.CreateReadOnlySpan(ref data, 1), bufferUsage);
    }

    protected GraphicsBuffer CreateBuffer<T>(ReadOnlySpan<T> data, GraphicsBufferUsage bufferUsage) where T : unmanaged
    {
        var buffer = GraphicsService.CreateBuffer(GraphicsDevice, (uint)(Unsafe.SizeOf<T>() * data.Length), bufferUsage);
        GraphicsService.UpdateBuffer(buffer, 0, data);

        return buffer;
    }
    
    protected GraphicsBuffer CheckSizeAndIncreaseBuffer(GraphicsBuffer buffer, uint size, float increaseFactor = 1.5f)
    {
        var bufferDescription = GraphicsService.GetBufferDescription(buffer);

        if (size > bufferDescription.SizeInBytes)
        {
            GraphicsService.DeleteBuffer(buffer);
            return GraphicsService.CreateBuffer(GraphicsDevice, (uint)(size * increaseFactor), bufferDescription.Usage);
        }

        return buffer;
    }
    
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
        var assembly = typeof(BaseRenderer).Assembly;
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