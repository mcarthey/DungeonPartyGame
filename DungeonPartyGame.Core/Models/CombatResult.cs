namespace DungeonPartyGame.Core.Models;

public class CombatResult
{
    public int RoundNumber { get; set; }
    public Character Actor { get; set; } = null!;
    public List<TargetResult> Targets { get; set; } = new();
    public string SkillName { get; set; } = string.Empty;
    public bool IsFinalTurn { get; set; }
    public string SummaryText { get; set; } = string.Empty;
    
    // For single target compatibility
    public Character Target => Targets.FirstOrDefault()?.Target!;
    public int Damage => Targets.FirstOrDefault()?.Damage ?? 0;
    public bool TargetDefeated => Targets.FirstOrDefault()?.TargetDefeated ?? false;
    
    // Aliases for compatibility with ViewModels
    public string LogMessage => SummaryText;
    public bool IsFinalRound => IsFinalTurn;
}