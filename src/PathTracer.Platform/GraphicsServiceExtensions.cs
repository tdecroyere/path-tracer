using Microsoft.Extensions.DependencyInjection;
using PathTracer.Platform.Graphics;
using PathTracer.Platform.Platforms.VeldridLibrary;

namespace PathTracer.Platform;

public static class GraphicsServiceExtensions
{
    // TODO: To remove when we switch to full native
    public static void UseGraphicsPlatform(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IGraphicsService, VeldridGraphicsService>();
    }
}