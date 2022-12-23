namespace PathTracer;

public readonly record struct RenderCommand : ICommand
{
    public required RenderSettings RenderSettings { get; init; }
}
