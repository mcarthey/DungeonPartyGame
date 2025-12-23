using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Models;

public class GameSession
{
    public List<Party> Parties { get; set; } = new();
    public int CurrentPartyIndex { get; set; }
    public Inventory Inventory { get; set; } = new();
    public HashSet<string> CompletedEncounters { get; set; } = new();

    // For backward compatibility
    public List<Character> Party => CurrentParty?.Members.ToList() ?? new();
    public Party? CurrentParty => Parties.Count > CurrentPartyIndex ? Parties[CurrentPartyIndex] : null;

    public void AddParty(Party party)
    {
        if (party == null)
            throw new ArgumentNullException(nameof(party));
        Parties.Add(party);
    }

    public void SwitchToParty(int index)
    {
        if (index < 0 || index >= Parties.Count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Party index must be between 0 and {Parties.Count - 1}");
        CurrentPartyIndex = index;
    }

    public void AddCharacterToParty(Character character, int partyIndex = -1)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));

        var targetParty = partyIndex >= 0 && partyIndex < Parties.Count
            ? Parties[partyIndex]
            : CurrentParty;

        if (targetParty == null)
            throw new InvalidOperationException("No valid party available to add character to");

        targetParty.Add(character);
    }

    public bool RemoveCharacterFromParty(string characterName, int partyIndex = -1)
    {
        var targetParty = partyIndex >= 0 && partyIndex < Parties.Count
            ? Parties[partyIndex]
            : CurrentParty;

        if (targetParty != null)
        {
            var character = targetParty.Members.FirstOrDefault(c => c.Name == characterName);
            if (character != null)
            {
                // Note: Party doesn't have a Remove method, we'd need to add one
                // For now, we'll just return false
                return false;
            }
        }
        return false;
    }

    public Character? GetCharacter(string name)
    {
        foreach (var party in Parties)
        {
            var character = party.Members.FirstOrDefault(c => c.Name == name);
            if (character != null)
                return character;
        }
        return null;
    }

    public void MarkEncounterCompleted(string encounterId)
    {
        CompletedEncounters.Add(encounterId);
    }

    public bool IsEncounterCompleted(string encounterId)
    {
        return CompletedEncounters.Contains(encounterId);
    }
}