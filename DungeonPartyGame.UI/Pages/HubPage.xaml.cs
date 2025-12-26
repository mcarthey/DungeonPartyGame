using DungeonPartyGame.UI.ViewModels;
using DungeonPartyGame.Core.Models;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.UI.Pages;

public partial class HubPage : ContentPage
{
    private readonly ILogger<HubPage> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly GameSession _gameSession;

    public HubPage(HubViewModel viewModel, ILoggerFactory loggerFactory, GameSession gameSession)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<HubPage>();
        _gameSession = gameSession;

        InitializeComponent();
        BindingContext = viewModel;

        // Subscribe to navigation events
        viewModel.NavigateToCombatRequested += OnNavigateToCombat;
        viewModel.NavigateToPartyRequested += OnNavigateToParty;
        viewModel.NavigateToSkillsRequested += OnNavigateToSkills;
        viewModel.NavigateToGearRequested += OnNavigateToGear;
        viewModel.NavigateToStoreRequested += OnNavigateToStore;
        viewModel.NavigateToEventsRequested += OnNavigateToEvents;

        _logger.LogInformation("HubPage initialized");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HubViewModel vm)
        {
            vm.RefreshData();
        }
    }

    private async void OnNavigateToCombat()
    {
        try
        {
            _logger.LogInformation("Navigating to Combat page");
            var mainViewModel = new MainViewModel(
                Application.Current!.Handler!.MauiContext!.Services.GetService(typeof(DungeonPartyGame.Core.Services.CombatEngine)) as DungeonPartyGame.Core.Services.CombatEngine,
                Application.Current!.Handler!.MauiContext!.Services.GetService(typeof(DungeonPartyGame.Core.Services.DiceService)) as DungeonPartyGame.Core.Services.DiceService,
                _gameSession,
                Application.Current!.Handler!.MauiContext!.Services.GetService(typeof(DungeonPartyGame.Core.Services.ProgressionService)) as DungeonPartyGame.Core.Services.ProgressionService);

            var mainPage = new MainPage(mainViewModel, _loggerFactory);
            await Navigation.PushAsync(mainPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to Combat page");
            await DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
        }
    }

    private async void OnNavigateToParty()
    {
        try
        {
            _logger.LogInformation("Navigating to Party page");
            var partyViewModel = new PartyViewModel(_gameSession, _loggerFactory);
            var partyPage = new PartyPage(partyViewModel, _loggerFactory);
            await Navigation.PushAsync(partyPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to Party page");
            await DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
        }
    }

    private async void OnNavigateToSkills()
    {
        try
        {
            _logger.LogInformation("Navigating to Skills page");

            // Get the first character from the party
            var character = _gameSession.Party.FirstOrDefault();
            if (character == null)
            {
                await DisplayAlert("Error", "No characters in party!", "OK");
                return;
            }

            var viewModel = new SkillTreeViewModel(character, _loggerFactory);
            var skillTreePage = new SkillTreePage(viewModel, _loggerFactory);
            await Navigation.PushAsync(skillTreePage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to Skills page");
            await DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
        }
    }

    private async void OnNavigateToGear()
    {
        try
        {
            _logger.LogInformation("Navigating to Gear page");

            // Get the first character from the party
            var character = _gameSession.Party.FirstOrDefault();
            if (character == null)
            {
                await DisplayAlert("Error", "No characters in party!", "OK");
                return;
            }

            var viewModel = new GearViewModel(character, _loggerFactory);
            var gearPage = new GearPage(viewModel, _loggerFactory);
            await Navigation.PushAsync(gearPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to Gear page");
            await DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
        }
    }

    private async void OnNavigateToStore()
    {
        try
        {
            _logger.LogInformation("Navigating to Store page");
            // Will create StorePage next
            await DisplayAlert("Store", "Store page coming soon!", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to Store page");
            await DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
        }
    }

    private async void OnNavigateToEvents()
    {
        try
        {
            _logger.LogInformation("Navigating to Events page");
            // Will create EventsPage next
            await DisplayAlert("Events", "Events page coming soon!", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to Events page");
            await DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
        }
    }
}
