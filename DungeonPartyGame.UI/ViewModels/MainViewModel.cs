using System.Text;
using System.Windows.Input;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.UI.Pages;
using DungeonPartyGame.UI.Controls;

namespace DungeonPartyGame.UI.ViewModels;

public class MainViewModel : BindableObject
{
    private readonly CombatEngine _combatEngine;
    private readonly DiceService _diceService;
    private readonly GameSession _gameSession;
    private readonly ProgressionService _progressionService;

    private Character? _fighter;
    private Character? _rogue;
    private CombatSession? _currentSession;
    private string _combatLog = string.Empty;
    private CombatCanvas? _combatCanvas;

    public string CombatLog
    {
        get => _combatLog;
        set { _combatLog = value; OnPropertyChanged(); }
    }

    public Command CreateCharactersCommand { get; }
    public Command StartNewCombatCommand { get; }
    public Command NextRoundCommand { get; }
    public Command NavigateToPartyCommand { get; }
    public Command GrantXpCommand { get; }
    public GameSession GameSession => _gameSession;

    public MainViewModel(CombatEngine combatEngine, DiceService diceService, GameSession gameSession, ProgressionService progressionService)
    {
        _combatEngine = combatEngine;
        _diceService = diceService;
        _gameSession = gameSession;
        _progressionService = progressionService;

        CreateCharactersCommand = new Command(CreateCharacters);
        StartNewCombatCommand = new Command(StartNewCombat, () => _fighter != null && _rogue != null);
        NextRoundCommand = new Command(ExecuteNextRound, () => _currentSession != null && !_currentSession.IsComplete);
        NavigateToPartyCommand = new Command(async () => await NavigateToParty());
        GrantXpCommand = new Command(GrantXp);
    }

    public void SetCombatCanvas(CombatCanvas canvas)
    {
        _combatCanvas = canvas;
    }

    private void CreateCharacters()
    {
        _fighter = new Character(
            "Fighter",
            CharacterRole.Fighter,
            new Stats(15, 8, 5, 50)
        );
        _rogue = new Character(
            "Rogue",
            CharacterRole.Rogue,
            new Stats(8, 16, 7, 40)
        );

        // Add to party
        _gameSession.AddCharacterToParty(_fighter);
        _gameSession.AddCharacterToParty(_rogue);

        CombatLog = "Characters created and added to party. Press Start New Combat.";
        _currentSession = null;
        StartNewCombatCommand.ChangeCanExecute();
        NextRoundCommand.ChangeCanExecute();
    }

    private void StartNewCombat()
    {
        if (_fighter == null || _rogue == null)
        {
            CombatLog = "Create characters first.";
            return;
        }

        // Reset HP for replay
        _fighter.Stats.CurrentHealth = _fighter.Stats.MaxHealth;
        _rogue.Stats.CurrentHealth = _rogue.Stats.MaxHealth;

        var partyA = new Party();
        partyA.Add(_fighter);
        var partyB = new Party();
        partyB.Add(_rogue);

        _currentSession = _combatEngine.CreateSession(partyA, partyB);

        var initiativeWinner = _currentSession.CurrentAttacker.Name;
        CombatLog = $"Combat begins!\n{initiativeWinner} wins initiative.\n\n";

        // Initialize combat canvas
        _combatCanvas?.ClearCharacters();
        _combatCanvas?.AddCharacter(_fighter.Name, 100, 125, _fighter.Stats.CurrentHealth, _fighter.Stats.MaxHealth);
        _combatCanvas?.AddCharacter(_rogue.Name, 300, 125, _rogue.Stats.CurrentHealth, _rogue.Stats.MaxHealth);

        NextRoundCommand.ChangeCanExecute();
    }

    private void ExecuteNextRound()
    {
        if (_currentSession == null || _currentSession.IsComplete)
        {
            return;
        }

        var result = _combatEngine.ExecuteRound(_currentSession);

        // Trigger combat animations
        foreach (var targetResult in result.Targets)
        {
            _combatCanvas?.PlayAttackAnimation(
                result.Actor.Name,
                targetResult.Target.Name,
                targetResult.Damage,
                isCritical: false, // Can enhance this later with critical hit detection
                isMiss: targetResult.Damage == 0
            );

            // Update health bars
            _combatCanvas?.UpdateCharacterHealth(targetResult.Target.Name, targetResult.Target.Stats.CurrentHealth);
        }

        var sb = new StringBuilder(CombatLog);
        sb.AppendLine(result.LogMessage);
        if (result.IsFinalRound)
        {
            sb.AppendLine();
            sb.AppendLine(result.SummaryText);

            // Grant XP to winner
            if (_currentSession.Winner != null)
            {
                _progressionService.AddXp(_currentSession.Winner, 100);
                var winnerNames = string.Join(", ", _currentSession.Winner.AliveMembers.Select(c => c.Name));
                sb.AppendLine($"\n{winnerNames} gained 100 XP!");
            }
        }
        sb.AppendLine();
        CombatLog = sb.ToString();

        NextRoundCommand.ChangeCanExecute();
    }

    public event Action? NavigateToPartyRequested;

    private async Task NavigateToParty()
    {
        NavigateToPartyRequested?.Invoke();
    }

    private void GrantXp()
    {
        foreach (var character in _gameSession.Party)
        {
            _progressionService.AddXp(character, 50);
        }
        CombatLog += "\nGranted 50 XP to all party members.\n";
    }
}
