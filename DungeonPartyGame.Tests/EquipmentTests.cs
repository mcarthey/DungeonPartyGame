using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class EquipmentTests
{
    [Fact]
    public void Constructor_SetsWeaponCorrectly()
    {
        // Arrange
        var weapon = new Weapon("Sword", 5, 10, "Strength");

        // Act
        var equipment = new Equipment(weapon);

        // Assert
        Assert.Equal(weapon, equipment.Weapon);
    }
}