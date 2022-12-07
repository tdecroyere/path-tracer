using System.Numerics;
using PathTracer.Platform.Inputs;

namespace PathTracer.UI;

public interface IUIService
{
    void Resize(int width, int height, float uiScale);
    void Update(float deltaTime, InputState inputState);
    void Render();

    void BeginPanel(string title, PanelStyles styles = PanelStyles.None);
    void EndPanel();
    Vector2 GetPanelAvailableSize();

    void Text(string text);
}