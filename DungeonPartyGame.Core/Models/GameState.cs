namespace DungeonPartyGame.Core.Models;

public class GameState
{
    public Party Party { get; }
    public int CurrentStage { get; private set; } = 1;
    public string LastEvent { get; private set; } = "Game started";

    public GameState(Party party)
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
