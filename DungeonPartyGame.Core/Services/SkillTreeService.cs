using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class SkillTreeService
{
    private readonly Dictionary<string, SkillTreeDefinition> _skillTrees = new();

    public SkillTreeService()
    {
        InitializeSkillTrees();
    }

    public SkillTreeDefinition GetSkillTree(CharacterRole role)
    {
        return _skillTrees.GetValueOrDefault(role.ToString().ToLower());
    }

    public bool CanUnlockNode(Character character, SkillNode node)
    {
        var tree = GetSkillTree(character.Role);
        if (tree == null) return false;

        // Check if already unlocked
        if (character.Progression.HasUnlockedNode(node.NodeId)) return false;

        // Check skill points
        if (character.Progression.UnspentSkillPoints < node.CostPoints) return false;

        // Check prerequisites
        foreach (var prereqId in node.PrerequisiteNodeIds)
        {
            if (!character.Progression.HasUnlockedNode(prereqId)) return false;
        }

        return true;
    }

    public bool UnlockNode(Character character, SkillNode node)
    {
        if (!CanUnlockNode(character, node)) return false;

        character.Progression.UnspentSkillPoints -= node.CostPoints;
        character.Progression.UnlockNode(node.NodeId);

        // Add the skill to unlocked skills if it's an active skill
        var tree = GetSkillTree(character.Role);
        var skillDef = tree.GetSkillDefinition(node.SkillId);
        if (skillDef != null && skillDef.SkillType == SkillType.Active)
        {
            character.UnlockedSkills.Add(skillDef);
        }

        return true;
    }

    public IReadOnlyList<SkillNode> GetAvailableNodes(Character character)
    {
        var tree = GetSkillTree(character.Role);
        if (tree == null) return Array.Empty<SkillNode>();

        return tree.Nodes
            .Where(node => CanUnlockNode(character, node))
            .ToList();
    }

    private void InitializeSkillTrees()
    {
        // Fighter Skill Tree
        var fighterTree = new SkillTreeDefinition("fighter", "Fighter");

        // Root node
        var powerStrike = new SkillDefinition("power_strike", "Power Strike", "A powerful melee attack", SkillType.Active, 15, 2, ScalingStat.Strength, TargetingRule.SingleEnemy);
        var fighterRoot = new SkillNode("fighter_root", "power_strike", 1, true);
        fighterTree.AddSkillDefinition(powerStrike);
        fighterTree.AddNode(fighterRoot);

        // Damage branch
        var heavyBlow = new SkillDefinition("heavy_blow", "Heavy Blow", "Increased damage with strength", SkillType.Active, 20, 3, ScalingStat.Strength, TargetingRule.SingleEnemy);
        var damageNode1 = new SkillNode("fighter_damage_1", "heavy_blow", 2);
        damageNode1.AddPrerequisite("fighter_root");
        fighterTree.AddSkillDefinition(heavyBlow);
        fighterTree.AddNode(damageNode1);

        var crushingStrike = new SkillDefinition("crushing_strike", "Crushing Strike", "High damage attack", SkillType.Active, 25, 4, ScalingStat.Strength, TargetingRule.SingleEnemy);
        var damageNode2 = new SkillNode("fighter_damage_2", "crushing_strike", 3);
        damageNode2.AddPrerequisite("fighter_damage_1");
        fighterTree.AddSkillDefinition(crushingStrike);
        fighterTree.AddNode(damageNode2);

        // Survivability branch
        var toughSkin = new SkillDefinition("tough_skin", "Tough Skin", "Passive: Increased defense", SkillType.Passive, 5, 0, ScalingStat.Constitution, TargetingRule.Self);
        var survNode1 = new SkillNode("fighter_surv_1", "tough_skin", 2);
        survNode1.AddPrerequisite("fighter_root");
        fighterTree.AddSkillDefinition(toughSkin);
        fighterTree.AddNode(survNode1);

        var ironWill = new SkillDefinition("iron_will", "Iron Will", "Passive: Damage reduction", SkillType.Passive, 10, 0, ScalingStat.Constitution, TargetingRule.Self);
        var survNode2 = new SkillNode("fighter_surv_2", "iron_will", 3);
        survNode2.AddPrerequisite("fighter_surv_1");
        fighterTree.AddSkillDefinition(ironWill);
        fighterTree.AddNode(survNode2);

        // Rogue Skill Tree
        var rogueTree = new SkillTreeDefinition("rogue", "Rogue");

        // Root node
        var quickStrike = new SkillDefinition("quick_strike", "Quick Strike", "Fast melee attack", SkillType.Active, 12, 1, ScalingStat.Dexterity, TargetingRule.SingleEnemy);
        var rogueRoot = new SkillNode("rogue_root", "quick_strike", 1, true);
        rogueTree.AddSkillDefinition(quickStrike);
        rogueTree.AddNode(rogueRoot);

        // Crit branch
        var precision = new SkillDefinition("precision", "Precision", "Passive: Increased crit chance", SkillType.Passive, 8, 0, ScalingStat.Dexterity, TargetingRule.Self);
        var critNode1 = new SkillNode("rogue_crit_1", "precision", 2);
        critNode1.AddPrerequisite("rogue_root");
        rogueTree.AddSkillDefinition(precision);
        rogueTree.AddNode(critNode1);

        var deadlyPrecision = new SkillDefinition("deadly_precision", "Deadly Precision", "Passive: High crit chance", SkillType.Passive, 15, 0, ScalingStat.Dexterity, TargetingRule.Self);
        var critNode2 = new SkillNode("rogue_crit_2", "deadly_precision", 3);
        critNode2.AddPrerequisite("rogue_crit_1");
        rogueTree.AddSkillDefinition(deadlyPrecision);
        rogueTree.AddNode(critNode2);

        // Evasion branch
        var nimble = new SkillDefinition("nimble", "Nimble", "Passive: Increased dodge", SkillType.Passive, 6, 0, ScalingStat.Dexterity, TargetingRule.Self);
        var evadeNode1 = new SkillNode("rogue_evade_1", "nimble", 2);
        evadeNode1.AddPrerequisite("rogue_root");
        rogueTree.AddSkillDefinition(nimble);
        rogueTree.AddNode(evadeNode1);

        var shadowStep = new SkillDefinition("shadow_step", "Shadow Step", "Passive: High dodge chance", SkillType.Passive, 12, 0, ScalingStat.Dexterity, TargetingRule.Self);
        var evadeNode2 = new SkillNode("rogue_evade_2", "shadow_step", 3);
        evadeNode2.AddPrerequisite("rogue_evade_1");
        rogueTree.AddSkillDefinition(shadowStep);
        rogueTree.AddNode(evadeNode2);

        _skillTrees["fighter"] = fighterTree;
        _skillTrees["rogue"] = rogueTree;
    }
}