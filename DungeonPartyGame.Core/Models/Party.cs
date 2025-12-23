namespace DungeonPartyGame.Core.Models;

public class Party
{
    public List<Character> Members { get; set; } = new();

    public IReadOnlyList<Character> AliveMembers => Members.Where(c => c.Stats.CurrentHealth > 0).ToList();

    public bool IsDefeated => AliveMembers.Count == 0;

    public void Add(Character character)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));

        if (Members.Count >= 5)
            throw new InvalidOperationException("Party is full");

        Members.Add(character);
    }
}
