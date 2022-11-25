using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using PathTracer.Platform.Inputs;
using Veldrid;

namespace PathTracer;

public class ImGuiBackend : IDisposable
{
    private readonly ImGuiRenderer _imGuiRenderer;

    private bool _frameBegun;
    private int _windowWidth;
    private int _windowHeight;
    private Vector2 _scaleFactor;

    public ImGuiBackend(GraphicsDevice graphicsDevice, OutputDescription outputDescription, int width, int height, float uiScale)
    {
        _windowWidth = width;
        _windowHeight = height;
        _scaleFactor = new Vector2(uiScale, uiScale);

        var context = ImGui.CreateContext();

        ImGui.SetCurrentContext(context);
        ImGui.GetIO().Fonts.AddFontDefault();
        ImGui.GetIO().BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DpiEnableScaleFonts | ImGuiConfigFlags.DpiEnableScaleViewports | ImGuiConfigFlags.DockingEnable;

        //SetKeyMappings();

        SetPerFrameImGuiData(1.0f / 60f);

        var font = ImGui.GetIO().Fonts;
        _imGuiRenderer = new ImGuiRenderer(graphicsDevice, outputDescription, ref font);

        ImGui.NewFrame();
        _frameBegun = true;
    }
    
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _imGuiRenderer.Dispose();
        }
    }

    public void Resize(int width, int height, float uiScale)
    {
        _windowWidth = width;
        _windowHeight = height;
        _scaleFactor = new Vector2(uiScale, uiScale);
    }
    
    public void Render(CommandList commandList)
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();

            var drawData = ImGui.GetDrawData();
            _imGuiRenderer.RenderImDrawData(commandList, ref drawData);
        }
    }

    /// <summary>
    /// Updates ImGui input and IO configuration state.
    /// </summary>
    public void Update(float deltaTime, NativeInputState inputState)
    {
        if (_frameBegun)
        {
            ImGui.Render();
        }

        SetPerFrameImGuiData(deltaTime);
        UpdateImGuiInput(inputState);

        _frameBegun = true;
        ImGui.NewFrame();
    }

    /// <summary>
    /// Sets per-frame data based on the associated window.
    /// This is called by Update(float).
    /// </summary>
    private void SetPerFrameImGuiData(float deltaTime)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y);
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaTime;
    }

    private void ProcessKey(ImGuiIOPtr io, NativeInputObject inputObject, ImGuiKey key, char character)
    {
        io.KeysDown[(int)key] = inputObject.IsPressed;

        if (inputObject.HasRepeatChanged)
        {
            io.AddInputCharacter(character);
        }
    }

    private void UpdateImGuiInput(NativeInputState inputState)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.MouseDown[0] = inputState.Mouse.MouseLeftButton.IsPressed;
        //io.MouseDown[1] = rightPressed || snapshot.IsMouseDown(MouseButton.Right);
        //io.MouseDown[2] = middlePressed || snapshot.IsMouseDown(MouseButton.Middle);
        io.MousePos = new Vector2(inputState.Mouse.AxisX.Value, inputState.Mouse.AxisY.Value) / _scaleFactor;

        ProcessKey(io, inputState.Keyboard.KeyA, ImGuiKey.A, 'a');
        ProcessKey(io, inputState.Keyboard.KeyB, ImGuiKey.B, 'b');
        ProcessKey(io, inputState.Keyboard.KeyC, ImGuiKey.C, 'c');
        ProcessKey(io, inputState.Keyboard.KeyD, ImGuiKey.D, 'd');
        ProcessKey(io, inputState.Keyboard.KeyE, ImGuiKey.E, 'e');
        ProcessKey(io, inputState.Keyboard.KeyF, ImGuiKey.F, 'f');
        ProcessKey(io, inputState.Keyboard.KeyG, ImGuiKey.G, 'g');
        ProcessKey(io, inputState.Keyboard.KeyH, ImGuiKey.H, 'h');
        ProcessKey(io, inputState.Keyboard.KeyI, ImGuiKey.I, 'i');
        ProcessKey(io, inputState.Keyboard.KeyJ, ImGuiKey.J, 'j');
        ProcessKey(io, inputState.Keyboard.KeyK, ImGuiKey.K, 'k');
        ProcessKey(io, inputState.Keyboard.KeyL, ImGuiKey.L, 'l');
        ProcessKey(io, inputState.Keyboard.KeyM, ImGuiKey.M, 'm');
        ProcessKey(io, inputState.Keyboard.KeyN, ImGuiKey.N, 'n');
        ProcessKey(io, inputState.Keyboard.KeyO, ImGuiKey.O, 'o');
        ProcessKey(io, inputState.Keyboard.KeyP, ImGuiKey.P, 'p');
        ProcessKey(io, inputState.Keyboard.KeyQ, ImGuiKey.Q, 'q');
        ProcessKey(io, inputState.Keyboard.KeyR, ImGuiKey.R, 'r');
        ProcessKey(io, inputState.Keyboard.KeyS, ImGuiKey.S, 's');
        ProcessKey(io, inputState.Keyboard.KeyT, ImGuiKey.T, 't');
        ProcessKey(io, inputState.Keyboard.KeyU, ImGuiKey.U, 'u');
        ProcessKey(io, inputState.Keyboard.KeyV, ImGuiKey.V, 'v');
        ProcessKey(io, inputState.Keyboard.KeyW, ImGuiKey.W, 'w');
        ProcessKey(io, inputState.Keyboard.KeyX, ImGuiKey.X, 'x');
        ProcessKey(io, inputState.Keyboard.KeyY, ImGuiKey.Y, 'y');
        ProcessKey(io, inputState.Keyboard.KeyZ, ImGuiKey.Z, 'z');

        /*io.MouseWheel = snapshot.WheelDelta;

        IReadOnlyList<char> keyCharPresses = snapshot.KeyCharPresses;
        for (int i = 0; i < keyCharPresses.Count; i++)
        {
            char c = keyCharPresses[i];
            io.AddInputCharacter(c);
        }

        IReadOnlyList<KeyEvent> keyEvents = snapshot.KeyEvents;
        for (int i = 0; i < keyEvents.Count; i++)
        {
            KeyEvent keyEvent = keyEvents[i];
            io.KeysDown[(int)keyEvent.Key] = keyEvent.Down;
            if (keyEvent.Key == Key.ControlLeft)
            {
                _controlDown = keyEvent.Down;
            }
            if (keyEvent.Key == Key.ShiftLeft)
            {
                _shiftDown = keyEvent.Down;
            }
            if (keyEvent.Key == Key.AltLeft)
            {
                _altDown = keyEvent.Down;
            }
            if (keyEvent.Key == Key.WinLeft)
            {
                _winKeyDown = keyEvent.Down;
            }
        }

        io.KeyCtrl = _controlDown;
        io.KeyAlt = _altDown;
        io.KeyShift = _shiftDown;
        io.KeySuper = _winKeyDown;*/
    }
/*
    private static void SetKeyMappings()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
        io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
        io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
        io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
        io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
        io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
        io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
        io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
        io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
        io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
        io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.BackSpace;
        io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
        io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
        io.KeyMap[(int)ImGuiKey.Space] = (int)Key.Space;
        io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
        io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
        io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
        io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
        io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
        io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
    }*/
}