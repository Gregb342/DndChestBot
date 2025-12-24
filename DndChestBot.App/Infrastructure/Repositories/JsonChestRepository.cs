using System.Text.Json;
using DndChestBot.App.Domain.Models;

namespace DndChestBot.App.Infrastructure.Repositories;

public sealed class JsonChestRepository : IChestRepository
{
    private readonly string _dataDir;
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonChestRepository(string dataDir)
    {
        _dataDir = dataDir ?? throw new ArgumentNullException(nameof(dataDir));
        Directory.CreateDirectory(_dataDir);
    }

    public ChestState LoadOrCreate(ulong guildId)
    {
        lock (_lock)
        {
            var path = GetPath(guildId);
            if (!File.Exists(path))
            {
                var created = new ChestState { GuildId = guildId, GoldPieces = 0 };
                SaveInternal(path, created);
                return created;
            }

            var json = File.ReadAllText(path);
            var state = JsonSerializer.Deserialize<ChestState>(json, _jsonOptions)
                        ?? new ChestState { GuildId = guildId };

            // sécurité : au cas où guildId n'était pas persisté correctement
            state.GuildId = guildId;

            return state;
        }
    }

    public void Save(ChestState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        lock (_lock)
        {
            var path = GetPath(state.GuildId);
            SaveInternal(path, state);
        }
    }

    private void SaveInternal(string path, ChestState state)
    {
        var tmp = path + ".tmp";
        var json = JsonSerializer.Serialize(state, _jsonOptions);

        File.WriteAllText(tmp, json);
        File.Copy(tmp, path, overwrite: true);
        File.Delete(tmp);
    }

    private string GetPath(ulong guildId)
        => Path.Combine(_dataDir, $"guild-{guildId}.json");
}
