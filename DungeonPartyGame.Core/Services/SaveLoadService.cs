using System.Text.Json;
using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class SaveLoadService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true
    };

    public void SaveGameState(GameSession session, string filePath)
    {
        var gameState = new GameState
        {
            Parties = session.Parties,
            CurrentPartyIndex = session.CurrentPartyIndex,
            Inventory = session.Inventory,
            CompletedEncounters = session.CompletedEncounters,
            SaveTimestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(gameState, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    public GameSession LoadGameState(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Save file not found", filePath);
        }

        var json = File.ReadAllText(filePath);
        var gameState = JsonSerializer.Deserialize<GameState>(json, _jsonOptions);

        if (gameState == null)
        {
            throw new InvalidDataException("Failed to deserialize save file");
        }

        var gameSession = new GameSession
        {
            Parties = gameState.Parties,
            CurrentPartyIndex = gameState.CurrentPartyIndex,
            Inventory = gameState.Inventory,
            CompletedEncounters = gameState.CompletedEncounters
        };

        return gameSession;
    }

    public bool SaveFileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public DateTime GetSaveTimestamp(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return DateTime.MinValue;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var gameState = JsonSerializer.Deserialize<GameState>(json, _jsonOptions);
            return gameState?.SaveTimestamp ?? DateTime.MinValue;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
}