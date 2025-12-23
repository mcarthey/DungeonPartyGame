using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Xunit;

namespace DungeonPartyGame;

public class Phase3Tests : IDisposable
{
    private readonly string _testSavePath = Path.Combine(Path.GetTempPath(), "test_save.json");

    public void Dispose()
    {
        if (File.Exists(_testSavePath))
            File.Delete(_testSavePath);
    }

    [Fact]
    public void SaveLoadService_CanSaveAndLoadGameState()
    {
        // Arrange
        var saveLoadService = new SaveLoadService();

        var fighter = new Character("Fighter", CharacterRole.Fighter, new Stats(15, 12, 14, 100));
        fighter.Progression.Level = 3;
        fighter.Progression.Experience = 150;

        var party = new Party();
        party.Add(fighter);

        var inventory = new Inventory();
        inventory.AddGold(500);
        inventory.AddUpgradeShards(25);

        var gear = new GearInstance("test_sword", "iron_sword");
        inventory.AddGearItem(gear);

        var gameSession = new GameSession();
        gameSession.AddParty(party);
        gameSession.Inventory = inventory;

        // Act
        saveLoadService.SaveGameState(gameSession, _testSavePath);
        var loadedSession = saveLoadService.LoadGameState(_testSavePath);

        // Assert
        Assert.Single(loadedSession.Parties);
        Assert.Equal(500, loadedSession.Inventory.Gold);
        Assert.Equal(25, loadedSession.Inventory.UpgradeShards);
        Assert.Single(loadedSession.Inventory.GearItems);

        var loadedCharacter = loadedSession.Parties[0].Members[0];
        Assert.Equal("Fighter", loadedCharacter.Name);
        Assert.Equal(CharacterRole.Fighter, loadedCharacter.Role);
        Assert.Equal(3, loadedCharacter.Progression.Level);
        Assert.Equal(150, loadedCharacter.Progression.Experience);
    }

    [Fact]
    public void GearUpgradeService_CanUpgradeGear()
    {
        // Arrange
        var gearService = new GearService();
        var upgradeService = new GearUpgradeService(gearService);
        var inventory = new Inventory();
        inventory.AddUpgradeShards(1000); // Add plenty of shards

        var gear = new GearInstance("test_sword", "iron_sword");

        // Debug: Check if gear definition exists
        var definition = gearService.GetGearDefinition("iron_sword");
        Assert.NotNull(definition); // Ensure the gear definition exists

        // Debug: Check max level
        var isMax = upgradeService.IsMaxLevel(gear);
        Assert.False(isMax, $"Gear is already at max level");

        // Act & Assert
        var canUpgrade = upgradeService.CanUpgrade(gear, inventory);
        Assert.True(canUpgrade, $"Cannot upgrade gear. Cost: {upgradeService.GetUpgradeCost(gear)}, Shards: {inventory.UpgradeShards}, IsMax: {isMax}");

        Assert.True(upgradeService.Upgrade(gear, inventory));
        Assert.Equal(1, gear.UpgradeLevel);
        Assert.True(inventory.UpgradeShards < 1000); // Shards were spent
    }

    [Fact]
    public void CharacterDevelopment_CanAllocateStatPoints()
    {
        // Arrange
        var character = new Character("TestChar", CharacterRole.Fighter, new Stats(10, 10, 10, 100));
        character.Progression.UnspentStatPoints = 5;

        // Act
        var success = character.AllocateStatPoint(StatType.Attack, 2);

        // Assert
        Assert.True(success);
        Assert.Equal(12, character.Stats.Strength); // 10 + 2
        Assert.Equal(3, character.Progression.UnspentStatPoints); // 5 - 2
    }

    [Fact]
    public void InventoryManagement_CanEquipAndUnequipGear()
    {
        // Arrange
        var gearService = new GearService();
        var upgradeService = new GearUpgradeService(gearService);
        var inventoryService = new InventoryManagementService(gearService, upgradeService);

        var character = new Character("TestChar", CharacterRole.Fighter, new Stats(10, 10, 10, 100));
        var inventory = new Inventory();
        var gear = new GearInstance("test_sword", "iron_sword");
        inventory.AddGearItem(gear);

        // Act
        var equipSuccess = inventoryService.EquipGear(character, gear.InstanceId, inventory);
        var unequipSuccess = inventoryService.UnequipGear(character, GearSlot.Weapon, inventory);

        // Assert
        Assert.True(equipSuccess);
        Assert.True(unequipSuccess);
        Assert.Contains(gear, inventory.GearItems);
        Assert.DoesNotContain(GearSlot.Weapon, character.Equipment.Keys);
    }
}