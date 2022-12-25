using System.Numerics;
using ImGuiNET;
using PathTracer.Platform;
using PathTracer.Platform.GraphicsLegacy;
using PathTracer.Platform.Inputs;

namespace PathTracer.UI.ImGuiProvider;

public class ImGuiUIService : IUIService
{
    private readonly INativeUIService _nativeUIService;
    private readonly IGraphicsService _graphicsService;

    private readonly IDictionary<Texture, nint> _textureIdList;

    private ImGuiBackend? _imGuiBackend;
    private ImGuiRenderer? _imGuiRenderer;
    private CommandList? _commandList;

    public ImGuiUIService(INativeUIService nativeUIService, IGraphicsService graphicsService)
    {
        _nativeUIService = nativeUIService;
        _graphicsService = graphicsService;

        _textureIdList = new Dictionary<Texture, nint>();
    }

    public void Init(NativeWindow window, GraphicsDevice graphicsDevice)
    {
        var renderSize = _nativeUIService.GetWindowRenderSize(window);

        _imGuiBackend = new ImGuiBackend(renderSize.Width, renderSize.Height, renderSize.UIScale);
        _imGuiRenderer = new ImGuiRenderer(_graphicsService, graphicsDevice, "Menlo-Regular");
        _commandList = _graphicsService.CreateCommandList(graphicsDevice);
    }

    public void Resize(NativeWindowSize windowSize)
    {
        if (_imGuiBackend is null)
        {
            throw new InvalidOperationException("You need call the init method first.");
        }

        _imGuiBackend.Resize(windowSize.Width, windowSize.Height, windowSize.UIScale);
    }

    public void Update(float deltaTime, InputState inputState)
    {
        if (_imGuiBackend is null)
        {
            throw new InvalidOperationException("You need call the init method first.");
        }

        _imGuiBackend.Update(deltaTime, inputState);
        
        var dockId = ImGui.GetID("PathTracerDock");

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        var windowFlags = ImGuiWindowFlags.NoDocking;
        windowFlags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.Begin("PathTracer", windowFlags);
        ImGui.PopStyleVar();
        ImGui.PopStyleVar();
        
        ImGui.DockSpace(dockId, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.AutoHideTabBar);
    }

    public void Render()
    {
        if (_imGuiBackend is null || _imGuiRenderer is null || _commandList is null)
        {
            throw new InvalidOperationException("You need call the init method first.");
        }

        ImGui.End();
        //ImGui.ShowDemoWindow();
        _imGuiBackend.Render();

        var imGuiDrawData = ImGui.GetDrawData();
        imGuiDrawData.ScaleClipRects(imGuiDrawData.FramebufferScale);

        _graphicsService.ResetCommandList(_commandList.Value);
        _imGuiRenderer.RenderImDrawData(_commandList.Value, ref imGuiDrawData);
        _graphicsService.SubmitCommandList(_commandList.Value);
    }

    public bool BeginPanel(string title, PanelStyles panelStyles)
    {
        if ((panelStyles & PanelStyles.NoPadding) != 0)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }
        var flags = ImGuiWindowFlags.NoCollapse;

        if ((panelStyles & PanelStyles.NoTitle) != 0)
        {
            flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration;
        }

        var result = ImGui.Begin(title, flags);

        if ((panelStyles & PanelStyles.NoPadding) != 0)
        {
            ImGui.PopStyleVar();
        }

        return result;
    }

    public void EndPanel()
    {
        ImGui.End();
    }
    
    public Vector2 GetPanelAvailableSize()
    {
        return ImGui.GetContentRegionAvail();
    }
    
    public void PushId(string id)
    {
        ImGui.PushID(id);
    }

    public void PopId()
    {
        ImGui.PopID();
    }

    public bool CollapsingHeader(string text, bool isVisibleByDefault)
    {
        var flags = ImGuiTreeNodeFlags.CollapsingHeader;

        if (isVisibleByDefault)
        {
            flags |= ImGuiTreeNodeFlags.DefaultOpen;
        }

        return ImGui.CollapsingHeader(text, flags);
    }

    public void Text(string text)
    {
        ImGui.Text(text);
    }

    public void NewLine()
    {
        ImGui.NewLine();
    }

    public void Separator()
    {
        ImGui.Separator();
    }

    public void Image(Texture texture, int width, int height)
    {
        if (_imGuiRenderer is null)
        {
            throw new InvalidOperationException("You need call the init method first.");
        }

        // TODO: We need a way to clean the unused ids
        if (!_textureIdList.ContainsKey(texture))
        {
            var id = _imGuiRenderer.RegisterTexture(texture);
            _textureIdList.Add(texture, id);
        }

        var textureId = _textureIdList[texture];
        ImGui.Image(textureId, new Vector2(width, height));
    }

    public bool Button(string text, ControlStyles controlStyles)
    {
        if ((controlStyles & ControlStyles.Disabled) != 0)
        {
            ImGui.BeginDisabled();
        }

        var result = ImGui.Button(text);

        if ((controlStyles & ControlStyles.Disabled) != 0)
        {
            ImGui.EndDisabled();
        }

        return result;
    }
    
    public bool InputText(string label, ref string text, int maxLength)
    {
        return ImGui.InputText(label, ref text, (uint)maxLength);
    }

    public bool DragFloat3(string label, ref Vector3 value, float increment)
    {
        return ImGui.DragFloat3(label, ref value, increment);
    }

    public bool DragFloat(string label, ref float value, float increment)
    {
        return ImGui.DragFloat(label, ref value, increment);
    }
    
    public bool ColorEdit3(string label, ref Vector3 value)
    {
        return ImGui.ColorEdit3(label, ref value);
    }

    public bool BeginCombo(string label, string previewValue)
    {
        return ImGui.BeginCombo(label, previewValue);
    }

    public void EndCombo()
    {
        ImGui.EndCombo();
    }

    public bool Selectable(string text, bool isSelected)
    {
        var result = ImGui.Selectable(text);

        if (isSelected)
        {
            ImGui.SetItemDefaultFocus();
        }
        
        return result;
    }

    public void Progressbar(float value)
    {
        ImGui.ProgressBar(value, new Vector2(0.0f, 0.0f));
    }
}