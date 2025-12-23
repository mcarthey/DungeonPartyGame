using System.Text;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;

namespace DungeonPartyGame.UI.ViewModels;

public class MainViewModel : BindableObject
{
    private readonly CombatEngine _combatEngine;
    private readonly DiceService _diceService;

    private Party? _playerParty;
    private Party? _enemyParty;
    private CombatSession? _currentSession;
    private string _combatLog = string.Empty;

    public string CombatLog
    {
        get => _combatLog;
        set { _combatLog = value; OnPropertyChanged(); }
    }

    public Command CreatePartiesCommand { get; }
    public Command StartNewCombatCommand { get; }
    public Command NextTurnCommand { get; }

    public MainViewModel(CombatEngine combatEngine, DiceService diceService)
    {
        _combatEngine = combatEngine;
        _diceService = diceService;

        CreatePartiesCommand = new Command(CreateParties);
        StartNewCombatCommand = new Command(StartNewCombat, () => _playerParty != null && _enemyParty != null);
        NextTurnCommand = new Command(ExecuteNextTurn, () => _currentSession != null && !_currentSession.IsComplete);
    }

    private void CreateParties()
    {
        // Create player party
        _playerParty = new Party();
        _playerParty.Add(new Character(
            "Fighter",
            Role.Tank,
            new Stats(15, 8, 5, 50),
            new Equipment(new Weapon("Sword", 6, 10, "Strength")),
            new List<Skill>
            {
                new Skill("Power Strike", "A heavy blow.", TargetingRule.SingleEnemy, 1.3, 1),
                new Skill("Slash", "A quick slash.", TargetingRule.SingleEnemy, 1.0, 0)
            }
        ));
        _playerParty.Add(new Character(
            "Rogue",
            Role.DPS,
            new Stats(8, 16, 7, 40),
            new Equipment(new Weapon("Dagger", 4, 8, "Dexterity")),
            new List<Skill>
            {
                new Skill("Quick Stab", "Fast and precise.", TargetingRule.SingleEnemy, 1.1, 1),
                new Skill("Sneak Attack", "Hits from the shadows.", TargetingRule.SingleEnemy, 1.4, 2)
            }
        ));

        // Create enemy party
        _enemyParty = new Party();
        _enemyParty.Add(new Character(
            "Goblin Warrior",
            Role.Tank,
            new Stats(12, 10, 4, 45),
            new Equipment(new Weapon("Club", 3, 7, "Strength")),
            new List<Skill>
            {
                new Skill("Bash", "A heavy club swing.", TargetingRule.SingleEnemy, 1.2, 1),
                new Skill("Smash", "Crushing blow.", TargetingRule.SingleEnemy, 1.0, 0)
            }
        ));
        _enemyParty.Add(new Character(
            "Goblin Mage",
            Role.Support,
            new Stats(6, 14, 8, 35),
            new Equipment(new Weapon("Staff", 2, 5, "Intelligence")),
            new List<Skill>
            {
                new Skill("Magic Missile", "Arcane projectile.", TargetingRule.SingleEnemy, 1.1, 1),
                new Skill("Zap", "Electric shock.", TargetingRule.SingleEnemy, 1.0, 0)
            }
        ));

        CombatLog = "Parties created. Press Start New Battle.";
        _currentSession = null;
        StartNewCombatCommand.ChangeCanExecute();
        NextTurnCommand.ChangeCanExecute();
    }

    private void StartNewCombat()
    {
        if (_playerParty == null || _enemyParty == null)
        {
            CombatLog = "Create parties first.";
            return;
        }

        // Reset HP for replay
        foreach (var character in _playerParty.Members.Concat(_enemyParty.Members))
        {
            character.Stats.CurrentHealth = character.Stats.MaxHealth;
        }

        _currentSession = _combatEngine.CreateSession(_playerParty, _enemyParty);

        CombatLog = $"Battle begins!\nRound {_currentSession.RoundNumber}\n\n";

        NextTurnCommand.ChangeCanExecute();
    }

    private void ExecuteNextTurn()
    {
        if (_currentSession == null || _currentSession.IsComplete)
        {
            return;
        }

        var result = _combatEngine.ExecuteTurn(_currentSession);

        var sb = new StringBuilder(CombatLog);
        sb.AppendLine($"Round {result.RoundNumber}");
        sb.AppendLine(result.SummaryText);
        if (result.IsFinalTurn)
        {
            sb.AppendLine();
            sb.AppendLine(result.IsFinalTurn ? "Battle Complete!" : "");
        }
        sb.AppendLine();
        CombatLog = sb.ToString();

        NextTurnCommand.ChangeCanExecute();
    }
}
