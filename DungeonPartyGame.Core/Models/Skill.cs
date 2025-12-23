namespace DungeonPartyGame.Core.Models;

public class Skill
{
    public string Name { get; }
    public string Description { get; }
    public SkillType Type { get; }
    public TargetingRule Targeting { get; }
    public double DamageMultiplier { get; }
    public int Cooldown { get; }
    public StatusEffect? AppliedEffect { get; }
    private int _lastUsedRound = -1000;

    public Skill(string name, string description, SkillType type, TargetingRule targeting, double damageMultiplier, int cooldown, StatusEffect? appliedEffect = null)
    {
        Name = name;
        Description = description;
        Type = type;
        Targeting = targeting;
        DamageMultiplier = damageMultiplier;
        Cooldown = cooldown;
        AppliedEffect = appliedEffect;
    }

    public bool CanUse(int currentRound) => Type == SkillType.Passive || currentRound - _lastUsedRound >= Cooldown;
    public void MarkUsed(int currentRound) => _lastUsedRound = currentRound;
}
