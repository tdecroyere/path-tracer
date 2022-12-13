using System.Numerics;
using PathTracer.Platform;
using PathTracer.Platform.GraphicsLegacy;
using PathTracer.Platform.Inputs;

namespace PathTracer.UI;

// TODO: Dissociate the rendering of some panels so we can use a swapchain into it
// for native rendering
public interface IUIService
{
    void Init(NativeWindow window, GraphicsDevice graphicsDevice);
    void Resize(NativeWindowSize windowSize);
    void Update(float deltaTime, InputState inputState);
    void Render();

    // TODO: Do we need that?
    nint RegisterTexture(Texture texture);
    void UpdateTexture(nint id, Texture texture);

    bool BeginPanel(string title, PanelStyles styles = PanelStyles.None);
    void EndPanel();
    Vector2 GetPanelAvailableSize();

    bool CollapsingHeader(string text, bool isVisibleByDefault = true);
    void Text(string text);
    void NewLine();
    void Image(nint textureId, int width, int height);
    bool Button(string text, ControlStyles styles = ControlStyles.None);
    bool InputText(string label, ref string text, int maxLength = 255);

    bool BeginCombo(string label, string previewValue);
    void EndCombo();
    bool Selectable(string text, bool isSelected = false);
}