using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class ProgressionService
{
    public void AddXp(Character character, int amount)
    {
        character.AddExperience(amount);
    }

    public void AddXp(Party party, int amount)
    {
        foreach (var character in party.AliveMembers)
        {
            AddXp(character, amount);
        }
    }

    public int GetExperienceForLevel(int level)
    {
        return level * 100; // Simple curve
    }

    public int GetTotalExperienceForLevel(int level)
    {
        int total = 0;
        for (int i = 1; i < level; i++)
        {
            total += GetExperienceForLevel(i);
        }
        return total;
    }
}