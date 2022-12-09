﻿using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
serviceCollection.UseNativePlatform();
serviceCollection.UseGraphicsPlatform();
//serviceCollection.UseImGui();

// TODO: Use builder pattern
serviceCollection.AddScoped<IImageWriter<TextureImage>, TextureImageWriter>();
serviceCollection.AddScoped<IRenderer<TextureImage>, Renderer<TextureImage>>();
serviceCollection.AddScoped<IImageWriter<PpmImage>, PpmImageWriter>();
serviceCollection.AddScoped<IRenderer<PpmImage>, Renderer<PpmImage>>();
serviceCollection.AddScoped<PathTracerApplication>();

var serviceProvider = serviceCollection.BuildServiceProvider();

serviceProvider.GetRequiredService<PathTracerApplication>()
               .Run();