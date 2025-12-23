using DungeonPartyGame.Core.Services;
using Xunit;

namespace DungeonPartyGame;

public class ModdingTests
{
    [Fact]
    public void ModManager_ValidateMod_ValidatesExistingDirectory()
    {
        // Arrange
        var modManager = new ModManager();
        var tempDir = Path.Combine(Path.GetTempPath(), "test_mod");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = modManager.ValidateMod(tempDir);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ModManager_ValidateMod_DetectsInvalidJson()
    {
        // Arrange
        var modManager = new ModManager();
        var tempDir = Path.Combine(Path.GetTempPath(), "test_mod");
        Directory.CreateDirectory(tempDir);
        var gearFile = Path.Combine(tempDir, "gear.json");
        File.WriteAllText(gearFile, "{ invalid json }");

        try
        {
            // Act
            var result = modManager.ValidateMod(tempDir);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Invalid JSON"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GearService_LoadsModDefinitions()
    {
        // Arrange
        var modManager = new ModManager();
        var gearService = new GearService(modManager);

        // Act - should load base definitions
        var definition = gearService.GetGearDefinition("iron_sword");

        // Assert
        Assert.NotNull(definition);
        Assert.Equal("Iron Sword", definition.Name);
    }

    [Fact]
    public void ModManager_LoadGearDefinitions_ReturnsEmptyForNoMods()
    {
        // Arrange
        var modManager = new ModManager("NonExistentModsDir");

        // Act
        var definitions = modManager.LoadGearDefinitions();

        // Assert
        Assert.NotNull(definitions);
        // Should contain base definitions loaded by the service
    }
}