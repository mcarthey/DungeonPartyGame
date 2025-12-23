using System.Text.Json.Serialization;

namespace DungeonPartyGame.Core.Models;

public class GameState
{
    public List<Party> Parties { get; set; } = new();
    public int CurrentPartyIndex { get; set; }
    public Inventory Inventory { get; set; } = new();
    public HashSet<string> CompletedEncounters { get; set; } = new();
    public DateTime SaveTimestamp { get; set; }

    // For backward compatibility
    [JsonIgnore]
    public Party CurrentParty => Parties.Count > CurrentPartyIndex ? Parties[CurrentPartyIndex] : null!;
}
