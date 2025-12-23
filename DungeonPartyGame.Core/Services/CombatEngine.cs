using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class CombatEngine
{
    private readonly DiceService _dice;
    public CombatEngine(DiceService dice)
    {
        _dice = dice;
    }

    public virtual CombatSession CreateSession(Character a, Character b)
    {
        return new CombatSession(a, b);
    }

    public virtual CombatResult ExecuteRound(CombatSession session)
    {
        if (session.IsComplete)
        {
            throw new InvalidOperationException("Combat session is already complete.");
        }

        var attacker = session.CurrentAttacker;
        var defender = session.CurrentDefender;
        var round = session.RoundNumber;

        var skill = attacker.ChooseSkill(round);
        var weapon = attacker.Equipment.Weapon;
        int baseDamage = _dice.Roll(weapon.MinDamage, weapon.MaxDamage);
        double skillMultiplier = skill.DamageMultiplier;
        int strBonus = attacker.Stats.Strength;
        int totalDamage = (int)Math.Round(baseDamage * skillMultiplier + strBonus);
        defender.ApplyDamage(totalDamage);

        var isDefeated = !defender.IsAlive;
        var isFinalRound = isDefeated;

        var result = new CombatResult
        {
            Round = round,
            Attacker = attacker.Name,
            Defender = defender.Name,
            SkillUsed = skill.Name,
            Damage = totalDamage,
            DefenderHP = defender.Stats.CurrentHealth,
            IsDefeated = isDefeated,
            IsFinalRound = isFinalRound,
            LogMessage = $"Round {round}\n{attacker.Name} uses {skill.Name}\nDamage: {totalDamage}\n{defender.Name} HP: {defender.Stats.CurrentHealth}",
            SummaryText = isDefeated
                ? $"{attacker.Name} defeats {defender.Name}!"
                : $"{attacker.Name} attacks {defender.Name} for {totalDamage} damage."
        };

        if (isDefeated)
        {
            session.CompleteCombat(attacker);
        }
        else
        {
            session.AdvanceTurn();
        }

        return result;
    }
}