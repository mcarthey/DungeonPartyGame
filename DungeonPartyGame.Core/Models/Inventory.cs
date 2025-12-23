namespace DungeonPartyGame.Core.Models;

public class Inventory
{
    public int Gold { get; set; }
    public int UpgradeShards { get; set; }
    public List<GearInstance> GearItems { get; } = new();

    public void AddGold(int amount)
    {
        Gold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            return true;
        }
        return false;
    }

    public void AddUpgradeShards(int amount)
    {
        UpgradeShards += amount;
    }

    public bool SpendUpgradeShards(int amount)
    {
        if (UpgradeShards >= amount)
        {
            UpgradeShards -= amount;
            return true;
        }
        return false;
    }

    public void AddGearItem(GearInstance gear)
    {
        GearItems.Add(gear);
    }

    public bool RemoveGearItem(string instanceId)
    {
        var item = GearItems.FirstOrDefault(g => g.InstanceId == instanceId);
        if (item != null)
        {
            GearItems.Remove(item);
            return true;
        }
        return false;
    }
}