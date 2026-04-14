using Echo_of_Records.Models;
using Echo_of_Records.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Echo_of_Records.Controllers
{
    public class MainController
    {
        public LightSource Light { get; private set; }
        public Player Player { get; private set; }
        public LevelManager LevelManager { get; private set; }
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private float _glimmerAngle = 0; // Для пульсации яркости
        private float _waveAngle = 0;

        public List<Rectangle> Obstacles => LevelManager.GetCurrentLevel().Obstacles;

        public MainController()
        {
            LevelManager = new LevelManager();
            var level = LevelManager.GetCurrentLevel();
            Light = new LightSource(400, 300);
            Player = new Player(level.SpawnPoint.X, level.SpawnPoint.Y);
        }

        public void KeyDown(Keys key) => pressedKeys.Add(key);
        public void KeyUp(Keys key) => pressedKeys.Remove(key);

        public void UpdateLightPosition(float x, float y)
        {
            Light.X = (int)x;
            Light.Y = (int)y;
        }

        public void Update()
        {
            var currentLevel = LevelManager.GetCurrentLevel();

            // --- 1. ГОРИЗОНТАЛЬНОЕ ДВИЖЕНИЕ (X) ---
            if (pressedKeys.Contains(Keys.A))
            {
                Player.Position.X -= Player.Speed;
                CheckWallCollision(-1);
            }
            if (pressedKeys.Contains(Keys.D))
            {
                Player.Position.X += Player.Speed;
                CheckWallCollision(1);
            }

            // --- 2. ВЕРТИКАЛЬНОЕ ДВИЖЕНИЕ (Y) ---
            Player.VelocityY += Player.Gravity;
            Player.Position.Y += Player.VelocityY;
            Player.IsGrounded = false;

            Rectangle playerBounds = Player.Bounds;

            foreach (var obstacle in Obstacles)
            {
                if (playerBounds.IntersectsWith(obstacle))
                {
                    if (Player.VelocityY > 0) // Падение на книгу
                    {
                        Player.Position.Y = obstacle.Top - Player.Height + 5;
                        Player.VelocityY = 0;
                        Player.IsGrounded = true;
                    }
                    else if (Player.VelocityY < 0) // Удар головой
                    {
                        Player.Position.Y = obstacle.Bottom - 5;
                        Player.VelocityY = 0;
                    }
                }
            }

            // --- 3. ПРИЗЕМЛЕНИЕ НА ТЕНЬ ---
            if (!Player.IsGrounded)
            {
                if (CheckIfFeetInShadow(0) || CheckIfFeetInShadow(8))
                {
                    for (int i = 0; i < 15; i++)
                    {
                        if (CheckIfFeetInShadow(0)) Player.Position.Y -= 1;
                        else break;
                    }
                    Player.VelocityY = 0;
                    Player.IsGrounded = true;
                }
            }

            // Внутри метода Update() контроллера
            float targetAlpha = 0.2f; // Объявляем ОДИН РАЗ в начале

            // Ошибка CS0128 лечится удалением повторного 'bool'
            bool onShadow = CheckIfFeetInShadow(0) || CheckIfFeetInShadow(5);

            if (Player.IsGrounded && onShadow)
            {
                targetAlpha = 1.0f;
            }
            else if (!Player.IsGrounded)
            {
                targetAlpha = 0.4f;
            }

            // Теперь targetAlpha точно существует и видна здесь
            Player.CurrentAlpha += (targetAlpha - Player.CurrentAlpha) * 0.15f;

            // ДОБАВЛЯЕМ МЕРЦАНИЕ (Glimmer)
            _glimmerAngle += 0.15f;
            float glimmer = (float)Math.Sin(_glimmerAngle) * 0.07f; // Амплитуда мерцания

            Player.CurrentAlpha += glimmer;

            // Ограничители (чтобы дух не исчез совсем и не стал "вырвиглазным")
            if (Player.CurrentAlpha < 0.1f) Player.CurrentAlpha = 0.1f;
            if (Player.CurrentAlpha > 1.0f) Player.CurrentAlpha = 1.0f;

            // --- 4. ПРЫЖОК ---
            if (Player.IsGrounded && pressedKeys.Contains(Keys.Space))
            {
                Player.VelocityY = Player.JumpForce;
                Player.Position.Y -= 5; // Уменьшил рывок, чтобы не было резкого скачка яркости
                Player.IsGrounded = false;
            }

            // --- 5. ЭФФЕКТ ПАРЕНИЯ ---
            _waveAngle += 0.08f;
            Player.VisualY = Player.Position.Y + (float)Math.Sin(_waveAngle) * 4;

            // Проверки выхода и бездны
            if (Player.Bounds.IntersectsWith(currentLevel.ExitGate))
            {
                LevelManager.NextLevel();
                ResetToLevelSpawn();
            }
            if (Player.Position.Y > 1100) ResetToLevelSpawn();
        }

        private void CheckWallCollision(int direction)
        {
            foreach (var obstacle in Obstacles)
            {
                // Если застряли в стене — выталкиваем в обратную сторону
                while (Player.Bounds.IntersectsWith(obstacle))
                {
                    Player.Position.X -= direction;
                }
            }
        }

        private bool CheckIfFeetInShadow(float offset)
        {
            // Точка проверки — середина ног персонажа
            PointF checkPoint = new PointF(Player.Position.X + Player.Width / 2, Player.Position.Y + Player.Height + offset);
            foreach (var rect in Obstacles)
            {
                var tempObs = new Obstacle(rect.X, rect.Y, rect.Width, rect.Height);
                var polys = ShadowEngine.GetShadowPolygons(Light, tempObs);
                foreach (var poly in polys)
                {
                    if (Physics.IsPointInPolygon(checkPoint, poly)) return true;
                }
            }
            return false;
        }

        private void ResetToLevelSpawn()
        {
            var level = LevelManager.GetCurrentLevel();
            Player.Position = new PointF(level.SpawnPoint.X, level.SpawnPoint.Y);
            Player.VelocityY = 0;
            Player.IsGrounded = false;
            Player.CurrentAlpha = 1.0f;
        }
    }
}