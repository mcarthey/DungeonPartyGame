using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaColor = Microsoft.Xna.Framework.Color;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.MonoGame.Screens;

/// <summary>
/// Combat screen - displays turn-based combat with sprite-based characters
/// Integrates with existing CombatEngine from Core
/// </summary>
public class CombatScreen : Screen
{
    private readonly ILogger<CombatScreen> _logger;
    private readonly GameSession _gameSession;
    private readonly CombatEngine _combatEngine;
    private readonly DiceService _diceService;
    private readonly ProgressionService _progressionService;

    private Texture2D? _whitePixel;
    private SpriteFont? _regularFont;

    private readonly List<CharacterSprite> _heroSprites = new();
    private readonly List<CharacterSprite> _enemySprites = new();
    private readonly List<DamageNumber> _damageNumbers = new();

    private Character? _fighter;
    private Character? _rogue;
    private Character? _goblin;

    private string _combatLog = "";
    private bool _combatActive = false;
    private MouseState _previousMouseState;

    public CombatScreen(
        ILoggerFactory loggerFactory,
        GameSession gameSession,
        CombatEngine combatEngine,
        DiceService diceService,
        ProgressionService progressionService)
    {
        _logger = loggerFactory.CreateLogger<CombatScreen>();
        _gameSession = gameSession;
        _combatEngine = combatEngine;
        _diceService = diceService;
        _progressionService = progressionService;
    }

    public override void LoadContent()
    {
        base.LoadContent();

        // Create 1x1 white pixel for drawing
        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { XnaColor.White });

        // Initialize combat
        InitializeCombat();

