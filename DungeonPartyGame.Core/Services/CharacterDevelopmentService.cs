using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class CharacterDevelopmentService
{
    private readonly SkillTreeService _skillTreeService;

    public CharacterDevelopmentService(SkillTreeService skillTreeService)
    {
        _skillTreeService = skillTreeService;
    }

    public bool AllocateStatPoint(Character character, StatType statType, int points = 1)
    {
        return character.AllocateStatPoint(statType, points);
    }

    public bool UnlockSkill(Character character, string skillNodeId)
    {
        return _skillTreeService.UnlockNode(character, _skillTreeService.GetSkillTree(character.Role).Nodes
            .FirstOrDefault(n => n.NodeId == skillNodeId)!);
    }

    public IEnumerable<SkillNode> GetAvailableSkillNodes(Character character)
    {
        return _skillTreeService.GetAvailableNodes(character);
    }

    public bool CanLevelUp(Character character)
    {
        return character.Progression.CanLevelUp();
    }

    public void AddExperience(Character character, int amount)
    {
        character.AddExperience(amount);
    }

    public int GetExperienceForNextLevel(Character character)
    {
        return character.Progression.GetExperienceForNextLevel();
    }

    public Dictionary<StatType, int> GetStatAllocationOptions(Character character)
    {
        return new Dictionary<StatType, int>
        {
            { StatType.Attack, character.Progression.UnspentStatPoints },
            { StatType.Defense, character.Progression.UnspentStatPoints },
            { StatType.MaxHealth, character.Progression.UnspentStatPoints },
            { StatType.Crit, character.Progression.UnspentStatPoints },
            { StatType.Dodge, character.Progression.UnspentStatPoints }
        };
    }

    public Stats GetEffectiveStats(Character character)
    {
        // This would delegate to GearService in a full implementation
        // For now, return base stats
        return character.Stats;
    }
}