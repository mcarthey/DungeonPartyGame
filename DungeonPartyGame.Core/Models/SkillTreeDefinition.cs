namespace DungeonPartyGame.Core.Models;

public class SkillTreeDefinition
{
    public string TreeId { get; }
    public string Name { get; }
    public List<SkillNode> Nodes { get; } = new();
    public Dictionary<string, SkillDefinition> SkillDefinitions { get; } = new();

    public SkillTreeDefinition(string treeId, string name)
    {
        TreeId = treeId;
        Name = name;
    }

    public void AddNode(SkillNode node)
    {
        Nodes.Add(node);
    }

    public void AddSkillDefinition(SkillDefinition skillDef)
    {
        SkillDefinitions[skillDef.Id] = skillDef;
    }

    public SkillNode? GetNode(string nodeId)
    {
        return Nodes.FirstOrDefault(n => n.NodeId == nodeId);
    }

    public SkillDefinition? GetSkillDefinition(string skillId)
    {
        return SkillDefinitions.GetValueOrDefault(skillId);
    }
}