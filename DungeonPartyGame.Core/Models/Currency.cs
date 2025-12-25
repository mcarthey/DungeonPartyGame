namespace DungeonPartyGame.Core.Models;

public enum CurrencyType
{
    Gold,           // Soft currency - earned through gameplay
    Gems,           // Premium currency - IAP or rare rewards
    EventTokens,    // Limited-time event currency
    BattlePoints    // Season/battle pass currency
}

public class Currency
{
    public CurrencyType Type { get; set; }
    public int Amount { get; set; }

    public Currency(CurrencyType type, int amount)
    {
        Type = type;
        Amount = amount;
    }
}

public class PlayerWallet
{
    private readonly Dictionary<CurrencyType, int> _currencies = new();

    public PlayerWallet()
    {
        // Initialize with starting amounts
        _currencies[CurrencyType.Gold] = 1000;
        _currencies[CurrencyType.Gems] = 50;
        _currencies[CurrencyType.EventTokens] = 0;
        _currencies[CurrencyType.BattlePoints] = 0;
    }

    public int GetCurrency(CurrencyType type)
    {
        return _currencies.TryGetValue(type, out var amount) ? amount : 0;
    }

    public bool HasEnough(CurrencyType type, int amount)
    {
        return GetCurrency(type) >= amount;
    }

    public bool CanAfford(Currency cost)
    {
        return HasEnough(cost.Type, cost.Amount);
    }

    public bool CanAfford(List<Currency> costs)
    {
        return costs.All(cost => HasEnough(cost.Type, cost.Amount));
    }

    public bool TrySpend(CurrencyType type, int amount)
    {
        if (!HasEnough(type, amount))
            return false;

        _currencies[type] -= amount;
        return true;
    }

    public bool TrySpend(Currency cost)
    {
        return TrySpend(cost.Type, cost.Amount);
    }

    public bool TrySpend(List<Currency> costs)
    {
        // Check if we can afford all costs first
        if (!CanAfford(costs))
            return false;

        // Spend all costs
        foreach (var cost in costs)
        {
            _currencies[cost.Type] -= cost.Amount;
        }

        return true;
    }

    public void Add(CurrencyType type, int amount)
    {
        if (!_currencies.ContainsKey(type))
            _currencies[type] = 0;

        _currencies[type] += amount;
    }

    public void Add(Currency currency)
    {
        Add(currency.Type, currency.Amount);
    }

    public void SetCurrency(CurrencyType type, int amount)
    {
        _currencies[type] = Math.Max(0, amount);
    }

    // For UI display
    public Dictionary<CurrencyType, int> GetAllCurrencies()
    {
        return new Dictionary<CurrencyType, int>(_currencies);
    }
}
