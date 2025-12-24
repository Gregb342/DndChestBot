namespace DndChestBot.App.Domain.Models;

public enum ChestLedgerEntryType
{
    GoldDeposit = 1,
    GoldWithdraw = 2,
    ItemAdded = 3,
    ItemRemoved = 4,
    AdminSet = 5
}
