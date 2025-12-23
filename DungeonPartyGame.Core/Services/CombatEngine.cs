using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class CombatEngine
{
    private readonly DiceService _dice;
    private readonly GearService _gearService;

    public CombatEngine(DiceService dice, GearService gearService)
    {
        _dice = dice;
        _gearService = gearService;
    }

    public virtual CombatSession CreateSession(Party partyA, Party partyB)
    {
        return new CombatSession(partyA, partyB);
    }

    public virtual CombatResult ExecuteRound(CombatSession session)
    {
        if (session.IsComplete)
        {
            throw new InvalidOperationException("Combat session is already complete.");
        }

        var currentTurn = session.GetCurrentTurn();
        if (currentTurn == null)
        {
            throw new InvalidOperationException("No current turn available.");
        }

        var actor = currentTurn.Actor;
        // Use basic attack instead of skill selection

        // Select target: lowest HP enemy
        var enemyParty = currentTurn.OwningParty == session.PartyA ? session.PartyB : session.PartyA;
        var target = enemyParty.AliveMembers.OrderBy(c => c.Stats.CurrentHealth).FirstOrDefault();

        if (target == null)
        {
            throw new InvalidOperationException("No valid target found.");
        }

        // Calculate damage using effective stats
        var actorStats = _gearService.GetEffectiveStats(actor);
        var targetStats = _gearService.GetEffectiveStats(target);

        // Simple damage formula: base damage + attack stat - defense
        int baseDamage = 10; // Base attack damage
        int attackBonus = actorStats.Attack;
        int defenseReduction = targetStats.Defense / 4; // Simple defense calculation

        int totalDamage = Math.Max(1, baseDamage + attackBonus - defenseReduction);

        target.ApplyDamage(totalDamage);
        var targetDefeated = !target.IsAlive;

        var result = new CombatResult
        {
            RoundNumber = session.RoundNumber,
            Actor = actor,
            Target = target,
            SkillName = "Basic Attack",
            Damage = totalDamage,
            TargetDefeated = targetDefeated,
            IsFinalTurn = session.IsComplete, // Will be updated after AdvanceTurn
            SummaryText = $"{actor.Name} attacks {target.Name} ({totalDamage} dmg)\n{target.Name} HP: {target.Stats.CurrentHealth}"
        };

        // Advance turn and check completion
        session.AdvanceTurn();
        result.IsFinalTurn = session.IsComplete;

        if (session.IsComplete && session.WinningParty != null)
        {
            result.SummaryText += $"\n\n{(currentTurn.OwningParty == session.WinningParty ? "Victory!" : "Defeat!")}";
        }

        return result;
    }
}