// See https://aka.ms/new-console-template for more information
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

Console.WriteLine("Hello, World!");

var glslVersion = "#version 150";

var nativeWindowSettings = new NativeWindowSettings()
{
    Size = new(1280, 720),
    Title = "Path Tracer ImGUI",
    APIVersion = new(3, 2),
    Profile = ContextProfile.Core,
    Flags = ContextFlags.ForwardCompatible
};

var nativeWindow = new NativeWindow(nativeWindowSettings);
nativeWindow.TryGetCurrentMonitorScale(out var horizontalScale, out var verticalScale);

nativeWindow.Size = new Vector2i((int)(nativeWindow.Size.X * horizontalScale), (int)(nativeWindow.Size.Y * verticalScale));
nativeWindow.Context.MakeCurrent();
nativeWindow.VSync = VSyncMode.On;

Run(nativeWindow);

static unsafe void Run(NativeWindow nativeWindow)
{
    var windowPtr = nativeWindow.WindowPtr;

    while (!GLFW.WindowShouldClose(windowPtr))
    {
        nativeWindow.ProcessEvents();

        GLFW.SwapBuffers(windowPtr);
    }
}
