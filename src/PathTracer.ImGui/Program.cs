using System.Diagnostics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using PathTracer;
using PathTracer.Platform;
using PathTracer.Platform.NativeUI;
using Veldrid;
using Veldrid.Sdl2;

var serviceCollection = new ServiceCollection();
serviceCollection.UsePathTracerPlatform();

var serviceProvider = serviceCollection.BuildServiceProvider();

var nativeApplicationService = serviceProvider.GetRequiredService<INativeApplicationService>();
var nativeUIService = serviceProvider.GetRequiredService<INativeUIService>();

var nativeApplication = nativeApplicationService.CreateApplication("PathTracer IMGui");
var nativeWindow2 = nativeUIService.CreateWindow(nativeApplication, "Path Tracer IMGui", 1280, 720, NativeWindowState.Normal);

Sdl2Native.SDL_Init(SDLInitFlags.Video);

var nativeWindow = new Sdl2Window("Path Tracer IMGui", 100, 100, 1280, 720, SDL_WindowFlags.Resizable | SDL_WindowFlags.Shown | SDL_WindowFlags.AllowHighDpi, false);

var graphicsDevice = CreateGraphicsDevice(nativeWindow);
var imGuiController = new ImGuiController(graphicsDevice, graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, nativeWindow.Width, nativeWindow.Height);

var cpuTexture = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)nativeWindow.Width, (uint)nativeWindow.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging, TextureType.Texture2D));
var texture = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)nativeWindow.Width, (uint)nativeWindow.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
var textureView = graphicsDevice.ResourceFactory.CreateTextureView(texture);
var textureData = new byte[nativeWindow.Width * nativeWindow.Height * 4].AsSpan();

for (var i = 0; i < textureData.Length; i++)
{
    textureData[i] = 255;
}

Console.WriteLine($"Native Window Size: {nativeWindow.Width}x{nativeWindow.Height}");
Console.WriteLine($"FrameBuffer Size: {graphicsDevice.MainSwapchain.Framebuffer.Width}x{graphicsDevice.MainSwapchain.Framebuffer.Height}");

var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
var frameCount = 0;
var random = new Random();
var stopwatch = new Stopwatch();

var currentWidth = nativeWindow.Width;
var currentHeight = nativeWindow.Height;

while (nativeWindow.Exists)
{
    var snapshot = nativeWindow.PumpEvents();
    if (!nativeWindow.Exists) { break; }

    if (currentWidth != nativeWindow.Width || currentHeight != nativeWindow.Height)
    {
        graphicsDevice.MainSwapchain.Resize((uint)nativeWindow.Width, (uint)nativeWindow.Height);
        imGuiController.WindowResized(nativeWindow.Width, nativeWindow.Height);

        graphicsDevice.DisposeWhenIdle(cpuTexture);
        graphicsDevice.DisposeWhenIdle(texture);
        graphicsDevice.DisposeWhenIdle(textureView);

        textureData = new byte[nativeWindow.Width * nativeWindow.Height * 4].AsSpan();
        cpuTexture = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)nativeWindow.Width, (uint)nativeWindow.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging, TextureType.Texture2D));
        texture = graphicsDevice.ResourceFactory.CreateTexture(new TextureDescription((uint)nativeWindow.Width, (uint)nativeWindow.Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
        textureView = graphicsDevice.ResourceFactory.CreateTextureView(texture);
        imGuiController.ResetSurfaceTexture();

        Console.WriteLine($"Resize Native Window Size: {nativeWindow.Width}x{nativeWindow.Height}");
        Console.WriteLine($"Resize FrameBuffer Size: {graphicsDevice.MainSwapchain.Framebuffer.Width}x{graphicsDevice.MainSwapchain.Framebuffer.Height}");

        currentWidth = nativeWindow.Width;
        currentHeight = nativeWindow.Height;
    }

    imGuiController.Update(1.0f / 60.0f, snapshot);

    stopwatch.Restart();
    for (var i = 0; i < textureData.Length; i+=4)
    {
        random.NextBytes(textureData.Slice(i, 3));

        textureData[i + 3] = 255;
    }
    stopwatch.Stop();
    Console.WriteLine($"Delta: {stopwatch.ElapsedMilliseconds}");

    graphicsDevice.UpdateTexture(cpuTexture, textureData, 0, 0, 0, cpuTexture.Width, cpuTexture.Height, 1, 0, 0);
    ImGui.ShowDemoWindow();

    ImGui.Text("Teeeest");

    commandList.Begin();
    commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);
    commandList.ClearColorTarget(0, new RgbaFloat(1, 1, 0, 1));

    commandList.CopyTexture(cpuTexture, texture);

    imGuiController.RenderTexture(graphicsDevice, commandList, textureView);
    imGuiController.Render(graphicsDevice, commandList);

    commandList.End();

    graphicsDevice.SubmitCommands(commandList);
    graphicsDevice.SwapBuffers(graphicsDevice.MainSwapchain);

    frameCount++;
}

static unsafe GraphicsDevice CreateGraphicsDevice(Sdl2Window nativeWindow)
{
    var graphicsDeviceOptions = new GraphicsDeviceOptions(
            debug: true, 
            swapchainDepthFormat: null, 
            syncToVerticalBlank: true, 
            ResourceBindingModel.Improved, 
            preferDepthRangeZeroToOne: true, 
            preferStandardClipSpaceYDirection: true);

    SDL_SysWMinfo sysWmInfo;
    Sdl2Native.SDL_GetVersion(&sysWmInfo.version);
    Sdl2Native.SDL_GetWMWindowInfo(nativeWindow.SdlWindowHandle, &sysWmInfo);

    GraphicsDevice? graphicsDevice = null;

    if (OperatingSystem.IsWindows())
    {
        Win32WindowInfo w32Info = Unsafe.Read<Win32WindowInfo>(&sysWmInfo.info);
        var swapchainSource = SwapchainSource.CreateWin32(w32Info.Sdl2Window, w32Info.hinstance);

        var swapchainDescription = new SwapchainDescription(
                        swapchainSource,
                        (uint)nativeWindow.Width,
                        (uint)nativeWindow.Height,
                        graphicsDeviceOptions.SwapchainDepthFormat,
                        graphicsDeviceOptions.SyncToVerticalBlank,
                        graphicsDeviceOptions.SwapchainSrgbFormat);

        graphicsDevice = GraphicsDevice.CreateVulkan(graphicsDeviceOptions, swapchainDescription);
    }

    else if (OperatingSystem.IsMacOS())
    {
        CocoaWindowInfo cocoaInfo = Unsafe.Read<CocoaWindowInfo>(&sysWmInfo.info);
        IntPtr nsWindow = cocoaInfo.Window;
        var swapchainSource = SwapchainSource.CreateNSWindow(nsWindow);

        var swapchainDescription = new SwapchainDescription(
                        swapchainSource,
                        (uint)nativeWindow.Width,
                        (uint)nativeWindow.Height,
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