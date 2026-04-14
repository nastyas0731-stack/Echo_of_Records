using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Echo_of_Records.Models
{
    public class Player
    {
        // Позиция и физика
        public PointF Position;
        public float VelocityY = 0;
        public float VisualY;

        // Константы движения
        public const float Speed = 5f;
        public const float JumpForce = -12f;
        public const float Gravity = 0.8f;

        // Состояние и размеры
        public bool IsGrounded = false;
        public float Width = 130;
        public float Height = 130;

        // Эффект тени (свечение)
        public float CurrentAlpha = 1.0f;

        // Хитбокс: специально заужен (30 пикселей с боков), чтобы не застревать в углах
        public Rectangle Bounds => new Rectangle(
            (int)Position.X + 30,
            (int)Position.Y + 10,
            (int)Width - 60,
            (int)Height - 20
        );

        public Player(float x, float y)
        {
            Position = new PointF(x, y);
            VisualY = y;
        }
    }
}