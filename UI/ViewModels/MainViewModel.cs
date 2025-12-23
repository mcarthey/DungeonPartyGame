using System.Text;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;

namespace DungeonPartyGame.UI.ViewModels;

public class MainViewModel : BindableObject
{
    private readonly CombatEngine _combatEngine;
    private readonly DiceService _diceService;

    private Character? _fighter;
    private Character? _rogue;
    private List<CombatResult> _combatResults = new();
    private string _combatLog = string.Empty;

    public string CombatLog
    {
        get => _combatLog;
        set { _combatLog = value; OnPropertyChanged(); }
    }

    public Command CreateCharactersCommand { get; }
    public Command StartCombatCommand { get; }

    public MainViewModel(CombatEngine combatEngine, DiceService diceService)
    {
        _combatEngine = combatEngine;
        _diceService = diceService;

        CreateCharactersCommand = new Command(CreateCharacters);
        StartCombatCommand = new Command(StartCombat, () => _fighter != null && _rogue != null);
    }

    private void CreateCharacters()
    {
        _fighter = new Character(
            "Fighter",
            new Stats(15, 8, 5, 50),
            new Equipment(new Weapon("Sword", 6, 10, "Strength")),
            new List<Skill>
            {
                new Skill("Power Strike", "A heavy blow.", 1.3, 1),
                new Skill("Slash", "A quick slash.", 1.0, 0)
            }
        );
        _rogue = new Character(
            "Rogue",
            new Stats(8, 16, 7, 40),
            new Equipment(new Weapon("Dagger", 4, 8, "Dexterity")),
            new List<Skill>
            {
                new Skill("Quick Stab", "Fast and precise.", 1.1, 1),
                new Skill("Sneak Attack", "Hits from the shadows.", 1.4, 2)
            }
        );
        CombatLog = "Characters created. Press Start Combat.";
        _combatResults.Clear();
        StartCombatCommand.ChangeCanExecute();
    }

    private void StartCombat()
    {
        if (_fighter == null || _rogue == null)
        {
            CombatLog = "Create characters first.";
            return;
        }
        // Reset HP for replay
        _fighter.Stats.CurrentHealth = _fighter.Stats.MaxHealth;
        _rogue.Stats.CurrentHealth = _rogue.Stats.MaxHealth;
        _combatResults = _combatEngine.RunCombat(_fighter, _rogue).ToList();
        CombatLog = FormatCombatLog(_combatResults);
    }

    private string FormatCombatLog(IEnumerable<CombatResult> results)
    {
        var sb = new StringBuilder();
        foreach (var r in results)
        {
            sb.AppendLine(r.LogMessage);
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
