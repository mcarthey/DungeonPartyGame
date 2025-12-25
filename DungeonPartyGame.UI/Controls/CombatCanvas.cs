using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using DungeonPartyGame.UI.Models;

namespace DungeonPartyGame.UI.Controls;

public class CombatCanvas : SKCanvasView
{
    private readonly List<CharacterVisual> _characters = new();
    private readonly List<CombatAnimation> _activeAnimations = new();
    private bool _isAnimating = false;
    private DateTime _lastUpdate = DateTime.Now;

    // Colors
    private readonly SKColor _healthBarBackground = new SKColor(50, 50, 50);
    private readonly SKColor _healthBarFill = new SKColor(76, 175, 80);
    private readonly SKColor _healthBarLowFill = new SKColor(244, 67, 54);
    private readonly SKColor _damageColor = new SKColor(255, 87, 34);
    private readonly SKColor _criticalColor = new SKColor(255, 193, 7);
    private readonly SKColor _missColor = new SKColor(158, 158, 158);
    private readonly SKColor _heroColor = new SKColor(33, 150, 243);
    private readonly SKColor _enemyColor = new SKColor(244, 67, 54);

    public CombatCanvas()
    {
        PaintSurface += OnPaintSurface;
        EnableTouchEvents = true;
    }

    public void AddCharacter(string name, float x, float y, float health, float maxHealth)
    {
        _characters.Add(new CharacterVisual(name, x, y, health, maxHealth));
        InvalidateSurface();
    }

    public void UpdateCharacterHealth(string name, float health)
    {
        var character = _characters.FirstOrDefault(c => c.Name == name);
        if (character != null)
        {
            character.Health = Math.Max(0, health);
            InvalidateSurface();
        }
    }

    public void ClearCharacters()
    {
        _characters.Clear();
        _activeAnimations.Clear();
        InvalidateSurface();
    }

    public void PlayAttackAnimation(string attackerName, string targetName, int damage, bool isCritical = false, bool isMiss = false)
    {
        var attacker = _characters.FirstOrDefault(c => c.Name == attackerName);
        var target = _characters.FirstOrDefault(c => c.Name == targetName);

        if (attacker != null && target != null)
        {
            // Start attack animation
            attacker.IsAttacking = true;
            attacker.AttackProgress = 0;

            // Add damage number to target
            target.DamageNumbers.Add(new DamageNumber(damage, target.X, target.Y - 30, isCritical, isMiss));

            // Start animation loop if not already running
            if (!_isAnimating)
            {
                _isAnimating = true;
                StartAnimationLoop();
            }
        }
    }

    private async void StartAnimationLoop()
    {
        while (_isAnimating)
        {
            var now = DateTime.Now;
            var deltaTime = (float)(now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;

            UpdateAnimations(deltaTime);
            InvalidateSurface();

            // Check if we should stop animating
            if (_characters.All(c => !c.IsAttacking) &&
                _characters.All(c => c.DamageNumbers.Count == 0))
            {
                _isAnimating = false;
            }

            await Task.Delay(16); // ~60 FPS
        }
    }

    private void UpdateAnimations(float deltaTime)
    {
        foreach (var character in _characters)
        {
            // Update attack animation
            if (character.IsAttacking)
            {
                character.AttackProgress += deltaTime * 3.0f; // 3x speed
                if (character.AttackProgress >= 1.0f)
                {
                    character.IsAttacking = false;
                    character.AttackProgress = 0;
                }
            }

            // Update damage numbers
            for (int i = character.DamageNumbers.Count - 1; i >= 0; i--)
            {
                var dmgNum = character.DamageNumbers[i];
                dmgNum.Lifetime += deltaTime;
                dmgNum.Y -= deltaTime * 50; // Float upward
                dmgNum.Opacity = Math.Max(0, 1.0f - dmgNum.Lifetime);

                if (dmgNum.Lifetime >= 1.0f)
                {
                    character.DamageNumbers.RemoveAt(i);
                }
            }
        }
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(new SKColor(30, 30, 40));

        if (_characters.Count == 0)
        {
            DrawEmptyState(canvas, e.Info.Width, e.Info.Height);
            return;
        }

        foreach (var character in _characters)
        {
            DrawCharacter(canvas, character);
        }
    }

    private void DrawEmptyState(SKCanvas canvas, int width, int height)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(150, 150, 150),
            TextSize = 24,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        canvas.DrawText("Combat View - Waiting for combat to start...",
            width / 2, height / 2, paint);
    }

    private void DrawCharacter(SKCanvas canvas, CharacterVisual character)
    {
        float x = character.X;
        float y = character.Y;

        // Apply attack animation (character moves forward when attacking)
        if (character.IsAttacking)
        {
            float attackOffset = (float)Math.Sin(character.AttackProgress * Math.PI) * 20;
            x += attackOffset;
        }

        // Determine if this is a hero (left side) or enemy (right side)
        bool isHero = character.X < 200;
        var characterColor = isHero ? _heroColor : _enemyColor;

        // Draw character body (simple circle for POC)
        using (var paint = new SKPaint
        {
            Color = characterColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawCircle(x, y, 30, paint);
        }

        // Draw character outline
        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        })
        {
            canvas.DrawCircle(x, y, 30, paint);
        }

        // Draw character name
        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText(character.Name, x, y + 50, paint);
        }

        // Draw health bar
        DrawHealthBar(canvas, character, x, y - 50);

        // Draw damage numbers
        foreach (var dmgNum in character.DamageNumbers)
        {
            DrawDamageNumber(canvas, dmgNum);
        }
    }

    private void DrawHealthBar(SKCanvas canvas, CharacterVisual character, float x, float y)
    {
        float barWidth = 80;
        float barHeight = 8;
        float healthPercent = character.MaxHealth > 0 ? character.Health / character.MaxHealth : 0;

        // Background
        using (var paint = new SKPaint
        {
            Color = _healthBarBackground,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(x - barWidth / 2, y, barWidth, barHeight, paint);
        }

        // Health fill
        var fillColor = healthPercent > 0.3f ? _healthBarFill : _healthBarLowFill;
        using (var paint = new SKPaint
        {
            Color = fillColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(x - barWidth / 2, y, barWidth * healthPercent, barHeight, paint);
        }

        // Border
        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        })
        {
            canvas.DrawRect(x - barWidth / 2, y, barWidth, barHeight, paint);
        }

        // Health text
        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 12,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText($"{(int)character.Health}/{(int)character.MaxHealth}",
                x, y - 5, paint);
        }
    }

    private void DrawDamageNumber(SKCanvas canvas, DamageNumber dmgNum)
    {
        SKColor color;
        string text;

        if (dmgNum.IsMiss)
        {
            color = _missColor;
            text = "MISS";
        }
        else if (dmgNum.IsCritical)
        {
            color = _criticalColor;
            text = $"{dmgNum.Value}!";
        }
        else
        {
            color = _damageColor;
            text = dmgNum.Value.ToString();
        }

        using var paint = new SKPaint
        {
            Color = color.WithAlpha((byte)(dmgNum.Opacity * 255)),
            TextSize = dmgNum.IsCritical ? 28 : 20,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = dmgNum.IsCritical
        };

        canvas.DrawText(text, dmgNum.X, dmgNum.Y, paint);
    }
}
