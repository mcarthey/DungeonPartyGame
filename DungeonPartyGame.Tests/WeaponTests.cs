using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class WeaponTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly_WithStatModifier()
    {
        // Act
        var weapon = new Weapon("Sword", 5, 10, "Strength");

        // Assert
        Assert.Equal("Sword", weapon.Name);
        Assert.Equal(5, weapon.MinDamage);
        Assert.Equal(10, weapon.MaxDamage);
        Assert.Equal("Strength", weapon.StatModifier);
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly_WithoutStatModifier()
    {
        // Act
        var weapon = new Weapon("Club", 3, 8);

        // Assert
        Assert.Equal("Club", weapon.Name);
        Assert.Equal(3, weapon.MinDamage);
        Assert.Equal(8, weapon.MaxDamage);
        Assert.Null(weapon.StatModifier);
    }
}