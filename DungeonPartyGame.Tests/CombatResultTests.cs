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
        Assert.Equal(0, result.RoundNumber);
        Assert.Null(result.Actor);
        Assert.Null(result.Target);
        Assert.Equal(string.Empty, result.SkillName);
        Assert.Equal(0, result.Damage);
        Assert.False(result.TargetDefeated);
        Assert.False(result.IsFinalTurn);
        Assert.Equal(string.Empty, result.SummaryText);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var result = new CombatResult();
        var actor = CreateTestCharacter("Fighter");
        var target = CreateTestCharacter("Rogue");

        // Act
        result.RoundNumber = 1;
        result.Actor = actor;
        result.Target = target;
        result.SkillName = "Power Strike";
        result.Damage = 25;
        result.TargetDefeated = false;
        result.IsFinalTurn = false;
        result.SummaryText = "Fighter attacks Rogue with Power Strike (25 dmg)\nRogue HP: 15";

        // Assert
        Assert.Equal(1, result.RoundNumber);
        Assert.Equal(actor, result.Actor);
        Assert.Equal(target, result.Target);
        Assert.Equal("Power Strike", result.SkillName);
        Assert.Equal(25, result.Damage);
        Assert.False(result.TargetDefeated);
        Assert.False(result.IsFinalTurn);
        Assert.Equal("Fighter attacks Rogue with Power Strike (25 dmg)\nRogue HP: 15", result.SummaryText);
    }

    private static Character CreateTestCharacter(string name)
    {
        var stats = new Stats(10, 10, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        return new Character(name, Role.Tank, stats, equipment, skills);
    }
}