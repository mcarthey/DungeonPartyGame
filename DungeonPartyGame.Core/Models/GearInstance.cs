namespace DungeonPartyGame.Core.Models;

public class GearInstance
{
    public string InstanceId { get; }
    public string DefinitionId { get; }
    public int CurrentTier { get; set; }
    public int UpgradeLevel { get; set; }

    public GearInstance(string instanceId, string definitionId, int currentTier = 1, int upgradeLevel = 0)
    {
        if (string.IsNullOrEmpty(instanceId))
            throw new ArgumentException("Instance ID cannot be null or empty", nameof(instanceId));
        if (string.IsNullOrEmpty(definitionId))
            throw new ArgumentException("Definition ID cannot be null or empty", nameof(definitionId));
        if (currentTier < 1)
            throw new ArgumentException("Current tier must be at least 1", nameof(currentTier));
        if (upgradeLevel < 0)
            throw new ArgumentException("Upgrade level cannot be negative", nameof(upgradeLevel));

        InstanceId = instanceId;
        DefinitionId = definitionId;
        CurrentTier = currentTier;
        UpgradeLevel = upgradeLevel;
    }

    public double GetUpgradeMultiplier()
    {
        return 1.0 + (0.10 * UpgradeLevel); // 10% increase per upgrade level
    }
}