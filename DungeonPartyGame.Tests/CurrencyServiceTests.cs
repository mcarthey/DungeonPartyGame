using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Tests;

public class CurrencyServiceTests
{
    private readonly Mock<ILogger<CurrencyService>> _mockLogger;
    private readonly CurrencyService _currencyService;

    public CurrencyServiceTests()
    {
        _mockLogger = new Mock<ILogger<CurrencyService>>();
        _currencyService = new CurrencyService(_mockLogger.Object);
    }

    [Fact]
    public void GetBalance_ReturnsCorrectInitialValues()
    {
        // Act
        var gold = _currencyService.GetBalance(CurrencyType.Gold);
        var gems = _currencyService.GetBalance(CurrencyType.Gems);
        var tokens = _currencyService.GetBalance(CurrencyType.EventTokens);
        var points = _currencyService.GetBalance(CurrencyType.BattlePoints);

        // Assert
        Assert.Equal(1000, gold);
        Assert.Equal(50, gems);
        Assert.Equal(0, tokens);
        Assert.Equal(0, points);
    }

    [Fact]
    public void AddCurrency_IncreasesBalance()
    {
        // Arrange
        var initialGold = _currencyService.GetBalance(CurrencyType.Gold);

        // Act
        _currencyService.AddCurrency(CurrencyType.Gold, 500, "Test reward");

        // Assert
        Assert.Equal(initialGold + 500, _currencyService.GetBalance(CurrencyType.Gold));
    }

    [Fact]
    public void AddCurrency_FiresCurrencyChangedEvent()
    {
        // Arrange
        var eventFired = false;
        CurrencyType? firedType = null;
        int oldAmount = 0;
        int newAmount = 0;

        _currencyService.CurrencyChanged += (type, old, newVal) =>
        {
            eventFired = true;
            firedType = type;
            oldAmount = old;
            newAmount = newVal;
        };

        // Act
        _currencyService.AddCurrency(CurrencyType.Gold, 100, "Test");

        // Assert
        Assert.True(eventFired);
        Assert.Equal(CurrencyType.Gold, firedType);
        Assert.Equal(1000, oldAmount);
        Assert.Equal(1100, newAmount);
    }

    [Fact]
    public void TrySpend_WithSufficientFunds_DecreasesBalance()
    {
        // Act
        var success = _currencyService.TrySpend(CurrencyType.Gold, 500, "Test purchase");

        // Assert
        Assert.True(success);
        Assert.Equal(500, _currencyService.GetBalance(CurrencyType.Gold));
    }

    [Fact]
    public void TrySpend_WithInsufficientFunds_ReturnsFalseAndDoesNotDecrease()
    {
        // Act
        var success = _currencyService.TrySpend(CurrencyType.Gold, 2000, "Test purchase");

        // Assert
        Assert.False(success);
        Assert.Equal(1000, _currencyService.GetBalance(CurrencyType.Gold));
    }

    [Fact]
    public void CanAfford_WithSufficientFunds_ReturnsTrue()
    {
        // Act
        var canAfford = _currencyService.CanAfford(CurrencyType.Gold, 500);

        // Assert
        Assert.True(canAfford);
    }

    [Fact]
    public void CanAfford_WithInsufficientFunds_ReturnsFalse()
    {
        // Act
        var canAfford = _currencyService.CanAfford(CurrencyType.Gold, 2000);

        // Assert
        Assert.False(canAfford);
    }

    [Fact]
    public void TrySpend_MultiCurrency_WithSufficientFunds_Succeeds()
    {
        // Arrange
        var costs = new List<Currency>
        {
            new Currency(CurrencyType.Gold, 500),
            new Currency(CurrencyType.Gems, 10)
        };

        // Act
        var success = _currencyService.TrySpend(costs, "Multi-currency purchase");

        // Assert
        Assert.True(success);
        Assert.Equal(500, _currencyService.GetBalance(CurrencyType.Gold));
        Assert.Equal(40, _currencyService.GetBalance(CurrencyType.Gems));
    }

    [Fact]
    public void TrySpend_MultiCurrency_WithInsufficientFunds_FailsAndDoesNotDecrease()
    {
        // Arrange
        var costs = new List<Currency>
        {
            new Currency(CurrencyType.Gold, 500),
            new Currency(CurrencyType.Gems, 100) // Not enough gems
        };

        // Act
        var success = _currencyService.TrySpend(costs, "Multi-currency purchase");

        // Assert
        Assert.False(success);
        Assert.Equal(1000, _currencyService.GetBalance(CurrencyType.Gold)); // Unchanged
        Assert.Equal(50, _currencyService.GetBalance(CurrencyType.Gems)); // Unchanged
    }

    [Fact]
    public void SetCurrency_UpdatesBalanceCorrectly()
    {
        // Act
        _currencyService.SetCurrency(CurrencyType.Gold, 5000);

        // Assert
        Assert.Equal(5000, _currencyService.GetBalance(CurrencyType.Gold));
    }

    [Fact]
    public void SetCurrency_WithNegativeValue_SetsToZero()
    {
        // Act
        _currencyService.SetCurrency(CurrencyType.Gold, -100);

        // Assert
        Assert.Equal(0, _currencyService.GetBalance(CurrencyType.Gold));
    }

    [Fact]
    public void GetCurrencySymbol_ReturnsCorrectSymbols()
    {
        // Act & Assert
        Assert.Equal("💰", _currencyService.GetCurrencySymbol(CurrencyType.Gold));
        Assert.Equal("💎", _currencyService.GetCurrencySymbol(CurrencyType.Gems));
        Assert.Equal("🎫", _currencyService.GetCurrencySymbol(CurrencyType.EventTokens));
        Assert.Equal("⭐", _currencyService.GetCurrencySymbol(CurrencyType.BattlePoints));
    }

    [Fact]
    public void GetFormattedBalance_ReturnsCorrectFormat()
    {
        // Act
        var formatted = _currencyService.GetFormattedBalance(CurrencyType.Gold);

        // Assert
        Assert.Equal("💰 1,000", formatted);
    }

    [Fact]
    public void GetAllBalances_ReturnsAllCurrencies()
    {
        // Act
        var balances = _currencyService.GetAllBalances();

        // Assert
        Assert.Equal(4, balances.Count);
        Assert.Equal(1000, balances[CurrencyType.Gold]);
        Assert.Equal(50, balances[CurrencyType.Gems]);
        Assert.Equal(0, balances[CurrencyType.EventTokens]);
        Assert.Equal(0, balances[CurrencyType.BattlePoints]);
    }

    [Fact]
    public void AddCurrency_WithList_AddsAllCorrectly()
    {
        // Arrange
        var currencies = new List<Currency>
        {
            new Currency(CurrencyType.Gold, 500),
            new Currency(CurrencyType.Gems, 25)
        };

        // Act
        _currencyService.AddCurrency(currencies, "Test reward");

        // Assert
        Assert.Equal(1500, _currencyService.GetBalance(CurrencyType.Gold));
        Assert.Equal(75, _currencyService.GetBalance(CurrencyType.Gems));
    }

    [Fact]
    public void MultipleOperations_MaintainCorrectBalance()
    {
        // Arrange & Act
        _currencyService.AddCurrency(CurrencyType.Gold, 1000, "Reward");
        _currencyService.TrySpend(CurrencyType.Gold, 500, "Purchase");
        _currencyService.AddCurrency(CurrencyType.Gold, 250, "Reward");
        _currencyService.TrySpend(CurrencyType.Gold, 100, "Purchase");

        // Assert
        Assert.Equal(1650, _currencyService.GetBalance(CurrencyType.Gold));
    }
}
