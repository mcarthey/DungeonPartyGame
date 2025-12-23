using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Moq;
using Xunit;

namespace DungeonPartyGame;

public class CombatEngineTests
{
    private readonly Mock<DiceService> _diceServiceMock;
    private readonly Mock<GearService> _gearServiceMock;
    private readonly Mock<ISkillSelector> _skillSelectorMock;
    private readonly CombatEngine _combatEngine;

    public CombatEngineTests()
    {
        _diceServiceMock = new Mock<DiceService>(MockBehavior.Loose, (Random)null);
        _gearServiceMock = new Mock<GearService>();
        _skillSelectorMock = new Mock<ISkillSelector>();
        _combatEngine = new CombatEngine(_diceServiceMock.Object, _gearServiceMock.Object, _skillSelectorMock.Object);
    }

    [Fact]
    public void CreateSession_ReturnsNewCombatSession()
    {
        // Arrange
        var partyA = CreateTestParty("A");
        var partyB = CreateTestParty("B");

        // Act
        var session = _combatEngine.CreateSession(partyA, partyB);

        // Assert
        Assert.Equal(partyA, session.PartyA);
        Assert.Equal(partyB, session.PartyB);
    }

    [Fact]
    public void ExecuteRound_ThrowsException_WhenSessionComplete()
    {
        // Arrange
        var partyA = CreateTestParty("A");
        var partyB = CreateTestParty("B");
        var session = _combatEngine.CreateSession(partyA, partyB);
        session.CompleteCombat(partyA);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _combatEngine.ExecuteRound(session));
        Assert.Equal("Combat session is already complete.", exception.Message);
    }

    [Fact]
    public void ExecuteRound_ThrowsException_WhenNoCurrentTurn()
    {
        // Arrange
        var partyA = CreateTestParty("A");
        var partyB = CreateTestParty("B");
        var session = _combatEngine.CreateSession(partyA, partyB);

        // Clear turn queue
        while (session.TurnQueue.Count > 0)
        {
            session.TurnQueue.Dequeue();
        }

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _combatEngine.ExecuteRound(session));
        Assert.Equal("No current turn available.", exception.Message);
    }

    [Fact]
    public void ExecuteRound_ThrowsException_WhenNoValidTarget()
    {
        // Arrange
        var partyA = CreateTestParty("A");
        var partyB = new Party(); // Empty party
        var session = _combatEngine.CreateSession(partyA, partyB);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _combatEngine.ExecuteRound(session));
        Assert.Equal("No valid target found.", exception.Message);
    }

    [Fact]
    public void ExecuteRound_ThrowsNotImplementedException_ForUnsupportedTargeting()
    {
        // Arrange
        var partyA = new Party();
        var charA = CreateTestCharacter("A", Role.Tank);
        var skill = new Skill("Area Attack", "Attacks all", TargetingRule.AllEnemies, 1.0, 0);
        charA.UnlockedSkills.Clear();
        charA.UnlockedSkills.Add(skill);
        charA.EquippedSkills.Clear();
        charA.EquippedSkills.Add(skill);
        partyA.Add(charA);

        var partyB = CreateTestParty("B");
        var session = _combatEngine.CreateSession(partyA, partyB);

        _skillSelectorMock.Setup(s => s.SelectSkill(charA, session)).Returns(skill);

        // Act & Assert
        var exception = Assert.Throws<NotImplementedException>(() => _combatEngine.ExecuteRound(session));
        Assert.Equal("Targeting rule AllEnemies not yet implemented.", exception.Message);
    }

    [Fact]
    public void ExecuteRound_CalculatesDamageCorrectly()
    {
        // Arrange
        _diceServiceMock.Setup(d => d.Roll(5, 10)).Returns(7); // Base damage = 7
        var partyA = CreateTestParty("A", strength: 3);
        var partyB = CreateTestParty("B");
        var session = _combatEngine.CreateSession(partyA, partyB);

        var actor = partyA.Members[0];
        var target = partyB.Members[0];
        _gearServiceMock.Setup(g => g.GetEffectiveStats(actor)).Returns(new EffectiveStats(3, 0, 100, 0, 0));
        _gearServiceMock.Setup(g => g.GetEffectiveStats(target)).Returns(new EffectiveStats(0, 0, 100, 0, 0));

        var skill = new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0);
        _skillSelectorMock.Setup(s => s.SelectSkill(actor, session)).Returns(skill);

        // Act
        var result = _combatEngine.ExecuteRound(session);

        // Assert
        Assert.Equal(1, result.RoundNumber);
        Assert.Equal("A", result.Actor.Name);
        Assert.Equal("B", result.Target.Name);
        Assert.Equal("Attack", result.SkillName);
        Assert.Equal(10, result.Damage); // 7 + 3 = 10
        Assert.Equal(90, result.Target.Stats.CurrentHealth); // 100 - 10
        Assert.False(result.TargetDefeated);
        Assert.False(result.IsFinalTurn);
        Assert.Contains("A uses Attack on B (10 dmg)", result.SummaryText);
    }

    [Fact]
    public void ExecuteRound_CompletesCombat_WhenTargetDies()
    {
        // Arrange
        _diceServiceMock.Setup(d => d.Roll(5, 10)).Returns(10); // Max damage
        var partyA = CreateTestParty("A", strength: 5);
        var partyB = new Party();
        var charB = CreateTestCharacter("B", Role.Tank, maxHealth: 15); // Low HP
        partyB.Add(charB);
        var session = _combatEngine.CreateSession(partyA, partyB);

        // Act
        var result = _combatEngine.ExecuteRound(session);

        // Assert
        Assert.True(result.TargetDefeated);
        Assert.True(result.IsFinalTurn);
        Assert.Contains("Victory!", result.SummaryText);
        Assert.True(session.IsComplete);
        Assert.Equal(partyA, session.WinningParty);
    }

    [Fact]
    public void ExecuteRound_AdvancesTurn()
    {
        // Arrange
        _diceServiceMock.Setup(d => d.Roll(5, 10)).Returns(1); // Min damage
        var partyA = CreateTestParty("A");
        var partyB = CreateTestParty("B");
        var session = _combatEngine.CreateSession(partyA, partyB);
        var initialQueueCount = session.TurnQueue.Count;

        // Act
        _combatEngine.ExecuteRound(session);

        // Assert
        Assert.Equal(initialQueueCount - 1, session.TurnQueue.Count);
    }

    [Fact]
    public void ExecuteRound_SelectsLowestHPTarget()
    {
        // Arrange
        _diceServiceMock.Setup(d => d.Roll(5, 10)).Returns(1);
        var partyA = CreateTestParty("A");
        var partyB = new Party();
        var charB1 = CreateTestCharacter("B1", Role.Tank, maxHealth: 50);
        charB1.ApplyDamage(10); // 40 HP left
        var charB2 = CreateTestCharacter("B2", Role.Tank, maxHealth: 50);
        charB2.ApplyDamage(5);  // 45 HP left
        partyB.Add(charB1);
        partyB.Add(charB2);
        var session = _combatEngine.CreateSession(partyA, partyB);

        // Act
        var result = _combatEngine.ExecuteRound(session);

        // Assert
        Assert.Equal(charB1, result.Target); // Lower HP target selected
    }

    private static Party CreateTestParty(string name, int strength = 10, int maxHealth = 100)
    {
        var party = new Party();
        var character = CreateTestCharacter(name, Role.Tank, strength, maxHealth);
        party.Add(character);
        return party;
    }

    private static Character CreateTestCharacter(string name, Role role, int strength = 10, int maxHealth = 100)
    {
        var stats = new Stats(strength, 10, 10, maxHealth);
        var character = new Character(name, role, stats);
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        character.UnlockedSkills.AddRange(skills);
        character.EquippedSkills.AddRange(skills);
        return character;
    }
}
