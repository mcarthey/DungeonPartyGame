namespace DungeonPartyGame.Core.Models;

public class Weapon
{
    public string Name { get; }
    public int MinDamage { get; }
    public int MaxDamage { get; }
    public string? StatModifier { get; }

    public Weapon(string name, int minDamage, int maxDamage, string? statModifier = null)
    {
        Name = name;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
        StatModifier = statModifier;
    }
}