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
    DynamicResource<GraphicsBuffer> CreateDynamicBuffer(GraphicsHeap graphicsHeap, SwapChain swapChain, GraphicsBufferUsage usage, nuint sizeInBytes);

    Shader CreateShader(GraphicsDevice graphicsDevice, ReadOnlySpan<byte> byteCode);
}

public interface IGraphicsAllocator
{

}

public interface IGraphicsResource
{
    nint NativePointer { get; }
}

public readonly record struct DynamicResource<T> : IGraphicsResource where T : IGraphicsResource
{
    private readonly IGraphicsServiceHighLevel _graphicsService;
    private readonly SwapChain _swapChain;

    public DynamicResource(IGraphicsServiceHighLevel graphicsService, SwapChain swapChain, T resource1, T resource2)
    {
        _graphicsService = graphicsService;
        _swapChain = swapChain;

        Resource1 = resource1;
        Resource2 = resource2;
    }

    public T Resource1 { get; init; }
    public T Resource2 { get; init; }

    public nint NativePointer => (_graphicsService.GetCurrentFrameIndex(_swapChain) % 2 == 0) ? Resource1.NativePointer : Resource2.NativePointer;
}


public interface ITest
{
    void Test1();

    private void Test2()
    {

    }
}