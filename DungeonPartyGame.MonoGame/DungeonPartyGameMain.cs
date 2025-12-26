using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DungeonPartyGame.MonoGame.Screens;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonPartyGame.MonoGame;

public class DungeonPartyGameMain : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private ScreenManager _screenManager = null!;
    private IServiceProvider _serviceProvider = null!;

    // Make these accessible to screens
    public new GraphicsDevice GraphicsDevice => base.GraphicsDevice;
    public SpriteBatch SpriteBatch => _spriteBatch;
    public IServiceProvider Services => _serviceProvider;

    public DungeonPartyGameMain()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set window size
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
    }

    protected override void Initialize()
    {
        // Set up dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Initialize screen manager
        _screenManager = new ScreenManager(this);

        base.Initialize();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Core services
        services.AddSingleton<DiceService>();
        services.AddSingleton<CombatEngine>();
        services.AddSingleton<ProgressionService>();
        services.AddSingleton<CurrencyService>();
        services.AddSingleton<StoreService>();
        services.AddSingleton<EventService>();
        services.AddSingleton<DailyRewardService>();

        // Game session
        services.AddSingleton(sp => CreateDefaultGameSession());
    }

    private GameSession CreateDefaultGameSession()
    {
        var session = new GameSession();

        // Create default party with test characters
        var party = new Party { Name = "Hero Party" };

        var fighter = new Character
        {
            Name = "Ragnar",
            Class = "Fighter",
            Level = 30
        };
        fighter.Stats.Strength = 18;
        fighter.Stats.Dexterity = 12;
        fighter.Stats.Constitution = 16;
        fighter.Stats.MaxHealth = 50;
        fighter.Stats.CurrentHealth = 50;

        var rogue = new Character
        {
            Name = "Shadow",
            Class = "Rogue",
            Level = 28
        };
        rogue.Stats.Strength = 10;
        rogue.Stats.Dexterity = 18;
        rogue.Stats.Constitution = 12;
        rogue.Stats.MaxHealth = 40;
        rogue.Stats.CurrentHealth = 40;

        party.AddMember(fighter);
        party.AddMember(rogue);
        session.AddParty(party);

        return session;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Start with the Hub screen
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>();
        var gameSession = _serviceProvider.GetRequiredService<GameSession>();
        var currencyService = _serviceProvider.GetRequiredService<CurrencyService>();
        var dailyRewardService = _serviceProvider.GetRequiredService<DailyRewardService>();

        var hubScreen = new HubScreen(logger, gameSession, currencyService, dailyRewardService);
        _screenManager.PushScreen(hubScreen);
    }

    protected override void Update(GameTime gameTime)
    {
        // Handle global input (like exit)
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        // Update current screen
        var mouseState = Mouse.GetState();
        _screenManager.HandleInput(keyboardState, mouseState);
        _screenManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _screenManager.Draw(gameTime);

        base.Draw(gameTime);
    }

    public void SwitchToScreen(Screen screen)
    {
        _screenManager.ChangeScreen(screen);
    }

    public void PushScreen(Screen screen)
    {
        _screenManager.PushScreen(screen);
    }

    public void PopScreen()
    {
        _screenManager.PopScreen();
    }
}
