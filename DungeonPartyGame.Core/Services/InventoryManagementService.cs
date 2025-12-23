using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

public class InventoryManagementService
{
    private readonly GearService _gearService;
    private readonly GearUpgradeService _upgradeService;

    public InventoryManagementService(GearService gearService, GearUpgradeService upgradeService)
    {
        _gearService = gearService;
        _upgradeService = upgradeService;
    }

    public bool EquipGear(Character character, string gearInstanceId, Inventory inventory)
    {
        var gear = inventory.GearItems.FirstOrDefault(g => g.InstanceId == gearInstanceId);
        if (gear == null)
            return false;

        return _gearService.EquipGear(character, gear);
    }

    public bool UnequipGear(Character character, GearSlot slot, Inventory inventory)
    {
        if (!character.Equipment.TryGetValue(slot, out var gear))
            return false;

        character.Equipment.Remove(slot);
        inventory.AddGearItem(gear);
        return true;
    }

    public bool SellGear(string gearInstanceId, Inventory inventory, int sellPrice)
    {
        var gear = inventory.GearItems.FirstOrDefault(g => g.InstanceId == gearInstanceId);
        if (gear == null)
            return false;

        inventory.GearItems.Remove(gear);
        inventory.AddGold(sellPrice);
        return true;
    }

    public int GetSellPrice(GearInstance gear)
    {
        var definition = _gearService.GetGearDefinition(gear.DefinitionId);
        if (definition == null)
            return 0;

        // Base sell price is 50% of buy price, modified by upgrade level
        int rarityMultiplier = (int)definition.Rarity + 1;
        int basePrice = definition.BaseTier * 5 * rarityMultiplier;
        double upgradeMultiplier = gear.GetUpgradeMultiplier();
        return (int)(basePrice * upgradeMultiplier * 0.5);
    }

    public bool UpgradeGear(string gearInstanceId, Inventory inventory)
    {
        var gear = inventory.GearItems.FirstOrDefault(g => g.InstanceId == gearInstanceId);
        if (gear == null)
            return false;

        return _upgradeService.Upgrade(gear, inventory);
    }

    public bool CanUpgradeGear(string gearInstanceId, Inventory inventory)
    {
        var gear = inventory.GearItems.FirstOrDefault(g => g.InstanceId == gearInstanceId);
        if (gear == null)
            return false;

        return _upgradeService.CanUpgrade(gear, inventory);
    }

    public GearInstance? GetGearById(string gearInstanceId, Inventory inventory)
    {
        return inventory.GearItems.FirstOrDefault(g => g.InstanceId == gearInstanceId);
    }

    public IEnumerable<GearInstance> GetEquippedGear(Character character)
    {
        return character.Equipment.Values;
    }

    public IEnumerable<GearInstance> GetInventoryGear(Inventory inventory)
    {
        return inventory.GearItems;
    }

    public bool TransferGearBetweenInventories(string gearInstanceId, Inventory fromInventory, Inventory toInventory)
    {
        var gear = fromInventory.GearItems.FirstOrDefault(g => g.InstanceId == gearInstanceId);
        if (gear == null)
            return false;

        fromInventory.GearItems.Remove(gear);
        toInventory.AddGearItem(gear);
        return true;
    }

    public Dictionary<GearSlot, List<GearInstance>> GetCompatibleGear(Character character, Inventory inventory)
    {
        var result = new Dictionary<GearSlot, List<GearInstance>>();

        foreach (var gear in inventory.GearItems)
        {
            var definition = _gearService.GetGearDefinition(gear.DefinitionId);
            if (definition != null)
            {
                if (!result.ContainsKey(definition.Slot))
                    result[definition.Slot] = new List<GearInstance>();

                result[definition.Slot].Add(gear);
            }
        }

        return result;
    }
}