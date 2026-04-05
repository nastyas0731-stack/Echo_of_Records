using Echo_of_Records.Models;
using Echo_of_Records.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Echo_of_Records.Controllers
{
    public class MainController
    {
        public LightSource Light { get; private set; }
        public List<Obstacle> Obstacles { get; private set; }
        public Player Player { get; private set; }
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();

        public MainController()
        {
            Light = new LightSource(400, 300);
            Obstacles = new List<Obstacle>();
            Player = new Player(400, 100);

            // Тестовые объекты (книги/препятствия)
            Obstacles.Add(new Obstacle(200, 200, 120, 40));
            Obstacles.Add(new Obstacle(500, 350, 60, 180));
        }

        public void KeyDown(Keys key) => pressedKeys.Add(key);
        public void KeyUp(Keys key) => pressedKeys.Remove(key);

        public void UpdateLightPosition(float x, float y)
        {
            Light.X = x;
            Light.Y = y;
        }

        public void Update()
        {
            // 1. Горизонтальное движение с проверкой столкновений
            float nextX = Player.Position.X;
            if (pressedKeys.Contains(Keys.A)) nextX -= Player.Speed;
            if (pressedKeys.Contains(Keys.D)) nextX += Player.Speed;

            RectangleF futureRect = new RectangleF(nextX, Player.Position.Y, Player.Width, Player.Height);
            bool wallCollision = false;
            foreach (var obs in Obstacles)
            {
                if (futureRect.IntersectsWith(obs.Rect)) { wallCollision = true; break; }
            }
            if (!wallCollision) Player.Position.X = nextX;

            // 2. Проверка состояния: стоим ли мы на тени или застряли в ней
            bool isInside = CheckIfFeetInShadow(0);  // Ноги внутри черного
            bool isNear = CheckIfFeetInShadow(10);   // Опора совсем рядом под ногами

            if (isInside)
            {
                // Эффект "всплытия": если тень наезжает на нас, поднимаем героя на поверхность
                for (int i = 0; i < 20; i++)
                {
                    if (CheckIfFeetInShadow(0))
                    {
                        Player.Position.Y -= 1; // Поднимаем по 1 пикселю до границы
                    }
                    else break;
                }

                Player.VelocityY = 0;
                Player.IsGrounded = true;
            }
            else if (isNear)
            {
                // Просто стоим на поверхности
                Player.VelocityY = 0;
                Player.IsGrounded = true;
            }
            else
            {
                // Под ногами пусто — падаем
                Player.IsGrounded = false;
                Player.VelocityY += Player.Gravity;
            }

            // 3. Прыжок
            if (Player.IsGrounded && pressedKeys.Contains(Keys.Space))
            {
                Player.VelocityY = Player.JumpForce;
                Player.Position.Y -= 15; // Принудительный отрыв от тени
                Player.IsGrounded = false;
            }

            // Применяем накопленную вертикальную скорость
            Player.Position.Y += Player.VelocityY;

            // Респаун при падении за границы экрана
            if (Player.Position.Y > 1200) ResetPosition();
        }

        // Вспомогательный метод для проверки коллизий с тенями
        private bool CheckIfFeetInShadow(float offset)
        {
            PointF checkPoint = new PointF(
                Player.Position.X + Player.Width / 2,
                Player.Position.Y + Player.Height + offset
            );

            foreach (var obs in Obstacles)
            {
                var polys = ShadowEngine.GetShadowPolygons(Light, obs);
                foreach (var poly in polys)
                {
                    // Проверь имя класса в Utils (Physics или GeometryHelper)
                    if (Physics.IsPointInPolygon(checkPoint, poly)) return true;
                }
            }
            return false;
        }

        private void ResetPosition()
        {
            Player.Position = new PointF(400, 100);
            Player.VelocityY = 0;
            Player.IsGrounded = false;
        }
    }
}