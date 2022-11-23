
using Veldrid.Sdl2;

Console.WriteLine("Hello, World!");


Sdl2Native.SDL_Init(SDLInitFlags.Video);

var nativeWindow = new Sdl2Window("Path Tracer IMGui", 100, 100, 1280, 720, SDL_WindowFlags.Resizable | SDL_WindowFlags.Shown, false);

while (nativeWindow.Exists)
{
    var snapshot = nativeWindow.PumpEvents();
    if (!nativeWindow.Exists) { break; }
}