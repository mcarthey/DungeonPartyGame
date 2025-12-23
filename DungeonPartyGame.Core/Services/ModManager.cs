using System.Text.Json;
using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Core.Services;

/// <summary>
/// Service for managing game mods and loading external content
/// </summary>
public class ModManager
{
    private readonly string _modsDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public ModManager(string modsDirectory = "Mods")
    {
        _modsDirectory = modsDirectory;
        Directory.CreateDirectory(_modsDirectory);
    }

    /// <summary>
    /// Loads all gear definitions from mod files
    /// </summary>
    public Dictionary<string, GearItemDefinition> LoadGearDefinitions()
    {
        var definitions = new Dictionary<string, GearItemDefinition>();

        // Load base game definitions first
        LoadBaseGearDefinitions(definitions);

        // Load mod definitions
        foreach (var modDir in Directory.GetDirectories(_modsDirectory))
        {
            var gearFile = Path.Combine(modDir, "gear.json");
            if (File.Exists(gearFile))
            {
                try
                {
                    var modDefinitions = LoadGearDefinitionsFromFile(gearFile);
                    foreach (var def in modDefinitions)
                    {
                        definitions[def.Id] = def; // Mods can override base definitions
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue loading other mods
                    Console.WriteLine($"Error loading gear definitions from {gearFile}: {ex.Message}");
                }
            }
        }

        return definitions;
    }

    /// <summary>
    /// Loads all skill definitions from mod files
    /// </summary>
    public Dictionary<string, SkillDefinition> LoadSkillDefinitions()
    {
        var definitions = new Dictionary<string, SkillDefinition>();

        // Load base game definitions first
        LoadBaseSkillDefinitions(definitions);

        // Load mod definitions
        foreach (var modDir in Directory.GetDirectories(_modsDirectory))
        {
            var skillsFile = Path.Combine(modDir, "skills.json");
            if (File.Exists(skillsFile))
            {
                try
                {
                    var modDefinitions = LoadSkillDefinitionsFromFile(skillsFile);
                    foreach (var def in modDefinitions)
                    {
                        definitions[def.Id] = def; // Mods can override base definitions
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading skill definitions from {skillsFile}: {ex.Message}");
                }
            }
        }

        return definitions;
    }

    /// <summary>
    /// Loads all skill tree definitions from mod files
    /// </summary>
    public Dictionary<string, SkillTreeDefinition> LoadSkillTreeDefinitions()
    {
        var definitions = new Dictionary<string, SkillTreeDefinition>();

        // Load base game definitions first
        LoadBaseSkillTreeDefinitions(definitions);

        // Load mod definitions
        foreach (var modDir in Directory.GetDirectories(_modsDirectory))
        {
            var treesFile = Path.Combine(modDir, "skilltrees.json");
            if (File.Exists(treesFile))
            {
                try
                {
                    var modDefinitions = LoadSkillTreeDefinitionsFromFile(treesFile);
                    foreach (var def in modDefinitions)
                    {
                        definitions[def.TreeId] = def; // Mods can override base definitions
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading skill tree definitions from {treesFile}: {ex.Message}");
                }
            }
        }

        return definitions;
    }

    /// <summary>
    /// Validates a mod directory structure
    /// </summary>
    public ModValidationResult ValidateMod(string modDirectory)
    {
        var result = new ModValidationResult { IsValid = true, Errors = new List<string>() };

        if (!Directory.Exists(modDirectory))
        {
            result.IsValid = false;
            result.Errors.Add("Mod directory does not exist");
            return result;
        }

        // Check for required files (optional for now, but good to validate)
        var gearFile = Path.Combine(modDirectory, "gear.json");
        var skillsFile = Path.Combine(modDirectory, "skills.json");
        var treesFile = Path.Combine(modDirectory, "skilltrees.json");

        // Validate JSON files if they exist
        foreach (var file in new[] { gearFile, skillsFile, treesFile })
        {
            if (File.Exists(file))
            {
                try
                {
                    var content = File.ReadAllText(file);
                    JsonDocument.Parse(content);
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Invalid JSON in {Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }

        return result;
    }

    private void LoadBaseGearDefinitions(Dictionary<string, GearItemDefinition> definitions)
    {
        // This would load the base game gear definitions
        // For now, we'll assume they're loaded elsewhere
    }

    private void LoadBaseSkillDefinitions(Dictionary<string, SkillDefinition> definitions)
    {
        // This would load the base game skill definitions
    }

    private void LoadBaseSkillTreeDefinitions(Dictionary<string, SkillTreeDefinition> definitions)
    {
        // This would load the base game skill tree definitions
    }

    private List<GearItemDefinition> LoadGearDefinitionsFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<GearItemDefinition>>(json, _jsonOptions)
               ?? new List<GearItemDefinition>();
    }

    private List<SkillDefinition> LoadSkillDefinitionsFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<SkillDefinition>>(json, _jsonOptions)
               ?? new List<SkillDefinition>();
    }

    private List<SkillTreeDefinition> LoadSkillTreeDefinitionsFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<List<SkillTreeDefinition>>(json, _jsonOptions)
               ?? new List<SkillTreeDefinition>();
    }
}

public class ModValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}