using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;

namespace DungeonPartyGame.UI.ViewModels;

public class SkillTreeViewModel : INotifyPropertyChanged
{
    private readonly GameSession _gameSession;
    private readonly SkillTreeService _skillTreeService;
    private readonly Character _character;
    private SkillNode? _selectedNode;

    public event Action? NavigateBackRequested;

    public Character SelectedCharacter => _character;

    public int UnspentSkillPoints => _character.Progression.UnspentSkillPoints;

    public ObservableCollection<SkillNode> AvailableNodes { get; } = new();
    public ObservableCollection<SkillDefinition> UnlockedSkills { get; } = new();

    public SkillNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            _selectedNode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanUnlockSelectedNode));
        }
    }

    public bool CanUnlockSelectedNode => SelectedNode != null && _skillTreeService.CanUnlockNode(_character, SelectedNode);

    public ICommand UnlockNodeCommand { get; }
    public ICommand NavigateBackCommand { get; }

    public SkillTreeViewModel(Character character)
    {
        _gameSession = new GameSession(); // This should be injected, but for now create a new one
        _character = character;
        _skillTreeService = new SkillTreeService();

        UnlockNodeCommand = new Command(OnUnlockNode, () => CanUnlockSelectedNode);
        NavigateBackCommand = new Command(() => NavigateBackRequested?.Invoke());

        LoadAvailableNodes();
        LoadUnlockedSkills();
    }

    private void LoadAvailableNodes()
    {
        AvailableNodes.Clear();
        var tree = _skillTreeService.GetSkillTree(_character.Role);
        foreach (var node in tree.Nodes)
        {
            if (!_character.Progression.UnlockedSkillNodes.Contains(node.NodeId))
            {
                AvailableNodes.Add(node);
            }
        }
    }

    private void LoadUnlockedSkills()
    {
        UnlockedSkills.Clear();
        foreach (var nodeId in _character.Progression.UnlockedSkillNodes)
        {
            var tree = _skillTreeService.GetSkillTree(_character.Role);
            var node = tree.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node != null)
            {
                if (tree.SkillDefinitions.TryGetValue(node.SkillId, out var skill))
                {
                    UnlockedSkills.Add(skill);
                }
            }
        }
    }

    private void OnUnlockNode()
    {
        if (SelectedNode != null && _skillTreeService.UnlockNode(_character, SelectedNode))
        {
            LoadAvailableNodes();
            LoadUnlockedSkills();
            OnPropertyChanged(nameof(UnspentSkillPoints));
            (UnlockNodeCommand as Command)?.ChangeCanExecute();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}