using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.UI.Pages;

namespace DungeonPartyGame.UI.ViewModels;

public class PartyViewModel : INotifyPropertyChanged
{
    private readonly GameSession _gameSession;
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

    public PartyViewModel(GameSession gameSession)
    {
        _gameSession = gameSession;

        // Initialize party members
        foreach (var character in _gameSession.Party)
        {
            PartyMembers.Add(character);
        }

        SelectCharacterCommand = new Command<Character>(OnSelectCharacter);
        NavigateToSkillsCommand = new Command(() => NavigateToSkillsRequested?.Invoke());
        NavigateToGearCommand = new Command(() => NavigateToGearRequested?.Invoke());
        NavigateBackCommand = new Command(() => NavigateBackRequested?.Invoke());
    }

    private void OnSelectCharacter(Character character)
    {
        SelectedCharacter = character;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}