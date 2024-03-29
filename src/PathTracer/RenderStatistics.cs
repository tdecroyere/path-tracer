namespace PathTracer;

public record struct RenderStatistics
{
    public long RenderDuration { get; set; }
    public long CurrentFrameTime { get; set; }
    public int FramesPerSeconds { get; set; }
    public DateTime LastRenderTime { get; set; }
    public bool IsFileRenderingActive { get; set; }
    public int RenderWidth { get; set; }
    public int RenderHeight { get; set; }
    public long AllocatedManagedMemory { get; set; }
    public int GCGen0Count { get; set; }
    public int GCGen1Count { get; set; }
    public int GCGen2Count { get; set; }
}
