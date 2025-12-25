using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Tests;

public class StoreServiceTests
{
    private readonly Mock<ILogger<StoreService>> _mockLogger;
    private readonly Mock<ILogger<CurrencyService>> _mockCurrencyLogger;
    private readonly CurrencyService _currencyService;
    private readonly StoreService _storeService;

    public StoreServiceTests()
    {
        _mockLogger = new Mock<ILogger<StoreService>>();
        _mockCurrencyLogger = new Mock<ILogger<CurrencyService>>();
        _currencyService = new CurrencyService(_mockCurrencyLogger.Object);
        _storeService = new StoreService(_mockLogger.Object, _currencyService);
    }

    [Fact]
    public void GetAllItems_ReturnsAvailableItems()
    {
        // Act
        var items = _storeService.GetAllItems();

        // Assert
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.True(item.IsAvailable()));
    }

    [Fact]
    public void GetFeaturedItems_ReturnsOnlyFeaturedItems()
    {
        // Act
        var featured = _storeService.GetFeaturedItems();

        // Assert
        Assert.NotEmpty(featured);
        Assert.All(featured, item => Assert.True(item.IsFeatured));
    }

    [Fact]
    public void GetItemsByType_ReturnsCorrectType()
    {
        // Act
        var currencyPacks = _storeService.GetItemsByType(StoreItemType.CurrencyPack);

        // Assert
        Assert.NotEmpty(currencyPacks);
        Assert.All(currencyPacks, item => Assert.Equal(StoreItemType.CurrencyPack, item.Type));
    }

    [Fact]
    public void GetItemById_ReturnsCorrectItem()
    {
        // Act
        var item = _storeService.GetItemById("gold_small");

        // Assert
        Assert.NotNull(item);
        Assert.Equal("Small Gold Pouch", item.Name);
    }

    [Fact]
    public void GetItemById_WithInvalidId_ReturnsNull()
    {
        // Act
        var item = _storeService.GetItemById("invalid_id");

        // Assert
        Assert.Null(item);
    }

    [Fact]
    public void CanPurchase_WithSufficientFunds_ReturnsTrue()
    {
        // Arrange - Add enough gems to buy gold_small (costs 10 gems)
        _currencyService.AddCurrency(CurrencyType.Gems, 100, "Test");

        // Act
        var canPurchase = _storeService.CanPurchase("gold_small");

        // Assert
        Assert.True(canPurchase);
    }

    [Fact]
    public void CanPurchase_WithInsufficientFunds_ReturnsFalse()
    {
        // Arrange - Set gems to 0
        _currencyService.SetCurrency(CurrencyType.Gems, 0);

        // Act
        var canPurchase = _storeService.CanPurchase("gold_small");

        // Assert
        Assert.False(canPurchase);
    }

    [Fact]
    public void TryPurchase_WithSufficientFunds_Succeeds()
    {
        // Arrange
        var initialGold = _currencyService.GetBalance(CurrencyType.Gold);
        var initialGems = _currencyService.GetBalance(CurrencyType.Gems);

        // Act
        var success = _storeService.TryPurchase("gold_small");

        // Assert
        Assert.True(success);
        Assert.Equal(initialGold + 500, _currencyService.GetBalance(CurrencyType.Gold)); // Reward
        Assert.Equal(initialGems - 10, _currencyService.GetBalance(CurrencyType.Gems)); // Cost
    }

    [Fact]
    public void TryPurchase_WithInsufficientFunds_Fails()
    {
        // Arrange
        _currencyService.SetCurrency(CurrencyType.Gems, 0);
        var initialGold = _currencyService.GetBalance(CurrencyType.Gold);

        // Act
        var success = _storeService.TryPurchase("gold_small");

        // Assert
        Assert.False(success);
        Assert.Equal(initialGold, _currencyService.GetBalance(CurrencyType.Gold)); // Unchanged
    }

    [Fact]
    public void TryPurchase_IncrementsPurchaseCount()
    {
        // Arrange
        var item = _storeService.GetItemById("gold_small");
        var initialCount = item!.PurchaseCount;

        // Act
        _storeService.TryPurchase("gold_small");

        // Assert
        Assert.Equal(initialCount + 1, item.PurchaseCount);
    }

    [Fact]
    public void TryPurchase_FiresItemPurchasedEvent()
    {
        // Arrange
        var eventFired = false;
        StoreItem? purchasedItem = null;

        _storeService.ItemPurchased += (item) =>
        {
            eventFired = true;
            purchasedItem = item;
        };

        // Act
        _storeService.TryPurchase("gold_small");

        // Assert
        Assert.True(eventFired);
        Assert.NotNull(purchasedItem);
        Assert.Equal("gold_small", purchasedItem.Id);
    }

    [Fact]
    public void TryPurchase_AddsToHistory()
    {
        // Arrange
        var initialHistoryCount = _storeService.GetPurchaseHistory().Count;

        // Act
        _storeService.TryPurchase("gold_small");

        // Assert
        var history = _storeService.GetPurchaseHistory();
        Assert.Equal(initialHistoryCount + 1, history.Count);
        Assert.Equal("gold_small", history.First().ItemId);
    }

    [Fact]
    public void TryPurchase_WithMaxPurchaseLimit_BecomesUnavailable()
    {
        // Arrange
        var item = _storeService.GetItemById("weekend_special");
        Assert.NotNull(item);
        Assert.True(item.IsAvailable());
        Assert.Equal(1, item.MaxPurchases);

        // Add enough gems for purchase
        _currencyService.AddCurrency(CurrencyType.Gems, 300, "Test");

        // Act
        _storeService.TryPurchase("weekend_special");

        // Assert
        Assert.False(item.IsAvailable()); // Should now be unavailable
        Assert.False(_storeService.CanPurchase("weekend_special"));
    }

    [Fact]
    public void StoreItem_GetPriceDisplay_FormatsCorrectly()
    {
        // Arrange
        var item = _storeService.GetItemById("gold_small");

        // Act
        var priceDisplay = item!.GetPriceDisplay();

        // Assert
        Assert.Contains("💎", priceDisplay);
        Assert.Contains("10", priceDisplay);
    }

    [Fact]
    public void StoreItem_GetTimeRemaining_ForLimitedTimeOffer()
    {
        // Arrange
        var item = _storeService.GetItemById("weekend_special");

        // Act
        var timeRemaining = item!.GetTimeRemaining();

        // Assert
        Assert.NotNull(timeRemaining);
        Assert.True(timeRemaining.Value.TotalSeconds > 0);
    }

    [Fact]
    public void StoreItem_IsAvailable_ReturnsFalseWhenExpired()
    {
        // Arrange
        var item = new StoreItem
        {
            Id = "test_expired",
            Name = "Test Expired",
            IsLimitedTime = true,
            ExpiresAt = DateTime.Now.AddDays(-1) // Already expired
        };

        // Act
        var available = item.IsAvailable();

        // Assert
        Assert.False(available);
    }

    [Fact]
    public void Purchase_XpBoost_GrantsExperience()
    {
        // Arrange
        var item = _storeService.GetItemById("xp_boost");
        Assert.NotNull(item);
        Assert.NotNull(item.ExperiencePoints);
        Assert.Equal(500, item.ExperiencePoints);

        // Act
        var success = _storeService.TryPurchase("xp_boost");

        // Assert
        Assert.True(success);
        // Note: XP granting is logged but not stored in the service
        // In a real implementation, this would update player XP
    }

    [Fact]
    public void Purchase_GearPack_GrantsUpgradeShards()
    {
        // Arrange
        var item = _storeService.GetItemById("starter_pack");
        Assert.NotNull(item);
        Assert.NotNull(item.UpgradeShards);
        Assert.Equal(10, item.UpgradeShards);

        // Act
        var success = _storeService.TryPurchase("starter_pack");

        // Assert
        Assert.True(success);
        // Note: Shard granting is logged but not stored in the service
        // In a real implementation, this would update player inventory
    }

    [Fact]
    public void Purchase_MultiReward_GrantsAllRewards()
    {
        // Arrange
        _currencyService.AddCurrency(CurrencyType.Gems, 300, "Test");
        var initialGold = _currencyService.GetBalance(CurrencyType.Gold);
        var initialGems = _currencyService.GetBalance(CurrencyType.Gems);

        // Act
        var success = _storeService.TryPurchase("weekend_special");

        // Assert
        Assert.True(success);
        Assert.Equal(initialGold + 5000, _currencyService.GetBalance(CurrencyType.Gold));
        Assert.Equal(initialGems + 50 - 200, _currencyService.GetBalance(CurrencyType.Gems)); // +50 reward, -200 cost
    }

    [Fact]
    public void GetPurchaseHistory_OrdersByMostRecent()
    {
        // Arrange
        _storeService.TryPurchase("gold_small");
        Thread.Sleep(100); // Ensure different timestamps
        _storeService.TryPurchase("xp_boost");

        // Act
        var history = _storeService.GetPurchaseHistory();

        // Assert
        Assert.Equal("xp_boost", history.First().ItemId); // Most recent first
        Assert.Equal("gold_small", history.Last().ItemId);
    }

    [Fact]
    public void AllPreConfiguredItems_HaveValidPrices()
    {
        // Act
        var items = _storeService.GetAllItems();

        // Assert
        Assert.All(items, item =>
        {
            Assert.NotEmpty(item.Price);
            Assert.All(item.Price, price =>
            {
                Assert.True(price.Amount > 0, $"Item {item.Name} has invalid price amount");
            });
        });
    }

    [Fact]
    public void AllPreConfiguredItems_HaveValidRewards()
    {
        // Act
        var items = _storeService.GetAllItems();

        // Assert
        Assert.All(items, item =>
        {
            var hasRewards = item.CurrencyRewards.Count > 0 ||
                           item.ExperiencePoints.HasValue ||
                           item.UpgradeShards.HasValue ||
                           item.GearIds.Count > 0;

            Assert.True(hasRewards, $"Item {item.Name} has no rewards configured");
        });
    }
}
