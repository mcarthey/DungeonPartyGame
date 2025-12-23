using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class CombatSessionTests
{
    [Fact]
    public void Constructor_SetsFightersAndInitiative_HigherDexGoesFirst()
    {
        // Arrange
        var fighterA = CreateTestCharacter("A", 10, 8); // Lower DEX
        var fighterB = CreateTestCharacter("B", 10, 12); // Higher DEX

        // Act
        var session = new CombatSession(fighterA, fighterB);

        // Assert
        Assert.Equal(fighterA, session.FighterA);
        Assert.Equal(fighterB, session.FighterB);
        Assert.Equal(fighterB, session.CurrentAttacker); // B has higher DEX
        Assert.Equal(fighterA, session.CurrentDefender);
        Assert.Equal(1, session.RoundNumber);
        Assert.False(session.IsComplete);
        Assert.Null(session.Winner);
    }

    [Fact]
    public void Constructor_SetsFightersAndInitiative_LowerDexGoesFirst_WhenEqual()
    {
        // Arrange
        var fighterA = CreateTestCharacter("A", 10, 10);
        var fighterB = CreateTestCharacter("B", 10, 10);

        // Act
        var session = new CombatSession(fighterA, fighterB);

        // Assert
        Assert.Equal(fighterA, session.CurrentAttacker); // A goes first when DEX equal
        Assert.Equal(fighterB, session.CurrentDefender);
    }

    [Fact]
    public void AdvanceTurn_SwapsAttackerAndDefender_IncrementsRound()
    {
        // Arrange
        var fighterA = CreateTestCharacter("A", 10, 8);
        var fighterB = CreateTestCharacter("B", 10, 12);
        var session = new CombatSession(fighterA, fighterB);

        // Act
        session.AdvanceTurn();

        // Assert
        Assert.Equal(fighterA, session.CurrentAttacker);
        Assert.Equal(fighterB, session.CurrentDefender);
        Assert.Equal(2, session.RoundNumber);
    }

    [Fact]
    public void AdvanceTurn_DoesNothing_WhenComplete()
    {
        // Arrange
        var fighterA = CreateTestCharacter("A", 10, 8);
        var fighterB = CreateTestCharacter("B", 10, 12);
        var session = new CombatSession(fighterA, fighterB);
        session.CompleteCombat(fighterA);

        // Act
        session.AdvanceTurn();

        // Assert
        Assert.Equal(fighterB, session.CurrentAttacker); // Should not change
        Assert.Equal(fighterA, session.CurrentDefender);
        Assert.Equal(1, session.RoundNumber); // Should not increment
    }

    [Fact]
    public void CompleteCombat_SetsCompleteAndWinner()
    {
        // Arrange
        var fighterA = CreateTestCharacter("A", 10, 8);
        var fighterB = CreateTestCharacter("B", 10, 12);
        var session = new CombatSession(fighterA, fighterB);

        // Act
        session.CompleteCombat(fighterA);

        // Assert
        Assert.True(session.IsComplete);
        Assert.Equal(fighterA, session.Winner);
    }

    private static Character CreateTestCharacter(string name, int strength, int dexterity)
    {
        var stats = new Stats(strength, dexterity, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", 1.0, 0) };
        return new Character(name, stats, equipment, skills);
    }
}