namespace PathTracer;

public class UIManager : IUIManager
{
    private readonly IUIService _uiService;
    private readonly ICommandManager _commandManager;
    private readonly ReadOnlyMemory<RenderResolutionItem> _resolutionItems;

    private RenderSettings _renderSettings;

    public UIManager(IUIService uiService, ICommandManager commandManager)
    {
        _uiService = uiService;
        _commandManager = commandManager;

        _resolutionItems = new RenderResolutionItem[]
        {
            new RenderResolutionItem { Name = "Ultra HD", Width = 3840, Height = 2160 },
            new RenderResolutionItem { Name = "Full HD", Width = 1920, Height = 1080 }
        };

        _renderSettings = new RenderSettings
        {
            Resolution = _resolutionItems.Span[0],
            OutputPath = "TestData/Output.png"
        };
    }

    public void Init(NativeWindow window, GraphicsDevice graphicsDevice)
    {
        _uiService.Init(window, graphicsDevice);
    }

    public void Resize(NativeWindowSize windowSize)
    {
        _uiService.Resize(windowSize);
    }

    public Vector2 Update(float deltaTime, InputState inputState, TextureImage renderImage, RenderStatistics renderStatistics, Scene scene)
    {
        _uiService.Update(deltaTime, inputState);

        var availableViewportSize = Vector2.Zero;

        if (_uiService.BeginPanel("Render", PanelStyles.NoTitle | PanelStyles.NoPadding))
        {
            availableViewportSize = _uiService.GetPanelAvailableSize();

            if (renderImage.Width != 0 && renderImage.Height != 0)
            {
                _uiService.Image(renderImage.GpuTexture, (int)availableViewportSize.X, (int)availableViewportSize.Y);
            }

            _uiService.EndPanel();
        }

        if (_uiService.BeginPanel("Inspector"))
        {
            BuildStatistics(renderStatistics);
            BuildSceneProperties(scene);
            BuildRenderToImage(renderStatistics);

            _uiService.EndPanel();
        }

        return availableViewportSize;
    }

    public void Render()
    {
        _uiService.Render();
    }

    private void BuildStatistics(RenderStatistics renderStatistics)
    {
        if (_uiService.CollapsingHeader("Statistics"))
        {
            _uiService.Text($"FrameTime: {renderStatistics.CurrentFrameTime} ms (FPS: {renderStatistics.FramesPerSeconds})");
            _uiService.Text($"RenderSize: {renderStatistics.RenderWidth}x{renderStatistics.RenderHeight}");
            _uiService.Text($"Last render duration: {renderStatistics.RenderDuration} ms");
            _uiService.Text($"Last render time: {renderStatistics.LastRenderTime}");
            _uiService.Text($"Allocated manager memory: {Utils.ConvertBytesToMegaBytes(renderStatistics.AllocatedManagedMemory)} MB");
            _uiService.Text($"GC count: Gen0={renderStatistics.GCGen0Count}, Gen1={renderStatistics.GCGen1Count}, Gen2={renderStatistics.GCGen2Count}");
            _uiService.NewLine();
        }
    }

    private void BuildSceneProperties(Scene scene)
    {
        if (_uiService.CollapsingHeader("Scene"))
        {
            for (var i = 0; i < scene.Spheres.Count; i++)
            {
                _uiService.PushId(i.ToString());

                // TODO: Can we do something better here?
                var sphere = scene.Spheres[i];

                var position = sphere.Position;
                var radius = sphere.Radius;
                var albedo = sphere.Albedo;

                if (_uiService.DragFloat3("Position", ref position))
                {
                    sphere.Position = position;
                    scene.Spheres[i] = sphere;
                    scene.HasChanged = true;
                }

                if (_uiService.DragFloat("Radius", ref radius))
                {
                    sphere.Radius = radius;
                    scene.Spheres[i] = sphere;
                    scene.HasChanged = true;
                }

                if (_uiService.ColorEdit3("Albedo", ref albedo))
                {
                    sphere.Albedo = albedo;
                    scene.Spheres[i] = sphere;
                    scene.HasChanged = true;
                }

                _uiService.Separator();
                _uiService.PopId();
            }

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