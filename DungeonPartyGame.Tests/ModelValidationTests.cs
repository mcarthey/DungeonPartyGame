using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class ModelValidationTests
{
    [Fact]
    public void Character_Constructor_ValidatesParameters()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Character(null!, CharacterRole.Fighter, new Stats(10, 10, 10, 100)));
        Assert.Throws<ArgumentNullException>(() => new Character("Test", CharacterRole.Fighter, null!));
    }

    [Fact]
    public void Character_AllocateStatPoint_ValidatesStatType()
    {
        // Arrange
        var character = new Character("Test", CharacterRole.Fighter, new Stats(10, 10, 10, 100));
        character.Progression.UnspentStatPoints = 5;

        // Act & Assert
        Assert.False(character.AllocateStatPoint((StatType)999, 1)); // Invalid stat type
    }

    [Fact]
    public void Party_Add_ValidatesCapacity()
    {
        // Arrange
        var party = new Party();
        var character = new Character("Test", CharacterRole.Fighter, new Stats(10, 10, 10, 100));

        // Add 5 characters (max)
        for (int i = 0; i < 5; i++)
        {
            party.Add(new Character($"Char{i}", CharacterRole.Fighter, new Stats(10, 10, 10, 100)));
        }

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => party.Add(character));
    }

    [Fact]
    public void Party_Add_PreventsNullCharacters()
    {
        // Arrange
        var party = new Party();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => party.Add(null!));
    }

    [Fact]
    public void Inventory_AddGold_PreventsNegativeValues()
    {
        // Arrange
        var inventory = new Inventory();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => inventory.AddGold(-100));
    }

    [Fact]
    public void Inventory_SpendGold_ValidatesSufficientFunds()
    {
        // Arrange
        var inventory = new Inventory();
        inventory.AddGold(50);

        // Act & Assert
        Assert.False(inventory.SpendGold(100)); // Not enough gold
        Assert.True(inventory.SpendGold(25)); // Enough gold
        Assert.Equal(25, inventory.Gold);
    }

    [Fact]
    public void Stats_Constructor_ValidatesParameters()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Stats(-1, 10, 10, 100)); // Negative strength
        Assert.Throws<ArgumentException>(() => new Stats(10, 10, 10, 0)); // Zero max health
    }

    [Fact]
    public void CharacterProgression_AddExperience_HandlesLevelUps()
    {
        // Arrange
        var progression = new CharacterProgression();

        // Act
        progression.AddExperience(150); // Should level up once (100 XP needed)

        // Assert
        Assert.Equal(2, progression.Level);
        Assert.Equal(50, progression.Experience); // 150 - 100 = 50 remaining
        Assert.Equal(3, progression.UnspentStatPoints); // 3 points per level
    }

    [Fact]
    public void CharacterProgression_LevelUp_IncreasesStats()
    {
        // Arrange
        var progression = new CharacterProgression();
        progression.AddExperience(100); // Level up to 2

        // Assert
        Assert.Equal(2, progression.Level);
        Assert.Equal(3, progression.UnspentStatPoints);
    }

    [Fact]
    public void StatusEffect_Tick_ReducesDuration()
    {
        // Arrange
        var effect = new StatusEffect("Test", "Test effect", 3, EffectType.StatModifier, 5, StatType.Attack);

        // Act
        effect.Tick();

        // Assert
        Assert.Equal(2, effect.Duration);
    }

    [Fact]
    public void GearInstance_Constructor_ValidatesParameters()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new GearInstance(null!, "definition"));
        Assert.Throws<ArgumentException>(() => new GearInstance("", "definition"));
        Assert.Throws<ArgumentException>(() => new GearInstance("id", null!));
        Assert.Throws<ArgumentException>(() => new GearInstance("id", ""));
        Assert.Throws<ArgumentException>(() => new GearInstance("id", "definition", 0));
        Assert.Throws<ArgumentException>(() => new GearInstance("id", "definition", 1, -1));
    }

    [Fact]
    public void GameSession_AddParty_ValidatesNullParty()
    {
        // Arrange
        var session = new GameSession();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => session.AddParty(null!));
    }

    [Fact]
    public void GameSession_SwitchToParty_ValidatesIndex()
    {
        // Arrange
        var session = new GameSession();
        session.AddParty(new Party());

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => session.SwitchToParty(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => session.SwitchToParty(10));
    }
}