using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class CombatEngine
{
    private readonly DiceService _dice;
    public CombatEngine(DiceService dice)
    {
        _dice = dice;
    }

    public virtual CombatSession CreateSession(Party partyA, Party partyB)
    {
        return new CombatSession(partyA, partyB);
    }

    public virtual CombatResult ExecuteTurn(CombatSession session)
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
        var skill = actor.ChooseSkill(session.RoundNumber);

        // For now, only implement SingleEnemy targeting
        if (skill.Targeting != TargetingRule.SingleEnemy)
        {
            throw new NotImplementedException($"Targeting rule {skill.Targeting} not yet implemented.");
        }

        // Select target: lowest HP enemy
        var enemyParty = currentTurn.OwningParty == session.PartyA ? session.PartyB : session.PartyA;
        var target = enemyParty.AliveMembers.OrderBy(c => c.Stats.CurrentHealth).FirstOrDefault();

        if (target == null)
        {
            throw new InvalidOperationException("No valid target found.");
        }

        // Calculate damage
        var weapon = actor.Equipment.Weapon;
        int baseDamage = _dice.Roll(weapon.MinDamage, weapon.MaxDamage);
        double skillMultiplier = skill.DamageMultiplier;
        int strBonus = actor.Stats.Strength;
        int totalDamage = (int)Math.Round(baseDamage * skillMultiplier + strBonus);

        target.ApplyDamage(totalDamage);
        var targetDefeated = !target.IsAlive;

        var result = new CombatResult
        {
            RoundNumber = session.RoundNumber,
            Actor = actor,
            Target = target,
            SkillName = skill.Name,
            Damage = totalDamage,
            TargetDefeated = targetDefeated,
            IsFinalTurn = session.IsComplete, // Will be updated after AdvanceTurn
            SummaryText = $"{actor.Name} attacks {target.Name} with {skill.Name} ({totalDamage} dmg)\n{target.Name} HP: {target.Stats.CurrentHealth}"
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