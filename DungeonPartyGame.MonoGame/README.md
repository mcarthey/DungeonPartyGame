# DungeonPartyGame.MonoGame

MonoGame-based UI for Dungeon Party Game. This project replaces the MAUI UI layer with a game engine approach while preserving 100% of the Core backend logic.

## Architecture

```
DungeonPartyGame.MonoGame/
├── Program.cs                    # Entry point
├── DungeonPartyGameMain.cs       # Main game class, DI setup
├── Screens/
│   ├── Screen.cs                 # Base screen class
│   ├── ScreenManager.cs          # Screen navigation system
│   ├── HubScreen.cs              # Main hub (replaces HubPage)
│   ├── CombatScreen.cs           # Turn-based combat (replaces MainPage)
│   └── [Future screens...]
├── Sprites/
│   └── [Sprite classes...]
└── Content/
    └── [Art assets, fonts...]
```

## Integration with Core

This project uses **100% of the existing backend**:

### Services Used:
- ✅ `CombatEngine` - Combat logic
- ✅ `DiceService` - Dice rolling
- ✅ `ProgressionService` - Leveling and XP
- ✅ `CurrencyService` - Gold, Gems, etc.
- ✅ `StoreService` - In-game store
- ✅ `EventService` - Events and quests
- ✅ `DailyRewardService` - Daily rewards

### Models Used:
- ✅ `GameSession` - Game state
- ✅ `Character` - Player characters
- ✅ `Party` - Party management
- ✅ All combat, equipment, skill models

## Key Features

### Current Implementation:
- **HubScreen**: Main navigation hub with currency display, daily rewards
- **CombatScreen**: Turn-based combat with procedural sprite rendering
- **Screen Management**: Push/pop navigation system
- **Service Integration**: Full dependency injection with Core services

### Rendering Approach:
- Procedural sprites (for POC)
- Can be replaced with actual sprite sheets/textures
- Uses MonoGame's SpriteBatch for efficient 2D rendering

## How to Run

```bash
cd DungeonPartyGame.MonoGame
dotnet run
```

### Controls:
- **Mouse**: Click buttons to navigate
- **ESC**: Exit application

## Screens

### 1. HubScreen
- Displays player currency (Gold, Gems)
- Navigation buttons to:
  - ⚔️ Combat
  - 👥 Party
  - 🎯 Skills
  - ⚔️ Gear
  - 🏪 Store
- Daily reward system

### 2. CombatScreen
- Turn-based combat interface
- Character sprites with health bars
- Damage numbers with animations
- Combat log display
- **Uses existing CombatEngine service**

### Future Screens:
- PartyScreen
- SkillTreeScreen
- GearScreen
- StoreScreen
- EventsScreen

## Adding Custom Art Assets

To replace procedural rendering with actual sprites:

1. **Add sprites to Content folder**
2. **Set up MonoGame Content Pipeline**:
   ```xml
   <ItemGroup>
     <Content Include="Content/**/*.*">
       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     </Content>
   </ItemGroup>
   ```
3. **Load in LoadContent()**:
   ```csharp
   var fighterTexture = Content.Load<Texture2D>("Sprites/Fighter");
   ```
4. **Render with SpriteBatch**:
   ```csharp
   SpriteBatch.Draw(fighterTexture, position, Color.White);
   ```

## Fonts

Currently using fallback rectangle rendering for text. To add custom fonts:

1. Create `.spritefont` files in `Content/Fonts/`
2. Load in screen's `LoadContent()`:
   ```csharp
   _titleFont = Game.Content.Load<SpriteFont>("Fonts/TitleFont");
   ```
3. Text will automatically render with loaded font

## Advantages over MAUI

1. **Full control** over rendering pipeline
2. **Better performance** for game-like UI
3. **Easier sprite management** and animations
4. **Cross-platform** (Desktop, Mobile, Console)
5. **Mature 2D game framework** (used by Stardew Valley, Celeste, Terraria)

## Backend Compatibility

**Zero changes to Core required!**

All game logic remains in `DungeonPartyGame.Core`:
- Combat calculations
- Progression systems
- Currency management
- Store logic
- Event systems

MonoGame is **purely presentational** - it calls Core services and displays results.

## Testing

All Core services are tested in `DungeonPartyGame.Tests`. The MonoGame UI doesn't require special testing since it has no business logic - it's all in Core.

## Next Steps

1. Add proper sprite assets (commissioned art or placeholder sprites)
2. Implement remaining screens (Party, Skills, Gear, Store, Events)
3. Add particle effects for combat
4. Add sound effects and music
5. Implement sprite sheet animations
6. Add touch controls for mobile deployment
