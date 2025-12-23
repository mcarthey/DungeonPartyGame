namespace DungeonPartyGame.Core.Models;

public class Stats
{
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }

    public Stats(int strength, int dexterity, int intelligence, int maxHealth)
    {
        Strength = strength;
        Dexterity = dexterity;
        Intelligence = intelligence;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public Stats Clone() => new(Strength, Dexterity, Intelligence, MaxHealth) { CurrentHealth = this.CurrentHealth };
}