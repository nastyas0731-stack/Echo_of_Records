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
        public PointF Position; // Координаты (центр или ноги)
        public float VelocityY = 0; // Скорость падения
        public const float Speed = 5f; // Скорость бега
        public const float JumpForce = -12f; // Сила прыжка
        public const float Gravity = 0.8f; // Сила притяжения

        public bool IsGrounded = false; // Стоит ли на тени?
        public float Width = 20; // Размеры для отрисовки
        public float Height = 30;

        public Player(float x, float y)
        {
            Position = new PointF(x, y);
        }
    }
}
