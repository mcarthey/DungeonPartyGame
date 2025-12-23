using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.UI.Pages;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.UI.ViewModels;

public class PartyViewModel : INotifyPropertyChanged
{
    private readonly GameSession _gameSession;
    private readonly ILogger<PartyViewModel> _logger;
    private Character? _selectedCharacter;

    public event Action? NavigateToSkillsRequested;
    public event Action? NavigateToGearRequested;
    public event Action? NavigateBackRequested;

    public ObservableCollection<Character> PartyMembers { get; } = new();

    public Character? SelectedCharacter
    {
        get => _selectedCharacter;
        set
        {
            _selectedCharacter = value;
            OnPropertyChanged();
        }
    }

    public ICommand SelectCharacterCommand { get; }
    public ICommand NavigateToSkillsCommand { get; }
    public ICommand NavigateToGearCommand { get; }
    public ICommand NavigateBackCommand { get; }

    public PartyViewModel(GameSession gameSession, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PartyViewModel>();
        _logger.LogInformation("PartyViewModel constructor called");

        try
        {
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
            _logger.LogInformation("GameSession assigned successfully");

            // Initialize party members
            var partyMembers = _gameSession.Party;
            _logger.LogInformation("Retrieved {Count} party members from GameSession", partyMembers.Count);

            foreach (var character in partyMembers)
            {
                PartyMembers.Add(character);
                _logger.LogDebug("Added character {Name} to PartyMembers", character.Name);
            }

            SelectCharacterCommand = new Command<Character>(OnSelectCharacter);
            NavigateToSkillsCommand = new Command(() => NavigateToSkillsRequested?.Invoke());
            NavigateToGearCommand = new Command(() => NavigateToGearRequested?.Invoke());
            NavigateBackCommand = new Command(() => NavigateBackRequested?.Invoke());

            _logger.LogInformation("PartyViewModel initialized successfully with {Count} party members", PartyMembers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing PartyViewModel");
            throw;
        }
    }

    private void OnSelectCharacter(Character character)
    {
        _logger.LogInformation("Character selected: {Name}", character?.Name ?? "null");

        try
        {
            SelectedCharacter = character;
            _logger.LogDebug("SelectedCharacter set successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting character {Name}", character?.Name ?? "null");
            throw;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}