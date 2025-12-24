using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DndChestBot.App.Discord;
using DndChestBot.App.Infrastructure.Repositories;
using DndChestBot.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace DndChestBot.App;

public static class Program
{
    public static async Task Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var discordToken = configuration["Discord:Token"];
        var devGuildIdRaw = configuration["Discord:DevGuildId"];

        if (string.IsNullOrWhiteSpace(discordToken))
        {
            Console.WriteLine("❌ Discord:Token manquant dans la configuration.");
            return;
        }

        ulong? devGuildId = null;
        if (ulong.TryParse(devGuildIdRaw, out var parsed))
            devGuildId = parsed;

        var services = ConfigureServices(configuration);

        var client = services.GetRequiredService<DiscordSocketClient>();
        var interactions = services.GetRequiredService<InteractionService>();
        var handler = services.GetRequiredService<InteractionHandler>();

        client.Log += msg => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };
        interactions.Log += msg => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };

        await handler.InitializeAsync();

        await client.LoginAsync(TokenType.Bot, discordToken);
        await client.StartAsync();

        Console.WriteLine("✅ Bot démarré. CTRL+C pour quitter.");
        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices(IConfiguration configuration)
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
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(new DiscordSocketClient(socketConfig))
            .AddSingleton(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>(), interactionConfig))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<IChestRepository>(_ => new JsonChestRepository(Path.Combine(AppContext.BaseDirectory, "data")))
            .AddSingleton<ChestService>()
            .BuildServiceProvider();

        return services;
    }
}
