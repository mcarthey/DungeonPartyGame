namespace DungeonPartyGame.Core.Models;

public class StatusEffect
{
    public string Name { get; }
    public string Description { get; }
    public int Duration { get; private set; } // Remaining rounds
    public EffectType Type { get; }
    public int Value { get; } // e.g., damage per turn, stat bonus
    public StatType? AffectedStat { get; } // For buffs/debuffs

    public StatusEffect(string name, string description, int duration, EffectType type, int value, StatType? affectedStat = null)
    {
        Name = name;
        Description = description;
        Duration = duration;
        Type = type;
        Value = value;
        AffectedStat = affectedStat;
    }

    public void Tick() => Duration--;

    public bool IsExpired => Duration <= 0;

    public int ApplyEffect(int baseValue)
    {
        return Type switch
        {
            EffectType.DamageOverTime => baseValue - Value, // Reduce HP
            EffectType.HealOverTime => baseValue + Value,   // Increase HP
            EffectType.StatModifier => baseValue + Value,   // Modify stat
            _ => baseValue
        };
    }
}

public enum EffectType
{
    DamageOverTime, // e.g., Poison
    HealOverTime,   // e.g., Regeneration
    StatModifier    // e.g., Strength Buff
}

public enum StatType
{
    Attack,
    Defense,
    MaxHealth,
    Crit,
    Dodge
}