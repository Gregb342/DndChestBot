namespace DndChestBot.App.Domain.Models;

public sealed class ChestLedgerEntry
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public ChestLedgerEntryType EntryType { get; set; }

    public string CharacterName { get; set; } = string.Empty;

    public ulong UserId { get; set; }

    public ulong ChannelId { get; set; }

    // Gold
    public int? AmountGold { get; set; }
    public int? GoldBalanceAfter { get; set; }

    // Item
    public Guid? ItemId { get; set; }
    public string? ItemNameSnapshot { get; set; }
    public int? ItemQuantity { get; set; }
    public string? NotesSnapshot { get; set; }
}
