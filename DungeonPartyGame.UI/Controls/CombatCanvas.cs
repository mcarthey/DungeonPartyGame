using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using DungeonPartyGame.UI.Models;

namespace DungeonPartyGame.UI.Controls;

public class CombatCanvas : SKCanvasView
{
    private readonly List<CharacterVisual> _characters = new();
    private readonly ParticleSystem _particleSystem = new();
    private bool _isAnimating = false;
    private DateTime _lastUpdate = DateTime.Now;
    private float _screenShakeIntensity = 0;
    private readonly Random _random = new();

    // Enhanced color palette
    private readonly SKColor _bgGradientTop = new SKColor(20, 24, 40);
    private readonly SKColor _bgGradientBottom = new SKColor(40, 30, 50);
    private readonly SKColor _healthBarBackground = new SKColor(40, 40, 40);
    private readonly SKColor _healthBarFill = new SKColor(46, 204, 113);
    private readonly SKColor _healthBarLowFill = new SKColor(231, 76, 60);
    private readonly SKColor _damageColor = new SKColor(255, 87, 34);
    private readonly SKColor _criticalColor = new SKColor(255, 215, 0);
    private readonly SKColor _missColor = new SKColor(158, 158, 158);
    private readonly SKColor _healColor = new SKColor(46, 204, 113);

    public CombatCanvas()
    {
        PaintSurface += OnPaintSurface;
        EnableTouchEvents = true;
    }

    public void AddCharacter(string name, float x, float y, float health, float maxHealth, Models.CharacterRole role = Models.CharacterRole.Fighter)
    {
        _characters.Add(new CharacterVisual(name, x, y, health, maxHealth, role));
        InvalidateSurface();
    }

    public void UpdateCharacterHealth(string name, float health)
    {
        var character = _characters.FirstOrDefault(c => c.Name == name);
        if (character != null)
        {
            character.Health = Math.Max(0, health);
            if (character.Health <= 0)
            {
                character.SetState(CharacterState.Dead);
            }
            InvalidateSurface();
        }
    }

    public void ClearCharacters()
    {
        _characters.Clear();
        _particleSystem.Clear();
        InvalidateSurface();
    }

    public void PlayAttackAnimation(string attackerName, string targetName, int damage, bool isCritical = false, bool isMiss = false)
    {
        var attacker = _characters.FirstOrDefault(c => c.Name == attackerName);
        var target = _characters.FirstOrDefault(c => c.Name == targetName);

        if (attacker != null && target != null)
        {
            // Set attacker to attacking state
            attacker.SetState(CharacterState.Attacking);

            if (isMiss)
            {
                // Dodge animation for target
                target.SetState(CharacterState.Dodging);
                target.DodgeTime = 0;

                // Add miss text
                target.DamageNumbers.Add(new DamageNumber(0, target.X, target.Y - 40, false, true, false));
            }
            else
            {
                // Hit animation
                target.SetState(CharacterState.Hurt);
                target.HitFlashTime = 0.2f;

                // Add damage number
                var damageNum = new DamageNumber(damage, target.X, target.Y - 40, isCritical, false, false);
                target.DamageNumbers.Add(damageNum);

                // Particle effects
                if (isCritical)
                {
                    _particleSystem.SpawnImpactSparks(target.X, target.Y, 30);
                    _screenShakeIntensity = 8.0f;
                }
                else
                {
                    _particleSystem.SpawnBloodSplatter(target.X, target.Y, 12);
                    _screenShakeIntensity = 3.0f;
                }
            }

            // Start animation loop if not already running
            if (!_isAnimating)
            {
                _isAnimating = true;
                StartAnimationLoop();
            }
        }
    }

    public void PlayHealAnimation(string targetName, int healAmount)
    {
        var target = _characters.FirstOrDefault(c => c.Name == targetName);
        if (target != null)
        {
            target.DamageNumbers.Add(new DamageNumber(healAmount, target.X, target.Y - 40, false, false, true));
            _particleSystem.SpawnHealEffect(target.X, target.Y, 15);

            if (!_isAnimating)
            {
                _isAnimating = true;
                StartAnimationLoop();
            }
        }
    }

    public void SetVictoryState(string characterName)
    {
        var character = _characters.FirstOrDefault(c => c.Name == characterName);
        if (character != null)
        {
            character.SetState(CharacterState.Victory);
            InvalidateSurface();
        }
    }

