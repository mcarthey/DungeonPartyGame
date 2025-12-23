namespace DungeonPartyGame.Core.Models;

public class TargetResult
{
    public Character Target { get; set; } = null!;
    public int Damage { get; set; }
    public bool IsHealing { get; set; }
    public bool TargetDefeated { get; set; }
}