using Microsoft.Extensions.DependencyInjection;
using PathTracer.UI.ImGuiProvider;

namespace PathTracer.UI;

public static class ServiceExtensions
{
    public static void UseImGui(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUIService, ImGuiUIService>();
    }
}