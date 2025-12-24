namespace DndChestBot.App.Domain.Models;

public sealed class ChestItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

    public ulong AddedByUserId { get; set; }

    /// <summary>
    /// Référence courte affichable (ex: A3F2) dérivée du Guid.
    /// </summary>
    public string Ref => Id.ToString("N")[..4].ToUpperInvariant();
}
