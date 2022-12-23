using Microsoft.Extensions.DependencyInjection;
using PathTracer.UI.ImGuiProvider;

namespace PathTracer.UI;

public static class ServiceExtensions
{
    public static void UseUI(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUIService, ImGuiUIService>();
        serviceCollection.AddSingleton<ICommandManager, CommandManager>();
    }
}