using System.Collections.ObjectModel;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.UI.ViewModels;

public class StoreViewModel : BindableObject
{
    private readonly StoreService _storeService;
    private readonly CurrencyService _currencyService;
    private readonly ILogger<StoreViewModel> _logger;

    private string _goldDisplay = "💰 0";
    private string _gemsDisplay = "💎 0";

    public ObservableCollection<StoreItem> FeaturedItems { get; } = new();
    public ObservableCollection<StoreItem> CurrencyPacks { get; } = new();
    public ObservableCollection<StoreItem> GearPacks { get; } = new();
    public ObservableCollection<StoreItem> Consumables { get; } = new();

    public string GoldDisplay
    {
        get => _goldDisplay;
        set { _goldDisplay = value; OnPropertyChanged(); }
    }

    public string GemsDisplay
    {
        get => _gemsDisplay;
        set { _gemsDisplay = value; OnPropertyChanged(); }
    }

    public Command<StoreItem> PurchaseCommand { get; }
    public Command RefreshCommand { get; }

    public StoreViewModel(StoreService storeService, CurrencyService currencyService, ILogger<StoreViewModel> logger)
    {
        _storeService = storeService;
        _currencyService = currencyService;
        _logger = logger;

        PurchaseCommand = new Command<StoreItem>(PurchaseItem, CanPurchaseItem);
        RefreshCommand = new Command(RefreshStore);

        // Subscribe to currency changes
        _currencyService.CurrencyChanged += OnCurrencyChanged;

        // Subscribe to purchases
        _storeService.ItemPurchased += OnItemPurchased;

        LoadStoreItems();
        UpdateCurrencyDisplay();
    }

    private void LoadStoreItems()
    {
        FeaturedItems.Clear();
        CurrencyPacks.Clear();
        GearPacks.Clear();
        Consumables.Clear();

        var featured = _storeService.GetFeaturedItems();
        foreach (var item in featured)
        {
            FeaturedItems.Add(item);
        }

        var currencyPacks = _storeService.GetItemsByType(StoreItemType.CurrencyPack);
        foreach (var item in currencyPacks)
        {
            CurrencyPacks.Add(item);
        }

        var gearPacks = _storeService.GetItemsByType(StoreItemType.GearPack);
        gearPacks.AddRange(_storeService.GetItemsByType(StoreItemType.BattlePass));
        foreach (var item in gearPacks)
        {
            GearPacks.Add(item);
        }

        var consumables = _storeService.GetItemsByType(StoreItemType.Consumable);
        foreach (var item in consumables)
        {
            Consumables.Add(item);
        }

        _logger.LogInformation($"Loaded store: {FeaturedItems.Count} featured, {CurrencyPacks.Count} currency packs, {GearPacks.Count} gear packs, {Consumables.Count} consumables");
    }

    private bool CanPurchaseItem(StoreItem? item)
    {
        if (item == null)
            return false;

        return _storeService.CanPurchase(item.Id);
    }

    private async void PurchaseItem(StoreItem? item)
    {
        if (item == null)
            return;

        if (!_storeService.CanPurchase(item.Id))
        {
            await Application.Current!.MainPage!.DisplayAlert("Insufficient Funds",
                $"You don't have enough {string.Join(" or ", item.Price.Select(p => _currencyService.GetCurrencySymbol(p.Type) + p.Amount))}",
                "OK");
            return;
        }

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm Purchase",
            $"Purchase {item.Name} for {item.GetPriceDisplay()}?",
            "Buy",
            "Cancel");

        if (!confirm)
            return;

        if (_storeService.TryPurchase(item.Id))
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Purchase Successful!",
                $"You bought {item.Name}!",
                "OK");

            RefreshStore();
        }
    }

    private void RefreshStore()
    {
        LoadStoreItems();
        UpdateCurrencyDisplay();
        PurchaseCommand.ChangeCanExecute();
    }

    private void UpdateCurrencyDisplay()
    {
        GoldDisplay = $"💰 {_currencyService.GetBalance(CurrencyType.Gold):N0}";
        GemsDisplay = $"💎 {_currencyService.GetBalance(CurrencyType.Gems):N0}";
    }

    private void OnCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
    {
        UpdateCurrencyDisplay();
        PurchaseCommand.ChangeCanExecute();
    }

    private void OnItemPurchased(StoreItem item)
    {
        RefreshStore();
    }
}
