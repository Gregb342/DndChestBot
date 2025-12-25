using Discord.Interactions;
using DndChestBot.App.Services;

namespace DndChestBot.App.Discord.Modules;

[Group("coffre", "Gestion du coffre de groupe")]
public sealed class ChestModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ChestService _service;

    public ChestModule(ChestService service)
    {
        _service = service;
    }

    [SlashCommand("help", "Affiche l'aide des commandes coffre")]
    public async Task HelpAsync()
    {
        Console.WriteLine($"[ChestModule] Command /coffre help - User: {Context.User.Username} ({Context.User.Id}), Guild: {Context.Guild?.Name ?? "DM"}");
        var msg =
            "🧾 **Commandes Coffre**\n" +
            "🪙 **Monnaie**\n" +
            "- `/coffre depot montant:<n> pj:<nom>`\n" +
            "- `/coffre retrait montant:<n> pj:<nom>`\n" +
            "- `/coffre solde`\n\n" +
            "🎒 **Objets**\n" +
            "- `/coffre item add nom:<texte> quantite:<n> note:<texte?> pj:<nom>`\n" +
            "- `/coffre item list`\n" +
            "- `/coffre item remove ref:<ABCD> quantite:<n> pj:<nom>`\n\n" +
            "📌 Règles : `pj` obligatoire, pas de retrait PO au-delà du solde.";
        await RespondAsync(msg, ephemeral: true);
    }

    [SlashCommand("solde", "Affiche le solde du coffre")]
    public async Task BalanceAsync()
    {
        Console.WriteLine($"[ChestModule] Command /coffre solde - User: {Context.User.Username} ({Context.User.Id}), Guild: {Context.Guild?.Name ?? "DM"} ({Context.Guild?.Id})");
        
        if (Context.Guild is null)
        {
            Console.WriteLine($"[ChestModule] Command REJECTED - Used in DM");
            await RespondAsync("❌ Cette commande doit être utilisée dans un serveur (pas en DM).", ephemeral: true);
            return;
        }

        var result = _service.GetBalance(Context.Guild.Id);
        await RespondAsync(result.Message);
    }

    [SlashCommand("depot", "Dépose des PO dans le coffre")]
    public async Task DepositAsync(
        [Summary("montant", "Nombre de PO à déposer")] int amount,
        [Summary("pj", "Nom du personnage")] string characterName)
    {
        Console.WriteLine($"[ChestModule] Command /coffre depot - User: {Context.User.Username} ({Context.User.Id}), Guild: {Context.Guild?.Name ?? "DM"} ({Context.Guild?.Id}), Amount: {amount}, Character: {characterName}");
        
        if (Context.Guild is null)
        {
            Console.WriteLine($"[ChestModule] Command REJECTED - Used in DM");
            await RespondAsync("❌ Cette commande doit être utilisée dans un serveur (pas en DM).", ephemeral: true);
            return;
        }

        var result = _service.DepositGold(Context.Guild.Id, amount, characterName, Context.User.Id, Context.Channel.Id);
        await RespondAsync(result.Message, ephemeral: !result.Success);
    }

    [SlashCommand("retrait", "Retire des PO du coffre")]
    public async Task WithdrawAsync(
        [Summary("montant", "Nombre de PO à retirer")] int amount,
        [Summary("pj", "Nom du personnage")] string characterName)
    {
        Console.WriteLine($"[ChestModule] Command /coffre retrait - User: {Context.User.Username} ({Context.User.Id}), Guild: {Context.Guild?.Name ?? "DM"} ({Context.Guild?.Id}), Amount: {amount}, Character: {characterName}");
        
        if (Context.Guild is null)
        {
            Console.WriteLine($"[ChestModule] Command REJECTED - Used in DM");
            await RespondAsync("❌ Cette commande doit être utilisée dans un serveur (pas en DM).", ephemeral: true);
            return;
        }

        var result = _service.WithdrawGold(Context.Guild.Id, amount, characterName, Context.User.Id, Context.Channel.Id);
        await RespondAsync(result.Message, ephemeral: !result.Success);
    }

    // ---- OBJETS ----

    [Group("item", "Gestion des objets du coffre")]
    public sealed class ChestItemGroup : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ChestService _service;

        public ChestItemGroup(ChestService service)
        {
            _service = service;
        }

        [SlashCommand("add", "Ajoute un objet au coffre")]
        public async Task AddItemAsync(
            [Summary("nom", "Nom de l'objet")] string name,
            [Summary("quantite", "Quantité")] int quantity,
            [Summary("pj", "Nom du personnage")] string characterName,
            [Summary("note", "Note optionnelle (ex: À faire estimer)")] string? note = null)
        {
            Console.WriteLine($"[ChestModule] Command /coffre item add - User: {Context.User.Username} ({Context.User.Id}), Guild: {Context.Guild?.Name ?? "DM"} ({Context.Guild?.Id}), Item: {name}, Quantity: {quantity}, Character: {characterName}, Note: {note ?? "none"}");
            
            if (Context.Guild is null)
            {
                Console.WriteLine($"[ChestModule] Command REJECTED - Used in DM");
                await RespondAsync("❌ Cette commande doit être utilisée dans un serveur (pas en DM).", ephemeral: true);
                return;
            }

            var result = _service.AddItem(Context.Guild.Id, name, quantity, note, characterName, Context.User.Id, Context.Channel.Id);
            await RespondAsync(result.Message, ephemeral: !result.Success);
        }

        [SlashCommand("list", "Liste les objets du coffre")]
        public async Task ListItemsAsync()
        {
            Console.WriteLine($"[ChestModule] Command /coffre item list - User: {Context.User.Username} ({Context.User.Id}), Guild: {Context.Guild?.Name ?? "DM"} ({Context.Guild?.Id})");
            
            if (Context.Guild is null)
            {
                Console.WriteLine($"[ChestModule] Command REJECTED - Used in DM");
                await RespondAsync("❌ Cette commande doit être utilisée dans un serveur (pas en DM).", ephemeral: true);
                return;
            }

            var msg = _service.ListItems(Context.Guild.Id, take: 20, skip: 0);
            await RespondAsync(msg, ephemeral: false);
        }

        [SlashCommand("remove", "Retire un objet du coffre via sa référence courte")]
        public async Task RemoveItemAsync(
            [Summary("ref", "Référence courte (ex: A3F2)")] string itemRef,
            [Summary("quantite", "Quantité à retirer")] int quantity,
            [Summary("pj", "Nom du personnage")] string characterName)
        {
            Console.WriteLine($"[ChestModule] Command /coffre item remove - User: {Context.User.Username} ({Context.User.Id}), Guild: {Context.Guild?.Name ?? "DM"} ({Context.Guild?.Id}), Ref: {itemRef}, Quantity: {quantity}, Character: {characterName}");
            
            if (Context.Guild is null)
            {
                Console.WriteLine($"[ChestModule] Command REJECTED - Used in DM");
                await RespondAsync("❌ Cette commande doit être utilisée dans un serveur (pas en DM).", ephemeral: true);
                return;
            }

            var result = _service.RemoveItemByRef(Context.Guild.Id, itemRef, quantity, characterName, Context.User.Id, Context.Channel.Id);
            await RespondAsync(result.Message, ephemeral: !result.Success);
        }
    }
}
