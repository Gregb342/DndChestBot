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
        _client.InteractionCreated += HandleInteraction;

        await _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        _client.Ready += async () =>
        {
            if (_devGuildId.HasValue)
                await _interactions.RegisterCommandsToGuildAsync(_devGuildId.Value);
            else
                await _interactions.RegisterCommandsGloballyAsync();
        };
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactions.ExecuteCommandAsync(ctx, _services);
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }
}
