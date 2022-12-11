using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
serviceCollection.UseNativePlatform();
serviceCollection.UseGraphicsPlatform();
serviceCollection.UseUI();

// TODO: Use builder pattern
serviceCollection.AddScoped<IImageWriter<TextureImage>, TextureImageWriter>();
serviceCollection.AddScoped<IRenderer<TextureImage>, Renderer<TextureImage>>();
serviceCollection.AddScoped<IImageWriter<FileImage>, FileImageWriter>();
serviceCollection.AddScoped<IRenderer<FileImage>, Renderer<FileImage>>();

serviceCollection.AddScoped<UIManager>();
serviceCollection.AddScoped<PathTracerApplication>();

var serviceProvider = serviceCollection.BuildServiceProvider();

serviceProvider.GetRequiredService<PathTracerApplication>()
               .Run();