using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Echo_of_Records.Models
{
    public class Obstacle
    {
        public RectangleF Bounds { get; set; }

        public Obstacle(float x, float y, float width, float height)
        {
            Bounds = new RectangleF(x, y, width, height);
        }
    }

    public class LightRift
    {
        public RectangleF BaseBounds { get; set; }
        public float Angle { get; set; }
        public float DamageIntensity { get; set; }

        public bool IsActive { get; set; } = true;
        private float _flickerTimer = 0;
        private float _flickerSpeed = 0.5f;
        public float CurrentFlickerAlpha { get; private set; } = 1.0f;

        private static readonly Random _rnd = new Random();

        public LightRift(float x, float y, float width, float height, float angle = 0, float damage = 0.15f)
        {
            // Делаем луч "бесконечным": увеличиваем высоту и задираем Y далеко вверх
            float infiniteHeight = 4000f;
            float topOffset = -2000f;

            BaseBounds = new RectangleF(x, y + topOffset, width, infiniteHeight);
            Angle = angle;
            DamageIntensity = damage;

            // ХАОС: Даем случайную начальную фазу, чтобы лучи мигали несинхронно
            _flickerTimer = (float)(_rnd.NextDouble() * Math.PI * 2);
        }

        public void UpdateFlicker()
        {
            _flickerTimer += _flickerSpeed;

            float rawSin = (float)Math.Sin(_flickerTimer);
            CurrentFlickerAlpha = rawSin > 0.3f ? 1.0f : 0.0f;

            IsActive = CurrentFlickerAlpha > 0.5f;
        }

        public GraphicsPath GetPath()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(BaseBounds);

            using (Matrix m = new Matrix())
            {
                // Поворот вокруг "центра" базового прямоугольника
                PointF center = new PointF(
                    BaseBounds.X + BaseBounds.Width / 2,
                    BaseBounds.Y + BaseBounds.Height / 2
                );
                m.RotateAt(Angle, center);
                path.Transform(m);
            }
            return path;
        }
    }
}