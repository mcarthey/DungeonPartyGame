using DungeonPartyGame.UI.ViewModels;

namespace DungeonPartyGame.UI.Pages;

public partial class PartyPage : ContentPage
{
    public PartyPage(PartyViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}