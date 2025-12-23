using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class GearService
{
    private readonly Dictionary<string, GearItemDefinition> _gearDefinitions = new();

    public GearService()
    {
        InitializeGearDefinitions();
    }

    public GearItemDefinition? GetGearDefinition(string gearId)
    {
        return _gearDefinitions.GetValueOrDefault(gearId);
    }

    public bool EquipGear(Character character, GearInstance gearInstance)
    {
        var gearDef = GetGearDefinition(gearInstance.DefinitionId);
        if (gearDef == null)
            return false;

        // Unequip existing item in this slot if any
        character.Equipment.Remove(gearDef.Slot);

        // Equip new item
        character.Equipment[gearDef.Slot] = gearInstance;
        return true;
    }

    public bool UnequipGear(Character character, GearSlot slot)
    {
        return character.Equipment.Remove(slot);
    }

    public EffectiveStats GetEffectiveStats(Character character)
    {
        return character.GetEffectiveStats(_gearDefinitions);
    }

    private void InitializeGearDefinitions()
    {
        // Weapons
        _gearDefinitions["iron_sword"] = new GearItemDefinition("iron_sword", "Iron Sword", GearSlot.Weapon, GearRarity.Common, 1, attackBonus: 5);
        _gearDefinitions["steel_sword"] = new GearItemDefinition("steel_sword", "Steel Sword", GearSlot.Weapon, GearRarity.Uncommon, 2, attackBonus: 8);
        _gearDefinitions["magic_sword"] = new GearItemDefinition("magic_sword", "Magic Sword", GearSlot.Weapon, GearRarity.Rare, 3, attackBonus: 12, critBonus: 5);

        // Helmets
        _gearDefinitions["leather_cap"] = new GearItemDefinition("leather_cap", "Leather Cap", GearSlot.Helmet, GearRarity.Common, 1, defenseBonus: 2);
        _gearDefinitions["iron_helmet"] = new GearItemDefinition("iron_helmet", "Iron Helmet", GearSlot.Helmet, GearRarity.Uncommon, 2, defenseBonus: 4, healthBonus: 10);

        // Chest armor
        _gearDefinitions["cloth_shirt"] = new GearItemDefinition("cloth_shirt", "Cloth Shirt", GearSlot.Chest, GearRarity.Common, 1, defenseBonus: 1);
        _gearDefinitions["leather_armor"] = new GearItemDefinition("leather_armor", "Leather Armor", GearSlot.Chest, GearRarity.Uncommon, 2, defenseBonus: 3, healthBonus: 15);
        _gearDefinitions["chain_mail"] = new GearItemDefinition("chain_mail", "Chain Mail", GearSlot.Chest, GearRarity.Rare, 3, defenseBonus: 6, healthBonus: 25);

        // Gloves
        _gearDefinitions["cloth_gloves"] = new GearItemDefinition("cloth_gloves", "Cloth Gloves", GearSlot.Gloves, GearRarity.Common, 1, attackBonus: 1);
        _gearDefinitions["leather_gloves"] = new GearItemDefinition("leather_gloves", "Leather Gloves", GearSlot.Gloves, GearRarity.Uncommon, 2, attackBonus: 2, critBonus: 3);

        // Boots
        _gearDefinitions["cloth_boots"] = new GearItemDefinition("cloth_boots", "Cloth Boots", GearSlot.Boots, GearRarity.Common, 1, dodgeBonus: 2);
        _gearDefinitions["leather_boots"] = new GearItemDefinition("leather_boots", "Leather Boots", GearSlot.Boots, GearRarity.Uncommon, 2, dodgeBonus: 4, healthBonus: 5);

        // Rings
        _gearDefinitions["copper_ring"] = new GearItemDefinition("copper_ring", "Copper Ring", GearSlot.Ring, GearRarity.Common, 1, critBonus: 2);
        _gearDefinitions["silver_ring"] = new GearItemDefinition("silver_ring", "Silver Ring", GearSlot.Ring, GearRarity.Uncommon, 2, critBonus: 4, attackBonus: 2);

        // Amulets
        _gearDefinitions["wooden_amulet"] = new GearItemDefinition("wooden_amulet", "Wooden Amulet", GearSlot.Amulet, GearRarity.Common, 1, healthBonus: 10);
        _gearDefinitions["bone_amulet"] = new GearItemDefinition("bone_amulet", "Bone Amulet", GearSlot.Amulet, GearRarity.Uncommon, 2, healthBonus: 20, defenseBonus: 2);
    }
}