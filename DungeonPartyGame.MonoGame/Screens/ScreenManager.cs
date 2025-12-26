using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DungeonPartyGame.MonoGame.Screens;

/// <summary>
/// Manages screen transitions and the active screen stack
/// </summary>
public class ScreenManager
{
    private readonly Stack<Screen> _screenStack = new();
    private readonly DungeonPartyGameMain _game;

    public Screen? CurrentScreen => _screenStack.Count > 0 ? _screenStack.Peek() : null;

    public ScreenManager(DungeonPartyGameMain game)
    {
        _game = game;
    }

    public void PushScreen(Screen screen)
    {
        // Deactivate current screen
        if (CurrentScreen != null)
        {
            CurrentScreen.IsActive = false;
        }

        screen.Initialize(_game);
        screen.LoadContent();
        _screenStack.Push(screen);
    }

    public void PopScreen()
    {
        if (_screenStack.Count > 0)
        {
            var screen = _screenStack.Pop();
            screen.UnloadContent();

            // Reactivate previous screen
            if (CurrentScreen != null)
            {
                CurrentScreen.IsActive = true;
            }
        }
    }

    public void ChangeScreen(Screen newScreen)
    {
        // Remove all screens and push the new one
        while (_screenStack.Count > 0)
        {
            PopScreen();
        }
        PushScreen(newScreen);
    }

    public void Update(GameTime gameTime)
    {
        CurrentScreen?.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        // Draw all visible screens from bottom to top
        foreach (var screen in _screenStack.Reverse())
        {
            if (screen.IsVisible)
            {
                screen.Draw(gameTime);
            }
        }
    }

    public void HandleInput(KeyboardState keyboardState, MouseState mouseState)
    {
        if (CurrentScreen?.IsActive == true)
        {
            CurrentScreen.HandleInput(keyboardState, mouseState);
        }
    }
}
