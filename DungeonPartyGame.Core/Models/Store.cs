namespace DungeonPartyGame.Core.Models;

public enum StoreItemType
{
    CurrencyPack,   // Buy gold/gems
    GearPack,       // Loot boxes or specific gear
    Consumable,     // Potions, boosters
    Cosmetic,       // Skins, effects
    BattlePass,     // Seasonal content
    SpecialOffer    // Limited-time bundles
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public class StoreItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StoreItemType Type { get; set; }
    public ItemRarity Rarity { get; set; }

    // Price
    public List<Currency> Price { get; set; } = new();

    // Rewards when purchased
    public List<Currency> CurrencyRewards { get; set; } = new();
    public List<string> GearIds { get; set; } = new(); // IDs of gear items
    public int? UpgradeShards { get; set; }
    public int? ExperiencePoints { get; set; }

    // Metadata
    public bool IsLimitedTime { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxPurchases { get; set; }
    public int PurchaseCount { get; set; }
    public bool IsFeatured { get; set; }
    public string IconEmoji { get; set; } = "📦";

    public bool IsAvailable()
    {
        if (IsLimitedTime && ExpiresAt.HasValue && DateTime.Now > ExpiresAt.Value)
            return false;

        if (MaxPurchases.HasValue && PurchaseCount >= MaxPurchases.Value)
            return false;

        return true;
    }

    public string GetPriceDisplay()
    {
        if (Price.Count == 0)
            return "Free";

        return string.Join(", ", Price.Select(p => $"{GetCurrencySymbol(p.Type)}{p.Amount}"));
    }

    public TimeSpan? GetTimeRemaining()
    {
        if (!IsLimitedTime || !ExpiresAt.HasValue)
            return null;

        var remaining = ExpiresAt.Value - DateTime.Now;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    private static string GetCurrencySymbol(CurrencyType type)
    {
        return type switch
        {
            CurrencyType.Gold => "💰",
            CurrencyType.Gems => "💎",
            CurrencyType.EventTokens => "🎫",
            CurrencyType.BattlePoints => "⭐",
            _ => ""
        };
    }
}

public class PurchaseTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public List<Currency> PricePaid { get; set; } = new();
    public DateTime PurchasedAt { get; set; } = DateTime.Now;
    public bool WasRealMoney { get; set; } // Track IAP vs in-game currency
}
