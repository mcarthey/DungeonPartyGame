using DungeonPartyGame.Core.Models;

public interface ISkillSelector
{
    Skill SelectSkill(Character actor, CombatSession session);
}