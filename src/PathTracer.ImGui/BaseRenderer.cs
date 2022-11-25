using System.Runtime.CompilerServices;
using Veldrid;

namespace PathTracer;

public abstract class BaseRenderer
{
    protected BaseRenderer(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
    }

    protected GraphicsDevice GraphicsDevice { get; init; }  

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
    }
    
    protected Shader LoadShader(string name, ShaderStages shaderStage)
    {
        var entryPoint = shaderStage switch
        {
            ShaderStages.Vertex when GraphicsDevice.BackendType == GraphicsBackend.Metal => "VS",
            ShaderStages.Fragment when GraphicsDevice.BackendType == GraphicsBackend.Metal => "FS",
            _ => "main"
        };

        var resourceName = GraphicsDevice.BackendType switch
        {
            GraphicsBackend.Vulkan => name + ".spv",
            GraphicsBackend.Metal => name + ".metallib",
            _ => throw new NotImplementedException()
        };     

        var shaderData = GetEmbeddedResourceBytes(resourceName);
        return GraphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(shaderStage, shaderData, entryPoint));
    }

    private static byte[] GetEmbeddedResourceBytes(string resourceName)
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
}