using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;

public class StoreService
{
    private readonly ILogger<StoreService> _logger;
    private readonly CurrencyService _currencyService;
    private readonly List<StoreItem> _storeItems = new();
    private readonly List<PurchaseTransaction> _purchaseHistory = new();

    public event Action<StoreItem>? ItemPurchased;

    public StoreService(ILogger<StoreService> logger, CurrencyService currencyService)
    {
        _logger = logger;
        _currencyService = currencyService;
        InitializeStore();
    }

    private void InitializeStore()
    {
        _storeItems.Clear();

        // Currency Packs
        _storeItems.Add(new StoreItem
        {
            Id = "gold_small",
            Name = "Small Gold Pouch",
            Description = "500 Gold coins",
            Type = StoreItemType.CurrencyPack,
            Rarity = ItemRarity.Common,
            Price = new List<Currency> { new Currency(CurrencyType.Gems, 10) },
            CurrencyRewards = new List<Currency> { new Currency(CurrencyType.Gold, 500) },
            IconEmoji = "💰"
        });

        _storeItems.Add(new StoreItem
        {
            Id = "gold_medium",
            Name = "Medium Gold Chest",
            Description = "2000 Gold coins",
            Type = StoreItemType.CurrencyPack,
            Rarity = ItemRarity.Uncommon,
            Price = new List<Currency> { new Currency(CurrencyType.Gems, 35) },
            CurrencyRewards = new List<Currency> { new Currency(CurrencyType.Gold, 2000) },
            IconEmoji = "💰",
            IsFeatured = true
        });

        _storeItems.Add(new StoreItem
        {
            Id = "gold_large",
            Name = "Large Gold Vault",
            Description = "10000 Gold coins + Bonus!",
            Type = StoreItemType.CurrencyPack,
            Rarity = ItemRarity.Rare,
            Price = new List<Currency> { new Currency(CurrencyType.Gems, 150) },
            CurrencyRewards = new List<Currency> { new Currency(CurrencyType.Gold, 10000) },
            IconEmoji = "💰"
        });

        _storeItems.Add(new StoreItem
        {
            Id = "gems_small",
            Name = "Handful of Gems",
            Description = "50 Premium Gems",
            Type = StoreItemType.CurrencyPack,
            Rarity = ItemRarity.Rare,
            Price = new List<Currency> { new Currency(CurrencyType.Gold, 5000) },
            CurrencyRewards = new List<Currency> { new Currency(CurrencyType.Gems, 50) },
            IconEmoji = "💎"
        });

        // Consumables
        _storeItems.Add(new StoreItem
        {
            Id = "xp_boost",
            Name = "XP Boost Potion",
            Description = "+50% XP for 1 hour",
            Type = StoreItemType.Consumable,
            Rarity = ItemRarity.Uncommon,
            Price = new List<Currency> { new Currency(CurrencyType.Gold, 200) },
            ExperiencePoints = 500,
            IconEmoji = "🧪"
        });

        _storeItems.Add(new StoreItem
        {
            Id = "health_potion",
            Name = "Greater Health Potion",
            Description = "Restore 100 HP",
            Type = StoreItemType.Consumable,
            Rarity = ItemRarity.Common,
            Price = new List<Currency> { new Currency(CurrencyType.Gold, 50) },
            IconEmoji = "❤️"
        });

        // Gear Packs
        _storeItems.Add(new StoreItem
        {
            Id = "starter_pack",
            Name = "Starter Gear Pack",
            Description = "Basic equipment bundle for new adventurers",
            Type = StoreItemType.GearPack,
            Rarity = ItemRarity.Common,
            Price = new List<Currency> { new Currency(CurrencyType.Gold, 500) },
            UpgradeShards = 10,
            IconEmoji = "🎁",
            IsFeatured = true
        });

        _storeItems.Add(new StoreItem
        {
            Id = "epic_loot_box",
            Name = "Epic Loot Box",
            Description = "Guaranteed Epic or better!",
            Type = StoreItemType.GearPack,
            Rarity = ItemRarity.Epic,
            Price = new List<Currency> { new Currency(CurrencyType.Gems, 100) },
            UpgradeShards = 50,
            IconEmoji = "📦"
        });

        // Special Offers (Limited Time)
        var weekendOffer = new StoreItem
        {
            Id = "weekend_special",
            Name = "Weekend Warrior Bundle",
            Description = "Gold, Gems, and XP Boost!",
            Type = StoreItemType.SpecialOffer,
            Rarity = ItemRarity.Legendary,
            Price = new List<Currency> { new Currency(CurrencyType.Gems, 200) },
            CurrencyRewards = new List<Currency>
            {
                new Currency(CurrencyType.Gold, 5000),
                new Currency(CurrencyType.Gems, 50)
            },
            ExperiencePoints = 1000,
            UpgradeShards = 25,
            IconEmoji = "⭐",
            IsLimitedTime = true,
            ExpiresAt = DateTime.Now.AddDays(2),
            MaxPurchases = 1,
            IsFeatured = true
        };
        _storeItems.Add(weekendOffer);

        // Battle Pass
        _storeItems.Add(new StoreItem
        {
            Id = "battle_pass_season1",
            Name = "Season 1 Battle Pass",
            Description = "Unlock 100 tiers of rewards!",
            Type = StoreItemType.BattlePass,
            Rarity = ItemRarity.Legendary,
            Price = new List<Currency> { new Currency(CurrencyType.Gems, 500) },
            CurrencyRewards = new List<Currency> { new Currency(CurrencyType.BattlePoints, 1000) },
            IconEmoji = "🏆"
        });

        _logger.LogInformation($"Initialized store with {_storeItems.Count} items");
    }

