using DungeonPartyGame.MonoGame.Screens;
using Microsoft.Xna.Framework;
using Xunit;

namespace DungeonPartyGame.Tests
{
    public class ScreenManagerTests
    {
        private class DummyScreen : Screen
        {
            public bool Loaded { get; private set; }
            public bool Initialized { get; private set; }

            public override void Initialize(DungeonPartyGame.MonoGame.DungeonPartyGameMain game)
            {
                // Do not call base.Initialize to avoid requiring GraphicsDevice in tests
                Initialized = true;
            }

            public override void LoadContent()
            {
                Loaded = true;
            }
        }

        private class DummyGame : DungeonPartyGame.MonoGame.DungeonPartyGameMain
        {
            // Keep default implementation; we won't call Run or graphics methods
        }

        [Fact]
        public void PushScreen_SetsCurrentScreen()
        {
            var game = new DummyGame();
            var manager = new ScreenManager(game);
            var screen = new DummyScreen();

            manager.PushScreen(screen);

            Assert.Same(screen, manager.CurrentScreen);
            Assert.True(screen.Initialized);
            Assert.True(screen.Loaded);
        }

        [Fact]
        public void PopScreen_ReactivatesPreviousScreen()
        {
            var game = new DummyGame();
            var manager = new ScreenManager(game);
            var screen1 = new DummyScreen();
            var screen2 = new DummyScreen();

            manager.PushScreen(screen1);
            manager.PushScreen(screen2);

            manager.PopScreen();

            Assert.Same(screen1, manager.CurrentScreen);
            Assert.True(manager.CurrentScreen.IsActive);
        }

        [Fact]
        public void ChangeScreen_ReplacesAllScreens()
        {
            var game = new DummyGame();
            var manager = new ScreenManager(game);
            var screen1 = new DummyScreen();
            var screen2 = new DummyScreen();

            manager.PushScreen(screen1);
            manager.ChangeScreen(screen2);

            Assert.Same(screen2, manager.CurrentScreen);
        }
    }
}
