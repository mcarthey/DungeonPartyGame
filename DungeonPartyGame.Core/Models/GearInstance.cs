namespace DungeonPartyGame.Core.Models;

public class GearInstance
{
    public string InstanceId { get; }
    public string DefinitionId { get; }
    public int CurrentTier { get; set; }
    public int UpgradeLevel { get; set; }

    public GearInstance(string instanceId, string definitionId, int currentTier = 1, int upgradeLevel = 0)
    {
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