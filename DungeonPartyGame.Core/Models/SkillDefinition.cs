namespace DungeonPartyGame.Core.Models;

public enum SkillType
{
    Active,
    Passive
}

public enum ScalingStat
{
    Strength,
    Dexterity,
    Intelligence,
    Constitution
}

public class SkillDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public SkillType SkillType { get; }
    public int BasePower { get; }
    public int Cooldown { get; }
    public ScalingStat ScalingStat { get; }
    public TargetingRule TargetingRule { get; }

    public SkillDefinition(string id, string name, string description, SkillType skillType,
                          int basePower, int cooldown, ScalingStat scalingStat, TargetingRule targetingRule)
    {
        Id = id;
        Name = name;
        Description = description;
        SkillType = skillType;
        BasePower = basePower;
        Cooldown = cooldown;
        ScalingStat = scalingStat;
        TargetingRule = targetingRule;
    }
}