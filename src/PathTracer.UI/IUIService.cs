using System.Numerics;
using PathTracer.Platform;
using PathTracer.Platform.GraphicsLegacy;
using PathTracer.Platform.Inputs;

namespace PathTracer.UI;

// TODO: Dissociate the rendering of some panels so we can use a swapchain into it
// for native rendering
public interface IUIService
{
    // TODO: Can we get rid ot the init method?
    void Init(NativeWindow window, GraphicsDevice graphicsDevice);
    void Resize(NativeWindowSize windowSize);
    void Update(float deltaTime, InputState inputState);
    void Render();

    bool BeginPanel(string title, PanelStyles styles = PanelStyles.None);
    void EndPanel();
    Vector2 GetPanelAvailableSize();

    void PushId(string id);
    void PopId();

    bool CollapsingHeader(string text, bool isVisibleByDefault = true);
    void Text(string text);
    void NewLine();
    void Separator();
    void Image(Texture texture, int width, int height);
    bool Button(string text, ControlStyles styles = ControlStyles.None);

    bool DragFloat3(string label, ref Vector3 value, float increment = 0.01f);
    bool DragFloat(string label, ref float value, float increment = 0.01f);
    bool ColorEdit3(string label, ref Vector3 value);
    bool InputText(string label, ref string text, int maxLength = 255);

    bool BeginCombo(string label, string previewValue);
    void EndCombo();
    bool Selectable(string text, bool isSelected = false);
}