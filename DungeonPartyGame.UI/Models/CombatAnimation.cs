using SkiaSharp;

namespace DungeonPartyGame.UI.Models;

public enum AnimationType
{
    Attack,
    Damage,
    Heal,
    Miss,
    Critical
}

public enum CharacterState
{
    Idle,
    Attacking,
    Hurt,
    Dodging,
    Dead,
    Victory
}

public enum CharacterRole
{
    Fighter,
    Rogue,
    Enemy
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
    public float BaseX { get; set; }
    public float BaseY { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public bool IsAlive => Health > 0;
    public CharacterState State { get; set; } = CharacterState.Idle;
    public CharacterRole Role { get; set; }
    public float AnimationTime { get; set; }
    public float HitFlashTime { get; set; }
    public float DodgeTime { get; set; }

    // Visual effects
    public List<DamageNumber> DamageNumbers { get; set; } = new();
    public List<StatusEffectIcon> StatusIcons { get; set; } = new();

    // Animation properties
    public float IdleBobOffset => (float)(Math.Sin(AnimationTime * 2) * 3);
    public bool IsFlashing => HitFlashTime > 0;

    public CharacterVisual(string name, float x, float y, float health, float maxHealth, CharacterRole role = CharacterRole.Fighter)
    {
        Name = name;
        X = x;
        Y = y;
        BaseX = x;
        BaseY = y;
        Health = health;
        MaxHealth = maxHealth;
        Role = role;
    }

    public void SetState(CharacterState newState)
    {
        State = newState;
        AnimationTime = 0;
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
    public bool IsHeal { get; set; }
    public float Lifetime { get; set; } // 0.0 to 1.0
    public float Scale { get; set; } = 1.0f;

    public DamageNumber(int value, float x, float y, bool isCritical = false, bool isMiss = false, bool isHeal = false)
    {
        Value = value;
        X = x;
        Y = y;
        IsCritical = isCritical;
        IsMiss = isMiss;
        IsHeal = isHeal;
    }
}

public class StatusEffectIcon
{
    public string Name { get; set; } = string.Empty;
    public SKColor Color { get; set; }
    public float Duration { get; set; }

    public StatusEffectIcon(string name, SKColor color)
    {
        Name = name;
        Color = color;
    }
}
