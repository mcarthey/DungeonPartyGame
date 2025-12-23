namespace DungeonPartyGame.Core.Models;

public class GearItemDefinition
{
    public string Id { get; }
    public string Name { get; }
    public GearSlot Slot { get; }
    public GearRarity Rarity { get; }
    public int BaseTier { get; }

    // Base stat modifiers
    public int AttackBonus { get; }
    public int DefenseBonus { get; }
    public int HealthBonus { get; }
    public int CritBonus { get; }
    public int DodgeBonus { get; }

    public string SpecialAffix { get; }

    public GearItemDefinition(string id, string name, GearSlot slot, GearRarity rarity, int baseTier,
                             int attackBonus = 0, int defenseBonus = 0, int healthBonus = 0,
                             int critBonus = 0, int dodgeBonus = 0, string specialAffix = "")
    {
        Id = id;
        Name = name;
        Slot = slot;
        Rarity = rarity;
        BaseTier = baseTier;
        AttackBonus = attackBonus;
        DefenseBonus = defenseBonus;
        HealthBonus = healthBonus;
        CritBonus = critBonus;
        DodgeBonus = dodgeBonus;
        SpecialAffix = specialAffix;
    }
}