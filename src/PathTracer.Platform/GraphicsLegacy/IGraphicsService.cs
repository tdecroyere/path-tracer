namespace PathTracer.Platform.GraphicsLegacy;

// NOTE: This interface is use only for the moment to make things work with Veldrid
// This one will be replaced by a more robust interface that allows to manage Direct3D, Vulkan and Metal
public interface IGraphicsService
{
    GraphicsDevice CreateDevice(NativeWindow window);
    void PresentSwapChain(GraphicsDevice graphicsDevice);

    CommandList CreateCommandList(GraphicsDevice graphicsDevice);
    void ResetCommandList(CommandList commandList);
    void SubmitCommandList(CommandList commandList);

    GraphicsBuffer CreateBuffer(GraphicsDevice graphicsDevice, nuint sizeInBytes, GraphicsBufferUsage usage);
    void DeleteBuffer(GraphicsBuffer buffer);
    GraphicsBufferDescription GetBufferDescription(GraphicsBuffer buffer);
    void UpdateBuffer<T>(GraphicsBuffer buffer, nuint offset, ReadOnlySpan<T> data) where T : unmanaged;

    Texture CreateTexture(GraphicsDevice graphicsDevice, int width, int height, int depth, int mipLevels, int arrayLayers, TextureFormat format, TextureUsage usage, TextureType type);
    void UpdateTexture<T>(Texture texture, ReadOnlySpan<T> data) where T : unmanaged;

    Shader CreateShader(GraphicsDevice graphicsDevice, ReadOnlySpan<byte> byteCode);

    ResourceLayout CreateResourceLayout(GraphicsDevice graphicsDevice, ReadOnlySpan<ResourceLayoutElement> elements);
    ResourceSet CreateResourceSet(ResourceLayout resourceLayout, GraphicsBuffer buffer);
    ResourceSet CreateResourceSet(ResourceLayout resourceLayout, Texture texture);

    PipelineState CreatePipelineState(GraphicsDevice graphicsDevice, Shader shader, ReadOnlySpan<ResourceLayout> layouts);

    // ClearColor
}