using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class GameStateTests
{
    [Fact]
    public void Constructor_SetsPartyCorrectly()
    {
        // Arrange
        var party = new Party();

        // Act
        var gameState = new GameState(party);

        // Assert
        Assert.Equal(party, gameState.Party);
        Assert.Equal(1, gameState.CurrentStage);
        Assert.Equal("Game started", gameState.LastEvent);
    }

    [Fact]
    public void AdvanceStage_IncrementsStageAndUpdatesEvent()
    {
        // Arrange
        var party = new Party();
        var gameState = new GameState(party);

        // Act
        gameState.AdvanceStage();

        // Assert
        Assert.Equal(2, gameState.CurrentStage);
        Assert.Equal("Advanced to stage 2", gameState.LastEvent);
    }

    [Fact]
    public void Log_UpdatesLastEvent()
    {
        // Arrange
        var party = new Party();
        var gameState = new GameState(party);

        // Act
        gameState.Log("Custom message");

        // Assert
        Assert.Equal("Custom message", gameState.LastEvent);
    }
}