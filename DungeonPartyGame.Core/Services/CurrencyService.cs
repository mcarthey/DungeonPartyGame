using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;

public class CurrencyService
{
    private readonly ILogger<CurrencyService> _logger;
    private readonly PlayerWallet _wallet;

    public event Action<CurrencyType, int, int>? CurrencyChanged; // type, oldAmount, newAmount

    public CurrencyService(ILogger<CurrencyService> logger)
    {
        _logger = logger;
        _wallet = new PlayerWallet();
    }

    public PlayerWallet GetWallet() => _wallet;

    public int GetBalance(CurrencyType type)
    {
        return _wallet.GetCurrency(type);
    }

    public bool CanAfford(CurrencyType type, int amount)
    {
        return _wallet.HasEnough(type, amount);
    }

    public bool CanAfford(List<Currency> costs)
    {
        return _wallet.CanAfford(costs);
    }

    public bool TrySpend(CurrencyType type, int amount, string reason = "")
    {
        var oldAmount = _wallet.GetCurrency(type);

        if (!_wallet.TrySpend(type, amount))
        {
            _logger.LogWarning($"Failed to spend {amount} {type}. Insufficient funds. Current: {oldAmount}");
            return false;
        }

        var newAmount = _wallet.GetCurrency(type);
        _logger.LogInformation($"Spent {amount} {type} for: {reason}. Balance: {oldAmount} -> {newAmount}");
        CurrencyChanged?.Invoke(type, oldAmount, newAmount);

        return true;
    }

    public bool TrySpend(List<Currency> costs, string reason = "")
    {
        if (!_wallet.CanAfford(costs))
        {
            _logger.LogWarning($"Failed to spend multiple currencies for: {reason}. Insufficient funds.");
            return false;
        }

        // Track old amounts for events
        var oldAmounts = costs.ToDictionary(c => c.Type, c => _wallet.GetCurrency(c.Type));

        if (!_wallet.TrySpend(costs))
            return false;

        // Fire events for each currency type
        foreach (var cost in costs)
        {
            var newAmount = _wallet.GetCurrency(cost.Type);
            _logger.LogInformation($"Spent {cost.Amount} {cost.Type} for: {reason}. Balance: {oldAmounts[cost.Type]} -> {newAmount}");
            CurrencyChanged?.Invoke(cost.Type, oldAmounts[cost.Type], newAmount);
        }

        return true;
    }

    public void AddCurrency(CurrencyType type, int amount, string reason = "")
    {
        var oldAmount = _wallet.GetCurrency(type);
        _wallet.Add(type, amount);
        var newAmount = _wallet.GetCurrency(type);

        _logger.LogInformation($"Added {amount} {type} for: {reason}. Balance: {oldAmount} -> {newAmount}");
        CurrencyChanged?.Invoke(type, oldAmount, newAmount);
    }

    public void AddCurrency(Currency currency, string reason = "")
    {
        AddCurrency(currency.Type, currency.Amount, reason);
    }

    public void AddCurrency(List<Currency> currencies, string reason = "")
    {
        foreach (var currency in currencies)
        {
            AddCurrency(currency.Type, currency.Amount, reason);
        }
    }

    public void SetCurrency(CurrencyType type, int amount)
    {
        var oldAmount = _wallet.GetCurrency(type);
        _wallet.SetCurrency(type, amount);
        var newAmount = _wallet.GetCurrency(type);

        _logger.LogInformation($"Set {type} to {amount}. Balance: {oldAmount} -> {newAmount}");
        CurrencyChanged?.Invoke(type, oldAmount, newAmount);
    }

    // Utility methods for UI
    public string GetCurrencySymbol(CurrencyType type)
    {
        return type switch
        {
            CurrencyType.Gold => "💰",
            CurrencyType.Gems => "💎",
            CurrencyType.EventTokens => "🎫",
            CurrencyType.BattlePoints => "⭐",
            _ => "?"
        };
    }

    public string GetFormattedBalance(CurrencyType type)
    {
        var amount = GetBalance(type);
        var symbol = GetCurrencySymbol(type);
        return $"{symbol} {amount:N0}";
    }

    public Dictionary<CurrencyType, int> GetAllBalances()
    {
        return _wallet.GetAllCurrencies();
    }
}
