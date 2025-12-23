using DungeonPartyGame.Core.Services;
using Xunit;

namespace DungeonPartyGame;

public class DiceServiceTests
{
    [Fact]
    public void Constructor_UsesProvidedRandom()
    {
        // Arrange
        var random = new TestRandom(5);

        // Act
        var diceService = new DiceService(random);

        // Assert
        Assert.Equal(6, diceService.Roll(1, 10)); // 1 + 5
    }

    [Fact]
    public void Constructor_UsesNewRandom_WhenNoneProvided()
    {
        // Act
        var diceService = new DiceService();

        // Assert - Just verify it doesn't throw and returns a value in range
        var result = diceService.Roll(1, 6);
        Assert.InRange(result, 1, 6);
    }

    [Fact]
    public void Roll_ReturnsValueInRange()
    {
        // Arrange
        var random = new TestRandom(3);
        var diceService = new DiceService(random);

        // Act
        var result = diceService.Roll(5, 15);

        // Assert
        Assert.Equal(8, result); // 5 + 3 = 8
    }

    [Fact]
    public void Roll_IncludesMaxValue()
    {
        // Arrange
        var random = new TestRandom(5); // Next(5, 11) would return 5+5=10
        var diceService = new DiceService(random);

        // Act
        var result = diceService.Roll(5, 10);

        // Assert
        Assert.Equal(10, result);
    }

    private class TestRandom : Random
    {
        private readonly int _fixedValue;

        public TestRandom(int fixedValue)
        {
            _fixedValue = fixedValue;
        }

        public override int Next(int minValue, int maxValue)
        {
            return minValue + _fixedValue;
        }
    }
}