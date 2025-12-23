using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class CombatEngine
{
    private readonly DiceService _dice;
    private readonly GearService _gearService;
    private readonly ISkillSelector _skillSelector;

    public CombatEngine(DiceService dice, GearService gearService, ISkillSelector skillSelector)
    {
        _dice = dice;
        _gearService = gearService;
        _skillSelector = skillSelector;
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
        var skill = _skillSelector.SelectSkill(actor, session);
        var usingSkill = skill != null && skill.CanUse(session.RoundNumber);

        if (usingSkill && skill.Targeting == TargetingRule.AllEnemies)
        {
            throw new NotImplementedException("Targeting rule AllEnemies not yet implemented.");
        }

        // Select target based on skill or default to lowest HP enemy
        Character target;

        if (usingSkill && skill.Targeting == TargetingRule.Self)
        {
            target = actor;
        }
        else
        {
            var enemyParty = currentTurn.OwningParty == session.PartyA
                ? session.PartyB
                : session.PartyA;

            target = enemyParty.AliveMembers
                .OrderBy(c => c.Stats.CurrentHealth)
                .FirstOrDefault();
        }

        if (target == null)
        {
            throw new InvalidOperationException("No valid target found.");
        }

        // Calculate damage using effective stats
        var actorStats = _gearService.GetEffectiveStats(actor);
        var targetStats = _gearService.GetEffectiveStats(target);

        int baseDamage = _dice.Roll(5, 10); // Roll 5d10 for base damage
        int attackBonus = actorStats.Attack;
        int defenseReduction = targetStats.Defense / 4;

        double multiplier = usingSkill ? skill.DamageMultiplier : 1.0;

        int totalDamage = Math.Max(
            1,
            (int)((baseDamage + attackBonus) * multiplier) - defenseReduction
        );

        target.ApplyDamage(totalDamage);
        var targetDefeated = !target.IsAlive;

        if (usingSkill)
        {
            skill.MarkUsed(session.RoundNumber);
        }

        var result = new CombatResult
        {
            RoundNumber = session.RoundNumber,
            Actor = actor,
            Target = target,
            SkillName = usingSkill ? skill.Name : "Basic Attack",
            Damage = totalDamage,
            TargetDefeated = targetDefeated,
            IsFinalTurn = session.IsComplete, // Will be updated after AdvanceTurn
            SummaryText =
                usingSkill
                    ? $"{actor.Name} uses {skill.Name} on {target.Name} ({totalDamage} dmg)\n{target.Name} HP: {target.Stats.CurrentHealth}"
                    : $"{actor.Name} attacks {target.Name} ({totalDamage} dmg)\n{target.Name} HP: {target.Stats.CurrentHealth}"
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