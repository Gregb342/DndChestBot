using DndChestBot.App.Domain.Models;

namespace DndChestBot.App.Infrastructure.Repositories;

public interface IChestRepository
{
    ChestState LoadOrCreate(ulong guildId);

    void Save(ChestState state);
}
