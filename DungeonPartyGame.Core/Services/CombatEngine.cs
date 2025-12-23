using System.Linq;
using System.Text;
using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class CombatEngine
{
    private readonly DiceService _dice;
    private readonly GearService _gearService;
    private readonly ISkillSelector _skillSelector;
    private readonly ICombatEventHandler? _eventHandler;

    public CombatEngine(DiceService dice, GearService gearService, ISkillSelector skillSelector, ICombatEventHandler? eventHandler = null)
    {
        _dice = dice;
        _gearService = gearService;
        _skillSelector = skillSelector;
        _eventHandler = eventHandler;
    }

    public virtual CombatSession CreateSession(Party partyA, Party partyB)
    {
        var session = new CombatSession(partyA, partyB);
        _eventHandler?.OnCombatStarted(partyA, partyB);
        return session;
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

        // Fire turn started event
        _eventHandler?.OnTurnStarted(actor);

        // Tick status effects at the start of turn
        TickStatusEffects(actor);

        var skill = _skillSelector.SelectSkill(actor, session);
        var usingSkill = skill != null && skill.CanUse(session.RoundNumber);

        // Determine targets based on skill targeting
        var targets = GetTargets(actor, session, skill, usingSkill);

        if (!targets.Any())
        {
            throw new InvalidOperationException("No valid targets found.");
        }

        var actorStats = _gearService.GetEffectiveStats(actor);
        var targetResults = new List<TargetResult>();

        // Cache effective stats for all potential targets to avoid repeated calculations
        var allCharacters = session.PartyA.Members.Concat(session.PartyB.Members).ToList();
        var effectiveStatsCache = new Dictionary<Character, EffectiveStats>();
        foreach (var character in allCharacters.Where(c => c.IsAlive))
        {
            effectiveStatsCache[character] = _gearService.GetEffectiveStats(character);
        }

        foreach (var target in targets)
        {
            var targetStats = effectiveStatsCache[target];

            int baseDamage = _dice.Roll(5, 10);
            int attackBonus = actorStats.Attack;
            int defenseReduction = targetStats.Defense / 4;

            double multiplier = usingSkill ? skill.DamageMultiplier : 1.0;
            bool isHealing = multiplier < 0; // Negative multiplier for healing

            int totalDamage = Math.Max(
                isHealing ? int.MinValue : 1,
                (int)((baseDamage + attackBonus) * Math.Abs(multiplier)) - defenseReduction
            );

            if (isHealing)
            {
                totalDamage = -totalDamage; // Negative for healing
            }

            target.ApplyDamage(totalDamage);
            var targetDefeated = !target.IsAlive;

            targetResults.Add(new TargetResult
            {
                Target = target,
                Damage = totalDamage,
                IsHealing = isHealing,
                TargetDefeated = targetDefeated
            });

            // Fire damage dealt event
            _eventHandler?.OnDamageDealt(actor, target, totalDamage, isHealing);

            // Fire character defeated event if applicable
            if (targetDefeated)
            {
                _eventHandler?.OnCharacterDefeated(target);
            }

            // Apply skill effect if present
            if (usingSkill && skill.AppliedEffect != null)
            {
                target.StatusEffects.Add(new StatusEffect(
                    skill.AppliedEffect.Name,
                    skill.AppliedEffect.Description,
                    skill.AppliedEffect.Duration,
                    skill.AppliedEffect.Type,
                    skill.AppliedEffect.Value,
                    skill.AppliedEffect.AffectedStat
                ));
                // Fire status effect applied event
                _eventHandler?.OnStatusEffectApplied(target, skill.AppliedEffect);
            }
        }

        if (usingSkill)
        {
            skill.MarkUsed(session.RoundNumber);
            // Fire skill used event
            _eventHandler?.OnSkillUsed(actor, skill, targetResults);
        }

        var result = new CombatResult
        {
            RoundNumber = session.RoundNumber,
            Actor = actor,
            Targets = targetResults,
            SkillName = usingSkill ? skill.Name : "Basic Attack",
            IsFinalTurn = session.IsComplete, // Will be updated after AdvanceTurn
            SummaryText = BuildSummaryText(actor, skill, usingSkill, targetResults)
        };

        // Advance turn and check completion
        session.AdvanceTurn();
        result.IsFinalTurn = session.IsComplete;

        // Fire turn ended event
        _eventHandler?.OnTurnEnded(actor);

        if (session.IsComplete && session.WinningParty != null)
        {
            var losingParty = session.PartyA == session.WinningParty ? session.PartyB : session.PartyA;
            _eventHandler?.OnCombatEnded(session.WinningParty, losingParty);
            result.SummaryText += $"\n\n{(currentTurn.OwningParty == session.WinningParty ? "Victory!" : "Defeat!")}";
        }

        return result;
    }

    private void TickStatusEffects(Character character)
    {
        for (int i = character.StatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = character.StatusEffects[i];
            effect.Tick();
            if (effect.Duration <= 0)
            {
                // Fire status effect expired event
                _eventHandler?.OnStatusEffectExpired(character, effect);
                character.StatusEffects.RemoveAt(i);
            }
        }
    }

    private List<Character> GetTargets(Character actor, CombatSession session, Skill skill, bool usingSkill)
    {
        var targetingRule = usingSkill ? skill.Targeting : TargetingRule.SingleEnemy;
        var allCharacters = session.PartyA.Members.Concat(session.PartyB.Members);
        var allies = session.PartyA.Members.Contains(actor) ? session.PartyA.Members : session.PartyB.Members;
        var enemies = allCharacters.Except(allies);

        return targetingRule switch
        {
            TargetingRule.SingleEnemy => enemies.Where(c => c.IsAlive).Take(1).ToList(),
            TargetingRule.AllEnemies => enemies.Where(c => c.IsAlive).ToList(),
            TargetingRule.Ally => allies.Where(c => c.IsAlive && c != actor).Take(1).ToList(),
            TargetingRule.AllAllies => allies.Where(c => c.IsAlive && c != actor).ToList(),
            TargetingRule.Self => new List<Character> { actor },
            _ => throw new InvalidOperationException($"Unknown targeting rule: {targetingRule}")
        };
    }

    private string BuildSummaryText(Character actor, Skill skill, bool usingSkill, List<TargetResult> targetResults)
    {
        var sb = new StringBuilder();

        if (usingSkill)
        {
            sb.AppendLine($"{actor.Name} uses {skill.Name}");
        }
        else
        {
            sb.AppendLine($"{actor.Name} performs a basic attack");
        }

        foreach (var targetResult in targetResults)
        {
            var target = targetResult.Target;
            var damageText = targetResult.IsHealing
                ? $"heals {Math.Abs(targetResult.Damage)} HP"
                : $"deals {targetResult.Damage} damage";

            sb.AppendLine($"  -> {target.Name} {damageText} (HP: {target.Stats.CurrentHealth})");

            if (targetResult.TargetDefeated)
            {
                sb.AppendLine($"  -> {target.Name} is defeated!");
            }
        }

        return sb.ToString().TrimEnd();
    }
}