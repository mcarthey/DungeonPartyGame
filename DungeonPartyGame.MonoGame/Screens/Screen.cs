using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DungeonPartyGame.MonoGame.Screens;

/// <summary>
/// Base class for all game screens (Hub, Combat, Party, etc.)
/// </summary>
public abstract class Screen
{
    protected DungeonPartyGameMain Game { get; private set; }
    protected SpriteBatch SpriteBatch { get; private set; }
    protected GraphicsDevice GraphicsDevice { get; private set; }

    public bool IsActive { get; set; } = true;
    public bool IsVisible { get; set; } = true;

    public virtual void Initialize(DungeonPartyGameMain game)
    {
        Game = game;
        SpriteBatch = game.SpriteBatch;
        GraphicsDevice = game.GraphicsDevice;
    }

    public virtual void LoadContent()
    {
        // Override to load screen-specific content
    }

    public virtual void UnloadContent()
    {
        // Override to unload screen-specific content
    }

    public virtual void Update(GameTime gameTime)
    {
        // Override to update screen logic
    }

    public virtual void Draw(GameTime gameTime)
    {
        // Override to draw screen
    }

    public virtual void HandleInput(KeyboardState keyboardState, MouseState mouseState)
    {
        // Override to handle input
    }
}
