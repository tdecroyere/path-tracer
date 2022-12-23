namespace PathTracer;

public record struct RenderSettings
{
    public required RenderResolutionItem Resolution { get; set; }
    public required string OutputPath { get; set; }
}
