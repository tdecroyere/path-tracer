using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using PathTracer;
using PathTracer.Platform;
using PathTracer.Platform.Inputs;
using PathTracer.Platform.NativeUI;
using Veldrid;

var serviceCollection = new ServiceCollection();
serviceCollection.UsePathTracerPlatform();

var serviceProvider = serviceCollection.BuildServiceProvider();

var nativeApplicationService = serviceProvider.GetRequiredService<INativeApplicationService>();
var nativeUIService = serviceProvider.GetRequiredService<INativeUIService>();
var nativeInputService = serviceProvider.GetRequiredService<INativeInputService>();

var nativeApplication = nativeApplicationService.CreateApplication("PathTracer IMGui");
var nativeWindow = nativeUIService.CreateWindow(nativeApplication, "Path Tracer IMGui", 1280, 720, NativeWindowState.Normal);
var renderSize = nativeUIService.GetWindowRenderSize(nativeWindow);

var graphicsDevice = CreateGraphicsDevice(nativeUIService, nativeWindow);
var imGuiBackend = new ImGuiBackend(graphicsDevice, graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, renderSize.Width, renderSize.Height, renderSize.UIScale);

var textureRenderer = new TextureRenderer(graphicsDevice, graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, renderSize.Width, renderSize.Height);
var textureData = new uint[renderSize.Width * renderSize.Height].AsSpan();
var textureId = imGuiBackend.RegisterTexture(textureRenderer.TextureView);

Console.WriteLine($"Native Window Size: {renderSize}");
Console.WriteLine($"FrameBuffer Size: {graphicsDevice.MainSwapchain.Framebuffer.Width}x{graphicsDevice.MainSwapchain.Framebuffer.Height}");

var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
var frameCount = 0;
var stopwatch = new Stopwatch();

var currentWidth = renderSize.Width;
var currentHeight = renderSize.Height;

var currentViewportWidth = 0;
var currentViewportHeight = 0;

var appStatus = new NativeApplicationStatus();
var inputState = new NativeInputState();

while (appStatus.IsRunning == 1)
{
    appStatus = nativeApplicationService.ProcessSystemMessages(nativeApplication);
    nativeInputService.UpdateInputState(nativeApplication, ref inputState);
    renderSize = nativeUIService.GetWindowRenderSize(nativeWindow);

    if (currentWidth != renderSize.Width || currentHeight != renderSize.Height)
    {
        graphicsDevice.MainSwapchain.Resize((uint)renderSize.Width, (uint)renderSize.Height);
        
        imGuiBackend.Resize(renderSize.Width, renderSize.Height, renderSize.UIScale);
       
        Console.WriteLine($"Resize Native Window Size: {renderSize}");
        Console.WriteLine($"Resize FrameBuffer Size: {graphicsDevice.MainSwapchain.Framebuffer.Width}x{graphicsDevice.MainSwapchain.Framebuffer.Height}");

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
    nativeUIService.SetWindowTitle(nativeWindow, $"Delta: {stopwatch.ElapsedMilliseconds}");

    ImGui.Begin("PathTracer");

    var dockId = ImGui.GetID("PathTracerDock");
    ImGui.DockSpace(dockId, Vector2.Zero);

    ImGui.ShowDemoWindow();

    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
    ImGui.Begin("Viewport");

    var size = ImGui.GetContentRegionAvail();
    var viewportWidth = (int)size.X;
    var viewportHeight = (int)size.Y;

    ImGui.Image(textureId, new Vector2(viewportWidth, viewportHeight));
    ImGui.PopStyleVar();
    ImGui.End();

    ImGui.End();

    if (currentViewportWidth != viewportWidth || currentViewportHeight != viewportHeight)
    {
        var textureWidth = (int)(viewportWidth * renderSize.UIScale);
        var textureHeight = (int)(viewportHeight * renderSize.UIScale);

        textureRenderer.Resize(textureWidth, textureHeight);
        imGuiBackend.UpdateTexture(textureId, textureRenderer.TextureView);
        
        textureData = new uint[textureWidth * textureHeight].AsSpan();

        Console.WriteLine($"Resize Viewport: {textureWidth}x{textureHeight}");

        currentViewportWidth = viewportWidth;
        currentViewportHeight = viewportHeight;
    }

    commandList.Begin();
    commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);
    commandList.ClearColorTarget(0, RgbaFloat.Black);

    textureRenderer.UpdateTexture<uint>(commandList, textureData);
    imGuiBackend.Render(commandList);

    commandList.End();

    graphicsDevice.SubmitCommands(commandList);
    graphicsDevice.SwapBuffers(graphicsDevice.MainSwapchain);

    frameCount++;
}

static unsafe GraphicsDevice CreateGraphicsDevice(INativeUIService nativeUIService, NativeWindow nativeWindow)
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

    GraphicsDevice? graphicsDevice = null;

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

        graphicsDevice = GraphicsDevice.CreateVulkan(graphicsDeviceOptions, swapchainDescription);
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

        graphicsDevice = GraphicsDevice.CreateMetal(graphicsDeviceOptions, swapchainDescription);
        graphicsDevice.MainSwapchain.Resize((uint)renderSize.Width, (uint)renderSize.Height);
    }

    if (graphicsDevice == null)
    {
        throw new InvalidOperationException("Create Graphics device: Unsupported OS");
    }

    return graphicsDevice;
}