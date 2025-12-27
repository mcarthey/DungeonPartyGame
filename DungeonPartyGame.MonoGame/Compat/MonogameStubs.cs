
using System;

namespace Microsoft.Xna.Framework
{
    public class Game : IDisposable
    {
        public Game() { }
        public virtual void Initialize() { }
        public virtual void LoadContent() { }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(GameTime gameTime) { }
        public Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice { get; } = new Microsoft.Xna.Framework.Graphics.GraphicsDevice();
        protected void OnInitialized() { }

        // Basic properties commonly used in MonoGame
        public Content.ContentManager Content { get; } = new Content.ContentManager();
        public bool IsMouseVisible { get; set; }

        public void Run() { /* no-op in stubs */ }
        public void Exit() { /* no-op */ }

        public void Dispose() { }
    }

    public struct GameTime { public TimeSpan ElapsedGameTime { get; set; } public TimeSpan TotalGameTime { get; set; } }
    public struct Vector2 { public float X; public float Y; public Vector2(float x, float y) { X = x; Y = y; } public static Vector2 Zero => new Vector2(0,0); public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y); public static Vector2 operator *(Vector2 a, float f) => new Vector2(a.X * f, a.Y * f); public static Vector2 operator +(Vector2 a, float f) => new Vector2(a.X + f, a.Y + f); public static Vector2 operator /(Vector2 a, float f) => new Vector2(a.X / f, a.Y / f); }
    public struct Rectangle { public int X; public int Y; public int Width; public int Height; public Rectangle(int x, int y, int w, int h) { X = x; Y = y; Width = w; Height = h; } public int Bottom => Y + Height; public int Right => X + Width; public Microsoft.Xna.Framework.Vector2 Center => new Microsoft.Xna.Framework.Vector2(X + Width / 2f, Y + Height / 2f); public bool Contains(Microsoft.Xna.Framework.Input.Point p) => p.X >= X && p.X <= Right && p.Y >= Y && p.Y <= Bottom; }
    public struct Color { public byte R; public byte G; public byte B; public byte A; public Color(byte r, byte g, byte b, byte a = 255) { R = r; G = g; B = b; A = a; } 
    public static Color CornflowerBlue => new Color(100,149,237);
    public static Color White => new Color(255,255,255);
    public static Color Gold => new Color(255,215,0);
    public static Color DarkRed => new Color(139,0,0);
    public static Color DarkBlue => new Color(0,0,139);
    public static Color DarkGreen => new Color(0,100,0);
    public static Color DarkOrange => new Color(255,140,0);
    public static Color Purple => new Color(128,0,128);
    public static Color Orange => new Color(255,165,0);
    public static Color Red => new Color(255,0,0);
    public static Color LightGreen => new Color(144,238,144);
    public static Color Green => new Color(0,128,0);
    public static Color Gray => new Color(128,128,128);
    public static Color Cyan => new Color(0,255,255);
    public static Color Black => new Color(0,0,0);
    public static Color DarkSlateGray => new Color(47,79,79);
    public static Color Silver => new Color(192,192,192);
    public static Color DarkOliveGreen => new Color(85,107,47);
    public static Color LimeGreen => new Color(50,205,50);
    public static Color WhiteSmoke => new Color(245,245,245);
    public static Color Gold2 => new Color(255,215,0);
    public static Color White2 => new Color(255,255,255);
    public static Color Cornflower => new Color(100,149,237);
    public static Color Farther => new Color(0,0,0);
    public static Color Lerp(Color a, Color b, float t) => a; 
    public static Color operator *(Color c, float f) => c;
    public static Color operator *(float f, Color c) => c;
    public static Color operator *(Color a, Color b) => a;
    public static Color operator +(Color a, Color b) => a; }
}

namespace Microsoft.Xna.Framework.Content
{
    public class ContentManager
    {
        public string RootDirectory { get; set; } = "Content";
        public T Load<T>(string assetName) where T : class => null!;
    }
}

namespace Microsoft.Xna.Framework.Graphics
{
    public class SpriteBatch
    {
        public SpriteBatch(object device) { }
        public void Begin() { }
        public void End() { }
        public void Draw(object texture, Microsoft.Xna.Framework.Vector2 position, object color) { }
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color) { }
        public void DrawString(SpriteFont spriteFont, string text, Microsoft.Xna.Framework.Vector2 position, Color color) { }
        public void DrawString(SpriteFont spriteFont, string text, Microsoft.Xna.Framework.Vector2 position, Color color, float rotation, Microsoft.Xna.Framework.Vector2 origin, float scale, SpriteEffects effects, float layerDepth) { }
    }

    public class GraphicsDevice { 
        public Viewport Viewport { get; } = new Viewport();
        public void Clear(Color color) { }
    }

    public class GraphicsDeviceManager
    {
        public GraphicsDeviceManager(Microsoft.Xna.Framework.Game game) { }
        public int PreferredBackBufferWidth { get; set; }
        public int PreferredBackBufferHeight { get; set; }
    }

    public class Texture2D : IDisposable { 
        public Texture2D(GraphicsDevice device, int width, int height) { }
        public void SetData<T>(T[] data) { }
        public void Dispose() { }
    }

    public class SpriteFont {
        public Microsoft.Xna.Framework.Vector2 MeasureString(string text) => new Microsoft.Xna.Framework.Vector2(0,0);
    }

    public struct Viewport { public int Width { get; set; } public int Height { get; set; } }
}

namespace Microsoft.Xna.Framework.Input
{
    public enum ButtonState { Released = 0, Pressed = 1 }
    public struct KeyboardState { public bool IsKeyDown(Keys key) => false; }
    public struct MouseState { public ButtonState LeftButton { get; set; } public int X { get; set; } public int Y { get; set; } }

    public static class Keyboard { public static KeyboardState GetState() => new KeyboardState(); }
    public static class Mouse { public static MouseState GetState() => new MouseState(); }

    public enum Keys { Escape }
    public struct Point { public int X; public int Y; public Point(int x, int y) { X = x; Y = y; } }
}

namespace Microsoft.Xna.Framework.Graphics
{
    public enum SpriteEffects { None }
}
