using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.UI.ViewModels;

public class GearViewModel : INotifyPropertyChanged
{
    private readonly GameSession _gameSession;
    private readonly GearService _gearService;
    private readonly GearUpgradeService _gearUpgradeService;
    private readonly Character _character;
    private readonly ILogger<GearViewModel> _logger;
    private GearInstance? _selectedGearItem;

    public event Action? NavigateBackRequested;

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

    public GearViewModel(Character character, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GearViewModel>();
        _logger.LogInformation("GearViewModel constructor called for character {Name}", character?.Name ?? "null");

        try
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _gameSession = new GameSession(); // This should be injected, but for now create a new one
            _gearService = new GearService();
            _gearUpgradeService = new GearUpgradeService(_gearService);

            EquipCommand = new Command<GearInstance>(OnEquip);
            UnequipCommand = new Command<GearSlot>(OnUnequip);
            UpgradeGearCommand = new Command(OnUpgradeGear, () => CanUpgradeSelectedGear);
            NavigateBackCommand = new Command(() => NavigateBackRequested?.Invoke());

            LoadEquippedGear();
            LoadInventoryGear();

            _logger.LogInformation("GearViewModel initialized successfully for character {Name}", _character.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing GearViewModel for character {Name}", character?.Name ?? "null");
            throw;
        }
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