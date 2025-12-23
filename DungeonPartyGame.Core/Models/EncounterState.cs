namespace DungeonPartyGame.Core.Models;

public class EncounterState
{
    public Party Party { get; }
    public int CurrentStage { get; private set; } = 1;
    public string LastEvent { get; private set; } = "Game started";

    public EncounterState(Party party)
    {
        Party = party;
    }

    public void AdvanceStage()
    {
        CurrentStage++;
        LastEvent = $"Advanced to stage {CurrentStage}";
    }

    public void Log(string message)
    {
        LastEvent = message;
    }
}
