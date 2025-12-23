namespace DungeonPartyGame.Core.Models;

public class CombatResult
{
    public int RoundNumber { get; set; }
    public Character Actor { get; set; } = null!;
    public Character Target { get; set; } = null!;
    public string SkillName { get; set; } = string.Empty;
    public int Damage { get; set; }
    public bool TargetDefeated { get; set; }
    public bool IsFinalTurn { get; set; }
    public string SummaryText { get; set; } = string.Empty;
    
    // Aliases for compatibility with ViewModels
    public string LogMessage => SummaryText;
    public bool IsFinalRound => IsFinalTurn;
}