namespace DungeonPartyGame.Core.Models;

public class Party
{
    public IReadOnlyList<Character> Members => _members;
    private readonly List<Character> _members = new();

    public IReadOnlyList<Character> AliveMembers => _members.Where(c => c.Stats.CurrentHealth > 0).ToList();

    public bool IsDefeated => AliveMembers.Count == 0;

    public void Add(Character character)
    {
        if (_members.Count >= 5)
            throw new InvalidOperationException("Party is full");

        _members.Add(character);
    }
}
