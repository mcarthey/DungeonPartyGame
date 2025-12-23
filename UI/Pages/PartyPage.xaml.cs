using DungeonPartyGame.UI.ViewModels;

namespace DungeonPartyGame.UI.Pages;

public partial class PartyPage : ContentPage
{
    public PartyPage(PartyViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        vm.NavigateToSkillsRequested += OnNavigateToSkillsRequested;
        vm.NavigateToGearRequested += OnNavigateToGearRequested;
        vm.NavigateBackRequested += OnNavigateBackRequested;
    }

    private async void OnNavigateToSkillsRequested()
    {
        if (BindingContext is PartyViewModel vm && vm.SelectedCharacter != null)
        {
            var skillTreeViewModel = new SkillTreeViewModel(vm.SelectedCharacter);
            await Navigation.PushAsync(new SkillTreePage(skillTreeViewModel));
        }
    }

    private async void OnNavigateToGearRequested()
    {
        if (BindingContext is PartyViewModel vm && vm.SelectedCharacter != null)
        {
            var gearViewModel = new GearViewModel(vm.SelectedCharacter);
            await Navigation.PushAsync(new GearPage(gearViewModel));
        }
    }

    private async void OnNavigateBackRequested()
    {
        await Navigation.PopAsync();
    }
}