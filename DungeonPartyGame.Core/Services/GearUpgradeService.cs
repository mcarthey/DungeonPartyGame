using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class GearUpgradeService
{
    private readonly GearService _gearService;

    public GearUpgradeService(GearService gearService)
    {
        _gearService = gearService;
    }

    public bool CanUpgrade(GearInstance gear, Inventory inventory)
    {
        if (IsMaxLevel(gear))
            return false;

        var definition = _gearService.GetGearDefinition(gear.DefinitionId);
        if (definition == null)
            return false;

        // Upgrade cost increases with level and rarity
        int rarityMultiplier = (int)definition.Rarity + 1; // Common = 1, Uncommon = 2, etc.
        int baseCost = definition.BaseTier * 10 * rarityMultiplier;
        int upgradeCost = baseCost * (gear.UpgradeLevel + 1);

        return inventory.UpgradeShards >= upgradeCost;
    }

    public bool Upgrade(GearInstance gear, Inventory inventory)
    {
        if (!CanUpgrade(gear, inventory))
            return false;

        var definition = _gearService.GetGearDefinition(gear.DefinitionId);
        if (definition == null)
            return false;

        int rarityMultiplier = (int)definition.Rarity + 1; // Common = 1, Uncommon = 2, etc.
        int baseCost = definition.BaseTier * 10 * rarityMultiplier;
        int upgradeCost = baseCost * (gear.UpgradeLevel + 1);

        if (!inventory.SpendUpgradeShards(upgradeCost))
            return false;

        gear.UpgradeLevel++;
        return true;
    }

    public bool IsMaxLevel(GearInstance gear)
    {
        var definition = _gearService.GetGearDefinition(gear.DefinitionId);
        if (definition == null)
            return true;

        // Max upgrade level based on rarity
        int maxLevel = ((int)definition.Rarity + 1) * 5; // Common: 5, Uncommon: 10, Rare: 15, etc.
        return gear.UpgradeLevel >= maxLevel;
    }

    public bool CanPromoteTier(GearInstance gear, Inventory inventory)
    {
        // Tier promotion requires upgrade shards and specific upgrade level
        if (gear.UpgradeLevel < 5) return false; // Must be maxed at current tier

        int shardCost = gear.CurrentTier * 5; // 5, 10, 15 shards for tiers 1->2, 2->3, 3->4
        return inventory.UpgradeShards >= shardCost;
    }

    public bool PromoteTier(GearInstance gear, Inventory inventory)
    {
        if (!CanPromoteTier(gear, inventory)) return false;

        int shardCost = gear.CurrentTier * 5;
        if (!inventory.SpendUpgradeShards(shardCost)) return false;

        gear.CurrentTier++;
        gear.UpgradeLevel = 0; // Reset upgrade level for new tier

        // Update rarity based on new tier
        var gearDef = _gearService.GetGearDefinition(gear.DefinitionId);
        if (gearDef != null)
        {
            // Simple rarity progression
            var newRarity = gear.CurrentTier switch
            {
                1 => GearRarity.Common,
                2 => GearRarity.Uncommon,
                3 => GearRarity.Rare,
                4 => GearRarity.Epic,
                _ => GearRarity.Legendary
            };
            // Note: In a real implementation, you'd create a new GearItemDefinition
            // For this POC, we'll just track the tier on the instance
        }

        return true;
    }

    public int GetUpgradeCost(GearInstance gear)
    {
        var definition = _gearService.GetGearDefinition(gear.DefinitionId);
        if (definition == null)
            return int.MaxValue;

        int rarityMultiplier = (int)definition.Rarity + 1; // Common = 1, Uncommon = 2, etc.
        int baseCost = definition.BaseTier * 10 * rarityMultiplier;
        return baseCost * (gear.UpgradeLevel + 1);
    }

    public int GetPromotionCost(GearInstance gear)
    {
        return gear.CurrentTier * 5;
    }
}