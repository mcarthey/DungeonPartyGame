using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaColor = Microsoft.Xna.Framework.Color;
using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Microsoft.Extensions.Logging;
using System.IO;
using SpriteFontPlus;

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
    private SpriteFont? _titleFont;
    private SpriteFont? _uiSmall;
    private SpriteFont? _uiMedium;

    private readonly List<CharacterSprite> _heroSprites = new();
    private readonly List<CharacterSprite> _enemySprites = new();
    private readonly List<DamageNumber> _damageNumbers = new();

    // UI state
    private float _nextButtonPulseTime = 0f;
    private int _previousScrollWheel = 0;

    private Character? _fighter;
    private Character? _rogue;
    private Character? _goblin;

    private string _combatLog = "";
    private readonly List<string> _combatLogLines = new();
    private int _combatLogScroll = 0;
    private const int MaxCombatLogLines = 500;
    private bool _combatActive = false;
    private MouseState _previousMouseState;

    private static List<string> WrapTextSimple(string text, int maxChars)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(text)) return result;
        var words = text.Split(' ');
        var current = "";
        foreach (var w in words)
        {
            if (current.Length + w.Length + 1 <= maxChars)
            {
                current = string.IsNullOrEmpty(current) ? w : current + " " + w;
            }
            else
            {
                if (!string.IsNullOrEmpty(current)) result.Add(current);
                if (w.Length <= maxChars)
                    current = w;
                else
                {
                    // Break long word
                    for (int i = 0; i < w.Length; i += maxChars)
                    {
                        int len = Math.Min(maxChars, w.Length - i);
                        result.Add(w.Substring(i, len));
                    }
                    current = "";
                }
            }
        }
        if (!string.IsNullOrEmpty(current)) result.Add(current);
        return result;
    }

    private void AddCombatLog(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        var lines = text.Split('\n');
        foreach (var l in lines)
        {
            // Estimate characters-per-line using viewport width and a rough character width when font not available
            int approxChars = Math.Max(20, (GraphicsDevice.Viewport.Width - 120) / 12);
            var wrapped = WrapTextSimple(l, approxChars);
            foreach (var w in wrapped)
            {
                _combatLogLines.Add(w);
                if (_combatLogLines.Count > MaxCombatLogLines) _combatLogLines.RemoveAt(0);
            }
        }
        // Auto-scroll to bottom
        _combatLogScroll = 0;
    }


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

        // Load fonts - prefer a bundled font in Content/Fonts, otherwise fall back to system arial (avoids content pipeline requirement)
        try
        {
            string bundledPath = Path.Combine(AppContext.BaseDirectory, "Content", "Fonts", "NotoSans-Regular.ttf");
            if (File.Exists(bundledPath))
            {
                var fontBytes = File.ReadAllBytes(bundledPath);
                var bakeSmall = TtfFontBaker.Bake(fontBytes, 14, 512, 512, new[] { CharacterRange.BasicLatin });
                _uiSmall = bakeSmall.CreateSpriteFont(GraphicsDevice);
                var bakeMed = TtfFontBaker.Bake(fontBytes, 18, 512, 512, new[] { CharacterRange.BasicLatin });
                _uiMedium = bakeMed.CreateSpriteFont(GraphicsDevice);
                var bakeLarge = TtfFontBaker.Bake(fontBytes, 36, 512, 512, new[] { CharacterRange.BasicLatin });
                _titleFont = bakeLarge.CreateSpriteFont(GraphicsDevice);
                _regularFont = _uiMedium;
                _logger.LogInformation("Loaded bundled font from Content/Fonts/NotoSans-Regular.ttf");
            }
            else
            {
                var windowsFonts = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                var arialPath = Path.Combine(windowsFonts, "arial.ttf");
                if (File.Exists(arialPath))
                {
                    var fontBytes = File.ReadAllBytes(arialPath);
                    var fontBakeRegular = TtfFontBaker.Bake(fontBytes, 18, 512, 512, new[] { CharacterRange.BasicLatin });
                    _regularFont = fontBakeRegular.CreateSpriteFont(GraphicsDevice);
                    var fontBakeTitle = TtfFontBaker.Bake(fontBytes, 36, 512, 512, new[] { CharacterRange.BasicLatin });
                    _titleFont = fontBakeTitle.CreateSpriteFont(GraphicsDevice);
                    var fontBakeSmall = TtfFontBaker.Bake(fontBytes, 14, 512, 512, new[] { CharacterRange.BasicLatin });
                    _uiSmall = fontBakeSmall.CreateSpriteFont(GraphicsDevice);
                    _uiMedium = _regularFont;
                }
                else
                {
                    _logger.LogWarning("No system font found and no bundled font present; using fallback rendering");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Runtime font generation failed; using fallback rendering");
        }

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

        // Update pulse for next turn button
        if (_combatActive)
        {
            _nextButtonPulseTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else
        {
            _nextButtonPulseTime = 0f;
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

        // Mouse wheel handling for combat log scrolling
        int wheelDelta = mouseState.ScrollWheelValue - _previousScrollWheel;
        if (wheelDelta != 0 && _combatLogLines.Count > 0)
        {
            // Positive wheelDelta means scroll up (older content)
            int lines = Math.Abs(wheelDelta) / 120; // typical mouse wheel step
            if (wheelDelta > 0)
                _combatLogScroll = Math.Min(Math.Max(0, _combatLogScroll + lines), Math.Max(0, _combatLogLines.Count - 1));
            else
                _combatLogScroll = Math.Max(0, _combatLogScroll - lines);
        }

        _previousScrollWheel = mouseState.ScrollWheelValue;
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
        if (!string.IsNullOrEmpty(result.SummaryText)) AddCombatLog(result.SummaryText);

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

        // Compute layout
        float titleScale = MathHelper.Clamp(GraphicsDevice.Viewport.Width / 800f * 1.4f, 1.0f, 1.6f);
        int titleY = 30;
        int arenaTop = titleY + (int)(titleScale * 30) + 10;
        int arenaHeight = Math.Max(300, GraphicsDevice.Viewport.Height - arenaTop - 220);

        // Draw combat arena
        DrawRectangle(new Rectangle(50, arenaTop, GraphicsDevice.Viewport.Width - 100, arenaHeight),
            new XnaColor(60, 55, 50));

        // Draw header (scaled and repositioned to avoid clipping)
        DrawText("⚔️ COMBAT ⚔️",
            new Vector2(GraphicsDevice.Viewport.Width / 2, titleY),
            XnaColor.Gold, titleScale, true);

        // Draw character sprites
        // Highlight the active attacker
        var active = _currentSession?.CurrentAttacker;
        CharacterSprite? activeSprite = null;
        foreach (var sprite in _heroSprites.Concat(_enemySprites))
        {
            if (sprite.Character == active) activeSprite = sprite;
        }

        if (activeSprite != null)
        {
            var b = activeSprite.GetBounds();
            var outline = new Rectangle((int)b.X - 6, (int)b.Y - 6, (int)b.Width + 12, (int)b.Height + 12);
            DrawRectangle(outline, XnaColor.Gold * 0.25f);
        }

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
            DrawText(dmg.Text, dmg.Position, dmg.Color, dmg.Scale, true);
        }

        // Draw unit labels and numeric HP
        foreach (var sprite in _heroSprites.Concat(_enemySprites))
        {
            DrawUnitLabel(sprite);
        }

        // Draw combat log
        DrawCombatLog();

        // Draw buttons
        DrawButtons();

        SpriteBatch.End();
    }

    private void DrawCombatLog()
    {
        var logRect = new Rectangle(50, GraphicsDevice.Viewport.Height - 160, GraphicsDevice.Viewport.Width - 100, 120);
        DrawRectangle(logRect, new XnaColor(20, 20, 30));
        DrawRectangleBorder(logRect, XnaColor.Gray, 2);

        float lineHeight = 18f;
        if (_regularFont != null) lineHeight = _regularFont.LineSpacing * 0.9f;
        int maxLines = Math.Max(1, (int)(logRect.Height / lineHeight) - 1);

        int total = _combatLogLines.Count;
        int start = Math.Max(0, total - maxLines - _combatLogScroll);
        int y = logRect.Y + 10;
        for (int i = start; i < total && i < start + maxLines; i++)
        {
            DrawText(_combatLogLines[i], new Vector2(logRect.X + 10, y), XnaColor.White, 0.9f, false);
            y += (int)lineHeight;
        }
    }

    private void DrawUnitLabel(CharacterSprite sprite)
    {
        // Name
        var namePos = new Vector2(sprite.Position.X, sprite.Position.Y - 80);
        DrawText(sprite.Character.Name, namePos, XnaColor.White, 0.9f, true);

        // HP text
        var barPos = sprite.GetHealthBarPosition();
        int barWidth = 60;
        var hpText = $"HP: {sprite.Character.Stats.CurrentHealth}/{sprite.Character.Stats.MaxHealth}";
        DrawText(hpText, new Vector2(barPos.X + barWidth + 8, barPos.Y - 2), XnaColor.White, 0.9f, false);
    }

    private void DrawButtons()
    {
        var mouseState = Mouse.GetState();
        var mousePoint = new Point(mouseState.X, mouseState.Y);

        // Next Turn button (larger + pulse when active)
        int btnW = 260;
        int btnH = 60;
        var nextTurnButton = new Rectangle(
            GraphicsDevice.Viewport.Width / 2 - btnW / 2,
            GraphicsDevice.Viewport.Height - 110,
            btnW, btnH);

        bool nextTurnHovered = nextTurnButton.Contains(mousePoint);
        float pulse = _combatActive ? (0.9f + 0.1f * (float)(Math.Sin(_nextButtonPulseTime * 3.0f) + 1.0) / 2.0f) : 1.0f;
        XnaColor baseGreen = _combatActive ? XnaColor.Green : XnaColor.Gray;
        XnaColor nextTurnColor = nextTurnHovered ? XnaColor.LightGreen : baseGreen;
        // modulate brightness via alpha-like multiplication
        nextTurnColor = nextTurnColor * pulse;

        DrawRectangle(nextTurnButton, nextTurnColor);
        DrawRectangleBorder(nextTurnButton, XnaColor.White, 2);
        DrawText("Next Turn", new Vector2(nextTurnButton.Center.X, nextTurnButton.Center.Y),
            XnaColor.White, 1.1f * pulse, true);

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
        }

        public Vector2 GetHealthBarPosition()
        {
            int barWidth = 60;
            int barHeight = 12;
            return new Vector2(Position.X - barWidth / 2, Position.Y + 40);
        }

        public Rectangle GetBounds()
        {
            switch (Type)
            {
                case CharacterSpriteType.Fighter:
                    return new Rectangle((int)Position.X - 30, (int)Position.Y - 40, 60, 80);
                case CharacterSpriteType.Rogue:
                    return new Rectangle((int)Position.X - 20, (int)Position.Y - 36, 40, 72);
                case CharacterSpriteType.Goblin:
                    return new Rectangle((int)Position.X - 22, (int)Position.Y - 30, 44, 60);
                default:
                    return new Rectangle((int)Position.X - 20, (int)Position.Y - 30, 40, 60);
            }
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
            int barHeight = 12;
            Vector2 barPos = new Vector2(pos.X - barWidth / 2, pos.Y + 40);

            // Background
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, barHeight),
                XnaColor.DarkRed);

            // Health
            float healthPercent = (float)Character.Stats.CurrentHealth / Math.Max(1, Character.Stats.MaxHealth);
            int healthWidth = (int)(barWidth * healthPercent);
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y, Math.Max(1, healthWidth), barHeight),
                XnaColor.LimeGreen);

            // Border (thicker)
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, 2), XnaColor.White);
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y + barHeight - 2, barWidth, 2), XnaColor.White);
            sb.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y, 2, barHeight), XnaColor.White);
            sb.Draw(pixel, new Rectangle((int)barPos.X + barWidth - 2, (int)barPos.Y, 2, barHeight), XnaColor.White);

            // Flash edge when hit
            if (_hitFlashTime > 0)
            {
                int flashW = Math.Min(barWidth, healthWidth + 6);
                sb.Draw(pixel, new Rectangle((int)barPos.X + Math.Max(0, healthWidth - 2), (int)barPos.Y - 2, 6, barHeight + 4), XnaColor.Yellow);
            }
        }
    }

    private class DamageNumber
    {
        public string Text { get; }
        public Vector2 Position { get; private set; }
        public XnaColor Color { get; }
        public float Lifetime { get; private set; }
        public float Scale { get; private set; }

        public DamageNumber(string text, Vector2 position, XnaColor color)
        {
            Text = text;
            Position = position + new Vector2(0, -30);
            Color = color;
            Lifetime = 0;
            Scale = 1.6f;
        }

        public void Update(float deltaTime)
        {
            Lifetime += deltaTime;
            Position += new Vector2(0, -30 * deltaTime); // Float upward
            // Shrink scale over first 1.0s, then settle
            float t = MathF.Min(1.0f, Lifetime / 0.9f);
            // MathF.Lerp isn't available on all target frameworks; use a simple linear interpolation.
            Scale = 1.6f + (0.95f - 1.6f) * t;
        }
    }
}
