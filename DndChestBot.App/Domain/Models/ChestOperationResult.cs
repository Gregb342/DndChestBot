namespace DndChestBot.App.Domain.Models;

public sealed record ChestOperationResult(
    bool Success,
    string Message,
    int GoldPieces,
    int ItemCount);
