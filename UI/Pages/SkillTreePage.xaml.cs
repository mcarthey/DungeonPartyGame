using DungeonPartyGame.UI.ViewModels;

namespace DungeonPartyGame.UI.Pages;

public partial class SkillTreePage : ContentPage
{
    public SkillTreePage(SkillTreeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        viewModel.NavigateBackRequested += OnNavigateBackRequested;
    }

    private async void OnNavigateBackRequested()
    {
        await Navigation.PopAsync();
    }
}