    private async void StartAnimationLoop()
    {
        while (_isAnimating)
        {
            var now = DateTime.Now;
            var deltaTime = (float)(now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;

            // Cap delta time to prevent huge jumps
            deltaTime = Math.Min(deltaTime, 0.05f);

            UpdateAnimations(deltaTime);
            InvalidateSurface();

            // Check if we should stop animating
            if (_characters.All(c => c.State == CharacterState.Idle || c.State == CharacterState.Dead || c.State == CharacterState.Victory) &&
                _characters.All(c => c.DamageNumbers.Count == 0) &&
                !_particleSystem.HasActiveParticles &&
                _screenShakeIntensity <= 0)
            {
                _isAnimating = false;
            }

            await Task.Delay(16); // ~60 FPS
        }
    }

    private void UpdateAnimations(float deltaTime)
    {
        // Update screen shake
        if (_screenShakeIntensity > 0)
        {
            _screenShakeIntensity = Math.Max(0, _screenShakeIntensity - deltaTime * 20);
        }

        // Update particles
        _particleSystem.Update(deltaTime);

        foreach (var character in _characters)
        {
            // Update animation time for idle bob
            character.AnimationTime += deltaTime;

            // Update hit flash
            if (character.HitFlashTime > 0)
            {
                character.HitFlashTime = Math.Max(0, character.HitFlashTime - deltaTime);
            }

            // State-based animation updates
            switch (character.State)
            {
                case CharacterState.Attacking:
                    // Attack animation lasts 0.5 seconds
                    if (character.AnimationTime > 0.5f)
                    {
                        character.SetState(CharacterState.Idle);
                    }
                    else
                    {
                        // Move forward and back
                        float progress = character.AnimationTime / 0.5f;
                        float offset = (float)Math.Sin(progress * Math.PI) * 30;
                        character.X = character.BaseX + (character.BaseX < 200 ? offset : -offset);
                    }
                    break;

                case CharacterState.Hurt:
                    if (character.AnimationTime > 0.3f)
                    {
                        character.SetState(CharacterState.Idle);
                    }
                    break;

                case CharacterState.Dodging:
                    if (character.AnimationTime > 0.4f)
                    {
                        character.SetState(CharacterState.Idle);
                        character.X = character.BaseX;
                        character.Y = character.BaseY;
                    }
                    else
                    {
                        // Quick sidestep
                        float progress = character.AnimationTime / 0.4f;
                        character.X = character.BaseX + (float)(Math.Sin(progress * Math.PI) * 20);
                    }
                    break;

                case CharacterState.Victory:
                    // Gentle bounce animation
                    character.Y = character.BaseY + (float)(Math.Sin(character.AnimationTime * 3) * 5);
                    break;

                case CharacterState.Idle:
                    // Reset to base position
                    character.X = character.BaseX;
                    character.Y = character.BaseY;
                    break;
            }

            // Update damage numbers
            for (int i = character.DamageNumbers.Count - 1; i >= 0; i--)
            {
                var dmgNum = character.DamageNumbers[i];
                dmgNum.Lifetime += deltaTime;
                dmgNum.Y -= deltaTime * 60; // Float upward faster
                dmgNum.Opacity = Math.Max(0, 1.0f - dmgNum.Lifetime);

                // Scale effect for critical hits
                if (dmgNum.IsCritical && dmgNum.Lifetime < 0.2f)
                {
                    dmgNum.Scale = 1.0f + (float)Math.Sin(dmgNum.Lifetime * 10) * 0.3f;
                }
                else
                {
                    dmgNum.Scale = 1.0f;
                }

                if (dmgNum.Lifetime >= 1.5f)
                {
                    character.DamageNumbers.RemoveAt(i);
                }
            }
        }
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // Calculate screen shake offset
        float shakeX = 0, shakeY = 0;
        if (_screenShakeIntensity > 0)
        {
            shakeX = ((float)_random.NextDouble() - 0.5f) * _screenShakeIntensity * 2;
            shakeY = ((float)_random.NextDouble() - 0.5f) * _screenShakeIntensity * 2;
        }

        canvas.Save();
        canvas.Translate(shakeX, shakeY);

        // Draw background gradient
        DrawBackground(canvas, e.Info.Width, e.Info.Height);

        if (_characters.Count == 0)
        {
            DrawEmptyState(canvas, e.Info.Width, e.Info.Height);
            canvas.Restore();
            return;
        }

        // Draw ground shadows
        foreach (var character in _characters)
        {
            DrawShadow(canvas, character);
        }

        // Draw particles behind characters
        _particleSystem.Draw(canvas);

        // Draw characters
        foreach (var character in _characters)
        {
            DrawCharacter(canvas, character);
        }

        canvas.Restore();
    }

    private void DrawBackground(SKCanvas canvas, int width, int height)
    {
        using var paint = new SKPaint();
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, height),
            new[] { _bgGradientTop, _bgGradientBottom },
            SKShaderTileMode.Clamp);

