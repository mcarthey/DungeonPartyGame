using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Models;

public class GameSession
{
    public List<Character> Party { get; } = new();
    public Inventory Inventory { get; } = new();

    public void AddCharacterToParty(Character character)
    {
        if (Party.Count < 5)
        {
            Party.Add(character);
        }
    }

    public bool RemoveCharacterFromParty(string characterName)
    {
        var character = Party.FirstOrDefault(c => c.Name == characterName);
        if (character != null)
        {
            Party.Remove(character);
            return true;
        }
        return false;
    }

    public Character? GetCharacter(string name)
    {
        return Party.FirstOrDefault(c => c.Name == name);
    }
}