// See https://aka.ms/new-console-template for more information
using Mochi.DearImGui;
using Mochi.DearImGui.OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

Console.WriteLine("Hello, World!");
Run();

static unsafe void Run()
{
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

    var imageData = new byte[nativeWindow.ClientSize.X * nativeWindow.ClientSize.Y * 4];

for (var i = 0; i < imageData.Length; i++)
{
    imageData[i] = 255;
}


    nativeWindow.Context.MakeCurrent();
    nativeWindow.VSync = VSyncMode.On;

    var glslVersion = "#version 150";

    // Setup Dear ImGui context
    ImGui.CHECKVERSION();
    ImGui.CreateContext();
    ImGuiIO* io = ImGui.GetIO();
    io->DisplayFramebufferScale = new System.Numerics.Vector2(2, 2);
    io->ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard; // Enable Keyboard Controls
    //io->ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad; // Enable Gamepad Controls
    io->ConfigFlags |= ImGuiConfigFlags.DockingEnable; // Enable Docking
    //io->ConfigFlags |= ImGuiConfigFlags.ViewportsEnable; // Enable Multi-Viewport / Platform Windows

    // Setup Dear ImGui style
    ImGui.StyleColorsDark();

    // When viewports are enabled we tweak WindowRounding/WindowBg so platform windows can look identical to regular ones.
    /*ImGuiStyle* style = ImGui.GetStyle();
    if (io->ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
    {
        style->WindowRounding = 0f;
        //BIOQUIRK: We should special-case this to make it more friendly. (https://github.com/MochiLibraries/Biohazrd/issues/139 would help here)
        style->Colors[(int)ImGuiCol.WindowBg].W = 1f;
    }*/

    // Setup Platform/Renderer backends
    var platformBackend = new PlatformBackend(nativeWindow, true);
    var rendererBackend = new RendererBackend(glslVersion);

    var textureId = GL.GenTexture();

    fixed (byte* p = imageData)
    {
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, nativeWindow.Size.X, nativeWindow.Size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (nint)p);
    }

    var vertexBufferId = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);

    var vertexData = new float[]
    {
           -1.0f, -1.0f, 0.0f, 0.0f,
    -1.0f,  1.0f, 0.0f, 0.0f,
     1.0f,  1.0f, 0.0f, 0.0f,
     1.0f, -1.0f, 0.0f, 0.0f,
    -1.0f, -1.0f, 0.0f, 0.0f,
    };

    fixed (float* p = vertexData)
    {
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertexData.Length, (nint)p, BufferUsageHint.StaticDraw);
    }

    var windowPtr = nativeWindow.WindowPtr;

    while (!GLFW.WindowShouldClose(windowPtr))
    {
        nativeWindow.ProcessEvents();

        // Start the Dear ImGui frame
        rendererBackend.NewFrame();
        platformBackend.NewFrame();
        ImGui.NewFrame();

        ImGui.ShowDemoWindow();

        ImGui.Render();

        GLFW.GetFramebufferSize(windowPtr, out int displayW, out int displayH);
        GL.Viewport(0, 0, displayW, displayH);
        GL.ClearColor(1, 1, 0, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit);


        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 5);
        GL.DisableVertexAttribArray(0);

        rendererBackend.RenderDrawData(ImGui.GetDrawData());
        if (io->ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
        {
            Window* backupCurrentContext = GLFW.GetCurrentContext();
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
            GLFW.MakeContextCurrent(backupCurrentContext);
        }

        nativeWindow.Context.SwapBuffers();
    }
}
