namespace PathTracer.Platform.GraphicsLegacy;

public interface IGraphicsService
{
    GraphicsDevice CreateDevice(NativeWindow window);
    void PresentSwapChain(GraphicsDevice graphicsDevice);

    GraphicsBuffer CreateBuffer(GraphicsDevice graphicsDevice, nuint sizeInBytes, GraphicsBufferUsage usage);
    void DeleteBuffer(GraphicsBuffer buffer);
    GraphicsBufferDescription GetBufferDescription(GraphicsBuffer buffer);
    void UpdateBuffer<T>(GraphicsBuffer buffer, nuint offset, ReadOnlySpan<T> data) where T : unmanaged;

    Shader CreateShader(GraphicsDevice graphicsDevice, ReadOnlySpan<byte> byteCode);

    

    PipelineState CreatePipelineState(GraphicsDevice graphicsDevice, Shader shader);
}