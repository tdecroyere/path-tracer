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
    private readonly ImGuiBackend _imGuiBackend;
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly CommandList _commandList;

    public ImGuiUIService(INativeUIService nativeUIService, IGraphicsService graphicsService, GraphicsDevice graphicsDevice, NativeWindow window)
    {
        var renderSize = nativeUIService.GetWindowRenderSize(window);

        _nativeUIService = nativeUIService;
        _graphicsService = graphicsService;
        _imGuiBackend = new ImGuiBackend(renderSize.Width, renderSize.Height, renderSize.UIScale);
        _imGuiRenderer = new ImGuiRenderer(graphicsService, graphicsDevice, "Menlo-Regular");
        _commandList = graphicsService.CreateCommandList(graphicsDevice);
    }

    public void Resize(int width, int height, float uiScale)
    {
        _imGuiBackend.Resize(width, height, uiScale);
    }

    public void Update(float deltaTime, InputState inputState)
    {
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
        ImGui.Begin("PathTracer", windowFlags);
        ImGui.PopStyleVar();
        
        ImGui.DockSpace(dockId, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.AutoHideTabBar);
    }

    public void Render()
    {
        ImGui.End();
        _imGuiBackend.Render();

        var imGuiDrawData = ImGui.GetDrawData();
        imGuiDrawData.ScaleClipRects(imGuiDrawData.FramebufferScale);

        _graphicsService.ResetCommandList(_commandList);
        _imGuiRenderer.RenderImDrawData(_commandList, ref imGuiDrawData);
        _graphicsService.SubmitCommandList(_commandList);
    }
    
    public nint RegisterTexture(Texture texture)
    {
        return _imGuiRenderer.RegisterTexture(texture);
    }

    public void UpdateTexture(nint id, Texture texture)
    {
        _imGuiRenderer.UpdateTexture(id, texture);
    }

    public void BeginPanel(string title, PanelStyles panelStyles)
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

        ImGui.Begin(title, flags);

        if ((panelStyles & PanelStyles.NoPadding) != 0)
        {
            ImGui.PopStyleVar();
        }
    }

    public void EndPanel()
    {
        ImGui.End();
    }
    
    public Vector2 GetPanelAvailableSize()
    {
        return ImGui.GetContentRegionAvail();
    }

    public void Text(string text)
    {
        ImGui.Text(text);
    }

    public void Image(nint textureId, int width, int height)
    {
        ImGui.Image(textureId, new Vector2(width, height));
    }
}