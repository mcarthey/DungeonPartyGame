namespace DungeonPartyGame.Core.Models;

public class Skill
{
    public string Name { get; }
    public string Description { get; }
    public TargetingRule Targeting { get; }
    public double DamageMultiplier { get; }
    public int Cooldown { get; }
    private int _lastUsedRound = -1000;

    public Skill(string name, string description, TargetingRule targeting, double damageMultiplier, int cooldown)
    {
        Name = name;
        Description = description;
        Targeting = targeting;
        DamageMultiplier = damageMultiplier;
        Cooldown = cooldown;
    }

    public bool CanUse(int currentRound) => currentRound - _lastUsedRound >= Cooldown;
    public void MarkUsed(int currentRound) => _lastUsedRound = currentRound;
}
