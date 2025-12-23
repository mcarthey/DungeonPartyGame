namespace DungeonPartyGame.Core.Models;

public class SkillNode
{
    public string NodeId { get; }
    public string SkillId { get; }
    public int CostPoints { get; }
    public List<string> PrerequisiteNodeIds { get; } = new();
    public bool IsRoot { get; }

    public SkillNode(string nodeId, string skillId, int costPoints, bool isRoot = false)
    {
        NodeId = nodeId;
        SkillId = skillId;
        CostPoints = costPoints;
        IsRoot = isRoot;
    }

    public void AddPrerequisite(string prerequisiteNodeId)
    {
        PrerequisiteNodeIds.Add(prerequisiteNodeId);
    }
}