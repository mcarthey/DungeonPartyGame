namespace DungeonPartyGame.Core.Models;

public class CombatSession
{
    public Party PartyA { get; }
    public Party PartyB { get; }
    public Queue<TurnOrderEntry> TurnQueue { get; private set; } = new();
    public int RoundNumber { get; private set; } = 1;
    public bool IsComplete { get; private set; }
    public Party? WinningParty { get; private set; }
    
    // Alias for compatibility with ViewModels
    public Party? Winner => WinningParty;

    public CombatSession(Party partyA, Party partyB)
    {
        PartyA = partyA;
        PartyB = partyB;
        GenerateTurnOrder();
    }

    private void GenerateTurnOrder()
    {
        var allCharacters = PartyA.AliveMembers.Concat(PartyB.AliveMembers)
            .OrderByDescending(c => c.Stats.Dexterity)
            .ToList();

        TurnQueue.Clear();
        foreach (var character in allCharacters)
        {
            var owningParty = PartyA.Members.Contains(character) ? PartyA : PartyB;
            TurnQueue.Enqueue(new TurnOrderEntry(character, owningParty));
        }
    }

    public TurnOrderEntry? GetCurrentTurn()
    {
        return TurnQueue.Count > 0 ? TurnQueue.Peek() : null;
    }
    
    // Alias for compatibility with ViewModels
    public Character? CurrentAttacker => GetCurrentTurn()?.Actor;

    public void AdvanceTurn()
    {
        if (IsComplete) return;

        if (TurnQueue.Count > 0)
        {
            TurnQueue.Dequeue();
        }

        // If turn queue is empty, start new round
        if (TurnQueue.Count == 0)
        {
            RoundNumber++;
            GenerateTurnOrder();
        }

        // Check victory conditions
        if (PartyA.IsDefeated)
        {
            IsComplete = true;
            WinningParty = PartyB;
        }
        else if (PartyB.IsDefeated)
        {
            IsComplete = true;
            WinningParty = PartyA;
        }
    }

    public void CompleteCombat(Party winner)
    {
        IsComplete = true;
        WinningParty = winner;
    }
}