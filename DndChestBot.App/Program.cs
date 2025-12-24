using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DndChestBot.App.Discord;
using DndChestBot.App.Infrastructure.Repositories;
using DndChestBot.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DndChestBot.App;

public static class Program
{
    public static async Task Main()
    {
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("❌ Variable d'environnement DISCORD_TOKEN manquante.");
            return;
        }

        var services = ConfigureServices();

        var client = services.GetRequiredService<DiscordSocketClient>();
        var interactions = services.GetRequiredService<InteractionService>();
        var handler = services.GetRequiredService<InteractionHandler>();

        client.Log += msg => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };
        interactions.Log += msg => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };

        await handler.InitializeAsync();

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        Console.WriteLine("✅ Bot démarré. CTRL+C pour quitter.");
        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices()
    {
        var socketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds,
            LogGatewayIntentWarnings = false
        };

        var interactionConfig = new InteractionServiceConfig
        {
            DefaultRunMode = RunMode.Async
        };

        var services = new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(socketConfig))
            .AddSingleton(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>(), interactionConfig))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<IChestRepository>(_ => new JsonChestRepository(Path.Combine(AppContext.BaseDirectory, "data")))
            .AddSingleton<ChestService>()
            .BuildServiceProvider();

        return services;
    }
}
