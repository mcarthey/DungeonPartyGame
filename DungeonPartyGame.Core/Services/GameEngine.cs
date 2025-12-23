using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class GameEngine
{
    private readonly GameState _state;

    public GameEngine(GameState state)
    {
        _state = state;
    }

    public GameState GetState() => _state;

    public void RunEncounter()
    {
        foreach (var member in _state.Party.Members)
        {
            member.ApplyDamage(Random.Shared.Next(5, 15));
        }

        _state.Log("Encounter resolved. Party took damage.");
    }

    public void CompleteStage()
    {
        foreach (var member in _state.Party.Members)
            member.LevelUp();

        _state.AdvanceStage();
    }
}
