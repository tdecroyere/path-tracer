using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using PathTracer;
using PathTracer.Platform;
using PathTracer.Platform.Graphics;
using PathTracer.Platform.Inputs;
using Veldrid;

var serviceCollection = new ServiceCollection();
serviceCollection.UseNativePlatform();
serviceCollection.UseGraphicsPlatform();

var serviceProvider = serviceCollection.BuildServiceProvider();

var nativeApplicationService = serviceProvider.GetRequiredService<INativeApplicationService>();
var nativeUIService = serviceProvider.GetRequiredService<INativeUIService>();
var nativeInputService = serviceProvider.GetRequiredService<IInputService>();
var graphicsService = serviceProvider.GetRequiredService<IGraphicsService>();

var nativeApplication = nativeApplicationService.CreateApplication("PathTracer IMGui");
var nativeWindowOld = nativeUIService.CreateWindow(nativeApplication, "Path Tracer IMGui", 1280, 720, NativeWindowState.Normal);
var nativeWindow = nativeUIService.CreateWindow(nativeApplication, "Path Tracer IMGui", 1280, 720, NativeWindowState.Normal);
var renderSize = nativeUIService.GetWindowRenderSize(nativeWindowOld);

var graphicsDevice = graphicsService.CreateDevice(nativeWindow);
Console.WriteLine(graphicsDevice);

var graphicsDeviceOld = CreateGraphicsDevice(nativeUIService, nativeWindowOld);
var imGuiBackend = new ImGuiBackend(renderSize.Width, renderSize.Height, renderSize.UIScale);
var imGuiRendererOld = new ImGuiRendererOld(graphicsDeviceOld, graphicsDeviceOld.MainSwapchain.Framebuffer.OutputDescription, "Menlo-Regular");
var imGuiRenderer = new ImGuiRenderer(graphicsService, graphicsDevice, "Menlo-Regular");

// TODO: Don't create texture when init do this on resize only
var textureRenderer = new TextureRenderer(graphicsDeviceOld, graphicsDeviceOld.MainSwapchain.Framebuffer.OutputDescription, renderSize.Width, renderSize.Height);
var textureData = new uint[renderSize.Width * renderSize.Height].AsSpan();
var textureId = imGuiRendererOld.RegisterTexture(textureRenderer.TextureView);

Console.WriteLine($"Native Window Size: {renderSize}");
Console.WriteLine($"FrameBuffer Size: {graphicsDeviceOld.MainSwapchain.Framebuffer.Width}x{graphicsDeviceOld.MainSwapchain.Framebuffer.Height}");

var commandList = graphicsDeviceOld.ResourceFactory.CreateCommandList();
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
    renderSize = nativeUIService.GetWindowRenderSize(nativeWindowOld);

    if (currentWidth != renderSize.Width || currentHeight != renderSize.Height)
    {
        graphicsDeviceOld.MainSwapchain.Resize((uint)renderSize.Width, (uint)renderSize.Height);
        
        imGuiBackend.Resize(renderSize.Width, renderSize.Height, renderSize.UIScale);
       
        Console.WriteLine($"Resize Native Window Size: {renderSize}");
        Console.WriteLine($"Resize FrameBuffer Size: {graphicsDeviceOld.MainSwapchain.Framebuffer.Width}x{graphicsDeviceOld.MainSwapchain.Framebuffer.Height}");

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
        imGuiRendererOld.UpdateTexture(textureId, textureRenderer.TextureView);
        //imGuiRenderer.UpdateTexture(textureId, textureRenderer.TextureView);
        
        textureData = new uint[textureWidth * textureHeight].AsSpan();

        Console.WriteLine($"Resize Viewport: {textureWidth}x{textureHeight}");
        
        currentViewportWidth = viewportWidth;
        currentViewportHeight = viewportHeight;
    }

    commandList.Begin();
    commandList.SetFramebuffer(graphicsDeviceOld.MainSwapchain.Framebuffer);
    commandList.ClearColorTarget(0, RgbaFloat.Black);

    textureRenderer.UpdateTexture<uint>(commandList, textureData);
    imGuiBackend.Render();

    var drawData = ImGui.GetDrawData();
    imGuiRendererOld.RenderImDrawData(commandList, ref drawData);
    //imGuiRenderer.RenderImDrawData(commandList, ref drawData);

    commandList.End();

    graphicsDeviceOld.SubmitCommands(commandList);
    graphicsDeviceOld.SwapBuffers(graphicsDeviceOld.MainSwapchain);

    frameCount++;
}

static unsafe Veldrid.GraphicsDevice CreateGraphicsDevice(INativeUIService nativeUIService, NativeWindow nativeWindow)
{
    var graphicsDeviceOptions = new GraphicsDeviceOptions(
            debug: true, 
            swapchainDepthFormat: null, 
            syncToVerticalBlank: true, 
            ResourceBindingModel.Improved, 
            preferDepthRangeZeroToOne: true, 
            preferStandardClipSpaceYDirection: true);

    var nativeWindowSystemHandle = nativeUIService.GetWindowSystemHandle(nativeWindow);
    var renderSize = nativeUIService.GetWindowRenderSize(nativeWindow);

    Veldrid.GraphicsDevice? graphicsDevice = null;

    if (OperatingSystem.IsWindows())
    {
        var swapchainSource = SwapchainSource.CreateWin32(nativeWindowSystemHandle, 0);

        var swapchainDescription = new SwapchainDescription(
                        swapchainSource,
                        (uint)renderSize.Width,
                        (uint)renderSize.Height,
                        graphicsDeviceOptions.SwapchainDepthFormat,
                        graphicsDeviceOptions.SyncToVerticalBlank,
                        graphicsDeviceOptions.SwapchainSrgbFormat);

        graphicsDevice = Veldrid.GraphicsDevice.CreateVulkan(graphicsDeviceOptions, swapchainDescription);
    }

    else if (OperatingSystem.IsMacOS())
    {
        var swapchainSource = SwapchainSource.CreateNSWindow(nativeWindowSystemHandle);

        var swapchainDescription = new SwapchainDescription(
                        swapchainSource,
                        (uint)renderSize.Width,
                        (uint)renderSize.Height,
                        graphicsDeviceOptions.SwapchainDepthFormat,
                        graphicsDeviceOptions.SyncToVerticalBlank,
                        graphicsDeviceOptions.SwapchainSrgbFormat);

        graphicsDevice = Veldrid.GraphicsDevice.CreateMetal(graphicsDeviceOptions, swapchainDescription);
        graphicsDevice.MainSwapchain.Resize((uint)renderSize.Width, (uint)renderSize.Height);
    }

    if (graphicsDevice == null)
    {
        throw new InvalidOperationException("Create Graphics device: Unsupported OS");
    }

    return graphicsDevice;
}