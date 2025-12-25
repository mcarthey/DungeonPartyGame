# Game Animation POC Documentation

## Overview

This POC adds visual combat animations to the Dungeon Party Game using **SkiaSharp** for 2D graphics rendering within the existing .NET MAUI architecture. The implementation maintains the current backend logic while adding an animated visual layer to the combat system.

## What Was Added

### 1. **SkiaSharp Integration**
- Added `SkiaSharp.Views.Maui.Controls` NuGet package (v3.116.1)
- Configured MAUI to use SkiaSharp in `MauiProgram.cs`
- No major architectural changes - seamlessly integrates with existing MAUI setup

### 2. **Animation Models**
**Location:** `DungeonPartyGame.UI/Models/CombatAnimation.cs`

New models to track animation state:
- `CombatAnimation` - Tracks animation type, progress, and participants
- `CharacterVisual` - Visual representation of characters with position, health, and effects
- `DamageNumber` - Floating damage/miss text with lifetime and opacity

### 3. **Combat Canvas Control**
**Location:** `DungeonPartyGame.UI/Controls/CombatCanvas.cs`

A custom SkiaSharp-based canvas that renders:
- **Character sprites** - Simple circles (blue for heroes, red for enemies) with health bars
- **Health bars** - Animated progress bars showing current/max health
- **Attack animations** - Characters move forward when attacking
- **Damage numbers** - Float upward with fading opacity (critical hits are gold, misses are gray)
- **Real-time updates** - 60 FPS animation loop

### 4. **UI Integration**
**Modified Files:**
- `DungeonPartyGame.UI/Pages/MainPage.xaml` - Added combat canvas between buttons and combat log
- `DungeonPartyGame.UI/Pages/MainPage.xaml.cs` - Passes canvas reference to ViewModel
- `DungeonPartyGame.UI/ViewModels/MainViewModel.cs` - Triggers animations during combat rounds

## How It Works

### Combat Flow with Animations

1. **Create Characters** → Characters are added to the backend
2. **Start New Combat** →
   - Backend creates combat session
   - Canvas initializes with character visuals positioned on screen
   - Fighter appears on the left (x: 100), Rogue on the right (x: 300)
3. **Next Round** →
   - Backend executes combat logic
   - Canvas plays attack animation:
     - Attacker moves forward
     - Damage number appears above target
     - Target's health bar updates
     - Numbers float up and fade out

### Animation Features

#### Character Rendering
- **Heroes** (left side): Blue circles
- **Enemies** (right side): Red circles
- **Health bars**: Green when > 30% health, red when ≤ 30%
- **Name labels**: Displayed below each character

#### Attack Animation Sequence
1. Attacker slides forward (sine wave motion)
2. Damage number spawns at target position
3. Number floats upward while fading
4. Health bar smoothly updates
5. Attacker returns to original position

#### Damage Number Types
- **Normal damage**: Orange text
- **Critical hits**: Yellow/gold text, larger size, bold
- **Misses**: Gray "MISS" text

## Usage Example

```csharp
// In ViewModel or Page
_combatCanvas?.AddCharacter("Fighter", 100, 125, currentHealth: 50, maxHealth: 50);
_combatCanvas?.AddCharacter("Enemy", 300, 125, currentHealth: 40, maxHealth: 40);

// Trigger attack animation
_combatCanvas?.PlayAttackAnimation(
    attackerName: "Fighter",
    targetName: "Enemy",
    damage: 15,
    isCritical: false,
    isMiss: false
);

// Update health after damage
_combatCanvas?.UpdateCharacterHealth("Enemy", newHealth: 25);
```

## Architecture Benefits

### Why This Approach?

1. **Non-invasive** - No changes to core game logic
2. **MAUI-compatible** - Works within existing framework
3. **Cross-platform** - SkiaSharp runs on Windows, macOS, iOS, Android
4. **Performant** - Hardware-accelerated 2D rendering
5. **Extensible** - Easy to add more effects, sprites, particles

### Comparison with Alternatives

| Approach | Pros | Cons | Verdict |
|----------|------|------|---------|
| **SkiaSharp** (chosen) | Works with MAUI, good 2D support, easy sprites | Limited 3D | ✅ Best for POC |
| **MAUI Animations** | Built-in, simple | Only UI animations, no custom drawing | ❌ Too limited |
| **MonoGame** | Full game engine, robust | Requires rewriting UI layer | ❌ Too invasive |

## Future Enhancement Ideas

### Easy Wins
- [ ] Add critical hit detection from combat system
- [ ] Different character sprites per role (Fighter, Rogue)
- [ ] Status effect indicators (poison, buffs)
- [ ] Screen shake on critical hits
- [ ] Sound effects integration

### Medium Effort
- [ ] Sprite sheet animation (walk, attack, idle cycles)
- [ ] Particle effects (fire, blood, sparkles)
- [ ] Multiple party members in formation
- [ ] Skill-specific visual effects
- [ ] Victory/defeat animations

### Advanced
- [ ] Replace circles with actual character art
- [ ] Background scenes (dungeon, forest)
- [ ] Camera zoom/pan effects
- [ ] Combo attack chains
- [ ] Full MonoGame migration for 3D

## Code Structure

```
DungeonPartyGame/
├── DungeonPartyGame.csproj          [Modified: Added SkiaSharp package]
├── MauiProgram.cs                   [Modified: Added .UseSkiaSharp()]
│
└── DungeonPartyGame.UI/
    ├── Models/
    │   └── CombatAnimation.cs       [NEW: Animation data models]
    │
    ├── Controls/
    │   └── CombatCanvas.cs          [NEW: SkiaSharp rendering]
    │
    ├── Pages/
    │   ├── MainPage.xaml            [Modified: Added canvas to UI]
    │   └── MainPage.xaml.cs         [Modified: Pass canvas to VM]
    │
    └── ViewModels/
        └── MainViewModel.cs         [Modified: Trigger animations]
```

## Testing the POC

1. Build and run the application
2. Click **"Create Characters"**
3. Click **"Start New Combat"** → See characters appear on canvas
4. Click **"Next Round"** → Watch attack animations play:
   - Character moves forward
   - Damage number floats up
   - Health bar decreases
5. Continue clicking **"Next Round"** until combat ends

## Performance Notes

- **Frame rate**: ~60 FPS animation loop
- **Auto-pause**: Animation loop stops when no active effects
- **Minimal overhead**: Only animates during combat rounds
- **Smooth on mobile**: SkiaSharp is optimized for touch devices

## Conclusion

This POC demonstrates that you can add professional-looking game animations to your MAUI-based RPG without major architectural changes. SkiaSharp provides a good balance between power and simplicity for 2D game graphics.

**Next Steps:**
- Test on different platforms (iOS, Android)
- Gather feedback on animation feel
- Decide on art style (sprites vs. simple shapes)
- Consider whether to expand SkiaSharp approach or evaluate MonoGame for a full game engine
