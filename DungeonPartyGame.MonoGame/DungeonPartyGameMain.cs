using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaColor = Microsoft.Xna.Framework.Color;
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
    public new IServiceProvider Services => _serviceProvider;

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
        services.AddSingleton<GearService>();
        services.AddSingleton<ISkillSelector, DefaultSkillSelector>();
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
        var party = new Party();

        var fighter = new Character("Ragnar", CharacterRole.Fighter, new Stats(18, 12, 16, 50));
        fighter.Stats.Crit = 0;
        fighter.Stats.Dodge = 0;

        var rogue = new Character("Shadow", CharacterRole.Rogue, new Stats(10, 18, 12, 40));
        rogue.Stats.Crit = 0;
        rogue.Stats.Dodge = 0;

        party.Add(fighter);
        party.Add(rogue);
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
        GraphicsDevice.Clear(XnaColor.CornflowerBlue);

        _screenManager.Draw(gameTime);

        base.Draw(gameTime);
    }

    public void SwitchToScreen(Screen screen)
    {
        _screenManager.ChangeScreen(screen);
    }

    public virtual void PushScreen(Screen screen)
    {
        _screenManager.PushScreen(screen);
    }

    public void PopScreen()
    {
        _screenManager.PopScreen();
    }
}
