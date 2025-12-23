namespace DungeonPartyGame.UI.Pages;

using DungeonPartyGame.UI.ViewModels;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
