namespace DungeonPartyGame.Core.Models;

public class Stats
{
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }
    public int Crit { get; set; }
    public int Dodge { get; set; }

    public Stats(int strength, int dexterity, int constitution, int maxHealth)
    {
        if (strength < 0) throw new ArgumentException("Strength cannot be negative", nameof(strength));
        if (dexterity < 0) throw new ArgumentException("Dexterity cannot be negative", nameof(dexterity));
        if (constitution < 0) throw new ArgumentException("Constitution cannot be negative", nameof(constitution));
        if (maxHealth <= 0) throw new ArgumentException("Max health must be positive", nameof(maxHealth));

        Strength = strength;
        Dexterity = dexterity;
        Constitution = constitution;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        Crit = 0;
        Dodge = 0;
    }

    public Stats Clone() => new(Strength, Dexterity, Constitution, MaxHealth) { CurrentHealth = this.CurrentHealth, Crit = this.Crit, Dodge = this.Dodge };
}