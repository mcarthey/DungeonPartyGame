using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class SkillTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Act
        var skill = new Skill("Power Strike", "A heavy blow", TargetingRule.SingleEnemy, 1.3, 2);

        // Assert
        Assert.Equal("Power Strike", skill.Name);
        Assert.Equal("A heavy blow", skill.Description);
        Assert.Equal(TargetingRule.SingleEnemy, skill.Targeting);
        Assert.Equal(1.3, skill.DamageMultiplier);
        Assert.Equal(2, skill.Cooldown);
    }

    [Fact]
    public void CanUse_ReturnsTrue_WhenCooldownExpired()
    {
        // Arrange
        var skill = new Skill("Power Strike", "A heavy blow", TargetingRule.SingleEnemy, 1.3, 2);
        skill.MarkUsed(1);

        // Act & Assert
        Assert.False(skill.CanUse(2)); // Round 2 - 1 = 1 < 2
        Assert.True(skill.CanUse(3));  // Round 3 - 1 = 2 >= 2
    }

    [Fact]
    public void CanUse_ReturnsTrue_WhenNeverUsed()
    {
        // Arrange
        var skill = new Skill("Power Strike", "A heavy blow", TargetingRule.SingleEnemy, 1.3, 2);

        // Act & Assert
        Assert.True(skill.CanUse(1));
    }

    [Fact]
    public void CanUse_ReturnsTrue_WhenCooldownIsZero()
    {
        // Arrange
        var skill = new Skill("Basic Attack", "Normal attack", TargetingRule.SingleEnemy, 1.0, 0);
        skill.MarkUsed(1);

        // Act & Assert
        Assert.True(skill.CanUse(2));
    }

    [Fact]
    public void MarkUsed_UpdatesLastUsedRound()
    {
        // Arrange
        var skill = new Skill("Power Strike", "A heavy blow", TargetingRule.SingleEnemy, 1.3, 2);

        // Act
        skill.MarkUsed(5);

        // Assert
        Assert.False(skill.CanUse(6)); // 6 - 5 = 1 < 2
        Assert.True(skill.CanUse(7));  // 7 - 5 = 2 >= 2
    }
}