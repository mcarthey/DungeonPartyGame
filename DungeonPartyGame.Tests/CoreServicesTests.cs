using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Xunit;

namespace DungeonPartyGame;

public class CoreServicesTests
{
    [Fact]
    public void DiceService_Roll_ReturnsValueInRange()
    {
        // Arrange
        var diceService = new DiceService(new Random(42)); // Fixed seed for predictable results

        // Act
        var result = diceService.Roll(1, 6);

        // Assert
        Assert.InRange(result, 1, 6);
    }

    [Fact]
    public void DiceService_Roll_IsDeterministicWithFixedSeed()
    {
        // Arrange
        var random1 = new Random(123);
        var random2 = new Random(123);
        var dice1 = new DiceService(random1);
        var dice2 = new DiceService(random2);

        // Act
        var result1 = dice1.Roll(1, 10);
        var result2 = dice2.Roll(1, 10);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GearService_GetGearDefinition_ReturnsValidDefinition()
    {
        // Arrange
        var gearService = new GearService();

        // Act
        var definition = gearService.GetGearDefinition("iron_sword");

        // Assert
        Assert.NotNull(definition);
        Assert.Equal("iron_sword", definition.Id);
        Assert.Equal("Iron Sword", definition.Name);
        Assert.Equal(GearRarity.Common, definition.Rarity);
    }

    [Fact]
    public void GearService_GetGearDefinition_ReturnsNullForInvalidId()
    {
        // Arrange
        var gearService = new GearService();

        // Act
        var definition = gearService.GetGearDefinition("nonexistent_gear");

        // Assert
        Assert.Null(definition);
    }

    [Fact]
    public void GearService_GetEffectiveStats_IncludesGearBonuses()
    {
        // Arrange
        var gearService = new GearService();
        var character = new Character("Test", CharacterRole.Fighter, new Stats(10, 10, 10, 100));

        var sword = new GearInstance("sword", "iron_sword");
        character.Equipment[GearSlot.Weapon] = sword;

        // Act
        var effectiveStats = gearService.GetEffectiveStats(character);

        // Assert
        Assert.Equal(15, effectiveStats.Attack); // 10 base + 5 from sword
        Assert.Equal(10, effectiveStats.Defense); // No defense bonus
    }

    [Fact]
    public void SkillTreeService_GetSkillTree_ReturnsValidTree()
    {
        // Arrange
        var skillTreeService = new SkillTreeService();

        // Act
        var tree = skillTreeService.GetSkillTree(CharacterRole.Fighter);

        // Assert
        Assert.NotNull(tree);
        Assert.Equal(CharacterRole.Fighter.ToString().ToLower(), tree.TreeId);
        Assert.NotEmpty(tree.Nodes);
    }

    [Fact]
    public void SkillTreeService_GetSkillTree_ReturnsNullForInvalidRole()
    {
        // Arrange
        var skillTreeService = new SkillTreeService();

        // Act
        var tree = skillTreeService.GetSkillTree((CharacterRole)999);

        // Assert
        Assert.Null(tree);
    }

    [Fact]
    public void CharacterDevelopmentService_AllocateStatPoint_ValidatesBounds()
    {
        // Arrange
        var skillTreeService = new SkillTreeService();
        var character = new Character("Test", CharacterRole.Fighter, new Stats(10, 10, 10, 100));
        var devService = new CharacterDevelopmentService(skillTreeService);

        // Act & Assert - Not enough points
        Assert.False(character.AllocateStatPoint(StatType.Attack, 10));

        // Arrange - Add points
        character.Progression.UnspentStatPoints = 5;

        // Act & Assert - Valid allocation
        Assert.True(character.AllocateStatPoint(StatType.Attack, 2));
        Assert.Equal(12, character.Stats.Strength);
        Assert.Equal(3, character.Progression.UnspentStatPoints);
    }

    [Fact]
    public void CharacterDevelopmentService_AllocateStatPoint_PreventsNegativeValues()
    {
        // Arrange
        var character = new Character("Test", CharacterRole.Fighter, new Stats(10, 10, 10, 100));
        character.Progression.UnspentStatPoints = 5;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => character.AllocateStatPoint(StatType.Attack, -1));
        Assert.Equal(10, character.Stats.Strength); // Unchanged
        Assert.Equal(5, character.Progression.UnspentStatPoints); // Unchanged
    }

    [Fact]
    public void GameEngine_StartNewGame_CreatesValidSession()
    {
        // Arrange
        var party = new Party();
        var encounterState = new EncounterState(party);
        var gameEngine = new GameEngine(encounterState);

        // Act
        var session = gameEngine.GetState();

        // Assert
        Assert.NotNull(session);
        Assert.Equal(party, session.Party);
    }

    [Fact]
    public void GameEngine_AddCharacterToGame_ValidatesPartyLimits()
    {
        // Arrange
        var party = new Party();
        var encounterState = new EncounterState(party);
        var gameEngine = new GameEngine(encounterState);
        var character = new Character("Test", CharacterRole.Fighter, new Stats(10, 10, 10, 100));

        // Act
        party.Add(character);

        // Assert
        Assert.Single(party.Members);
    }
}