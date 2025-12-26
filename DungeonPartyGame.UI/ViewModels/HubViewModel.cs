using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.UI.ViewModels;

public class HubViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private readonly CurrencyService _currencyService;
    private readonly EventService _eventService;
    private readonly DailyRewardService _dailyRewardService;
    private readonly ILogger<HubViewModel> _logger;

    private string _goldDisplay = "💰 0";
    private string _gemsDisplay = "💎 0";
    private string _playerName = "Adventurer";
    private int _playerLevel = 1;
    private int _currentXp = 0;
    private int _nextLevelXp = 1000;
    private bool _hasDailyReward = false;
    private string _dailyRewardText = "";
    private int _activeEventsCount = 0;

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

    public string PlayerName
    {
        get => _playerName;
        set { _playerName = value; OnPropertyChanged(); }
    }

    public int PlayerLevel
    {
        get => _playerLevel;
        set { _playerLevel = value; OnPropertyChanged(); }
    }

    public int CurrentXp
    {
        get => _currentXp;
        set { _currentXp = value; OnPropertyChanged(); OnPropertyChanged(nameof(XpProgress)); }
    }

    public int NextLevelXp
    {
        get => _nextLevelXp;
        set { _nextLevelXp = value; OnPropertyChanged(); OnPropertyChanged(nameof(XpProgress)); }
    }

    public double XpProgress => NextLevelXp > 0 ? (double)CurrentXp / NextLevelXp : 0;

    public bool HasDailyReward
    {
        get => _hasDailyReward;
        set { _hasDailyReward = value; OnPropertyChanged(); }
    }

    public string DailyRewardText
    {
        get => _dailyRewardText;
        set { _dailyRewardText = value; OnPropertyChanged(); }
    }

    public int ActiveEventsCount
    {
        get => _activeEventsCount;
        set { _activeEventsCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(EventBadgeVisible)); }
    }

    public bool EventBadgeVisible => ActiveEventsCount > 0;

    public Command NavigateToCombatCommand { get; }
    public Command NavigateToPartyCommand { get; }
    public Command NavigateToSkillsCommand { get; }
    public Command NavigateToGearCommand { get; }
    public Command NavigateToStoreCommand { get; }
    public Command NavigateToEventsCommand { get; }
    public Command ClaimDailyRewardCommand { get; }

    public event Action? NavigateToCombatRequested;
    public event Action? NavigateToPartyRequested;
    public event Action? NavigateToSkillsRequested;
    public event Action? NavigateToGearRequested;
    public event Action? NavigateToStoreRequested;
    public event Action? NavigateToEventsRequested;

    public HubViewModel(
        CurrencyService currencyService,
        EventService eventService,
        DailyRewardService dailyRewardService,
        ILogger<HubViewModel> logger)
    {
        _currencyService = currencyService;
        _eventService = eventService;
        _dailyRewardService = dailyRewardService;
        _logger = logger;

        NavigateToCombatCommand = new Command(() => NavigateToCombatRequested?.Invoke());
        NavigateToPartyCommand = new Command(() => NavigateToPartyRequested?.Invoke());
        NavigateToSkillsCommand = new Command(() => NavigateToSkillsRequested?.Invoke());
        NavigateToGearCommand = new Command(() => NavigateToGearRequested?.Invoke());
        NavigateToStoreCommand = new Command(() => NavigateToStoreRequested?.Invoke());
        NavigateToEventsCommand = new Command(() => NavigateToEventsRequested?.Invoke());
        ClaimDailyRewardCommand = new Command(ClaimDailyReward, () => HasDailyReward);

        // Subscribe to currency changes
        _currencyService.CurrencyChanged += OnCurrencyChanged;

        // Subscribe to daily reward
        _dailyRewardService.RewardClaimed += OnRewardClaimed;

        RefreshData();
    }

    public void RefreshData()
    {
        // Update currency display
        GoldDisplay = $"💰 {_currencyService.GetBalance(CurrencyType.Gold):N0}";
        GemsDisplay = $"💎 {_currencyService.GetBalance(CurrencyType.Gems):N0}";

        // Check daily reward
        CheckDailyReward();

        // Check active events
        ActiveEventsCount = _eventService.GetActiveEvents().Count;

        _logger.LogInformation("Hub data refreshed");
    }

    private void OnCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
    {
        if (type == CurrencyType.Gold)
        {
            GoldDisplay = $"💰 {newAmount:N0}";
        }
        else if (type == CurrencyType.Gems)
        {
            GemsDisplay = $"💎 {newAmount:N0}";
        }
    }

    private void CheckDailyReward()
    {
        HasDailyReward = _dailyRewardService.CanClaimDailyReward();

        if (HasDailyReward)
        {
            var reward = _dailyRewardService.GetTodaysReward();
            DailyRewardText = reward != null
                ? $"🎁 Day {_dailyRewardService.GetLoginStreak().CurrentStreak} Reward: {reward.GetRewardDisplay()}"
                : "🎁 Daily Reward Ready!";
        }
        else
        {
            DailyRewardText = "✅ Daily reward claimed!";
        }

        ClaimDailyRewardCommand.ChangeCanExecute();
    }

    private void ClaimDailyReward()
    {
        if (_dailyRewardService.TryClaimDailyReward())
        {
            _logger.LogInformation("Daily reward claimed from hub");
            CheckDailyReward();
        }
    }

    private void OnRewardClaimed(DailyReward reward)
    {
        CheckDailyReward();
    }
}
