namespace DungeonPartyGame.UI.Pages;

using DungeonPartyGame.UI.ViewModels;
using Microsoft.Extensions.Logging;

public partial class MainPage : ContentPage
{
    private readonly ILogger<MainPage> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public MainPage(MainViewModel vm, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MainPage>();
        _logger.LogInformation("MainPage constructor called");

        try
        {
            InitializeComponent();
            BindingContext = vm;

            if (vm is MainViewModel mainViewModel)
            {
                mainViewModel.NavigateToPartyRequested += OnNavigateToPartyRequested;
                _logger.LogInformation("NavigateToPartyRequested event handler attached");
            }
            else
            {
                _logger.LogWarning("ViewModel is not MainViewModel type");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing MainPage");
            throw;
        }
    }

    private async void OnNavigateToPartyRequested()
    {
        _logger.LogInformation("NavigateToPartyRequested event triggered");

        try
        {
            var mainViewModel = (MainViewModel)BindingContext;
            if (mainViewModel?.GameSession == null)
            {
                _logger.LogError("GameSession is null in MainViewModel");
                await DisplayAlert("Error", "Game session is not initialized. Please restart the application.", "OK");
                return;
            }

            _logger.LogInformation("Creating PartyViewModel with GameSession");
            var partyViewModel = new PartyViewModel(mainViewModel.GameSession, _loggerFactory);

            _logger.LogInformation("Creating PartyPage");
            var partyPage = new PartyPage(partyViewModel, _loggerFactory);

            _logger.LogInformation("Navigating to PartyPage");
            await Navigation.PushAsync(partyPage);

            _logger.LogInformation("Successfully navigated to PartyPage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to party page");
            await DisplayAlert("Error", $"Failed to open party management: {ex.Message}", "OK");
        }
    }
}
