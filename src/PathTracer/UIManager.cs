namespace PathTracer;

public class UIManager
{
    private readonly IUIService _uiService;

    private readonly ReadOnlyMemory<RenderResolutionItem> _resolutionItems;

    private RenderResolutionItem _currentResolutionItem;
    private string _outputPath;

    public UIManager(IUIService uiService)
    {
        _uiService = uiService;
        _outputPath = "../../../TestData.ppm";

        _resolutionItems = new RenderResolutionItem[]
        {
            new RenderResolutionItem() { Name = "Ultra HD", Width = 3840, Height = 2160 },
            new RenderResolutionItem() { Name = "Full HD", Width = 1920, Height = 1080 }
        };

        _currentResolutionItem = _resolutionItems.Span[0];
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
            BuildRenderToImage();

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

    private void BuildRenderToImage()
    {
        if (_uiService.CollapsingHeader("Render To Image", isVisibleByDefault: false))
        {
            if (_uiService.BeginCombo("Resolution", _currentResolutionItem.Name))
            {
                for (var i = 0; i < _resolutionItems.Length; i++)
                {
                    var resolutionItem = _resolutionItems.Span[i];

                    if (_uiService.Selectable($"{resolutionItem.Name} ({resolutionItem.Width}x{resolutionItem.Height})", resolutionItem == _currentResolutionItem))
                    {
                        _currentResolutionItem = resolutionItem;
                    }
                }

                _uiService.EndCombo();
            }

            _uiService.InputText("Output", ref _outputPath);
            _uiService.NewLine();

            if (_uiService.Button("Render"))
            {
                Console.WriteLine("RENDER");
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
}

internal readonly record struct RenderResolutionItem
{
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}