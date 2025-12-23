namespace DungeonPartyGame.Core.Models;

public class EffectiveStats
{
    public int Attack { get; }
    public int Defense { get; }
    public int MaxHealth { get; }
    public int Crit { get; }
    public int Dodge { get; }

    public EffectiveStats(int attack, int defense, int maxHealth, int crit, int dodge)
    {
        Attack = attack;
        Defense = defense;
        MaxHealth = maxHealth;
        Crit = crit;
        Dodge = dodge;
    }
}

public class Character
{
    public string Name { get; set; }
    public CharacterRole Role { get; set; }
    public Stats Stats { get; set; }
    public CharacterProgression Progression { get; set; }
    public Dictionary<GearSlot, GearInstance> Equipment { get; set; }
    public List<Skill> UnlockedSkills { get; set; }
    public List<Skill> EquippedSkills { get; set; }
    public List<StatusEffect> StatusEffects { get; set; }

    public Character() 
    {
        Progression = new CharacterProgression();
        Equipment = new Dictionary<GearSlot, GearInstance>();
        UnlockedSkills = new List<Skill>();
        EquippedSkills = new List<Skill>();
        StatusEffects = new List<StatusEffect>();
    } // For JSON deserialization

    public Character(string name, CharacterRole role, Stats stats)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Role = role;
        Stats = stats ?? throw new ArgumentNullException(nameof(stats));
        Progression = new CharacterProgression();
        Equipment = new Dictionary<GearSlot, GearInstance>();
        UnlockedSkills = new List<Skill>();
        EquippedSkills = new List<Skill>();
        StatusEffects = new List<StatusEffect>();
    }

    public bool IsAlive => Stats.CurrentHealth > 0;

    public void ApplyDamage(int amount)
    {
        Stats.CurrentHealth = Math.Max(0, Stats.CurrentHealth - amount);
    }

    public void AddExperience(int xp)
    {
        Progression.AddExperience(xp);
    }

    public void LevelUp()
    {
        Progression.LevelUp();
        // Increase base stats on level up (reduced base increase, player allocates points)
        Stats.MaxHealth += 5;
        Stats.CurrentHealth = Stats.MaxHealth;
        // Player will allocate the remaining points
    }

    public bool AllocateStatPoint(StatType statType, int points = 1)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be positive", nameof(points));

        if (Progression.UnspentStatPoints < points)
            return false;

        if (!Enum.IsDefined(typeof(StatType), statType))
            return false;

        switch (statType)
        {
            case StatType.Attack:
                Stats.Strength += points;
                break;
            case StatType.Defense:
                Stats.Constitution += points;
                break;
            case StatType.MaxHealth:
                Stats.MaxHealth += points * 10;
                Stats.CurrentHealth += points * 10;
                break;
            case StatType.Crit:
                Stats.Crit += points;
                break;
            case StatType.Dodge:
                Stats.Dodge += points;
                break;
        }

        Progression.UnspentStatPoints -= points;
        return true;
    }

    public EffectiveStats GetEffectiveStats(Dictionary<string, GearItemDefinition> gearDefinitions)
    {
        int attackBonus = 0;
        int defenseBonus = 0;
        int healthBonus = 0;
        int critBonus = 0;
        int dodgeBonus = 0;

        foreach (var gear in Equipment.Values)
        {
            if (gearDefinitions.TryGetValue(gear.DefinitionId, out var definition))
            {
                var multiplier = gear.GetUpgradeMultiplier();
                attackBonus += (int)(definition.AttackBonus * multiplier);
                defenseBonus += (int)(definition.DefenseBonus * multiplier);
                healthBonus += (int)(definition.HealthBonus * multiplier);
                critBonus += (int)(definition.CritBonus * multiplier);
                dodgeBonus += (int)(definition.DodgeBonus * multiplier);
            }
        }

        return new EffectiveStats(
            ApplyStatusEffects(Stats.Strength + attackBonus, StatType.Attack),
            ApplyStatusEffects(Stats.Constitution + defenseBonus, StatType.Defense),
            ApplyStatusEffects(Stats.MaxHealth + healthBonus, StatType.MaxHealth),
            ApplyStatusEffects(Stats.Crit + critBonus, StatType.Crit),
            ApplyStatusEffects(Stats.Dodge + dodgeBonus, StatType.Dodge)
        );
    }

    private int ApplyStatusEffects(int baseValue, StatType statType)
    {
        int modifiedValue = baseValue;
        // Apply passive skill effects
        foreach (var skill in UnlockedSkills.Where(s => s.Type == SkillType.Passive && s.AppliedEffect?.Type == EffectType.StatModifier && s.AppliedEffect.AffectedStat == statType))
        {
            modifiedValue = skill.AppliedEffect.ApplyEffect(modifiedValue);
        }
        // Apply temporary status effects
        foreach (var effect in StatusEffects.Where(e => e.Type == EffectType.StatModifier && e.AffectedStat == statType))
        {
            modifiedValue = effect.ApplyEffect(modifiedValue);
        }
        return modifiedValue;
    }
}
