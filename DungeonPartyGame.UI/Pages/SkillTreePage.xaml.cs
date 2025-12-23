using DungeonPartyGame.UI.ViewModels;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.UI.Pages;

public partial class SkillTreePage : ContentPage
{
    private readonly ILogger<SkillTreePage> _logger;

    public SkillTreePage(SkillTreeViewModel viewModel, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SkillTreePage>();
        _logger.LogInformation("SkillTreePage constructor called");

        try
        {
            InitializeComponent();
            BindingContext = viewModel;

            viewModel.NavigateBackRequested += OnNavigateBackRequested;

            _logger.LogInformation("SkillTreePage initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing SkillTreePage");
            throw;
        }
    }

    private async void OnNavigateBackRequested()
    {
        _logger.LogInformation("NavigateBackRequested event triggered");

        try
        {
            _logger.LogInformation("Navigating back from SkillTreePage");
            await Navigation.PopAsync();
            _logger.LogInformation("Successfully navigated back from SkillTreePage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back from SkillTreePage");
            await DisplayAlert("Error", $"Failed to go back: {ex.Message}", "OK");
        }
    }
}