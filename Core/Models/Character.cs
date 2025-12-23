namespace DungeonPartyGame.Core.Models;

public class Character
{
    public string Name { get; }
    public Stats Stats { get; }
    public Equipment Equipment { get; }
    public List<Skill> Skills { get; }

    public Character(string name, Stats stats, Equipment equipment, List<Skill> skills)
    {
        Name = name;
        Stats = stats;
        Equipment = equipment;
        Skills = skills;
    }

    public bool IsAlive => Stats.CurrentHealth > 0;

    public void ApplyDamage(int amount)
    {
        Stats.CurrentHealth = Math.Max(0, Stats.CurrentHealth - amount);
    }

    public void GainLevel()
    {
        Stats.MaxHealth += 10;
        Stats.CurrentHealth = Stats.MaxHealth;
    }

    public Skill ChooseSkill(int currentRound)
    {
        foreach (var skill in Skills)
        {
            if (skill.CanUse(currentRound))
            {
                skill.MarkUsed(currentRound);
                return skill;
            }
        }
        // fallback: always return first
        var first = Skills[0];
        first.MarkUsed(currentRound);
        return first;
    }
}
