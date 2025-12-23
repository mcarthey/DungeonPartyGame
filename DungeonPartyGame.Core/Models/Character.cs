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
    public string Name { get; }
    public CharacterRole Role { get; }
    public Stats Stats { get; }
    public CharacterProgression Progression { get; }
    public Dictionary<GearSlot, GearInstance> Equipment { get; }
    public List<Skill> UnlockedSkills { get; }
    public List<Skill> EquippedSkills { get; }

    public Character(string name, CharacterRole role, Stats stats)
    {
        Name = name;
        Role = role;
        Stats = stats;
        Progression = new CharacterProgression();
        Equipment = new Dictionary<GearSlot, GearInstance>();
        UnlockedSkills = new List<Skill>();
        EquippedSkills = new List<Skill>();
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
        // Increase base stats on level up
        Stats.MaxHealth += 10;
        Stats.CurrentHealth = Stats.MaxHealth;
        Stats.Strength += 1;
        Stats.Dexterity += 1;
        Stats.Constitution += 1;
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
            Stats.Strength + attackBonus,
            Stats.Constitution + defenseBonus,
            Stats.MaxHealth + healthBonus,
            Stats.Crit + critBonus,
            Stats.Dodge + dodgeBonus
        );
    }
}