        paint.Shader = shader;
        canvas.DrawRect(0, 0, width, height, paint);

        // Draw floor line
        using var linePaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 30),
            StrokeWidth = 2,
            Style = SKPaintStyle.Stroke
        };
        canvas.DrawLine(0, height * 0.7f, width, height * 0.7f, linePaint);
    }

    private void DrawShadow(SKCanvas canvas, CharacterVisual character)
    {
        float shadowY = character.BaseY + 40;
        using var paint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 50),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5)
        };

        canvas.DrawOval(character.X, shadowY, 25, 8, paint);
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

        canvas.DrawText("⚔️ Combat Arena - Awaiting Warriors ⚔️",
            width / 2, height / 2, paint);
    }

    private void DrawCharacter(SKCanvas canvas, CharacterVisual character)
    {
        float x = character.X;
        float y = character.Y;

        // Add idle bob for living characters
        if (character.State == CharacterState.Idle && character.IsAlive)
        {
            y += character.IdleBobOffset;
        }

        // Determine if hero or enemy
        bool isHero = character.BaseX < 200;

        // Draw sprite based on role
        switch (character.Role)
        {
            case Models.CharacterRole.Fighter:
                DrawFighterSprite(canvas, x, y, character);
                break;
            case Models.CharacterRole.Rogue:
                DrawRogueSprite(canvas, x, y, character);
                break;
            case Models.CharacterRole.Enemy:
                DrawEnemySprite(canvas, x, y, character);
                break;
        }

        // Draw character name
        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 14,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        })
        {
            // Add text shadow
            using var shadowPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 150),
                TextSize = 14,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            };
            canvas.DrawText(character.Name, x + 1, y + 56, shadowPaint);
            canvas.DrawText(character.Name, x, y + 55, paint);
        }

        // Draw health bar
        DrawHealthBar(canvas, character, x, y - 55);

        // Draw damage numbers
        foreach (var dmgNum in character.DamageNumbers)
        {
            DrawDamageNumber(canvas, dmgNum);
        }

        // Draw status icons
        DrawStatusIcons(canvas, character, x, y - 70);
    }

    private void DrawFighterSprite(SKCanvas canvas, float x, float y, CharacterVisual character)
    {
        bool isFlipped = character.BaseX < 200;
        canvas.Save();

        // Apply hit flash
        SKColor bodyColor = new SKColor(52, 152, 219); // Blue
        SKColor armorColor = new SKColor(189, 195, 199); // Silver
        if (character.IsFlashing)
        {
            bodyColor = SKColors.White;
            armorColor = SKColors.White;
        }

        // Opacity for dead characters
        byte alpha = character.State == CharacterState.Dead ? (byte)100 : (byte)255;

        // Body (larger circle for fighter)
        using (var paint = new SKPaint
        {
            Color = bodyColor.WithAlpha(alpha),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawCircle(x, y, 28, paint);
        }

        // Armor plates (rectangles)
        using (var paint = new SKPaint
        {
            Color = armorColor.WithAlpha(alpha),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(x - 15, y - 10, 30, 8, paint);
            canvas.DrawRect(x - 15, y + 5, 30, 8, paint);
        }

        // Shield (left side)
        using (var paint = new SKPaint
        {
            Color = new SKColor(231, 76, 60).WithAlpha(alpha), // Red shield
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            float shieldX = isFlipped ? x - 25 : x + 25;
            canvas.DrawRoundRect(shieldX - 8, y - 12, 16, 24, 4, 4, paint);
        }

        // Weapon (sword - right side)
        using (var paint = new SKPaint
        {
            Color = new SKColor(149, 165, 166).WithAlpha(alpha),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4
        })
        {
            float weaponX = isFlipped ? x + 25 : x - 25;
            float weaponOffset = character.State == CharacterState.Attacking ? 10 : 0;
            canvas.DrawLine(weaponX, y - 20 - weaponOffset, weaponX, y + 10 - weaponOffset, paint);
        }

        // Eyes
        using (var paint = new SKPaint
        {
            Color = SKColors.White.WithAlpha(alpha),
            IsAntialias = true
        })
        {
            canvas.DrawCircle(x - 8, y - 5, 4, paint);
            canvas.DrawCircle(x + 8, y - 5, 4, paint);
        }

        canvas.Restore();
    }

    private void DrawRogueSprite(SKCanvas canvas, float x, float y, CharacterVisual character)
    {
        bool isFlipped = character.BaseX < 200;
        canvas.Save();

        SKColor bodyColor = new SKColor(46, 64, 83); // Dark gray/black
        SKColor accentColor = new SKColor(155, 89, 182); // Purple accent
        if (character.IsFlashing)
        {
            bodyColor = SKColors.White;
            accentColor = SKColors.White;
        }

        byte alpha = character.State == CharacterState.Dead ? (byte)100 : (byte)255;

        // Body (sleeker, smaller)
        using (var paint = new SKPaint
        {
            Color = bodyColor.WithAlpha(alpha),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawCircle(x, y, 24, paint);
        }

        // Hood
        using (var paint = new SKPaint
        {
            Color = bodyColor.WithAlpha(alpha),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            var path = new SKPath();
            path.MoveTo(x - 18, y - 15);
            path.LineTo(x, y - 28);
            path.LineTo(x + 18, y - 15);
            path.Close();
            canvas.DrawPath(path, paint);
        }

        // Dual daggers
        using (var paint = new SKPaint
        {
            Color = accentColor.WithAlpha(alpha),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3
        })
        {
            float weaponOffset = character.State == CharacterState.Attacking ? 15 : 0;
            // Left dagger
            canvas.DrawLine(x - 20, y + 5 - weaponOffset, x - 20, y - 10 - weaponOffset, paint);
            // Right dagger
            canvas.DrawLine(x + 20, y + 5 - weaponOffset, x + 20, y - 10 - weaponOffset, paint);
        }

        // Eyes (glowing)
        using (var paint = new SKPaint
        {
            Color = accentColor.WithAlpha(alpha),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2)
        })
        {
            canvas.DrawCircle(x - 7, y - 3, 3, paint);
            canvas.DrawCircle(x + 7, y - 3, 3, paint);
        }

        canvas.Restore();
    }

    private void DrawEnemySprite(SKCanvas canvas, float x, float y, CharacterVisual character)
    {
        canvas.Save();

        SKColor bodyColor = new SKColor(192, 57, 43); // Dark red
        SKColor accentColor = new SKColor(231, 76, 60); // Brighter red
        if (character.IsFlashing)
        {
            bodyColor = SKColors.White;
            accentColor = SKColors.White;
        }

        byte alpha = character.State == CharacterState.Dead ? (byte)100 : (byte)255;

        // Body
        using (var paint = new SKPaint
        {
            Color = bodyColor.WithAlpha(alpha),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawCircle(x, y, 26, paint);
        }

        // Horns
        using (var paint = new SKPaint
        {
            Color = accentColor.WithAlpha(alpha),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            var leftHorn = new SKPath();
            leftHorn.MoveTo(x - 15, y - 20);
            leftHorn.LineTo(x - 20, y - 35);
            leftHorn.LineTo(x - 10, y - 22);
            leftHorn.Close();

            var rightHorn = new SKPath();
            rightHorn.MoveTo(x + 15, y - 20);
            rightHorn.LineTo(x + 20, y - 35);
            rightHorn.LineTo(x + 10, y - 22);
            rightHorn.Close();

            canvas.DrawPath(leftHorn, paint);
            canvas.DrawPath(rightHorn, paint);
        }

        // Evil eyes
        using (var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 0).WithAlpha(alpha), // Yellow glow
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3)
        })
        {
            canvas.DrawCircle(x - 8, y - 5, 5, paint);
            canvas.DrawCircle(x + 8, y - 5, 5, paint);
        }

        // Fangs
        using (var paint = new SKPaint
        {
            Color = SKColors.White.WithAlpha(alpha),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            var fang1 = new SKPath();
            fang1.MoveTo(x - 5, y + 5);
            fang1.LineTo(x - 3, y + 15);
            fang1.LineTo(x - 7, y + 5);
            fang1.Close();

            var fang2 = new SKPath();
            fang2.MoveTo(x + 5, y + 5);
            fang2.LineTo(x + 3, y + 15);
            fang2.LineTo(x + 7, y + 5);
            fang2.Close();

            canvas.DrawPath(fang1, paint);
            canvas.DrawPath(fang2, paint);
        }

        canvas.Restore();
    }

    private void DrawHealthBar(SKCanvas canvas, CharacterVisual character, float x, float y)
    {
        float barWidth = 80;
        float barHeight = 10;
        float healthPercent = character.MaxHealth > 0 ? character.Health / character.MaxHealth : 0;

        // Background with border
        using (var paint = new SKPaint
        {
            Color = _healthBarBackground,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRoundRect(x - barWidth / 2, y, barWidth, barHeight, 3, 3, paint);
        }

        // Health fill with gradient
        if (healthPercent > 0)
        {
            var fillColor = healthPercent > 0.3f ? _healthBarFill : _healthBarLowFill;
            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(x - barWidth / 2, y),
                new SKPoint(x - barWidth / 2, y + barHeight),
                new[] { fillColor, fillColor.WithAlpha(180) },
                SKShaderTileMode.Clamp);

            paint.Shader = shader;
            var fillRect = new SKRect(x - barWidth / 2, y, x - barWidth / 2 + barWidth * healthPercent, y + barHeight);
            canvas.DrawRoundRect(fillRect, 3, 3, paint);
        }

        // Border
        using (var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 150),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        })
        {
            canvas.DrawRoundRect(x - barWidth / 2, y, barWidth, barHeight, 3, 3, paint);
        }

        // Health text with shadow
        using (var shadowPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 200),
            TextSize = 11,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        })
        {
            canvas.DrawText($"{(int)character.Health}/{(int)character.MaxHealth}", x + 1, y - 4, shadowPaint);
        }

        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 11,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        })
        {
            canvas.DrawText($"{(int)character.Health}/{(int)character.MaxHealth}", x, y - 5, paint);
        }
    }

    private void DrawDamageNumber(SKCanvas canvas, DamageNumber dmgNum)
    {
        SKColor color;
        string text;

        if (dmgNum.IsMiss)
        {
            color = _missColor;
            text = "MISS!";
        }
        else if (dmgNum.IsHeal)
        {
            color = _healColor;
            text = $"+{dmgNum.Value}";
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

        float fontSize = dmgNum.IsCritical ? 32 : 24;
        fontSize *= dmgNum.Scale;

        // Text shadow
        using (var shadowPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, (byte)(dmgNum.Opacity * 200)),
            TextSize = fontSize,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        })
        {
            canvas.DrawText(text, dmgNum.X + 2, dmgNum.Y + 2, shadowPaint);
        }

        // Main text
        using var paint = new SKPaint
        {
            Color = color.WithAlpha((byte)(dmgNum.Opacity * 255)),
            TextSize = fontSize,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        };

        // Add stroke for better visibility
        using var strokePaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, (byte)(dmgNum.Opacity * 180)),
            TextSize = fontSize,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            FakeBoldText = true
        };

        canvas.DrawText(text, dmgNum.X, dmgNum.Y, strokePaint);
        canvas.DrawText(text, dmgNum.X, dmgNum.Y, paint);
    }

    private void DrawStatusIcons(SKCanvas canvas, CharacterVisual character, float x, float y)
    {
        float iconSize = 12;
        float spacing = 16;
        float startX = x - (character.StatusIcons.Count * spacing) / 2;

        for (int i = 0; i < character.StatusIcons.Count; i++)
        {
            var icon = character.StatusIcons[i];
            float iconX = startX + i * spacing;

            using var paint = new SKPaint
            {
                Color = icon.Color,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            canvas.DrawCircle(iconX, y, iconSize / 2, paint);

            using var borderPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };

            canvas.DrawCircle(iconX, y, iconSize / 2, borderPaint);
        }
    }
}
