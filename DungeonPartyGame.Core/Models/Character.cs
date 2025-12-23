namespace DungeonPartyGame.Core.Models;

public class Character
{
    public string Name { get; }
    public Role Role { get; }
    public int Level { get; set; } = 1;
    public Stats Stats { get; }
    public Equipment Equipment { get; }
    public List<Skill> UnlockedSkills { get; } = new();
    public List<Skill> EquippedSkills { get; } = new();

    public Character(string name, Role role, Stats stats, Equipment equipment, List<Skill> skills)
    {
        Name = name;
        Role = role;
        Stats = stats;
        Equipment = equipment;
        UnlockedSkills.AddRange(skills);
        EquippedSkills.AddRange(skills); // For now, equip all unlocked skills
    }

    public bool IsAlive => Stats.CurrentHealth > 0;

    public void ApplyDamage(int amount)
    {
        Stats.CurrentHealth = Math.Max(0, Stats.CurrentHealth - amount);
    }

    public void GainLevel()
    {
        Level++;
        Stats.MaxHealth += 10;
        Stats.CurrentHealth = Stats.MaxHealth;
    }

    public Skill ChooseSkill(int currentRound)
    {
        foreach (var skill in EquippedSkills)
        {
            if (skill.CanUse(currentRound))
            {
                skill.MarkUsed(currentRound);
                return skill;
            }
        }
        // fallback: always return first
        var first = EquippedSkills[0];
        first.MarkUsed(currentRound);
        return first;
    }
}
