namespace DungeonPartyGame.UI.Models;

public enum AnimationType
{
    Attack,
    Damage,
    Heal,
    Miss,
    Critical
}

public class CombatAnimation
{
    public AnimationType Type { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public int Value { get; set; }
    public float Progress { get; set; } // 0.0 to 1.0
    public bool IsComplete => Progress >= 1.0f;

    public CombatAnimation(AnimationType type, string characterName, string targetName = "", int value = 0)
    {
        Type = type;
        CharacterName = characterName;
        TargetName = targetName;
        Value = value;
        Progress = 0.0f;
    }
}

public class CharacterVisual
{
    public string Name { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public bool IsAlive => Health > 0;
    public bool IsAttacking { get; set; }
    public float AttackProgress { get; set; }

    // Visual effects
    public List<DamageNumber> DamageNumbers { get; set; } = new();

    public CharacterVisual(string name, float x, float y, float health, float maxHealth)
    {
        Name = name;
        X = x;
        Y = y;
        Health = health;
        MaxHealth = maxHealth;
    }
}

public class DamageNumber
{
    public int Value { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Opacity { get; set; } = 1.0f;
    public bool IsCritical { get; set; }
    public bool IsMiss { get; set; }
    public float Lifetime { get; set; } // 0.0 to 1.0

    public DamageNumber(int value, float x, float y, bool isCritical = false, bool isMiss = false)
    {
        Value = value;
        X = x;
        Y = y;
        IsCritical = isCritical;
        IsMiss = isMiss;
    }
}
