using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.Sdl2;

Console.WriteLine("Hello, World!");
Run();

static unsafe void Run()
{
    Sdl2Native.SDL_Init(SDLInitFlags.Video);

    var nativeWindow = new Sdl2Window("Path Tracer IMGui", 100, 100, 1280, 720, SDL_WindowFlags.Resizable | SDL_WindowFlags.Shown, false);

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

    if (graphicsDevice == null)
    {
        Console.WriteLine("ERROR: Unsupported OS!");
        return;
    }

    while (nativeWindow.Exists)
    {
        var snapshot = nativeWindow.PumpEvents();
        if (!nativeWindow.Exists) { break; }

        graphicsDevice.SwapBuffers(graphicsDevice.MainSwapchain);
    }
}