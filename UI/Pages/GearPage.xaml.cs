using DungeonPartyGame.UI.ViewModels;

namespace DungeonPartyGame.UI.Pages;

public partial class GearPage : ContentPage
{
    public GearPage(GearViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}