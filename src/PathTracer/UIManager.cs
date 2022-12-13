namespace PathTracer;

// TODO: Create an interface
public class UIManager
{
    private readonly IUIService _uiService;
    private readonly ICommandManager _commandManager;

    private readonly ReadOnlyMemory<RenderResolutionItem> _resolutionItems;
    private readonly RenderSettings _renderSettings;

    public UIManager(IUIService uiService, ICommandManager commandManager)
    {
        _uiService = uiService;
        _commandManager = commandManager;

        _resolutionItems = new RenderResolutionItem[]
        {
            new RenderResolutionItem() { Name = "Ultra HD", Width = 3840, Height = 2160 },
            new RenderResolutionItem() { Name = "Full HD", Width = 1920, Height = 1080 }
        };

        _renderSettings = new RenderSettings
        {
            Resolution = _resolutionItems.Span[0],
            OutputPath = "TestData/Output.png"
        };
    }
    
    public Vector2 BuildUI(TextureImage? renderImage, RenderStatistics renderStatistics)
    {
        var availableViewportSize = Vector2.Zero;

        if (_uiService.BeginPanel("Render", PanelStyles.NoTitle | PanelStyles.NoPadding))
        {
            availableViewportSize = _uiService.GetPanelAvailableSize();

            if (renderImage is not null)
            {
                _uiService.Image(renderImage.Value.TextureId, (int)availableViewportSize.X, (int)availableViewportSize.Y);
            }

            _uiService.EndPanel();
        }

        if (_uiService.BeginPanel("Inspector"))
        {
            BuildStatistics(renderImage, renderStatistics);
            BuildRenderToImage(renderStatistics);

            _uiService.EndPanel();
        }

        return availableViewportSize;
    }

    private void BuildStatistics(TextureImage? renderImage, RenderStatistics renderStatistics)
    {
        if (_uiService.CollapsingHeader("Statistics"))
        {
            _uiService.Text($"FrameTime: {renderStatistics.CurrentFrameTime} ms (FPS: {renderStatistics.FramesPerSeconds})");
            _uiService.Text($"RenderSize: {renderImage?.Width}x{renderImage?.Height}");
            _uiService.Text($"Last render duration: {renderStatistics.RenderStopwatch.ElapsedMilliseconds} ms");
            _uiService.Text($"Last render time: {renderStatistics.LastRenderTime}");
            _uiService.NewLine();
        }
    }

    private void BuildRenderToImage(RenderStatistics renderStatistics)
    {
        if (_uiService.CollapsingHeader("Render To Image", isVisibleByDefault: false))
        {
            if (_uiService.BeginCombo("Resolution", _renderSettings.Resolution.Name))
            {
                for (var i = 0; i < _resolutionItems.Length; i++)
                {
                    var resolutionItem = _resolutionItems.Span[i];

                    if (_uiService.Selectable($"{resolutionItem.Name} ({resolutionItem.Width}x{resolutionItem.Height})", resolutionItem == _renderSettings.Resolution))
                    {
                        _renderSettings.Resolution = resolutionItem;
                    }
                }

                _uiService.EndCombo();
            }

            // TODO: Can we do something better here?
            var outputPath = _renderSettings.OutputPath;
            _uiService.InputText("Output", ref outputPath);
            _renderSettings.OutputPath = outputPath;
            
            _uiService.NewLine();

            if (_uiService.Button("Render", renderStatistics.IsFileRenderingActive ? ControlStyles.Disabled : ControlStyles.None))
            {
                _commandManager.SendCommand(new RenderCommand() { RenderSettings = _renderSettings });
            }

            if (renderStatistics.IsFileRenderingActive)
            {
                _uiService.Text("Rendering...");
            }
        }
    }
}

public record RenderStatistics
{
    public RenderStatistics()
    {
        RenderStopwatch = new Stopwatch();
    }

    public Stopwatch RenderStopwatch { get; set; }
    public long CurrentFrameTime { get; set; }
    public int FramesPerSeconds { get; set; }
    public DateTime LastRenderTime { get; set; }
    public bool IsFileRenderingActive { get; set; }
}

public record RenderCommand : ICommand
{
    public required RenderSettings RenderSettings { get; init; }
}

public record RenderSettings
{
    public required RenderResolutionItem Resolution { get; set; }
    public required string OutputPath { get; set; }
}

public record RenderResolutionItem
{
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}