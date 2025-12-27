using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using DungeonPartyGame.MonoGame;
using DungeonPartyGame.MonoGame.Screens;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Tests
{
    public class HubScreenTests
    {
        private class TestGame : DungeonPartyGameMain
        {
            public Screen? LastPushed { get; private set; }

            public override void PushScreen(Screen screen)
            {
                LastPushed = screen;
                // Do not call base.PushScreen to avoid GraphicsDevice/Content requirements
            }
        }

        [Fact]
        public void OnCombatClicked_PushesCombatScreen()
        {
            // Arrange: setup service provider with required services
            var services = new ServiceCollection();
            services.AddSingleton<DiceService>();
            services.AddSingleton<GearService>();
            services.AddSingleton<ISkillSelector, DefaultSkillSelector>();
            services.AddSingleton<CombatEngine>();
            services.AddSingleton<ProgressionService>();
            services.AddSingleton<CurrencyService>();
            services.AddSingleton<DailyRewardService>();
            services.AddSingleton<GameSession>();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var provider = services.BuildServiceProvider();

            // Create game and inject service provider via reflection
            var game = new TestGame();
            var spField = typeof(DungeonPartyGameMain).GetField("_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(spField);
            spField.SetValue(game, provider);

            // Create HubScreen and initialize with test game
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var session = provider.GetRequiredService<GameSession>();
            var currency = provider.GetRequiredService<CurrencyService>();
            var daily = provider.GetRequiredService<DailyRewardService>();

            var hub = new HubScreen(loggerFactory, session, currency, daily);
            // Initialize sets the Game property used by OnCombatClicked
            hub.Initialize(game);

            // Act: invoke private OnCombatClicked via reflection
            var method = typeof(HubScreen).GetMethod("OnCombatClicked", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(hub, null);

            // Assert: our test game recorded a CombatScreen pushed
            Assert.NotNull(game.LastPushed);
            Assert.IsType<CombatScreen>(game.LastPushed);
        }

        [Fact]
        public void OnCombatClicked_PushesCombatScreen_WithRuntimeDI()
        {
            // Arrange: use the game's ConfigureServices to build runtime service collection
            var game = new TestGame();
            var services = new ServiceCollection();
            var configure = typeof(DungeonPartyGameMain).GetMethod("ConfigureServices", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(configure);
            configure.Invoke(game, new object[] { services });
            var provider = services.BuildServiceProvider();

            // Inject runtime service provider into game instance
            var spField = typeof(DungeonPartyGameMain).GetField("_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(spField);
            spField.SetValue(game, provider);

            // Create HubScreen and initialize with test game
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var session = provider.GetRequiredService<GameSession>();
            var currency = provider.GetRequiredService<CurrencyService>();
            var daily = provider.GetRequiredService<DailyRewardService>();

            var hub = new HubScreen(loggerFactory, session, currency, daily);
            hub.Initialize(game);

            // Act: invoke private OnCombatClicked
            var method2 = typeof(HubScreen).GetMethod("OnCombatClicked", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method2);
            method2.Invoke(hub, null);

            // Assert: runtime DI setup allows pushing CombatScreen
            Assert.NotNull(game.LastPushed);
            Assert.IsType<CombatScreen>(game.LastPushed);
        }
    }
}
