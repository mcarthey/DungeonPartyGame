namespace DungeonPartyGame.Core.Models;

public class Equipment
{
    public Weapon Weapon { get; set; }

    public Equipment(Weapon weapon)
    {
        Weapon = weapon;
    }
}