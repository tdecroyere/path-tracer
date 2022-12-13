namespace PathTracer;

public record RenderCommand : ICommand
{
    public required RenderSettings RenderSettings { get; init; }
}