    public List<StoreItem> GetAllItems()
    {
        return _storeItems.Where(i => i.IsAvailable()).ToList();
    }

    public List<StoreItem> GetFeaturedItems()
    {
        return _storeItems.Where(i => i.IsFeatured && i.IsAvailable()).ToList();
    }

    public List<StoreItem> GetItemsByType(StoreItemType type)
    {
        return _storeItems.Where(i => i.Type == type && i.IsAvailable()).ToList();
    }

    public StoreItem? GetItemById(string id)
    {
        return _storeItems.FirstOrDefault(i => i.Id == id);
    }

    public bool CanPurchase(string itemId)
    {
        var item = GetItemById(itemId);
        if (item == null || !item.IsAvailable())
            return false;

        return _currencyService.CanAfford(item.Price);
    }

    public bool TryPurchase(string itemId)
    {
        var item = GetItemById(itemId);
        if (item == null)
        {
            _logger.LogWarning($"Attempted to purchase unknown item: {itemId}");
            return false;
        }

        if (!item.IsAvailable())
        {
            _logger.LogWarning($"Attempted to purchase unavailable item: {item.Name}");
            return false;
        }

        if (!_currencyService.TrySpend(item.Price, $"Purchase: {item.Name}"))
        {
            _logger.LogWarning($"Insufficient funds to purchase: {item.Name}");
            return false;
        }

        // Grant rewards
        if (item.CurrencyRewards.Count > 0)
        {
            _currencyService.AddCurrency(item.CurrencyRewards, $"Reward from: {item.Name}");
        }

        if (item.ExperiencePoints.HasValue)
        {
            _logger.LogInformation($"Granted {item.ExperiencePoints} XP from {item.Name}");
        }

        if (item.UpgradeShards.HasValue)
        {
            _logger.LogInformation($"Granted {item.UpgradeShards} upgrade shards from {item.Name}");
        }

        // Record transaction
        var transaction = new PurchaseTransaction
        {
            ItemId = item.Id,
            ItemName = item.Name,
            PricePaid = item.Price,
            PurchasedAt = DateTime.Now,
            WasRealMoney = item.Price.Any(p => p.Type == CurrencyType.Gems)
        };
        _purchaseHistory.Add(transaction);

        // Update purchase count
        item.PurchaseCount++;

        _logger.LogInformation($"Successfully purchased: {item.Name}");
        ItemPurchased?.Invoke(item);

        return true;
    }

    public List<PurchaseTransaction> GetPurchaseHistory()
    {
        return _purchaseHistory.OrderByDescending(t => t.PurchasedAt).ToList();
    }

    public void RefreshDailyDeals()
    {
        // This would regenerate daily/weekly rotating offers
        _logger.LogInformation("Refreshed daily deals");
    }
}
