namespace DungeonPartyGame.UI.Pages;

using DungeonPartyGame.UI.ViewModels;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        if (vm is MainViewModel mainViewModel)
        {
            mainViewModel.NavigateToPartyRequested += OnNavigateToPartyRequested;
        }
    }

    private async void OnNavigateToPartyRequested()
    {
        var mainViewModel = (MainViewModel)BindingContext;
        var partyViewModel = new PartyViewModel(mainViewModel.GameSession);
        await Navigation.PushAsync(new PartyPage(partyViewModel));
    }
}
