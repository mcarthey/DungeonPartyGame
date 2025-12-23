using DungeonPartyGame.UI.ViewModels;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.UI.Pages;

public partial class PartyPage : ContentPage
{
    private readonly ILogger<PartyPage> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public PartyPage(PartyViewModel vm, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<PartyPage>();
        _logger.LogInformation("PartyPage constructor called");

        try
        {
            InitializeComponent();
            BindingContext = vm;

            vm.NavigateToSkillsRequested += OnNavigateToSkillsRequested;
            vm.NavigateToGearRequested += OnNavigateToGearRequested;
            vm.NavigateBackRequested += OnNavigateBackRequested;

            _logger.LogInformation("PartyPage initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing PartyPage");
            throw;
        }
    }

    private async void OnNavigateToSkillsRequested()
    {
        _logger.LogInformation("NavigateToSkillsRequested event triggered");

        try
        {
            if (BindingContext is PartyViewModel vm)
            {
                if (vm.SelectedCharacter == null)
                {
                    _logger.LogWarning("No character selected for skills navigation");
                    await DisplayAlert("Selection Required", "Please select a character first.", "OK");
                    return;
                }

                _logger.LogInformation("Creating SkillTreeViewModel for character {Name}", vm.SelectedCharacter.Name);
                var skillTreeViewModel = new SkillTreeViewModel(vm.SelectedCharacter, _loggerFactory);

                _logger.LogInformation("Creating SkillTreePage");
                var skillTreePage = new SkillTreePage(skillTreeViewModel, _loggerFactory);

                _logger.LogInformation("Navigating to SkillTreePage");
                await Navigation.PushAsync(skillTreePage);

                _logger.LogInformation("Successfully navigated to SkillTreePage");
            }
            else
            {
                _logger.LogError("BindingContext is not PartyViewModel");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to skills page");
            await DisplayAlert("Error", $"Failed to open skills: {ex.Message}", "OK");
        }
    }

    private async void OnNavigateToGearRequested()
    {
        _logger.LogInformation("NavigateToGearRequested event triggered");

        try
        {
            if (BindingContext is PartyViewModel vm)
            {
                if (vm.SelectedCharacter == null)
                {
                    _logger.LogWarning("No character selected for gear navigation");
                    await DisplayAlert("Selection Required", "Please select a character first.", "OK");
                    return;
                }

                _logger.LogInformation("Creating GearViewModel for character {Name}", vm.SelectedCharacter.Name);
                var gearViewModel = new GearViewModel(vm.SelectedCharacter, _loggerFactory);

                _logger.LogInformation("Creating GearPage");
                var gearPage = new GearPage(gearViewModel, _loggerFactory);

                _logger.LogInformation("Navigating to GearPage");
                await Navigation.PushAsync(gearPage);

                _logger.LogInformation("Successfully navigated to GearPage");
            }
            else
            {
                _logger.LogError("BindingContext is not PartyViewModel");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to gear page");
            await DisplayAlert("Error", $"Failed to open gear: {ex.Message}", "OK");
        }
    }

    private async void OnNavigateBackRequested()
    {
        _logger.LogInformation("NavigateBackRequested event triggered");

        try
        {
            _logger.LogInformation("Navigating back");
            await Navigation.PopAsync();
            _logger.LogInformation("Successfully navigated back");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back");
            await DisplayAlert("Error", $"Failed to go back: {ex.Message}", "OK");
        }
    }
}