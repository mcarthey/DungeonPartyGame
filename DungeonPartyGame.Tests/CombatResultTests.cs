using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class CombatResultTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var result = new CombatResult();

        // Assert
        Assert.Equal(0, result.Round);
        Assert.Equal(string.Empty, result.Attacker);
        Assert.Equal(string.Empty, result.Defender);
        Assert.Equal(string.Empty, result.SkillUsed);
        Assert.Equal(0, result.Damage);
        Assert.Equal(0, result.DefenderHP);
        Assert.False(result.IsDefeated);
        Assert.Equal(string.Empty, result.LogMessage);
        Assert.False(result.IsFinalRound);
        Assert.Equal(string.Empty, result.SummaryText);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var result = new CombatResult();

        // Act
        result.Round = 1;
        result.Attacker = "Fighter";
        result.Defender = "Rogue";
        result.SkillUsed = "Power Strike";
        result.Damage = 25;
        result.DefenderHP = 15;
        result.IsDefeated = false;
        result.LogMessage = "Round 1\nFighter uses Power Strike\nDamage: 25\nRogue HP: 15";
        result.IsFinalRound = false;
        result.SummaryText = "Fighter attacks Rogue for 25 damage.";

        // Assert
        Assert.Equal(1, result.Round);
        Assert.Equal("Fighter", result.Attacker);
        Assert.Equal("Rogue", result.Defender);
        Assert.Equal("Power Strike", result.SkillUsed);
        Assert.Equal(25, result.Damage);
        Assert.Equal(15, result.DefenderHP);
        Assert.False(result.IsDefeated);
        Assert.Equal("Round 1\nFighter uses Power Strike\nDamage: 25\nRogue HP: 15", result.LogMessage);
        Assert.False(result.IsFinalRound);
        Assert.Equal("Fighter attacks Rogue for 25 damage.", result.SummaryText);
    }
}