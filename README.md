# ?? DndChestBot

Bot Discord pour gérer le coffre de groupe de votre campagne D&D. Suivez l'or (PO) et les objets partagés par votre équipe d'aventuriers !

## ?? Fonctionnalités

- ?? **Gestion de l'or** : Déposez et retirez des pièces d'or (PO) du coffre commun
- ?? **Gestion des objets** : Ajoutez, listez et retirez des objets avec notes optionnelles
- ?? **Historique complet** : Toutes les transactions sont enregistrées (ledger)
- ?? **Par serveur** : Chaque serveur Discord a son propre coffre indépendant
- ?? **Suivi par personnage** : Enregistre quel personnage a effectué chaque action

## ?? Installation

### Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Un compte Discord Developer avec un bot créé

### Configuration

1. **Clonez le dépôt**
   ```bash
   git clone https://github.com/Gregb342/DndChestBot.git
   cd DndChestBot
   ```

2. **Créez votre bot Discord**
   - Rendez-vous sur [Discord Developer Portal](https://discord.com/developers/applications)
   - Créez une nouvelle application
   - Dans l'onglet "Bot", créez un bot et copiez le token
   - Dans l'onglet "OAuth2 > URL Generator", sélectionnez :
     - Scopes : `bot`, `applications.commands`
     - Permissions : `Send Messages`, `Use Slash Commands`
   - Utilisez l'URL générée pour inviter le bot sur votre serveur

3. **Configurez le bot**
   
   Créez le fichier `DndChestBot.App/appsettings.Development.json` :
   ```json
   {
     "Discord": {
       "Token": "VOTRE_TOKEN_BOT_ICI",
       "DevGuildId": "VOTRE_GUILD_ID_ICI"
     }
   }
   ```
   
   > **Note** : Le `DevGuildId` est optionnel. Si renseigné, les commandes slash seront enregistrées instantanément sur ce serveur. Sinon, elles seront enregistrées globalement (peut prendre jusqu'à 1h).

4. **Lancez le bot**
   ```bash
   dotnet run --project DndChestBot.App/DndChestBot.App.csproj
   ```

   Vous devriez voir :
   ```
   ? Bot démarré. CTRL+C pour quitter.
   ```

## ?? Commandes Discord

Toutes les commandes utilisent le préfixe `/coffre`.

### ?? Gestion de l'or

| Commande | Description | Exemple |
|----------|-------------|---------|
| `/coffre solde` | Affiche le solde actuel du coffre | `/coffre solde` |
| `/coffre depot montant:<n> pj:<nom>` | Dépose des PO dans le coffre | `/coffre depot montant:50 pj:Gandalf` |
| `/coffre retrait montant:<n> pj:<nom>` | Retire des PO du coffre | `/coffre retrait montant:20 pj:Vito` |

### ?? Gestion des objets

| Commande | Description | Exemple |
|----------|-------------|---------|
| `/coffre item list` | Liste tous les objets du coffre | `/coffre item list` |
| `/coffre item add nom:<texte> quantite:<n> pj:<nom> [note:<texte>]` | Ajoute un objet | `/coffre item add nom:Potion de soin quantite:3 pj:Elara note:Trouvé dans le donjon` |
| `/coffre item remove ref:<ABCD> quantite:<n> pj:<nom>` | Retire un objet par sa référence | `/coffre item remove ref:A3F2 quantite:1 pj:Thorin` |

### ?? Aide

| Commande | Description |
|----------|-------------|
| `/coffre help` | Affiche l'aide des commandes |

## ?? Exemple d'utilisation

```
Gandalf: /coffre depot montant:100 pj:Gandalf
Bot: ?? Gandalf dépose 100 PO dans le coffre.
     Total du coffre : 100 PO

Vito: /coffre item add nom:Rubis quantite:1 pj:Vito note:À faire estimer
Bot: ?? Vito ajoute 1× Rubis (À faire estimer) au coffre.
     Coffre : 100 PO + 1 objets

Elara: /coffre item list
Bot: ?? Objets du coffre :
     1. #A3F2 — 1× Rubis — À faire estimer — Ajouté par Vito

Thorin: /coffre retrait montant:30 pj:Thorin
Bot: ?? Thorin retire 30 PO du coffre.
     Total du coffre : 70 PO

Gandalf: /coffre solde
Bot: ?? Le coffre contient actuellement 70 PO.
     ?? Objets : 1
```

## ??? Stockage des données

Les données sont stockées dans des fichiers JSON dans le dossier `data/` :
- Un fichier par serveur Discord : `chest_<GuildId>.json`
- Contient : solde d'or, liste des objets, historique complet

## ?? Tests

Pour exécuter les tests unitaires :
```bash
dotnet test
```

## ??? Architecture

```
DndChestBot.App/
??? Discord/              # Handlers et modules Discord
?   ??? InteractionHandler.cs
?   ??? Modules/
?       ??? ChestModule.cs
??? Domain/               # Modèles métier
?   ??? Models/
??? Infrastructure/       # Persistance
?   ??? Repositories/
??? Services/             # Logique métier
    ??? ChestService.cs

DndChestBot.Tests/        # Tests unitaires
```

## ?? Règles importantes

- ? Le paramètre `pj` (nom du personnage) est **obligatoire** pour toutes les actions
- ? Les montants doivent être **supérieurs à 0**
- ? Impossible de retirer plus d'or que le solde disponible
- ? Impossible de retirer plus d'objets que la quantité disponible
- ?? Chaque objet possède une **référence unique** (4 caractères) pour le retirer facilement

## ?? Contribution

Les contributions sont les bienvenues ! N'hésitez pas à ouvrir une issue ou une pull request.

## ?? Licence

Ce projet est sous licence MIT.

## ?? Auteur

Créé par [Gregb342](https://github.com/Gregb342)

---

*Bon jeu et que vos coffres soient toujours pleins !* ???
