using DungeonPartyGame.UI.ViewModels;

namespace DungeonPartyGame.UI.Pages;

public partial class GearPage : ContentPage
{
    public GearPage(GearViewModel viewModel)
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