        _logger.LogInformation("CombatScreen loaded successfully");
    }

    private void InitializeCombat()
    {
        // Get current party
        var party = _gameSession.CurrentParty;
        if (party == null)
        {
            _logger.LogWarning("No current party found in session");
            return;
        }

        // Get party members (use Role)
        _fighter = party.Members.FirstOrDefault(c => c.Role == DungeonPartyGame.Core.Models.CharacterRole.Fighter);
        _rogue = party.Members.FirstOrDefault(c => c.Role == DungeonPartyGame.Core.Models.CharacterRole.Rogue);

        if (_fighter == null || _rogue == null)
        {
            _logger.LogWarning("Party members not found in session");
            return;
        }

        // Create test enemy (use Role/Fighter for simple enemy)
        _goblin = new Character("Goblin", DungeonPartyGame.Core.Models.CharacterRole.Fighter, new Stats(8, 8, 8, 30));

        // Create enemy party and start session
        var enemyParty = new Party();
        enemyParty.Add(_goblin);

        var session = _combatEngine.CreateSession(party, enemyParty);
        _combatActive = true;
        _currentSession = session;

        // Create character sprites
        _heroSprites.Add(new CharacterSprite(
            _fighter,
            new Vector2(200, 300),
            CharacterSpriteType.Fighter));

        _heroSprites.Add(new CharacterSprite(
            _rogue,
            new Vector2(350, 350),
            CharacterSpriteType.Rogue));

        _enemySprites.Add(new CharacterSprite(
            _goblin,
            new Vector2(900, 300),
            CharacterSpriteType.Goblin));

        _combatLog = "Combat started! Click 'Next Turn' to proceed.";
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Update damage numbers
        for (int i = _damageNumbers.Count - 1; i >= 0; i--)
        {
            _damageNumbers[i].Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if (_damageNumbers[i].Lifetime > 2.0f)
            {
                _damageNumbers.RemoveAt(i);
            }
        }

        // Update character sprites
        foreach (var sprite in _heroSprites.Concat(_enemySprites))
        {
            sprite.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }

    public override void HandleInput(KeyboardState keyboardState, MouseState mouseState)
    {
        base.HandleInput(keyboardState, mouseState);

        // Check for button clicks
        if (mouseState.LeftButton == ButtonState.Pressed &&
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            var mousePoint = new Point(mouseState.X, mouseState.Y);

            // Next Turn button
            var nextTurnButton = new Rectangle(
                GraphicsDevice.Viewport.Width / 2 - 100,
                GraphicsDevice.Viewport.Height - 100,
                200, 50);

            if (nextTurnButton.Contains(mousePoint) && _combatActive)
            {
                ExecuteNextTurn();
            }

            // Back button
            var backButton = new Rectangle(50, GraphicsDevice.Viewport.Height - 100, 150, 50);
            if (backButton.Contains(mousePoint))
            {
                Game.PopScreen();
            }
        }

        _previousMouseState = mouseState;
    }

    private CombatSession? _currentSession;

    private void ExecuteNextTurn()
    {
        _logger.LogInformation("Executing next combat turn");

        if (_currentSession == null)
        {
            _combatLog = "No active combat session.";
            return;
        }

        var result = _combatEngine.ExecuteRound(_currentSession);

        // Use summary text for the log
        _combatLog = result.SummaryText ?? string.Empty;

        // Create damage number visuals from targets
        foreach (var targetResult in result.Targets)
        {
            var defenderSprite = _heroSprites.Concat(_enemySprites)
                .FirstOrDefault(s => s.Character == targetResult.Target);

            if (defenderSprite != null)
            {
                _damageNumbers.Add(new DamageNumber(
                    targetResult.Damage.ToString(),
                    defenderSprite.Position,
                    targetResult.IsHealing ? XnaColor.LightGreen : XnaColor.Red));

                // Trigger hit animation
                defenderSprite.TriggerHit();
            }
        }

        // Check for combat end
        if (result.IsFinalTurn)
        {
            _combatActive = false;
            if (_currentSession.IsComplete && _currentSession.WinningParty != null)
            {
                if (_currentSession.WinningParty == _currentSession.PartyA)
                    _combatLog += "\n🎉 VICTORY! 🎉";
                else
                    _combatLog += "\n💀 DEFEAT 💀";
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        SpriteBatch.Begin();

        // Draw background
        DrawRectangle(new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            new XnaColor(40, 35, 30));

        // Draw combat arena
        DrawRectangle(new Rectangle(50, 150, GraphicsDevice.Viewport.Width - 100, 400),
            new XnaColor(60, 55, 50));

        // Draw header
        DrawText("⚔️ COMBAT ⚔️",
            new Vector2(GraphicsDevice.Viewport.Width / 2, 50),
            XnaColor.Gold, 2.0f, true);

        // Draw character sprites
        foreach (var sprite in _heroSprites)
        {
            sprite.Draw(SpriteBatch, _whitePixel!);
        }

        foreach (var sprite in _enemySprites)
        {
            sprite.Draw(SpriteBatch, _whitePixel!);
        }

        // Draw damage numbers
        foreach (var dmg in _damageNumbers)
        {
            DrawText(dmg.Text, dmg.Position, dmg.Color, 1.5f, true);
        }

        // Draw combat log
        DrawCombatLog();

        // Draw buttons
        DrawButtons();

        SpriteBatch.End();
    }

    private void DrawCombatLog()
    {
        var logRect = new Rectangle(50, 580, GraphicsDevice.Viewport.Width - 100, 100);
        DrawRectangle(logRect, new XnaColor(20, 20, 30));
        DrawRectangleBorder(logRect, XnaColor.Gray, 2);

        DrawText(_combatLog, new Vector2(60, 590), XnaColor.White, 0.9f);
    }

    private void DrawButtons()
    {
        var mouseState = Mouse.GetState();
        var mousePoint = new Point(mouseState.X, mouseState.Y);

        // Next Turn button
        var nextTurnButton = new Rectangle(
            GraphicsDevice.Viewport.Width / 2 - 100,
            GraphicsDevice.Viewport.Height - 100,
            200, 50);

        bool nextTurnHovered = nextTurnButton.Contains(mousePoint);
        XnaColor nextTurnColor = _combatActive
            ? (nextTurnHovered ? XnaColor.LightGreen : XnaColor.Green)
            : XnaColor.Gray;

        DrawRectangle(nextTurnButton, nextTurnColor);
        DrawRectangleBorder(nextTurnButton, XnaColor.White, 2);
        DrawText("Next Turn", new Vector2(nextTurnButton.Center.X, nextTurnButton.Center.Y),
            XnaColor.White, 1.0f, true);

        // Back button
        var backButton = new Rectangle(50, GraphicsDevice.Viewport.Height - 100, 150, 50);
        bool backHovered = backButton.Contains(mousePoint);
        XnaColor backColor = backHovered ? XnaColor.DarkRed : XnaColor.DarkSlateGray;

        DrawRectangle(backButton, backColor);
        DrawRectangleBorder(backButton, XnaColor.White, 2);
        DrawText("← Back", new Vector2(backButton.Center.X, backButton.Center.Y),
            XnaColor.White, 1.0f, true);
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

        SpriteBatch.Draw(_whitePixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        SpriteBatch.Draw(_whitePixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        SpriteBatch.Draw(_whitePixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        SpriteBatch.Draw(_whitePixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    private void DrawText(string text, Vector2 position, XnaColor color, float scale = 1.0f, bool centered = false)
    {
        if (_regularFont != null)
        {
            Vector2 origin = centered ? _regularFont.MeasureString(text) / 2 : Vector2.Zero;
            SpriteBatch.DrawString(_regularFont, text, position, color, 0f, origin, scale, SpriteEffects.None, 0);
        }
        else
        {
            // Fallback rectangle rendering
            int width = (int)(text.Length * 10 * scale);
            int height = (int)(20 * scale);
            var rect = new Rectangle(
                (int)(centered ? position.X - width / 2 : position.X),
                (int)(centered ? position.Y - height / 2 : position.Y),
                width, height);
            DrawRectangle(rect, color * 0.5f);
        }
    }

    public override void UnloadContent()
    {
        base.UnloadContent();
        _whitePixel?.Dispose();
    }

    // Supporting classes
    private enum CharacterSpriteType
    {
        Fighter,
        Rogue,
        Goblin
    }

    private class CharacterSprite
    {
        public Character Character { get; }
        public Vector2 Position { get; private set; }
        public CharacterSpriteType Type { get; }

        private float _hitFlashTime = 0f;
        private Vector2 _hitOffset = Vector2.Zero;

        public CharacterSprite(Character character, Vector2 position, CharacterSpriteType type)
        {
            Character = character;
            Position = position;
            Type = type;
        }

        public void Update(float deltaTime)
        {
            // Update hit flash
            if (_hitFlashTime > 0)
            {
                _hitFlashTime -= deltaTime;
                _hitOffset = new Vector2(
                    (float)(Math.Sin(_hitFlashTime * 30) * 5),
                    0);
            }
            else
            {
                _hitOffset = Vector2.Zero;
            }
        }

        public void TriggerHit()
        {
            _hitFlashTime = 0.3f;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel)
        {
            Vector2 drawPos = Position + _hitOffset;
            XnaColor tintColor = _hitFlashTime > 0 ? XnaColor.Red : XnaColor.White;

            // Draw procedural sprite based on type
            switch (Type)
            {
                case CharacterSpriteType.Fighter:
                    DrawFighter(spriteBatch, whitePixel, drawPos, tintColor);
                    break;
                case CharacterSpriteType.Rogue:
                    DrawRogue(spriteBatch, whitePixel, drawPos, tintColor);
                    break;
                case CharacterSpriteType.Goblin:
                    DrawGoblin(spriteBatch, whitePixel, drawPos, tintColor);
                    break;
            }

            // Draw health bar
            DrawHealthBar(spriteBatch, whitePixel, drawPos);

            // Draw name (fallback rendering)
            var nameRect = new Rectangle((int)drawPos.X - 40, (int)drawPos.Y - 80, 80, 20);
            spriteBatch.Draw(whitePixel, nameRect, XnaColor.Black * 0.7f);
        }

        private void DrawFighter(SpriteBatch sb, Texture2D pixel, Vector2 pos, XnaColor tint)
        {
            // Body
            sb.Draw(pixel, new Rectangle((int)pos.X - 20, (int)pos.Y - 30, 40, 60),
                new XnaColor(XnaColor.DarkBlue.ToVector4() * tint.ToVector4()));
            // Shield
            sb.Draw(pixel, new Rectangle((int)pos.X - 30, (int)pos.Y - 20, 15, 40),
                new XnaColor(XnaColor.Silver.ToVector4() * tint.ToVector4()));
            // Sword
            sb.Draw(pixel, new Rectangle((int)pos.X + 20, (int)pos.Y - 30, 5, 50),
                new XnaColor(XnaColor.Gray.ToVector4() * tint.ToVector4()));
        }

        private void DrawRogue(SpriteBatch sb, Texture2D pixel, Vector2 pos, XnaColor tint)
        {
            // Body
            sb.Draw(pixel, new Rectangle((int)pos.X - 15, (int)pos.Y - 25, 30, 50),
                new XnaColor(XnaColor.DarkGreen.ToVector4() * tint.ToVector4()));
            // Daggers
            sb.Draw(pixel, new Rectangle((int)pos.X - 25, (int)pos.Y - 10, 3, 20),
                new XnaColor(XnaColor.Silver.ToVector4() * tint.ToVector4()));
            sb.Draw(pixel, new Rectangle((int)pos.X + 22, (int)pos.Y - 10, 3, 20),
                new XnaColor(XnaColor.Silver.ToVector4() * tint.ToVector4()));
        }

        private void DrawGoblin(SpriteBatch sb, Texture2D pixel, Vector2 pos, XnaColor tint)
        {
            // Body
            sb.Draw(pixel, new Rectangle((int)pos.X - 18, (int)pos.Y - 20, 36, 40),
                new XnaColor(XnaColor.DarkOliveGreen.ToVector4() * tint.ToVector4()));
            // Ears
            sb.Draw(pixel, new Rectangle((int)pos.X - 25, (int)pos.Y - 25, 8, 15),
                new XnaColor(XnaColor.DarkGreen.ToVector4() * tint.ToVector4()));
            sb.Draw(pixel, new Rectangle((int)pos.X + 17, (int)pos.Y - 25, 8, 15),
                new XnaColor(XnaColor.DarkGreen.ToVector4() * tint.ToVector4()));
        }

        private void DrawHealthBar(SpriteBatch sb, Texture2D pixel, Vector2 pos)
        {
            int barWidth = 60;
            int barHeight = 8;
            Vector2 barPos = new Vector2(pos.X - barWidth / 2, pos.Y + 40);

            // Background
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, barHeight),
                XnaColor.DarkRed);

            // Health
            float healthPercent = (float)Character.Stats.CurrentHealth / Character.Stats.MaxHealth;
            int healthWidth = (int)(barWidth * healthPercent);
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y, healthWidth, barHeight),
                XnaColor.LimeGreen);

            // Border
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, 1), XnaColor.White);
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y + barHeight, barWidth, 1), XnaColor.White);
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y, 1, barHeight), XnaColor.White);
            sb.Draw(pixel, new Rectangle((int)barPos.X + barWidth, (int)barPos.Y, 1, barHeight), XnaColor.White);
        }
    }

    private class DamageNumber
    {
        public string Text { get; }
        public Vector2 Position { get; private set; }
        public XnaColor Color { get; }
        public float Lifetime { get; private set; }

        public DamageNumber(string text, Vector2 position, XnaColor color)
        {
            Text = text;
            Position = position + new Vector2(0, -30);
            Color = color;
            Lifetime = 0;
        }

        public void Update(float deltaTime)
        {
            Lifetime += deltaTime;
            Position += new Vector2(0, -30 * deltaTime); // Float upward
        }
    }
}
