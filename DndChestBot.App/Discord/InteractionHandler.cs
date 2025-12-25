using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace DndChestBot.App.Discord;

public sealed class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly ulong? _devGuildId;

    public InteractionHandler(
        DiscordSocketClient client, 
        InteractionService interactions, 
        IServiceProvider services,
        IConfiguration config)
    {
        _client = client;
        _interactions = interactions;
        _services = services;

        if (ulong.TryParse(config["Discord:DevGuildId"], out var gid))
            _devGuildId = gid;
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("[InteractionHandler] Initializing interaction handler...");
        
        _client.InteractionCreated += HandleInteraction;

        await _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        Console.WriteLine("[InteractionHandler] Modules added successfully");

        _client.Ready += async () =>
        {
            Console.WriteLine("[InteractionHandler] Client is ready, registering commands...");
            
            if (_devGuildId.HasValue)
            {
                Console.WriteLine($"[InteractionHandler] Registering commands to dev guild: {_devGuildId.Value}");
                await _interactions.RegisterCommandsToGuildAsync(_devGuildId.Value);
            }
            else
            {
                Console.WriteLine("[InteractionHandler] Registering commands globally");
                await _interactions.RegisterCommandsGloballyAsync();
            }
            
            Console.WriteLine("[InteractionHandler] Commands registered successfully");
        };
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        Console.WriteLine($"[InteractionHandler] Interaction received - Type: {interaction.Type}, User: {interaction.User.Username} ({interaction.User.Id})");
        
        try
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactions.ExecuteCommandAsync(ctx, _services);
            Console.WriteLine($"[InteractionHandler] Interaction executed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[InteractionHandler] ERROR executing interaction: {ex.Message}");
            
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }
}
