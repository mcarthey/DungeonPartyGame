namespace DungeonPartyGame.Core.Models;

public class Character
{
    public string Name { get; }
    public CharacterRole Role { get; }
    public Stats Stats { get; }
    public CharacterProgression Progression { get; }
    public Dictionary<GearSlot, GearInstance> Equipment { get; }
    public List<SkillDefinition> UnlockedSkills { get; }

    public Character(string name, CharacterRole role, Stats stats)
    {
        Name = name;
        Role = role;
        Stats = stats;
        Progression = new CharacterProgression();
        Equipment = new Dictionary<GearSlot, GearInstance>();
        UnlockedSkills = new List<SkillDefinition>();
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

    public EffectiveStats GetEffectiveStats()
    {
        int attackBonus = 0;
        int defenseBonus = 0;
        int healthBonus = 0;
        int critBonus = 0;
        int dodgeBonus = 0;

        foreach (var gear in Equipment.Values)
        {
            var multiplier = gear.GetUpgradeMultiplier();
            attackBonus += (int)(gear.Definition.AttackBonus * multiplier);
            defenseBonus += (int)(gear.Definition.DefenseBonus * multiplier);
            healthBonus += (int)(gear.Definition.HealthBonus * multiplier);
            critBonus += (int)(gear.Definition.CritBonus * multiplier);
            dodgeBonus += (int)(gear.Definition.DodgeBonus * multiplier);
        }

        return new EffectiveStats(
            Stats.Strength + attackBonus,
            Stats.Dexterity + attackBonus,
            Stats.Constitution + defenseBonus,
            Stats.MaxHealth + healthBonus,
            Stats.Crit + critBonus,
            Stats.Dodge + dodgeBonus
        );
    }
}
