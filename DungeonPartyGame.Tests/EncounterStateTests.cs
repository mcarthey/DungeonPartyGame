using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class EncounterStateTests
{
    [Fact]
    public void Constructor_SetsPartyCorrectly()
    {
        // Arrange
        var party = new Party();

        // Act
        var encounterState = new EncounterState(party);

        // Assert
        Assert.Equal(party, encounterState.Party);
        Assert.Equal(1, encounterState.CurrentStage);
        Assert.Equal("Game started", encounterState.LastEvent);
    }

    [Fact]
    public void AdvanceStage_IncrementsStageAndUpdatesEvent()
    {
        // Arrange
        var party = new Party();
        var encounterState = new EncounterState(party);

        // Act
        encounterState.AdvanceStage();

        // Assert
        Assert.Equal(2, encounterState.CurrentStage);
        Assert.Equal("Advanced to stage 2", encounterState.LastEvent);
    }

    [Fact]
    public void Log_UpdatesLastEvent()
    {
        // Arrange
        var party = new Party();
        var encounterState = new EncounterState(party);

        // Act
        encounterState.Log("Custom message");

        // Assert
        Assert.Equal("Custom message", encounterState.LastEvent);
    }
}