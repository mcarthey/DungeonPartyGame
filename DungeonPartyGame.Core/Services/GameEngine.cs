using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class GameEngine
{
    private readonly EncounterState _state;
    private static readonly Random _random = new Random();

    public GameEngine(EncounterState state)
    {
        _state = state;
    }

    public EncounterState GetState() => _state;

    public void RunEncounter()
    {
        foreach (var member in _state.Party.Members)
        {
            member.ApplyDamage(_random.Next(5, 15));
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
