using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Xunit;

namespace DungeonPartyGame;

public class GameEngineTests
{
    [Fact]
    public void Constructor_SetsStateCorrectly()
    {
        // Arrange
        var party = new Party();
        var state = new EncounterState(party);

        // Act
        var engine = new GameEngine(state);

        // Assert
        Assert.Equal(state, engine.GetState());
    }

    [Fact]
    public void RunEncounter_AppliesDamageToAllPartyMembers()
    {
        // Arrange
        var party = new Party();
        var char1 = CreateTestCharacter("Char1");
        var char2 = CreateTestCharacter("Char2");
        party.Add(char1);
        party.Add(char2);
        var state = new EncounterState(party);
        var engine = new GameEngine(state);

        // Act
        engine.RunEncounter();

        // Assert
        Assert.True(char1.Stats.CurrentHealth < 100); // Should have taken damage
        Assert.True(char2.Stats.CurrentHealth < 100);
        Assert.Equal("Encounter resolved. Party took damage.", state.LastEvent);
    }

    [Fact]
    public void CompleteStage_LevelsUpAllPartyMembers_AdvancesStage()
    {
        // Arrange
        var party = new Party();
        var char1 = CreateTestCharacter("Char1");
        var char2 = CreateTestCharacter("Char2");
        party.Add(char1);
        party.Add(char2);
        var state = new EncounterState(party);
        var engine = new GameEngine(state);

        // Act
        engine.CompleteStage();

        // Assert
        Assert.Equal(110, char1.Stats.MaxHealth); // Increased by 10
        Assert.Equal(110, char1.Stats.CurrentHealth); // Restored to max
        Assert.Equal(110, char2.Stats.MaxHealth);
        Assert.Equal(110, char2.Stats.CurrentHealth);
        Assert.Equal(2, state.CurrentStage);
        Assert.Equal("Advanced to stage 2", state.LastEvent);
    }

    private static Character CreateTestCharacter(string name)
    {
        var stats = new Stats(10, 10, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        return new Character(name, Role.Tank, stats, equipment, skills);
    }
}