namespace DungeonPartyGame.Core.Models;

public class CharacterProgression
{
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int UnspentSkillPoints { get; set; } = 0;
    public int UnspentStatPoints { get; set; } = 0;
    public HashSet<string> UnlockedSkillNodeIds { get; set; } = new();
    
    // Alias for compatibility with ViewModels
    public HashSet<string> UnlockedSkillNodes => UnlockedSkillNodeIds;

    public void AddExperience(int amount)
    {
        Experience += amount;
        while (CanLevelUp())
        {
            LevelUp();
        }
    }

    public int GetExperienceForNextLevel()
    {
        // Simple leveling curve: 100 * level
        return Level * 100;
    }

    public bool CanLevelUp()
    {
        return Experience >= GetExperienceForNextLevel();
    }

    public void LevelUp()
    {
        if (!CanLevelUp()) return;

        Experience -= GetExperienceForNextLevel();
        Level++;
        UnspentSkillPoints += 2; // Grant 2 skill points per level
        UnspentStatPoints += 3; // Grant 3 stat points per level
    }

    public bool HasUnlockedNode(string nodeId)
    {
        return UnlockedSkillNodeIds.Contains(nodeId);
    }

    public void UnlockNode(string nodeId)
    {
        UnlockedSkillNodeIds.Add(nodeId);
    }
}