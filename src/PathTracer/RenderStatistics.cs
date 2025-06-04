namespace PathTracer;

public record struct RenderStatistics
{
    public RenderStatistics()
    {
        FileRenderingProgression = 100;
    }

    public long RenderDuration { get; set; }
    public long CurrentFrameTime { get; set; }
    public int FramesPerSeconds { get; set; }
    public DateTime LastRenderTime { get; set; }
    public int FileRenderingProgression { get; set; }
    public int RenderWidth { get; set; }
    public int RenderHeight { get; set; }
    public long AllocatedManagedMemory { get; set; }
    public int CpuUsage { get; set; }
    public int GCGen0Count { get; set; }
    public int GCGen1Count { get; set; }
    public int GCGen2Count { get; set; }
}
