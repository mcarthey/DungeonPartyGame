using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public interface ICombatEventHandler
{
    void OnSkillUsed(Character actor, Skill skill, List<TargetResult> targets);
    void OnDamageDealt(Character attacker, Character target, int damage, bool isHealing);
    void OnCharacterDefeated(Character defeatedCharacter);
    void OnStatusEffectApplied(Character target, StatusEffect effect);
    void OnStatusEffectExpired(Character target, StatusEffect effect);
    void OnTurnStarted(Character character);
    void OnTurnEnded(Character character);
    void OnCombatStarted(Party partyA, Party partyB);
    void OnCombatEnded(Party winner, Party loser);
}