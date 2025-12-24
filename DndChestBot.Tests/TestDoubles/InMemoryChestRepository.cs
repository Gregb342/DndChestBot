using System.Collections.Concurrent;
using DndChestBot.App.Domain.Models;
using DndChestBot.App.Infrastructure.Repositories;

namespace DndChestBot.Tests.TestDoubles;

/// <summary>
/// Repository en mémoire pour tests unitaires (pas de JSON, pas de lock disque).
/// </summary>
public sealed class InMemoryChestRepository : IChestRepository
{
    private readonly ConcurrentDictionary<ulong, ChestState> _store = new();

    public ChestState LoadOrCreate(ulong guildId)
    {
        // IMPORTANT: On renvoie la même instance (comme un state "chargé").
        // Les tests simulent un process unique.
        return _store.GetOrAdd(guildId, id => new ChestState { GuildId = id });
    }

    public void Save(ChestState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        _store[state.GuildId] = state;
    }
}
