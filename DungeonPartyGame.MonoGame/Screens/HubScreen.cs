using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaColor = Microsoft.Xna.Framework.Color;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.MonoGame.Screens;

/// <summary>
/// Main hub screen - shows currency, navigation buttons, and daily rewards
/// </summary>
public class HubScreen : Screen
{
    private readonly ILogger<HubScreen> _logger;
    private readonly GameSession _gameSession;
    private readonly CurrencyService _currencyService;
    private readonly DailyRewardService _dailyRewardService;

    private SpriteFont? _titleFont;
    private SpriteFont? _regularFont;
    private Texture2D? _whitePixel;

    private readonly List<Button> _buttons = new();
    private MouseState _previousMouseState;

    public HubScreen(
        ILoggerFactory loggerFactory,
        GameSession gameSession,
        CurrencyService currencyService,
        DailyRewardService dailyRewardService)
    {
        _logger = loggerFactory.CreateLogger<HubScreen>();
        _gameSession = gameSession;
        _currencyService = currencyService;
        _dailyRewardService = dailyRewardService;
    }

    public override void LoadContent()
    {
        base.LoadContent();

        // Create a 1x1 white pixel texture for drawing rectangles
        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { XnaColor.White });

        // Load fonts - using built-in MonoGame font for now
        // In a real game, you'd add custom fonts to Content folder
        try
        {
            // Try to load fonts if available (RegularFont used for both title and body)
            _titleFont = Game.Content.Load<SpriteFont>("Fonts/RegularFont");
            _regularFont = Game.Content.Load<SpriteFont>("Fonts/RegularFont");
        }
        catch
        {
            _logger.LogWarning("Custom fonts not found, using fallback rendering");
        }

        // Set up navigation buttons
        int centerX = GraphicsDevice.Viewport.Width / 2;
        int startY = 250;
        int buttonWidth = 300;
        int buttonHeight = 60;
        int spacing = 20;

        _buttons.Add(new Button(
            new Rectangle(centerX - buttonWidth / 2, startY, buttonWidth, buttonHeight),
            "⚔️ COMBAT",
            XnaColor.DarkRed,
            OnCombatClicked));

        _buttons.Add(new Button(
            new Rectangle(centerX - buttonWidth / 2, startY + (buttonHeight + spacing), buttonWidth, buttonHeight),
            "👥 PARTY",
            XnaColor.DarkBlue,
            OnPartyClicked));

        _buttons.Add(new Button(
            new Rectangle(centerX - buttonWidth / 2, startY + 2 * (buttonHeight + spacing), buttonWidth, buttonHeight),
            "🎯 SKILLS",
            XnaColor.DarkGreen,
            OnSkillsClicked));

        _buttons.Add(new Button(
            new Rectangle(centerX - buttonWidth / 2, startY + 3 * (buttonHeight + spacing), buttonWidth, buttonHeight),
            "⚔️ GEAR",
            XnaColor.DarkOrange,
            OnGearClicked));

        _buttons.Add(new Button(
            new Rectangle(centerX - buttonWidth / 2, startY + 4 * (buttonHeight + spacing), buttonWidth, buttonHeight),
            "🏪 STORE",
            XnaColor.Purple,
            OnStoreClicked));

        _logger.LogInformation("HubScreen loaded successfully");
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void HandleInput(KeyboardState keyboardState, MouseState mouseState)
    {
        base.HandleInput(keyboardState, mouseState);

        // Check button clicks
        if (mouseState.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            var mousePoint = new Point(mouseState.X, mouseState.Y);
            foreach (var button in _buttons)
            {
                if (button.Bounds.Contains(mousePoint))
                {
                    button.OnClick?.Invoke();
                }
            }
        }

        _previousMouseState = mouseState;
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        SpriteBatch.Begin();

        // Draw background
        DrawRectangle(new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            new XnaColor(30, 30, 46));

        // Draw header
        DrawRectangle(new Rectangle(0, 0, GraphicsDevice.Viewport.Width, 150),
            new XnaColor(44, 44, 62));

        // Draw title
        DrawText("🏰 DUNGEON PARTY HUB 🏰", new Vector2(GraphicsDevice.Viewport.Width / 2, 40),
            XnaColor.Gold, 2.0f, true);

        // Draw currency display
        int currencyY = 100;
        string goldText = $"💰 Gold: {_currencyService.GetFormattedBalance(CurrencyType.Gold)}";
        string gemsText = $"💎 Gems: {_currencyService.GetFormattedBalance(CurrencyType.Gems)}";

        DrawText(goldText, new Vector2(GraphicsDevice.Viewport.Width / 2 - 150, currencyY),
            XnaColor.Gold, 1.0f);
        DrawText(gemsText, new Vector2(GraphicsDevice.Viewport.Width / 2 + 150, currencyY),
            XnaColor.Cyan, 1.0f);

        // Draw buttons
        var mouseState = Mouse.GetState();
        var mousePoint = new Point(mouseState.X, mouseState.Y);

        foreach (var button in _buttons)
        {
            bool isHovered = button.Bounds.Contains(mousePoint);
            XnaColor buttonColor = isHovered ? XnaColor.Lerp(button.Color, XnaColor.White, 0.3f) : button.Color;

            // Draw button background
            DrawRectangle(button.Bounds, buttonColor);

            // Draw button border
            DrawRectangleBorder(button.Bounds, XnaColor.White, 2);

            // Draw button text
            DrawText(button.Text,
                new Vector2(button.Bounds.Center.X, button.Bounds.Center.Y),
                XnaColor.White, 1.2f, true);
        }

        // Draw footer instructions
        DrawText("ESC = Exit | Click buttons to navigate",
            new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height - 30),
            XnaColor.Gray, 0.8f, true);

