namespace DungeonPartyGame.Core.Models;

public class CombatSession
{
    public Character FighterA { get; }
    public Character FighterB { get; }
    public Character CurrentAttacker { get; private set; }
    public Character CurrentDefender { get; private set; }
    public int RoundNumber { get; private set; } = 1;
    public bool IsComplete { get; private set; }
    public Character? Winner { get; private set; }

    public CombatSession(Character a, Character b)
    {
        FighterA = a;
        FighterB = b;

        // Initiative: higher DEX goes first
        if (b.Stats.Dexterity > a.Stats.Dexterity)
        {
            CurrentAttacker = b;
            CurrentDefender = a;
        }
        else
        {
            CurrentAttacker = a;
            CurrentDefender = b;
        }
    }

    public void AdvanceTurn()
    {
        if (IsComplete) return;

        // Swap attacker and defender
        (CurrentAttacker, CurrentDefender) = (CurrentDefender, CurrentAttacker);
        RoundNumber++;
    }

    public void CompleteCombat(Character winner)
    {
        IsComplete = true;
        Winner = winner;
    }
}