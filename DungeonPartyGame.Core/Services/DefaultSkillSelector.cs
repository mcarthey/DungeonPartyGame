using DungeonPartyGame.Core.Models;

public class DefaultSkillSelector : ISkillSelector
{
    public Skill SelectSkill(Character actor, CombatSession session)
    {
        // Select first usable unlocked skill
        var skill = actor.UnlockedSkills
            .FirstOrDefault(s => s.CanUse(session.RoundNumber));

        return skill;
    }
}