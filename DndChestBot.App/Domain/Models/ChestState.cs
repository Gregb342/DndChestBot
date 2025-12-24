namespace DndChestBot.App.Domain.Models;

public sealed class ChestState
{
    public ulong GuildId { get; set; }

    public int GoldPieces { get; set; }

    public List<ChestItem> Items { get; set; } = new();

    public List<ChestLedgerEntry> Ledger { get; set; } = new();
}
