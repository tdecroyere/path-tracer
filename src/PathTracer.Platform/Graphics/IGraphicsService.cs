namespace PathTracer.Platform.Graphics;


// TODO: For each resource type, implement Delete/SetLabel Methods
public interface IGraphicsService
{
    GraphicsDevice CreateDevice();
 
    CommandQueue CreateCommandQueue(GraphicsDevice graphicsDevice, CommandQueueType type);

    SwapChain CreateSwapChain(CommandQueue commandQueue, NativeWindow window);
    void PresentSwapChain(SwapChain swapChain);

    GraphicsHeap CreateGraphicsHeap(GraphicsDevice graphicsDevice, GraphicsHeapType type, nuint sizeInBytes);
    GraphicsAllocationInfos GetBufferAllocationInfos(GraphicsHeap graphicsHeap, nuint sizeInBytes);

    GraphicsBuffer CreateBuffer(GraphicsHeap graphicsHeap, GraphicsBufferUsage usage, nuint heapOffset, nuint sizeInBytes);

    Shader CreateShader(GraphicsDevice graphicsDevice, ReadOnlySpan<byte> byteCode);
}

public interface IGraphicsServiceHighLevel
{
    GraphicsDevice CreateDevice();

    CommandQueue CreateCommandQueue(GraphicsDevice graphicsDevice, CommandQueueType type);

    SwapChain CreateSwapChain(CommandQueue commandQueue, NativeWindow window);
    void PresentSwapChain(SwapChain swapChain);
    uint GetCurrentFrameIndex(SwapChain swapChain);

    GraphicsHeap CreateGraphicsHeap(GraphicsDevice graphicsDevice, GraphicsHeapType type, nuint sizeInBytes, IGraphicsAllocator allocator);

    GraphicsBuffer CreateBuffer(GraphicsHeap graphicsHeap, GraphicsBufferUsage usage, nuint sizeInBytes);

    Shader CreateShader(GraphicsDevice graphicsDevice, ReadOnlySpan<byte> byteCode);
}

public interface IGraphicsAllocator
{

}

public interface ITest
{
    void Test1();

    private void Test2()
    {

    }
}