using System.Diagnostics;
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
var imGuiController = new ImGuiBackend(graphicsDevice, graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, renderSize.Width, renderSize.Height, renderSize.UIScale);

var cpuTexture = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)renderSize.Width, (uint)renderSize.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging, TextureType.Texture2D));
var texture = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)renderSize.Width, (uint)renderSize.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
var textureView = graphicsDevice.ResourceFactory.CreateTextureView(texture);
var textureData = new uint[renderSize.Width * renderSize.Height].AsSpan();

for (var i = 0; i < textureData.Length; i++)
{
    textureData[i] = 255;
}

Console.WriteLine($"Native Window Size: {renderSize}");
Console.WriteLine($"FrameBuffer Size: {graphicsDevice.MainSwapchain.Framebuffer.Width}x{graphicsDevice.MainSwapchain.Framebuffer.Height}");

var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
var frameCount = 0;
var stopwatch = new Stopwatch();

var currentWidth = renderSize.Width;
var currentHeight = renderSize.Height;

var appStatus = new NativeApplicationStatus();
var inputState = new NativeInputState();

while (appStatus.IsRunning == 1)
{
    appStatus = nativeApplicationService.ProcessSystemMessages(nativeApplication);

    renderSize = nativeUIService.GetWindowRenderSize(nativeWindow);

    if (currentWidth != renderSize.Width || currentHeight != renderSize.Height)
    {
        graphicsDevice.MainSwapchain.Resize((uint)renderSize.Width, (uint)renderSize.Height);
        imGuiController.WindowResized(renderSize.Width, renderSize.Height, renderSize.UIScale);

        graphicsDevice.DisposeWhenIdle(cpuTexture);
        graphicsDevice.DisposeWhenIdle(texture);
        graphicsDevice.DisposeWhenIdle(textureView);

        textureData = new uint[renderSize.Width * renderSize.Height].AsSpan();
        cpuTexture = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)renderSize.Width, (uint)renderSize.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging, TextureType.Texture2D));
        texture = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)renderSize.Width, (uint)renderSize.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
        textureView = graphicsDevice.ResourceFactory.CreateTextureView(texture);
        imGuiController.ResetSurfaceTexture();

        Console.WriteLine($"Resize Native Window Size: {renderSize}");
        Console.WriteLine($"Resize FrameBuffer Size: {graphicsDevice.MainSwapchain.Framebuffer.Width}x{graphicsDevice.MainSwapchain.Framebuffer.Height}");

        currentWidth = renderSize.Width;
        currentHeight = renderSize.Height;
    }

    imGuiController.Update(1.0f / 60.0f, inputState);

    stopwatch.Restart();
    for (var i = 0; i < textureData.Length; i++)
    {
        var color = (byte)(frameCount % 255);
        textureData[i] = (uint) (255 << 24 | color << 16 | color << 8 | color);
    }

    stopwatch.Stop();
    nativeUIService.SetWindowTitle(nativeWindow, $"Delta: {stopwatch.ElapsedMilliseconds}");

    graphicsDevice.UpdateTexture(cpuTexture, textureData, 0, 0, 0, cpuTexture.Width, cpuTexture.Height, 1, 0, 0);
    ImGui.ShowDemoWindow();

    ImGui.Text("Teeeest");

    commandList.Begin();
    commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);

    commandList.CopyTexture(cpuTexture, texture);

    imGuiController.RenderTexture(graphicsDevice, commandList, textureView);
    imGuiController.Render(graphicsDevice, commandList);

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
    }

    if (graphicsDevice == null)
    {
        throw new InvalidOperationException("Create Graphics device: Unsupported OS");
    }

    return graphicsDevice;
}