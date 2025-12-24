using DndChestBot.App.Services;
using DndChestBot.Tests.TestDoubles;
using Xunit;

namespace DndChestBot.Tests;

public sealed class ChestServiceTests
{
    private static ChestService CreateService(out InMemoryChestRepository repo)
    {
        repo = new InMemoryChestRepository();
        return new ChestService(repo);
    }

    [Fact]
    public void GetBalance_WhenNewGuild_ReturnsZeroGoldAndZeroItems()
    {
        var service = CreateService(out _);

        var result = service.GetBalance(guildId: 1);

        Assert.True(result.Success);
        Assert.Contains("0 PO", result.Message);
        Assert.Equal(0, result.GoldPieces);
        Assert.Equal(0, result.ItemCount);
    }

    [Fact]
    public void DepositGold_IncreasesBalance_AndReturnsMessage()
    {
        var service = CreateService(out _);

        var result = service.DepositGold(1, 10, "Vito", 11, 22);

        Assert.True(result.Success);
        Assert.Equal(10, result.GoldPieces);
        Assert.Contains("Vito", result.Message);
        Assert.Contains("10 PO", result.Message);
    }

    [Fact]
    public void DepositGold_RequiresCharacterName_AndReturnsCurrentState()
    {
        var service = CreateService(out _);

        var result = service.DepositGold(1, 10, "   ", 11, 22);

        Assert.False(result.Success);
        Assert.Contains("pj", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, result.GoldPieces);
        Assert.Equal(0, result.ItemCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void DepositGold_RequiresPositiveAmount_AndReturnsCurrentState(int amount)
    {
        var service = CreateService(out _);

        // on dépose d'abord pour vérifier que le fail renvoie bien l'état existant
        service.DepositGold(1, 10, "Vito", 11, 22);

        var result = service.DepositGold(1, amount, "Vito", 11, 22);

        Assert.False(result.Success);
        Assert.Contains("> 0", result.Message);
        Assert.Equal(10, result.GoldPieces);
    }

    [Fact]
    public void WithdrawGold_WhenEnoughGold_DecreasesBalance()
    {
        var service = CreateService(out _);

        service.DepositGold(1, 20, "Vito", 11, 22);
        var result = service.WithdrawGold(1, 5, "Vito", 11, 22);

        Assert.True(result.Success);
        Assert.Equal(15, result.GoldPieces);
        Assert.Contains("retire", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("15 PO", result.Message);
    }

    [Fact]
    public void WithdrawGold_WhenInsufficientGold_ReturnsFailureAndKeepsBalance()
    {
        var service = CreateService(out var repo);

        service.DepositGold(1, 10, "Vito", 11, 22);

        var result = service.WithdrawGold(1, 999, "Vito", 11, 22);

        Assert.False(result.Success);
        Assert.Equal(10, result.GoldPieces);

        var state = repo.LoadOrCreate(1);
        Assert.Equal(10, state.GoldPieces);
        Assert.Contains("refusé", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddItem_AddsNewItem_AndIncreasesItemCount()
    {
        var service = CreateService(out _);

        var result = service.AddItem(1, "Rubis", 1, "À faire estimer", "Vito", 11, 22);

        Assert.True(result.Success);
        Assert.Equal(1, result.ItemCount);
        Assert.Contains("Rubis", result.Message);
        Assert.Contains("À faire estimer", result.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddItem_RequiresName_AndReturnsCurrentState(string name)
    {
        var service = CreateService(out _);

        // état non vide
        service.AddItem(1, "Potion", 2, null, "Vito", 11, 22);

        var result = service.AddItem(1, name, 1, null, "Vito", 11, 22);

        Assert.False(result.Success);
        Assert.Contains("Nom", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, result.ItemCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_RequiresPositiveQuantity_AndReturnsCurrentState(int qty)
    {
        var service = CreateService(out _);

        // état non vide
        service.AddItem(1, "Potion", 2, null, "Vito", 11, 22);

        var result = service.AddItem(1, "Rubis", qty, null, "Vito", 11, 22);

        Assert.False(result.Success);
        Assert.Contains("> 0", result.Message);
        Assert.Equal(1, result.ItemCount);
    }

    [Fact]
    public void RemoveItemByRef_WhenQuantityEquals_RemovesItemCompletely()
    {
        var service = CreateService(out var repo);

        service.AddItem(1, "Rubis", 1, "À faire estimer", "Vito", 11, 22);

        var state = repo.LoadOrCreate(1);
        var itemRef = state.Items.Single().Ref;

        var result = service.RemoveItemByRef(1, itemRef, 1, "Vito", 11, 22);

        Assert.True(result.Success);
        Assert.Equal(0, repo.LoadOrCreate(1).Items.Count);
        Assert.Contains("objet retiré", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RemoveItemByRef_WhenQuantityLess_DecreasesQuantity()
    {
        var service = CreateService(out var repo);

        service.AddItem(1, "Potion", 3, null, "Vito", 11, 22);
        var itemRef = repo.LoadOrCreate(1).Items.Single().Ref;

        var result = service.RemoveItemByRef(1, itemRef, 2, "Vito", 11, 22);

        Assert.True(result.Success);
        var remaining = repo.LoadOrCreate(1).Items.Single().Quantity;
        Assert.Equal(1, remaining);
        Assert.Contains("reste 1", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RemoveItemByRef_WhenTooMuchRequested_FailsAndDoesNotChangeQuantity_AndReturnsCurrentState()
    {
        var service = CreateService(out var repo);

        service.AddItem(1, "Potion", 2, null, "Vito", 11, 22);
        var item = repo.LoadOrCreate(1).Items.Single();
        var itemRef = item.Ref;

        var result = service.RemoveItemByRef(1, itemRef, 99, "Vito", 11, 22);

        Assert.False(result.Success);
        Assert.Equal(1, result.ItemCount);
        Assert.Equal(2, repo.LoadOrCreate(1).Items.Single().Quantity);
        Assert.Contains("insuffisante", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RemoveItemByRef_WhenUnknownRef_FailsAndReturnsCurrentState()
    {
        var service = CreateService(out _);

        // état non vide
        service.AddItem(1, "Potion", 1, null, "Vito", 11, 22);

        var result = service.RemoveItemByRef(1, "ZZZZ", 1, "Vito", 11, 22);

        Assert.False(result.Success);
        Assert.Equal(1, result.ItemCount);
        Assert.Contains("introuvable", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ListItems_WhenEmpty_ReturnsEmptyMessage()
    {
        var service = CreateService(out _);

        var msg = service.ListItems(1);

        Assert.Contains("aucun objet", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ListItems_ReturnsRefsAndNames()
    {
        var service = CreateService(out _);

        service.AddItem(1, "Amulette", 1, "Quête", "Vito", 11, 22);
        service.AddItem(1, "Rubis", 2, "À estimer", "Vito", 11, 22);

        var msg = service.ListItems(1);

        Assert.Contains("Objets du coffre", msg);
        Assert.Contains("#", msg);
        Assert.Contains("Amulette", msg);
        Assert.Contains("Rubis", msg);
    }
}
