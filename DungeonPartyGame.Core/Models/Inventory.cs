namespace DungeonPartyGame.Core.Models;

public class Inventory
{
    public int Gold { get; set; }
    public int UpgradeShards { get; set; }
    public List<GearInstance> GearItems { get; set; } = new();

    public void AddGold(int amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        Gold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (Gold >= amount)
        {
            Gold -= amount;
            return true;
        }
        return false;
    }

    public void AddUpgradeShards(int amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        UpgradeShards += amount;
    }

    public bool SpendUpgradeShards(int amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (UpgradeShards >= amount)
        {
            UpgradeShards -= amount;
            return true;
        }
        return false;
    }

    public void AddGearItem(GearInstance gear)
    {
        if (gear == null)
            throw new ArgumentNullException(nameof(gear));
        GearItems.Add(gear);
    }

    public bool RemoveGearItem(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            throw new ArgumentException("Instance ID cannot be null or empty", nameof(instanceId));

        var item = GearItems.FirstOrDefault(g => g.InstanceId == instanceId);
        if (item != null)
        {
            GearItems.Remove(item);
            return true;
        }
        return false;
    }
}