        SpriteBatch.End();
    }

    private void DrawRectangle(Rectangle rect, XnaColor color)
    {
        if (_whitePixel != null)
        {
            SpriteBatch.Draw(_whitePixel, rect, color);
        }
    }

    private void DrawRectangleBorder(Rectangle rect, XnaColor color, int thickness)
    {
        if (_whitePixel == null) return;

        // Top
        SpriteBatch.Draw(_whitePixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        SpriteBatch.Draw(_whitePixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        // Left
        SpriteBatch.Draw(_whitePixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        SpriteBatch.Draw(_whitePixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    private void DrawText(string text, Vector2 position, XnaColor color, float scale = 1.0f, bool centered = false)
    {
        // Prefer a title font for large text, otherwise use the regular font if available
        SpriteFont? fontToUse = null;
        if (scale >= 2.0f && _titleFont != null)
        {
            fontToUse = _titleFont;
        }
        else if (_regularFont != null)
        {
            fontToUse = _regularFont;
        }
        else if (_titleFont != null)
        {
            fontToUse = _titleFont;
        }

        if (fontToUse != null)
        {
            Vector2 origin = centered ? fontToUse.MeasureString(text) / 2 : Vector2.Zero;
            SpriteBatch.DrawString(fontToUse, text, position, color, 0f, origin, scale, SpriteEffects.None, 0);
        }
        else
        {
            // Fallback: Draw colored rectangle as placeholder
            int width = (int)(text.Length * 10 * scale);
            int height = (int)(20 * scale);
            var rect = new Rectangle(
                (int)(centered ? position.X - width / 2 : position.X),
                (int)(centered ? position.Y - height / 2 : position.Y),
                width, height);
            DrawRectangle(rect, color * 0.5f);
        }
    }

    private void OnCombatClicked()
    {
        _logger.LogInformation("Combat button clicked");

        // Get required services from DI
        var combatEngine = Game.Services.GetService(typeof(CombatEngine)) as CombatEngine;
        var diceService = Game.Services.GetService(typeof(DiceService)) as DiceService;
        var progressionService = Game.Services.GetService(typeof(ProgressionService)) as ProgressionService;
        var loggerFactory = Game.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;

        if (combatEngine != null && diceService != null && progressionService != null && loggerFactory != null)
        {
            var combatScreen = new CombatScreen(
                loggerFactory,
                _gameSession,
                combatEngine,
                diceService,
                progressionService);

            Game.PushScreen(combatScreen);
        }
    }

    private void OnPartyClicked()
    {
        _logger.LogInformation("Party button clicked - not yet implemented");
    }

    private void OnSkillsClicked()
    {
        _logger.LogInformation("Skills button clicked - not yet implemented");
    }

    private void OnGearClicked()
    {
        _logger.LogInformation("Gear button clicked - not yet implemented");
    }

    private void OnStoreClicked()
    {
        _logger.LogInformation("Store button clicked - not yet implemented");
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _whitePixel?.Dispose();
    }

    private class Button
    {
        public Rectangle Bounds { get; }
        public string Text { get; }
        public XnaColor Color { get; }
        public Action? OnClick { get; }

        public Button(Rectangle bounds, string text, XnaColor color, Action? onClick = null)
        {
            Bounds = bounds;
            Text = text;
            Color = color;
            OnClick = onClick;
        }
    }
}
