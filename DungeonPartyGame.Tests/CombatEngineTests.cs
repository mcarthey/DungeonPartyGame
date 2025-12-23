using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Moq;
using Xunit;

namespace DungeonPartyGame;

public class CombatEngineTests
{
    private readonly Mock<DiceService> _diceServiceMock;
    private readonly CombatEngine _combatEngine;

    public CombatEngineTests()
    {
        _diceServiceMock = new Mock<DiceService>(MockBehavior.Loose, (Random)null);
        _combatEngine = new CombatEngine(_diceServiceMock.Object);
    }

    [Fact]
    public void CreateSession_ReturnsNewCombatSession()
    {
        // Arrange
        var fighterA = CreateTestCharacter("A");
        var fighterB = CreateTestCharacter("B");

        // Act
        var session = _combatEngine.CreateSession(fighterA, fighterB);

        // Assert
        Assert.Equal(fighterA, session.FighterA);
        Assert.Equal(fighterB, session.FighterB);
    }

    [Fact]
    public void ExecuteRound_ThrowsException_WhenSessionComplete()
    {
        // Arrange
        var fighterA = CreateTestCharacter("A");
        var fighterB = CreateTestCharacter("B");
        var session = _combatEngine.CreateSession(fighterA, fighterB);
        session.CompleteCombat(fighterA);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _combatEngine.ExecuteRound(session));
        Assert.Equal("Combat session is already complete.", exception.Message);
    }

    [Fact]
    public void ExecuteRound_CalculatesDamageCorrectly()
    {
        // Arrange
        _diceServiceMock.Setup(d => d.Roll(5, 10)).Returns(7); // Base damage = 7
        var fighterA = CreateTestCharacter("A", strength: 3);
        var fighterB = CreateTestCharacter("B");
        var session = _combatEngine.CreateSession(fighterA, fighterB);

        // Act
        var result = _combatEngine.ExecuteRound(session);

        // Assert
        Assert.Equal(1, result.Round);
        Assert.Equal("A", result.Attacker);
        Assert.Equal("B", result.Defender);
        Assert.Equal("Attack", result.SkillUsed);
        Assert.Equal(10, result.Damage); // 7 * 1.0 + 3 = 10
        Assert.Equal(90, result.DefenderHP); // 100 - 10
        Assert.False(result.IsDefeated);
        Assert.False(result.IsFinalRound);
    }

    [Fact]
    public void ExecuteRound_CompletesCombat_WhenDefenderDies()
    {
        // Arrange
        _diceServiceMock.Setup(d => d.Roll(5, 10)).Returns(10); // Max damage
        var fighterA = CreateTestCharacter("A", strength: 5);
        var fighterB = CreateTestCharacter("B", maxHealth: 15); // Low HP
        var session = _combatEngine.CreateSession(fighterA, fighterB);

        // Act
        var result = _combatEngine.ExecuteRound(session);

        // Assert
        Assert.True(result.IsDefeated);
        Assert.True(result.IsFinalRound);
        Assert.Equal("A defeats B!", result.SummaryText);
        Assert.True(session.IsComplete);
        Assert.Equal(fighterA, session.Winner);
    }

    [Fact]
    public void ExecuteRound_AdvancesTurn_WhenCombatContinues()
    {
        // Arrange
        _diceServiceMock.Setup(d => d.Roll(5, 10)).Returns(1); // Min damage
        var fighterA = CreateTestCharacter("A");
        var fighterB = CreateTestCharacter("B");
        var session = _combatEngine.CreateSession(fighterA, fighterB);

        // Act
        _combatEngine.ExecuteRound(session);

        // Assert
        Assert.Equal(fighterB, session.CurrentAttacker);
        Assert.Equal(fighterA, session.CurrentDefender);
        Assert.Equal(2, session.RoundNumber);
    }

    private static Character CreateTestCharacter(string name, int strength = 10, int maxHealth = 100)
    {
        var stats = new Stats(strength, 10, 10, maxHealth);
        var equipment = new Equipment(new Weapon("Sword", 5, 10));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", 1.0, 0) };
        return new Character(name, stats, equipment, skills);
    }
}