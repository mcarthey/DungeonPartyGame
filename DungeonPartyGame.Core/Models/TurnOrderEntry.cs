namespace DungeonPartyGame.Core.Models;

public class TurnOrderEntry
{
    public Character Actor { get; }
    public Party OwningParty { get; }

    public TurnOrderEntry(Character actor, Party owningParty)
    {
        Actor = actor;
        OwningParty = owningParty;
    }
}