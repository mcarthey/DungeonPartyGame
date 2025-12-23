namespace DungeonPartyGame.Core.Services;

public class DiceService
{
    private readonly Random _random;
    public DiceService(Random? random = null)
    {
        _random = random ?? new Random();
    }
    public int Roll(int min, int max) => _random.Next(min, max + 1);
}