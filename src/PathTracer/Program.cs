using Microsoft.Extensions.DependencyInjection;
using PathTracer;
using PathTracer.Platform;
using PathTracer.UI;

var serviceCollection = new ServiceCollection();
serviceCollection.UseNativePlatform();
serviceCollection.UseGraphicsPlatform();
//serviceCollection.UseImGui();

// TODO: Use builder pattern
serviceCollection.AddScoped<IImageWriter<TextureImage>, TextureImageWriter>();
serviceCollection.AddScoped<IRenderer<TextureImage>, Renderer<TextureImage>>();
serviceCollection.AddScoped<PathTracerApplication>();

var serviceProvider = serviceCollection.BuildServiceProvider();

serviceProvider.GetRequiredService<PathTracerApplication>()
               .Run();