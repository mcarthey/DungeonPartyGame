using DungeonPartyGame.UI.Pages;
using DungeonPartyGame.UI.ViewModels;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Core Services
        builder.Services.AddSingleton<DiceService>();
        builder.Services.AddSingleton<CombatEngine>();
        builder.Services.AddSingleton<SkillTreeService>();
        builder.Services.AddSingleton<ProgressionService>();
        builder.Services.AddSingleton<GearService>();
        builder.Services.AddSingleton<GearUpgradeService>(sp => new GearUpgradeService(sp.GetRequiredService<GearService>()));
        builder.Services.AddSingleton<ISkillSelector, DefaultSkillSelector>();

        // Hub & Monetization Services
        builder.Services.AddSingleton<CurrencyService>();
        builder.Services.AddSingleton<StoreService>();
        builder.Services.AddSingleton<EventService>();
        builder.Services.AddSingleton<DailyRewardService>();

        // Logging
        builder.Services.AddLogging();

        // Game session (shared state) - initialize with a default party
        builder.Services.AddSingleton<GameSession>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<GameSession>>();
            var gameSession = new GameSession();
            // Initialize with a default party
            var defaultParty = new Party();
            gameSession.AddParty(defaultParty);
            logger.LogInformation("GameSession initialized with default party");
            return gameSession;
        });

        // ViewModels - created manually in pages
        // builder.Services.AddTransient<PartyViewModel>();
        // builder.Services.AddTransient<SkillTreeViewModel>();
        // builder.Services.AddTransient<GearViewModel>();

        // ViewModels
        builder.Services.AddTransient<HubViewModel>();
        builder.Services.AddTransient<StoreViewModel>();
        builder.Services.AddTransient<MainViewModel>();

        // Pages
        builder.Services.AddTransient<HubPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<PartyPage>();
        builder.Services.AddTransient<SkillTreePage>();
        builder.Services.AddTransient<GearPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        return builder.Build();
    }
}
