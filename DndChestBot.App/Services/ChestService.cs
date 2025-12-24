using DndChestBot.App.Domain.Models;
using DndChestBot.App.Infrastructure.Repositories;

namespace DndChestBot.App.Services;

public sealed class ChestService
{
    private readonly IChestRepository _repo;

    public ChestService(IChestRepository repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    public ChestOperationResult GetBalance(ulong guildId)
    {
        var state = _repo.LoadOrCreate(guildId);

        return new ChestOperationResult(
            Success: true,
            Message: $"📦 Le coffre contient actuellement **{state.GoldPieces} PO**.\n🎒 Objets : **{state.Items.Count}**",
            GoldPieces: state.GoldPieces,
            ItemCount: state.Items.Count);
    }

    public ChestOperationResult DepositGold(ulong guildId, int amount, string characterName, ulong userId, ulong channelId)
    {
        var state = _repo.LoadOrCreate(guildId);

        if (string.IsNullOrWhiteSpace(characterName))
            return Fail(state, "❌ Le paramètre `pj` est obligatoire.");

        if (amount <= 0)
            return Fail(state, "❌ Le montant doit être > 0.");

        checked { state.GoldPieces += amount; }

        state.Ledger.Add(new ChestLedgerEntry
        {
            EntryType = ChestLedgerEntryType.GoldDeposit,
            CharacterName = characterName,
            UserId = userId,
            ChannelId = channelId,
            AmountGold = amount,
            GoldBalanceAfter = state.GoldPieces
        });

        _repo.Save(state);

        var msg = $"💰 **{characterName}** dépose **{amount} PO** dans le coffre.\n" +
                  $"Total du coffre : **{state.GoldPieces} PO**";

        return new ChestOperationResult(true, msg, state.GoldPieces, state.Items.Count);
    }

    public ChestOperationResult WithdrawGold(ulong guildId, int amount, string characterName, ulong userId, ulong channelId)
    {
        var state = _repo.LoadOrCreate(guildId);

        if (string.IsNullOrWhiteSpace(characterName))
            return Fail(state, "❌ Le paramètre `pj` est obligatoire.");

        if (amount <= 0)
            return Fail(state, "❌ Le montant doit être > 0.");

        if (amount > state.GoldPieces)
        {
            var msgRefuse =
                $"❌ Retrait refusé : demande **{amount} PO**, coffre = **{state.GoldPieces} PO**.\n" +
                $"Total du coffre : **{state.GoldPieces} PO**";

            return Fail(state, msgRefuse);
        }

        state.GoldPieces -= amount;

        state.Ledger.Add(new ChestLedgerEntry
        {
            EntryType = ChestLedgerEntryType.GoldWithdraw,
            CharacterName = characterName,
            UserId = userId,
            ChannelId = channelId,
            AmountGold = amount,
            GoldBalanceAfter = state.GoldPieces
        });

        _repo.Save(state);

        var msg = $"🪙 **{characterName}** retire **{amount} PO** du coffre.\n" +
                  $"Total du coffre : **{state.GoldPieces} PO**";

        return new ChestOperationResult(true, msg, state.GoldPieces, state.Items.Count);
    }

    public ChestOperationResult AddItem(
        ulong guildId,
        string name,
        int quantity,
        string? notes,
        string characterName,
        ulong userId,
        ulong channelId)
    {
        var state = _repo.LoadOrCreate(guildId);

        if (string.IsNullOrWhiteSpace(characterName))
            return Fail(state, "❌ Le paramètre `pj` est obligatoire.");

        if (string.IsNullOrWhiteSpace(name))
            return Fail(state, "❌ Nom d'objet obligatoire (ex: \"Rubis\").");

        if (quantity <= 0)
            return Fail(state, "❌ La quantité doit être > 0.");

        var item = new ChestItem
        {
            Name = name.Trim(),
            Quantity = quantity,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            AddedByUserId = userId,
            AddedAtUtc = DateTime.UtcNow
        };

        state.Items.Add(item);

        state.Ledger.Add(new ChestLedgerEntry
        {
            EntryType = ChestLedgerEntryType.ItemAdded,
            CharacterName = characterName,
            UserId = userId,
            ChannelId = channelId,
            ItemId = item.Id,
            ItemNameSnapshot = item.Name,
            ItemQuantity = item.Quantity,
            NotesSnapshot = item.Notes
        });

        _repo.Save(state);

        var notesPart = item.Notes is null ? "" : $" *({item.Notes})*";
        var msg =
            $"🎒 **{characterName}** ajoute **{item.Quantity}× {item.Name}**{notesPart} au coffre.\n" +
            $"Coffre : **{state.GoldPieces} PO** + **{state.Items.Count} objets**";

        return new ChestOperationResult(true, msg, state.GoldPieces, state.Items.Count);
    }

    public ChestOperationResult RemoveItemByRef(
        ulong guildId,
        string itemRef,
        int quantity,
        string characterName,
        ulong userId,
        ulong channelId)
    {
        var state = _repo.LoadOrCreate(guildId);

        if (string.IsNullOrWhiteSpace(characterName))
            return Fail(state, "❌ Le paramètre `pj` est obligatoire.");

        if (string.IsNullOrWhiteSpace(itemRef))
            return Fail(state, "❌ Référence objet obligatoire. Utilise `/coffre item list`.");

        if (quantity <= 0)
            return Fail(state, "❌ La quantité doit être > 0.");

        var normalized = itemRef.Trim().ToUpperInvariant();
        var item = state.Items.FirstOrDefault(i => i.Ref.Equals(normalized, StringComparison.OrdinalIgnoreCase));

        if (item is null)
            return Fail(state, $"❌ Objet introuvable : ref **{normalized}**. Utilise `/coffre item list`.");

        if (quantity > item.Quantity)
            return Fail(state, $"❌ Quantité insuffisante : demande **{quantity}**, disponible **{item.Quantity}** pour **{item.Name}** (#{item.Ref}).");

        item.Quantity -= quantity;

        var removedFully = item.Quantity == 0;
        if (removedFully)
            state.Items.Remove(item);

        state.Ledger.Add(new ChestLedgerEntry
        {
            EntryType = ChestLedgerEntryType.ItemRemoved,
            CharacterName = characterName,
            UserId = userId,
            ChannelId = channelId,
            ItemId = item.Id,
            ItemNameSnapshot = item.Name,
            ItemQuantity = quantity,
            NotesSnapshot = item.Notes
        });

        _repo.Save(state);

        var suffix = removedFully ? " (objet retiré du coffre)" : $" (reste {item.Quantity})";
        var msg =
            $"🧳 **{characterName}** retire **{quantity}× {item.Name}** (#{item.Ref}) du coffre{suffix}.\n" +
            $"Coffre : **{state.GoldPieces} PO** + **{state.Items.Count} objets**";

        return new ChestOperationResult(true, msg, state.GoldPieces, state.Items.Count);
    }

    public string ListItems(ulong guildId, int take = 10, int skip = 0)
    {
        var state = _repo.LoadOrCreate(guildId);

        if (state.Items.Count == 0)
            return "🎒 Le coffre ne contient aucun objet.";

        var items = state.Items
            .OrderBy(i => i.Name)
            .Skip(skip)
            .Take(take)
            .ToList();

        var lines = items.Select((i, idx) =>
        {
            var note = i.Notes is null ? "" : $" — {i.Notes}";
            var addedBy = $" — Ajouté par <@{i.AddedByUserId}>";
            return $"{skip + idx + 1}. **#{i.Ref}** — **{i.Quantity}× {i.Name}**{note}{addedBy}";
        });

        return "🎒 **Objets du coffre :**\n" + string.Join("\n", lines);
    }

    private static ChestOperationResult Fail(ChestState state, string message)
        => new(
            Success: false,
            Message: message,
            GoldPieces: state.GoldPieces,
            ItemCount: state.Items.Count);
}
