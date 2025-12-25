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
        Console.WriteLine($"[ChestService] GetBalance - GuildId: {guildId}");
        var state = _repo.LoadOrCreate(guildId);

        Console.WriteLine($"[ChestService] Balance consulté - GuildId: {guildId}, Gold: {state.GoldPieces} PO, Items: {state.Items.Count}");
        return new ChestOperationResult(
            Success: true,
            Message: $"📦 Le coffre contient actuellement **{state.GoldPieces} PO**.\n🧰 Objets : **{state.Items.Count}**",
            GoldPieces: state.GoldPieces,
            ItemCount: state.Items.Count);
    }

    public ChestOperationResult DepositGold(ulong guildId, int amount, string characterName, ulong userId, ulong channelId)
    {
        Console.WriteLine($"[ChestService] DepositGold - GuildId: {guildId}, Amount: {amount}, Character: {characterName}, UserId: {userId}");
        var state = _repo.LoadOrCreate(guildId);

        if (string.IsNullOrWhiteSpace(characterName))
        {
            Console.WriteLine($"[ChestService] DepositGold FAILED - Missing character name");
            return Fail(state, "❌ Le paramètre `pj` est obligatoire.");
        }

        if (amount <= 0)
        {
            Console.WriteLine($"[ChestService] DepositGold FAILED - Invalid amount: {amount}");
            return Fail(state, "❌ Le montant doit être > 0.");
        }

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

        Console.WriteLine($"[ChestService] DepositGold SUCCESS - New balance: {state.GoldPieces} PO");

        var msg = $"💰 **{characterName}** dépose **{amount} PO** dans le coffre.\n" +
                  $"Total du coffre : **{state.GoldPieces} PO**";

        return new ChestOperationResult(true, msg, state.GoldPieces, state.Items.Count);
    }

    public ChestOperationResult WithdrawGold(ulong guildId, int amount, string characterName, ulong userId, ulong channelId)
    {
        Console.WriteLine($"[ChestService] WithdrawGold - GuildId: {guildId}, Amount: {amount}, Character: {characterName}, UserId: {userId}");
        var state = _repo.LoadOrCreate(guildId);

        if (string.IsNullOrWhiteSpace(characterName))
        {
            Console.WriteLine($"[ChestService] WithdrawGold FAILED - Missing character name");
            return Fail(state, "❌ Le paramètre `pj` est obligatoire.");
        }

        if (amount <= 0)
        {
            Console.WriteLine($"[ChestService] WithdrawGold FAILED - Invalid amount: {amount}");
            return Fail(state, "❌ Le montant doit être > 0.");
        }

        if (amount > state.GoldPieces)
        {
            Console.WriteLine($"[ChestService] WithdrawGold FAILED - Insufficient funds: requested {amount} PO, available {state.GoldPieces} PO");
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

        Console.WriteLine($"[ChestService] WithdrawGold SUCCESS - New balance: {state.GoldPieces} PO");

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
        Console.WriteLine($"[ChestService] AddItem - GuildId: {guildId}, Item: {name}, Quantity: {quantity}, Character: {characterName}, UserId: {userId}");
        var state = _repo.LoadOrCreate(guildId);

        if (string.IsNullOrWhiteSpace(characterName))
        {
            Console.WriteLine($"[ChestService] AddItem FAILED - Missing character name");
            return Fail(state, "❌ Le paramètre `pj` est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine($"[ChestService] AddItem FAILED - Missing item name");
            return Fail(state, "❌ Nom d'objet obligatoire (ex: \"Rubis\").");
        }

        if (quantity <= 0)
        {
            Console.WriteLine($"[ChestService] AddItem FAILED - Invalid quantity: {quantity}");
            return Fail(state, "❌ La quantité doit être > 0.");
        }

        var item = new ChestItem
        {
            Name = name.Trim(),
            Quantity = quantity,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            AddedByUserId = userId,
            AddedByCharacterName = characterName.Trim(),
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

        Console.WriteLine($"[ChestService] AddItem SUCCESS - Item added: {item.Name} (#{item.Ref}), Total items: {state.Items.Count}");

        var notesPart = item.Notes is null ? "" : $" *({item.Notes})*";
        var msg =
            $"🧰 **{characterName}** ajoute **{item.Quantity}× {item.Name}**{notesPart} au coffre.\n" +
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
        Console.WriteLine($"[ChestService] RemoveItemByRef - GuildId: {guildId}, Ref: {itemRef}, Quantity: {quantity}, Character: {characterName}, UserId: {userId}");
        var state = _repo.LoadOrCreate(guildId);

        if (string.IsNullOrWhiteSpace(characterName))
        {
            Console.WriteLine($"[ChestService] RemoveItemByRef FAILED - Missing character name");
            return Fail(state, "❌ Le paramètre `pj` est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(itemRef))
        {
            Console.WriteLine($"[ChestService] RemoveItemByRef FAILED - Missing item reference");
            return Fail(state, "❌ Référence objet obligatoire. Utilise `/coffre item list`.");
        }

        if (quantity <= 0)
        {
            Console.WriteLine($"[ChestService] RemoveItemByRef FAILED - Invalid quantity: {quantity}");
            return Fail(state, "❌ La quantité doit être > 0.");
        }

        var normalized = itemRef.Trim().ToUpperInvariant();
        var item = state.Items.FirstOrDefault(i => i.Ref.Equals(normalized, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            Console.WriteLine($"[ChestService] RemoveItemByRef FAILED - Item not found: {normalized}");
            return Fail(state, $"❌ Objet introuvable : ref **{normalized}**. Utilise `/coffre item list`.");
        }

        if (quantity > item.Quantity)
        {
            Console.WriteLine($"[ChestService] RemoveItemByRef FAILED - Insufficient quantity: requested {quantity}, available {item.Quantity}");
            return Fail(state, $"❌ Quantité insuffisante : demande **{quantity}**, disponible **{item.Quantity}** pour **{item.Name}** (#{item.Ref}).");
        }

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

        Console.WriteLine($"[ChestService] RemoveItemByRef SUCCESS - Item removed: {item.Name} (#{normalized}), Fully removed: {removedFully}, Total items: {state.Items.Count}");

        var suffix = removedFully ? " (objet retiré du coffre)" : $" (reste {item.Quantity})";
        var msg =
            $"🧳 **{characterName}** retire **{quantity}× {item.Name}** (#{item.Ref}) du coffre{suffix}.\n" +
            $"Coffre : **{state.GoldPieces} PO** + **{state.Items.Count} objets**";

        return new ChestOperationResult(true, msg, state.GoldPieces, state.Items.Count);
    }

    public string ListItems(ulong guildId, int take = 10, int skip = 0)
    {
        Console.WriteLine($"[ChestService] ListItems - GuildId: {guildId}, Take: {take}, Skip: {skip}");
        var state = _repo.LoadOrCreate(guildId);

        if (state.Items.Count == 0)
        {
            Console.WriteLine($"[ChestService] ListItems - No items in chest");
            return "🧰 Le coffre ne contient aucun objet.";
        }

        var items = state.Items
            .OrderBy(i => i.Name)
            .Skip(skip)
            .Take(take)
            .ToList();

        Console.WriteLine($"[ChestService] ListItems - Displaying {items.Count} items out of {state.Items.Count} total");

        var lines = items.Select((i, idx) =>
        {
            var note = i.Notes is null ? "" : $" — {i.Notes}";
            var addedBy = $" — Ajouté par **{i.AddedByCharacterName}**";
            return $"{skip + idx + 1}. **#{i.Ref}** — **{i.Quantity}× {i.Name}**{note}{addedBy}";
        });

        return "🧰 **Objets du coffre :**\n" + string.Join("\n", lines);
    }

    private static ChestOperationResult Fail(ChestState state, string message)
        => new(
            Success: false,
            Message: message,
            GoldPieces: state.GoldPieces,
            ItemCount: state.Items.Count);
}
