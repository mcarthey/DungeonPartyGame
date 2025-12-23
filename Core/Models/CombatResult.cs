namespace DungeonPartyGame.Core.Models;

public class CombatResult
{
    public int Round { get; set; }
    public string Attacker { get; set; } = string.Empty;
    public string Defender { get; set; } = string.Empty;
    public string SkillUsed { get; set; } = string.Empty;
    public int Damage { get; set; }
    public int DefenderHP { get; set; }
    public bool IsDefeated { get; set; }
    public string LogMessage { get; set; } = string.Empty;
    public bool IsFinalRound { get; set; }
    public string SummaryText { get; set; } = string.Empty;
}