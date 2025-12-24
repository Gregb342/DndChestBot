# ?? DndChestBot

Discord bot to manage your D&D group's shared chest. Track gold pieces (GP) and items shared by your adventuring party!

## ?? Features

- ?? **Gold Management**: Deposit and withdraw gold pieces (GP) from the shared chest
- ?? **Item Management**: Add, list, and remove items with optional notes
- ?? **Complete History**: All transactions are logged (ledger)
- ?? **Per Server**: Each Discord server has its own independent chest
- ?? **Character Tracking**: Records which character performed each action

## ?? Installation

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A Discord Developer account with a bot created

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Gregb342/DndChestBot.git
   cd DndChestBot
   ```

2. **Create your Discord bot**
   - Go to [Discord Developer Portal](https://discord.com/developers/applications)
   - Create a new application
   - In the "Bot" tab, create a bot and copy the token
   - In the "OAuth2 > URL Generator" tab, select:
     - Scopes: `bot`, `applications.commands`
     - Permissions: `Send Messages`, `Use Slash Commands`
   - Use the generated URL to invite the bot to your server

3. **Configure the bot**
   
   Create the file `DndChestBot.App/appsettings.Development.json`:
   ```json
   {
     "Discord": {
       "Token": "YOUR_BOT_TOKEN_HERE",
       "DevGuildId": "YOUR_GUILD_ID_HERE"
     }
   }
   ```
   
   > **Note**: `DevGuildId` is optional. If provided, slash commands will be registered instantly on that server. Otherwise, they'll be registered globally (can take up to 1 hour).

4. **Run the bot**
   ```bash
   dotnet run --project DndChestBot.App/DndChestBot.App.csproj
   ```

   You should see:
   ```
   ? Bot démarré. CTRL+C pour quitter.
   ```

## ?? Discord Commands

All commands use the `/coffre` prefix.

### ?? Gold Management

| Command | Description | Example |
|---------|-------------|---------|
| `/coffre solde` | Display current chest balance | `/coffre solde` |
| `/coffre depot montant:<n> pj:<name>` | Deposit GP into the chest | `/coffre depot montant:50 pj:Gandalf` |
| `/coffre retrait montant:<n> pj:<name>` | Withdraw GP from the chest | `/coffre retrait montant:20 pj:Vito` |

### ?? Item Management

| Command | Description | Example |
|---------|-------------|---------|
| `/coffre item list` | List all items in the chest | `/coffre item list` |
| `/coffre item add nom:<text> quantite:<n> pj:<name> [note:<text>]` | Add an item | `/coffre item add nom:Healing Potion quantite:3 pj:Elara note:Found in dungeon` |
| `/coffre item remove ref:<ABCD> quantite:<n> pj:<name>` | Remove an item by reference | `/coffre item remove ref:A3F2 quantite:1 pj:Thorin` |

### ?? Help

| Command | Description |
|---------|-------------|
| `/coffre help` | Display command help |

## ?? Usage Example

```
Gandalf: /coffre depot montant:100 pj:Gandalf
Bot: ?? Gandalf dépose 100 PO dans le coffre.
     Total du coffre : 100 PO

Vito: /coffre item add nom:Ruby quantite:1 pj:Vito note:To be appraised
Bot: ?? Vito ajoute 1× Ruby (To be appraised) au coffre.
     Coffre : 100 PO + 1 objets

Elara: /coffre item list
Bot: ?? Objets du coffre :
     1. #A3F2 — 1× Ruby — To be appraised — Ajouté par Vito

Thorin: /coffre retrait montant:30 pj:Thorin
Bot: ?? Thorin retire 30 PO du coffre.
     Total du coffre : 70 PO

Gandalf: /coffre solde
Bot: ?? Le coffre contient actuellement 70 PO.
     ?? Objets : 1
```

## ??? Data Storage

Data is stored in JSON files in the `data/` folder:
- One file per Discord server: `chest_<GuildId>.json`
- Contains: gold balance, item list, complete history

## ?? Tests

To run unit tests:
```bash
dotnet test
```

## ??? Architecture

```
DndChestBot.App/
??? Discord/              # Discord handlers and modules
?   ??? InteractionHandler.cs
?   ??? Modules/
?       ??? ChestModule.cs
??? Domain/               # Business models
?   ??? Models/
??? Infrastructure/       # Persistence
?   ??? Repositories/
??? Services/             # Business logic
    ??? ChestService.cs

DndChestBot.Tests/        # Unit tests
```

## ?? Important Rules

- ? The `pj` parameter (character name) is **required** for all actions
- ? Amounts must be **greater than 0**
- ? Cannot withdraw more gold than the available balance
- ? Cannot remove more items than the available quantity
- ?? Each item has a **unique reference** (4 characters) for easy removal

## ?? Contributing

Contributions are welcome! Feel free to open an issue or pull request.

## ?? License

This project is licensed under the MIT License.

## ?? Author

Created by [Gregb342](https://github.com/Gregb342)

---

*Happy gaming and may your chests always be full!* ???
