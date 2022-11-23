// See https://aka.ms/new-console-template for more information
using OpenTK.Graphics.OpenGL;
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

// Scaling seems to work automatically on MacOSX
Console.WriteLine(nativeWindow.ClientSize);
//nativeWindow.TryGetCurrentMonitorScale(out var horizontalScale, out var verticalScale);
//nativeWindow.Size = new Vector2i((int)(nativeWindow.Size.X * horizontalScale), (int)(nativeWindow.Size.Y * verticalScale));

nativeWindow.Context.MakeCurrent();
nativeWindow.VSync = VSyncMode.On;

var imageData = new byte[nativeWindow.ClientSize.X * nativeWindow.ClientSize.Y * 4];

for (var i = 0; i < imageData.Length; i++)
{
    imageData[i] = 255;
}

Run(nativeWindow, imageData);

static unsafe void Run(NativeWindow nativeWindow, ReadOnlySpan<byte> imageData)
{
    var textureId = GL.GenTexture();

    fixed (byte* p = imageData)
    {
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, nativeWindow.Size.X, nativeWindow.Size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (nint)p);
    }

    var windowPtr = nativeWindow.WindowPtr;

    while (!GLFW.WindowShouldClose(windowPtr))
    {
        nativeWindow.ProcessEvents();

        GLFW.GetFramebufferSize(windowPtr, out int displayW, out int displayH);
        GL.Viewport(0, 0, displayW, displayH);
        GL.ClearColor(1, 1, 0, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(0, nativeWindow.Size.X, 0, nativeWindow.Size.Y, -1, 1);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();
        GL.Disable(EnableCap.Lighting);

        GL.Color4(1, 1, 1, 1);
        //GL.Enable(EnableCap.Texture2D);
        //GL.BindTexture(TextureTarget.Texture2D, textureId);
        //GL.TexSubImage2D()

        //GL.Translate(0, 0, -0.1f);

        GL.Begin(PrimitiveType.TriangleStrip);

        GL.Vertex2(0, 0);
        GL.Vertex2(0, 1);
        GL.Vertex2(1, 1);
        
        /*GL.TexCoord2(0, 0); 
        GL.Vertex2(0, 0);

        GL.TexCoord2(0, 1); 
        GL.Vertex2(0, 100);

        GL.TexCoord2(1, 1); 
        GL.Vertex2(100, 100);

        GL.TexCoord2(1, 0); 
        GL.Vertex2(100, 0);*/

        GL.End();

        nativeWindow.Context.SwapBuffers();
    }
}
