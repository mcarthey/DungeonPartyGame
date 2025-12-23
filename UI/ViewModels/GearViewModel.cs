using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;

namespace DungeonPartyGame.UI.ViewModels;

public class GearViewModel : INotifyPropertyChanged
{
    private readonly GameSession _gameSession;
    private readonly INavigation _navigation;
    private readonly GearService _gearService;
    private readonly GearUpgradeService _gearUpgradeService;
    private readonly Character _character;
    private GearInstance? _selectedGearItem;

    public Character SelectedCharacter => _character;
    public Inventory Inventory => _gameSession.Inventory;

    public ObservableCollection<KeyValuePair<GearSlot, GearInstance>> EquippedGear { get; } = new();
    public ObservableCollection<GearInstance> InventoryGear { get; } = new();

    public GearInstance? SelectedGearItem
    {
        get => _selectedGearItem;
        set
        {
            _selectedGearItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanUpgradeSelectedGear));
        }
    }

    public bool CanUpgradeSelectedGear => SelectedGearItem != null && _gearUpgradeService.CanUpgrade(SelectedGearItem, Inventory);

    public ICommand EquipCommand { get; }
    public ICommand UnequipCommand { get; }
    public ICommand UpgradeGearCommand { get; }
    public ICommand NavigateBackCommand { get; }

    public GearViewModel(GameSession gameSession, INavigation navigation, Character character)
    {
        _gameSession = gameSession;
        _navigation = navigation;
        _character = character;
        _gearService = new GearService();
        _gearUpgradeService = new GearUpgradeService(_gearService);

        EquipCommand = new Command<GearInstance>(OnEquip);
        UnequipCommand = new Command<GearSlot>(OnUnequip);
        UpgradeGearCommand = new Command(OnUpgradeGear, () => CanUpgradeSelectedGear);
        NavigateBackCommand = new Command(async () => await _navigation.PopAsync());

        LoadEquippedGear();
        LoadInventoryGear();
    }

    private void LoadEquippedGear()
    {
        EquippedGear.Clear();
        foreach (var kvp in _character.Equipment)
        {
            EquippedGear.Add(kvp);
        }
    }

    private void LoadInventoryGear()
    {
        InventoryGear.Clear();
        foreach (var gear in Inventory.GearItems)
        {
            InventoryGear.Add(gear);
        }
    }

    private void OnEquip(GearInstance gear)
    {
        if (_gearService.EquipGear(_character, gear))
        {
            LoadEquippedGear();
            LoadInventoryGear();
        }
    }

    private void OnUnequip(GearSlot slot)
    {
        if (_gearService.UnequipGear(_character, slot))
        {
            LoadEquippedGear();
            LoadInventoryGear();
        }
    }

    private void OnUpgradeGear()
    {
        if (SelectedGearItem != null && _gearUpgradeService.Upgrade(SelectedGearItem, Inventory))
        {
            LoadInventoryGear();
            OnPropertyChanged(nameof(CanUpgradeSelectedGear));
            (UpgradeGearCommand as Command)?.ChangeCanExecute();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}