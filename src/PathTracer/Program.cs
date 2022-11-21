using Microsoft.Extensions.DependencyInjection;
using PathTracer;
using PathTracer.Platform;

var serviceCollection = new ServiceCollection();
serviceCollection.UsePathTracerPlatform();

// TODO: Use builder pattern
serviceCollection.AddScoped<IImageWriter<PlatformImage>, PlatformImageWriter>();
serviceCollection.AddScoped<IRenderer<PlatformImage>, Renderer<PlatformImage>>();
serviceCollection.AddScoped<PathTracerApplication>();

var serviceProvider = serviceCollection.BuildServiceProvider();

await serviceProvider.GetRequiredService<PathTracerApplication>()
                     .RunAsync();