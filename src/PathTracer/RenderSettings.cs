namespace PathTracer;

public record RenderSettings
{
    public required RenderResolutionItem Resolution { get; set; }
    public required string OutputPath { get; set; }
}
