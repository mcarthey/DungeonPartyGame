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
    private readonly INavigation _navigation;
    private Character? _selectedCharacter;

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

    public PartyViewModel(GameSession gameSession, INavigation navigation)
    {
        _gameSession = gameSession;
        _navigation = navigation;

        // Initialize party members
        foreach (var character in _gameSession.Party)
        {
            PartyMembers.Add(character);
        }

        SelectCharacterCommand = new Command<Character>(OnSelectCharacter);
        NavigateToSkillsCommand = new Command(async () => await NavigateToSkills());
        NavigateToGearCommand = new Command(async () => await NavigateToGear());
        NavigateBackCommand = new Command(async () => await _navigation.PopAsync());
    }

    private void OnSelectCharacter(Character character)
    {
        SelectedCharacter = character;
    }

    private async Task NavigateToSkills()
    {
        if (SelectedCharacter != null)
        {
            var skillTreeViewModel = new SkillTreeViewModel(_gameSession, _navigation, SelectedCharacter);
            await _navigation.PushAsync(new SkillTreePage(skillTreeViewModel));
        }
    }

    private async Task NavigateToGear()
    {
        if (SelectedCharacter != null)
        {
            var gearViewModel = new GearViewModel(_gameSession, _navigation, SelectedCharacter);
            await _navigation.PushAsync(new GearPage(gearViewModel));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}