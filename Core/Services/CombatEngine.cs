using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class CombatEngine
{
    private readonly DiceService _dice;
    public CombatEngine(DiceService dice)
    {
        _dice = dice;
    }

    public IReadOnlyList<CombatResult> RunCombat(Character a, Character b)
    {
        var results = new List<CombatResult>();
        var round = 1;
        var attacker = a;
        var defender = b;

        // Initiative: higher DEX goes first
        if (b.Stats.Dexterity > a.Stats.Dexterity)
        {
            attacker = b;
            defender = a;
        }

        while (attacker.IsAlive && defender.IsAlive)
        {
            var skill = attacker.ChooseSkill(round);
            var weapon = attacker.Equipment.Weapon;
            int baseDamage = _dice.Roll(weapon.MinDamage, weapon.MaxDamage);
            double skillMultiplier = skill.DamageMultiplier;
            int strBonus = attacker.Stats.Strength;
            int totalDamage = (int)Math.Round(baseDamage * skillMultiplier + strBonus);
            defender.ApplyDamage(totalDamage);

            var result = new CombatResult
            {
                Round = round,
                Attacker = attacker.Name,
                Defender = defender.Name,
                SkillUsed = skill.Name,
                Damage = totalDamage,
                DefenderHP = defender.Stats.CurrentHealth,
                IsDefeated = !defender.IsAlive,
                LogMessage = $"Round {round}\n{attacker.Name} uses {skill.Name}\nDamage: {totalDamage}\n{defender.Name} HP: {defender.Stats.CurrentHealth}"
            };
            results.Add(result);

            if (!defender.IsAlive)
            {
                results.Add(new CombatResult
                {
                    Round = round,
                    Attacker = attacker.Name,
                    Defender = defender.Name,
                    SkillUsed = skill.Name,
                    Damage = 0,
                    DefenderHP = 0,
                    IsDefeated = true,
                    LogMessage = $"{attacker.Name} defeats {defender.Name}"
                });
                break;
            }

            // Swap
            (attacker, defender) = (defender, attacker);
            round++;
        }
        return results;
    }
}