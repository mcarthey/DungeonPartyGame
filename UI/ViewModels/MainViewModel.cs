using System.Text;
using System.Windows.Input;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.UI.Pages;

namespace DungeonPartyGame.UI.ViewModels;

public class MainViewModel : BindableObject
{
    private readonly CombatEngine _combatEngine;
    private readonly DiceService _diceService;
    private readonly GameSession _gameSession;
    private readonly INavigation _navigation;
    private readonly ProgressionService _progressionService;

    private Character? _fighter;
    private Character? _rogue;
    private CombatSession? _currentSession;
    private string _combatLog = string.Empty;

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

    public MainViewModel(CombatEngine combatEngine, DiceService diceService, GameSession gameSession, INavigation navigation, ProgressionService progressionService)
    {
        _combatEngine = combatEngine;
        _diceService = diceService;
        _gameSession = gameSession;
        _navigation = navigation;
        _progressionService = progressionService;

        CreateCharactersCommand = new Command(CreateCharacters);
        StartNewCombatCommand = new Command(StartNewCombat, () => _fighter != null && _rogue != null);
        NextRoundCommand = new Command(ExecuteNextRound, () => _currentSession != null && !_currentSession.IsComplete);
        NavigateToPartyCommand = new Command(async () => await NavigateToParty());
        GrantXpCommand = new Command(GrantXp);
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

        NextRoundCommand.ChangeCanExecute();
    }

    private void ExecuteNextRound()
    {
        if (_currentSession == null || _currentSession.IsComplete)
        {
            return;
        }

        var result = _combatEngine.ExecuteRound(_currentSession);

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

    private async Task NavigateToParty()
    {
        var partyViewModel = new PartyViewModel(_gameSession, _navigation);
        await _navigation.PushAsync(new PartyPage(partyViewModel));
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
