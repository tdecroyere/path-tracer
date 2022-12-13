using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
serviceCollection.UseNativePlatform();
serviceCollection.UseGraphicsPlatform();
serviceCollection.UseUI();

// TODO: Use builder pattern
serviceCollection.AddScoped<IImageWriter<TextureImage, CommandList>, TextureImageWriter>();
serviceCollection.AddScoped<IRenderer<TextureImage, CommandList>, Renderer<TextureImage, CommandList>>();
serviceCollection.AddScoped<IImageWriter<FileImage, string>, FileImageWriter>();
serviceCollection.AddScoped<IRenderer<FileImage, string>, Renderer<FileImage, string>>();

serviceCollection.AddScoped<UIManager>();
serviceCollection.AddScoped<RenderManager>();
serviceCollection.AddScoped<PathTracerApplication>();

var serviceProvider = serviceCollection.BuildServiceProvider();

serviceProvider.GetRequiredService<PathTracerApplication>()
               .Run();