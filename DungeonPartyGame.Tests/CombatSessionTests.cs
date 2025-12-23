using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class CombatSessionTests
{
    [Fact]
    public void Constructor_SetsPartiesAndGeneratesTurnOrder()
    {
        // Arrange
        var partyA = new Party();
        var charA1 = CreateTestCharacter("A1", 10, 8);
        var charA2 = CreateTestCharacter("A2", 10, 12);
        partyA.Add(charA1);
        partyA.Add(charA2);

        var partyB = new Party();
        var charB1 = CreateTestCharacter("B1", 10, 10);
        partyB.Add(charB1);

        // Act
        var session = new CombatSession(partyA, partyB);

        // Assert
        Assert.Equal(partyA, session.PartyA);
        Assert.Equal(partyB, session.PartyB);
        Assert.Equal(1, session.RoundNumber);
        Assert.False(session.IsComplete);
        Assert.Null(session.WinningParty);
        Assert.Equal(3, session.TurnQueue.Count); // All characters in order
    }

    [Fact]
    public void Constructor_OrdersTurnQueueByDexterityDescending()
    {
        // Arrange
        var partyA = new Party();
        var charA = CreateTestCharacter("A", 10, 5); // Low DEX
        partyA.Add(charA);

        var partyB = new Party();
        var charB = CreateTestCharacter("B", 10, 15); // High DEX
        partyB.Add(charB);

        // Act
        var session = new CombatSession(partyA, partyB);

        // Assert
        var firstTurn = session.GetCurrentTurn();
        Assert.NotNull(firstTurn);
        Assert.Equal(charB, firstTurn.Actor); // Higher DEX goes first
        Assert.Equal(partyB, firstTurn.OwningParty);
    }

    [Fact]
    public void GetCurrentTurn_ReturnsNull_WhenTurnQueueEmpty()
    {
        // Arrange
        var partyA = new Party();
        var partyB = new Party();
        var session = new CombatSession(partyA, partyB);

        // Clear the queue
        while (session.TurnQueue.Count > 0)
        {
            session.TurnQueue.Dequeue();
        }

        // Act
        var currentTurn = session.GetCurrentTurn();

        // Assert
        Assert.Null(currentTurn);
    }

    [Fact]
    public void AdvanceTurn_DequeuesCurrentTurn()
    {
        // Arrange
        var partyA = new Party();
        var charA = CreateTestCharacter("A", 10, 10);
        partyA.Add(charA);

        var partyB = new Party();
        var charB = CreateTestCharacter("B", 10, 10);
        partyB.Add(charB);

        var session = new CombatSession(partyA, partyB);
        var initialCount = session.TurnQueue.Count;

        // Act
        session.AdvanceTurn();

        // Assert
        Assert.Equal(initialCount - 1, session.TurnQueue.Count);
    }

    [Fact]
    public void AdvanceTurn_GeneratesNewRound_WhenQueueEmpty()
    {
        // Arrange
        var partyA = new Party();
        var charA = CreateTestCharacter("A", 10, 10);
        partyA.Add(charA);

        var partyB = new Party();
        var charB = CreateTestCharacter("B", 10, 10);
        partyB.Add(charB);

        var session = new CombatSession(partyA, partyB);

        // Clear the queue to simulate end of round
        while (session.TurnQueue.Count > 0)
        {
            session.TurnQueue.Dequeue();
        }

        // Act
        session.AdvanceTurn();

        // Assert
        Assert.Equal(2, session.RoundNumber);
        Assert.Equal(2, session.TurnQueue.Count); // New round generated
    }

    [Fact]
    public void AdvanceTurn_SetsComplete_WhenPartyADefeated()
    {
        // Arrange
        var partyA = new Party();
        var charA = CreateTestCharacter("A", 10, 10);
        charA.ApplyDamage(1000); // Kill character
        partyA.Add(charA);

        var partyB = new Party();
        var charB = CreateTestCharacter("B", 10, 10);
        partyB.Add(charB);

        var session = new CombatSession(partyA, partyB);

        // Act
        session.AdvanceTurn();

        // Assert
        Assert.True(session.IsComplete);
        Assert.Equal(partyB, session.WinningParty);
    }

    [Fact]
    public void AdvanceTurn_SetsComplete_WhenPartyBDefeated()
    {
        // Arrange
        var partyA = new Party();
        var charA = CreateTestCharacter("A", 10, 10);
        partyA.Add(charA);

        var partyB = new Party();
        var charB = CreateTestCharacter("B", 10, 10);
        charB.ApplyDamage(1000); // Kill character
        partyB.Add(charB);

        var session = new CombatSession(partyA, partyB);

        // Act
        session.AdvanceTurn();

        // Assert
        Assert.True(session.IsComplete);
        Assert.Equal(partyA, session.WinningParty);
    }

    [Fact]
    public void AdvanceTurn_DoesNothing_WhenAlreadyComplete()
    {
        // Arrange
        var partyA = new Party();
        var charA = CreateTestCharacter("A", 10, 10);
        partyA.Add(charA);

        var partyB = new Party();
        var charB = CreateTestCharacter("B", 10, 10);
        partyB.Add(charB);

        var session = new CombatSession(partyA, partyB);
        session.CompleteCombat(partyA);
        var initialRound = session.RoundNumber;

        // Act
        session.AdvanceTurn();

        // Assert
        Assert.Equal(initialRound, session.RoundNumber);
    }

    [Fact]
    public void CompleteCombat_SetsCompleteAndWinner()
    {
        // Arrange
        var partyA = new Party();
        var charA = CreateTestCharacter("A", 10, 10);
        partyA.Add(charA);

        var partyB = new Party();
        var charB = CreateTestCharacter("B", 10, 10);
        partyB.Add(charB);

        var session = new CombatSession(partyA, partyB);

        // Act
        session.CompleteCombat(partyA);

        // Assert
        Assert.True(session.IsComplete);
        Assert.Equal(partyA, session.WinningParty);
    }

    private static Character CreateTestCharacter(string name, int strength, int dexterity)
    {
        var stats = new Stats(strength, dexterity, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        return new Character(name, Role.Tank, stats, equipment, skills);
    }
}