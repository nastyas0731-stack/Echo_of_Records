using System;
using System.Drawing;

namespace Echo_of_Records.Models
{
    public class Player
    {
        public PointF Position { get; set; }
        public float VelocityY { get; set; }
        public bool IsGrounded { get; set; }
        public float CurrentAlpha { get; set; } = 1.0f;
        public float VisualY { get; set; }
        public float GlowAlpha { get; set; } = 0f;

        public float Gravity { get; set; } = 1.8f;
        public float JumpForce { get; set; } = -25f;
        public float Speed { get; set; } = 15f;

        public int Width { get; set; } = 120;
        public int Height { get; set; } = 150;

        public RectangleF Bounds => new RectangleF(Position.X, Position.Y, Width, Height);

        private float _floatTimer = 0; // Для эффекта парения

        public Player(float x, float y)
        {
            Position = new PointF(x, y);
            VisualY = y;
            IsGrounded = false;
        }
        public void Update()
        {
            // Эффект легкого покачивания в воздухе
            _floatTimer += 0.1f;
            float floatOffset = IsGrounded ? 0 : (float)Math.Sin(_floatTimer) * 5;
            VisualY = Position.Y + floatOffset;

            // Логика свечения (плавное появление)
            if (CurrentAlpha < 1.0f) GlowAlpha = Math.Min(1.0f, GlowAlpha + 0.05f);
            else GlowAlpha = Math.Max(0f, GlowAlpha - 0.05f);
        }
    }
}