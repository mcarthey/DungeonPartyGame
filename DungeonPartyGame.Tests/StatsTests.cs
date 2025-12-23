using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class StatsTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Act
        var stats = new Stats(15, 12, 10, 100);

        // Assert
        Assert.Equal(15, stats.Strength);
        Assert.Equal(12, stats.Dexterity);
        Assert.Equal(10, stats.Intelligence);
        Assert.Equal(100, stats.MaxHealth);
        Assert.Equal(100, stats.CurrentHealth);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new Stats(15, 12, 10, 100);
        original.CurrentHealth = 50;

        // Act
        var clone = original.Clone();

        // Assert
        Assert.Equal(original.Strength, clone.Strength);
        Assert.Equal(original.Dexterity, clone.Dexterity);
        Assert.Equal(original.Intelligence, clone.Intelligence);
        Assert.Equal(original.MaxHealth, clone.MaxHealth);
        Assert.Equal(original.CurrentHealth, clone.CurrentHealth);

        // Modify original and ensure clone is unaffected
        original.Strength = 20;
        Assert.Equal(15, clone.Strength);
    }
}