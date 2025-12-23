using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class CharacterTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100);
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };

        // Act
        var character = new Character("TestChar", Role.Tank, stats);
        character.UnlockedSkills.AddRange(skills);
        character.EquippedSkills.AddRange(skills);

        // Assert
        Assert.Equal("TestChar", character.Name);
        Assert.Equal(Role.Tank, character.Role);
        Assert.Equal(1, character.Level);
        Assert.Equal(stats, character.Stats);
        Assert.Equal(skills, character.UnlockedSkills);
        Assert.Equal(skills, character.EquippedSkills);
    }

    [Fact]
    public void IsAlive_ReturnsTrue_WhenCurrentHealthGreaterThanZero()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100) { CurrentHealth = 50 };
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        var character = new Character("TestChar", Role.Tank, stats, equipment, skills);

        // Act & Assert
        Assert.True(character.IsAlive);
    }

    [Fact]
    public void IsAlive_ReturnsFalse_WhenCurrentHealthIsZero()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100) { CurrentHealth = 0 };
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        var character = new Character("TestChar", Role.Tank, stats, equipment, skills);

        // Act & Assert
        Assert.False(character.IsAlive);
    }

    [Fact]
    public void ApplyDamage_ReducesCurrentHealth()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100) { CurrentHealth = 50 };
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        var character = new Character("TestChar", Role.Tank, stats, equipment, skills);

        // Act
        character.ApplyDamage(20);

        // Assert
        Assert.Equal(30, character.Stats.CurrentHealth);
    }

    [Fact]
    public void ApplyDamage_DoesNotGoBelowZero()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100) { CurrentHealth = 10 };
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        var character = new Character("TestChar", Role.Tank, stats, equipment, skills);

        // Act
        character.ApplyDamage(50);

        // Assert
        Assert.Equal(0, character.Stats.CurrentHealth);
    }

    [Fact]
    public void GainLevel_IncreasesLevelAndMaxHealthAndRestoresCurrentHealth()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100) { CurrentHealth = 50 };
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        var character = new Character("TestChar", Role.Tank, stats, equipment, skills);

        // Act
        character.GainLevel();

        // Assert
        Assert.Equal(2, character.Level);
        Assert.Equal(110, character.Stats.MaxHealth);
        Assert.Equal(110, character.Stats.CurrentHealth);
    }

    [Fact]
    public void ChooseSkill_ReturnsAvailableSkill()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skill1 = new Skill("Skill1", "First skill", TargetingRule.SingleEnemy, 1.0, 2);
        var skill2 = new Skill("Skill2", "Second skill", TargetingRule.SingleEnemy, 1.5, 0);
        var skills = new List<Skill> { skill1, skill2 };
        var character = new Character("TestChar", Role.Tank, stats, equipment, skills);

        // Act
        var chosenSkill = character.ChooseSkill(1);

        // Assert
        Assert.Equal(skill1, chosenSkill); // skill1 is first in list and available (never used)
    }

    [Fact]
    public void ChooseSkill_ReturnsFirstSkill_WhenNoneAvailable()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skill1 = new Skill("Skill1", "First skill", TargetingRule.SingleEnemy, 1.0, 2);
        var skill2 = new Skill("Skill2", "Second skill", TargetingRule.SingleEnemy, 1.5, 2);
        var skills = new List<Skill> { skill1, skill2 };
        var character = new Character("TestChar", Role.Tank, stats, equipment, skills);

        // Act
        var chosenSkill = character.ChooseSkill(1);

        // Assert
        Assert.Equal(skill1, chosenSkill);
    }

    [Fact]
    public void UnlockedSkills_ContainsSkillsPassedToConstructor()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        var character = new Character("TestChar", Role.Tank, stats, equipment, skills);

        // Act & Assert
        Assert.Equal(skills, character.UnlockedSkills);
    }

    [Fact]
    public void EquippedSkills_ContainsSkillsPassedToConstructor()
    {
        // Arrange
        var stats = new Stats(10, 10, 10, 100);
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        var character = new Character("TestChar", Role.Tank, stats);
        character.UnlockedSkills.AddRange(skills);
        character.EquippedSkills.AddRange(skills);

        // Act & Assert
        Assert.Equal(skills, character.EquippedSkills);
    }
}