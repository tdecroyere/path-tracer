using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using PathTracer;
using PathTracer.Platform;
using PathTracer.Platform.GraphicsLegacy;
using PathTracer.Platform.Inputs;

var serviceCollection = new ServiceCollection();
serviceCollection.UseNativePlatform();
serviceCollection.UseGraphicsPlatform();

var serviceProvider = serviceCollection.BuildServiceProvider();

var nativeApplicationService = serviceProvider.GetRequiredService<INativeApplicationService>();
var nativeUIService = serviceProvider.GetRequiredService<INativeUIService>();
var nativeInputService = serviceProvider.GetRequiredService<IInputService>();
var graphicsService = serviceProvider.GetRequiredService<IGraphicsService>();

var nativeApplication = nativeApplicationService.CreateApplication("PathTracer IMGui");
var nativeWindow = nativeUIService.CreateWindow(nativeApplication, "Path Tracer IMGui", 1280, 720, NativeWindowState.Normal);
var renderSize = nativeUIService.GetWindowRenderSize(nativeWindow);

var graphicsDevice = graphicsService.CreateDevice(nativeWindow);

var imGuiBackend = new ImGuiBackend(renderSize.Width, renderSize.Height, renderSize.UIScale);
var imGuiRenderer = new ImGuiRenderer(graphicsService, graphicsDevice, "Menlo-Regular");

// TODO: Don't create texture when init do this on resize only
var textureRenderer = new TextureRenderer(graphicsService, graphicsDevice, renderSize.Width, renderSize.Height);
var textureData = new uint[renderSize.Width * renderSize.Height].AsSpan();
var textureId = imGuiRenderer.RegisterTexture(textureRenderer.Texture);

Console.WriteLine($"Native Window Size: {renderSize}");

var commandList = graphicsService.CreateCommandList(graphicsDevice);
var frameCount = 0;
var stopwatch = new Stopwatch();

var currentWidth = renderSize.Width;
var currentHeight = renderSize.Height;

var currentViewportWidth = 0;
var currentViewportHeight = 0;

var appStatus = new NativeApplicationStatus();
var inputState = new InputState();

while (appStatus.IsRunning == 1)
{
    appStatus = nativeApplicationService.ProcessSystemMessages(nativeApplication);
    nativeInputService.UpdateInputState(nativeApplication, ref inputState);
    renderSize = nativeUIService.GetWindowRenderSize(nativeWindow);

    if (currentWidth != renderSize.Width || currentHeight != renderSize.Height)
    {
        graphicsService.ResizeSwapChain(graphicsDevice, renderSize.Width, renderSize.Height);
        
        imGuiBackend.Resize(renderSize.Width, renderSize.Height, renderSize.UIScale);
       
        Console.WriteLine($"Resize: {renderSize}");

        currentWidth = renderSize.Width;
        currentHeight = renderSize.Height;
    }

    imGuiBackend.Update(1.0f / 60.0f, inputState);

    stopwatch.Restart();
    for (var i = 0; i < textureData.Length; i++)
    {
        var color = (byte)(frameCount % 255);
        textureData[i] = (uint) (255 << 24 | color << 16 | color << 8 | color);
    }

    stopwatch.Stop();

    var dockId = ImGui.GetID("PathTracerDock");

    var viewport = ImGui.GetMainViewport();
    ImGui.SetNextWindowPos(viewport.WorkPos);
	ImGui.SetNextWindowSize(viewport.WorkSize);
	ImGui.SetNextWindowViewport(viewport.ID);

    var windowFlags = ImGuiWindowFlags.NoDocking;
    windowFlags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
	windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground;

    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
    ImGui.Begin("PathTracer", windowFlags);
    ImGui.PopStyleVar();
    
    ImGui.DockSpace(dockId, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.AutoHideTabBar);

    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
    ImGui.Begin("Viewport", ImGuiWindowFlags.NoTitleBar);
    ImGui.PopStyleVar();

    var size = ImGui.GetContentRegionAvail();
    var viewportWidth = (int)size.X;
    var viewportHeight = (int)size.Y;

    ImGui.Image(textureId, new Vector2(viewportWidth, viewportHeight));
    ImGui.End();
    
    ImGui.Begin("Inspector", ImGuiWindowFlags.NoCollapse);

    var visible = true;
    ImGui.CollapsingHeader("Status", ref visible);
    ImGui.Text($"Render Size: {viewportWidth * renderSize.UIScale}x{viewportHeight * renderSize.UIScale}");
    ImGui.Text($"Delta: {stopwatch.ElapsedMilliseconds}");
    
    var framerate = ImGui.GetIO().Framerate;
    ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");

    ImGui.End();

    ImGui.End();

    if (currentViewportWidth != viewportWidth || currentViewportHeight != viewportHeight)
    {
        var textureWidth = (int)(viewportWidth * renderSize.UIScale);
        var textureHeight = (int)(viewportHeight * renderSize.UIScale);

        // TODO: Crash if minimized
        textureRenderer.Resize(textureWidth, textureHeight);
        imGuiRenderer.UpdateTexture(textureId, textureRenderer.Texture);
        
        textureData = new uint[textureWidth * textureHeight].AsSpan();

        Console.WriteLine($"Resize Viewport: {textureWidth}x{textureHeight}");
        
        currentViewportWidth = viewportWidth;
        currentViewportHeight = viewportHeight;
    }

    imGuiBackend.Render();
    var imGuiDrawData = ImGui.GetDrawData();
    imGuiDrawData.ScaleClipRects(imGuiDrawData.FramebufferScale);

    graphicsService.ResetCommandList(commandList);
    graphicsService.ClearColor(commandList, new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
    
    textureRenderer.UpdateTexture<uint>(commandList, textureData);
    imGuiRenderer.RenderImDrawData(commandList, ref imGuiDrawData);

    graphicsService.SubmitCommandList(commandList);
    graphicsService.PresentSwapChain(graphicsDevice);

    frameCount++;
}