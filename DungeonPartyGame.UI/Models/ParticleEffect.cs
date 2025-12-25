using SkiaSharp;

namespace DungeonPartyGame.UI.Models;

public enum ParticleType
{
    Blood,
    Spark,
    Fire,
    Heal,
    Impact
}

public class Particle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public float Lifetime { get; set; } // 0.0 to 1.0
    public float MaxLifetime { get; set; } = 1.0f;
    public SKColor Color { get; set; }
    public float Size { get; set; }
    public ParticleType Type { get; set; }

    public bool IsExpired => Lifetime >= MaxLifetime;
    public float Alpha => Math.Max(0, 1.0f - Lifetime / MaxLifetime);

    public Particle(float x, float y, float vx, float vy, SKColor color, float size, ParticleType type = ParticleType.Spark)
    {
        X = x;
        Y = y;
        VelocityX = vx;
        VelocityY = vy;
        Color = color;
        Size = size;
        Type = type;
        Lifetime = 0;
        MaxLifetime = 0.8f;
    }

    public void Update(float deltaTime)
    {
        X += VelocityX * deltaTime * 60;
        Y += VelocityY * deltaTime * 60;
        VelocityY += deltaTime * 200; // Gravity
        Lifetime += deltaTime;
    }
}

public class ParticleSystem
{
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();

    public void SpawnBloodSplatter(float x, float y, int count = 15)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = (float)(_random.NextDouble() * 3 + 2);
            float vx = (float)Math.Cos(angle) * speed;
            float vy = (float)Math.Sin(angle) * speed - 2;

            _particles.Add(new Particle(x, y, vx, vy,
                new SKColor(220, 20, 20, 255),
                (float)(_random.NextDouble() * 3 + 2),
                ParticleType.Blood));
        }
    }

    public void SpawnImpactSparks(float x, float y, int count = 20)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = (float)(_random.NextDouble() * 5 + 3);
            float vx = (float)Math.Cos(angle) * speed;
            float vy = (float)Math.Sin(angle) * speed - 3;

            _particles.Add(new Particle(x, y, vx, vy,
                new SKColor(255, 200, 100, 255),
                (float)(_random.NextDouble() * 2 + 1),
                ParticleType.Spark));
        }
    }

    public void SpawnHealEffect(float x, float y, int count = 10)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = (float)(_random.NextDouble() * 2 + 1);
            float vx = (float)Math.Cos(angle) * speed;
            float vy = (float)Math.Sin(angle) * speed - 4;

            _particles.Add(new Particle(x, y, vx, vy,
                new SKColor(100, 255, 100, 255),
                (float)(_random.NextDouble() * 3 + 2),
                ParticleType.Heal));
        }
    }

    public void Update(float deltaTime)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            _particles[i].Update(deltaTime);
            if (_particles[i].IsExpired)
            {
                _particles.RemoveAt(i);
            }
        }
    }

    public void Draw(SKCanvas canvas)
    {
        foreach (var particle in _particles)
        {
            using var paint = new SKPaint
            {
                Color = particle.Color.WithAlpha((byte)(particle.Alpha * 255)),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            canvas.DrawCircle(particle.X, particle.Y, particle.Size, paint);
        }
    }

    public bool HasActiveParticles => _particles.Count > 0;

    public void Clear()
    {
        _particles.Clear();
    }
}
