using DungeonPartyGame.UI.ViewModels;

using DungeonPartyGame.UI.ViewModels;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.UI.Pages;

public partial class GearPage : ContentPage
{
    private readonly ILogger<GearPage> _logger;

    public GearPage(GearViewModel viewModel, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GearPage>();
        _logger.LogInformation("GearPage constructor called");

        try
        {
            InitializeComponent();
            BindingContext = viewModel;

            viewModel.NavigateBackRequested += OnNavigateBackRequested;

            _logger.LogInformation("GearPage initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing GearPage");
            throw;
        }
    }

    private async void OnNavigateBackRequested()
    {
        _logger.LogInformation("NavigateBackRequested event triggered");

        try
        {
            _logger.LogInformation("Navigating back from GearPage");
            await Navigation.PopAsync();
            _logger.LogInformation("Successfully navigated back from GearPage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back from GearPage");
            await DisplayAlert("Error", $"Failed to go back: {ex.Message}", "OK");
        }
    }
}