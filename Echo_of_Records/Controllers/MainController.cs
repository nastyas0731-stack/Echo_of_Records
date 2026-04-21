using Echo_of_Records.Models;
using Echo_of_Records.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Echo_of_Records.Controllers
{
    public enum GameState { MainMenu, Playing, Pause, GameOver }

    public class LightRift
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Angle { get; set; }
        public float CurrentAlpha { get; private set; } = 1.0f;

        private float _timer = 0;
        private Random _rnd = new Random();
        private float _driftOffset = 0;

        public LightRift(float x, float y, float w, float h, float angle)
        {
            X = x; Y = y; Width = w; Height = h; Angle = angle;
        }

        public void Update()
        {
            _timer += 0.15f;

            float baseAlpha = 0.4f + (float)Math.Abs(Math.Sin(_timer)) * 0.4f;

            if (_rnd.Next(100) > 90)
                CurrentAlpha = (float)_rnd.NextDouble() * 0.3f;
            else
                CurrentAlpha = baseAlpha;

            //ГОРИЗОНТАЛЬНОЕ ДРОЖАНИЕ
            _driftOffset = (float)Math.Sin(_timer * 2f) * 10f;
        }

        public GraphicsPath GetPath()
        {
            GraphicsPath path = new GraphicsPath();
            float halfW = Width / 2f;

            // Добавляем дрифт к координате X для эффекта нестабильности
            float currentX = X + _driftOffset;

            PointF[] pts = new PointF[]
            {
            new PointF(currentX - halfW, Y),
            new PointF(currentX + halfW, Y),
            new PointF(currentX + halfW, Y + Height),
            new PointF(currentX - halfW, Y + Height)
            };

            if (Angle != 0)
            {
                using (Matrix m = new Matrix())
                {
                    // Добавляем небольшое "качание" угла
                    float dynamicAngle = Angle + (float)Math.Sin(_timer * 0.5f) * 2f;
                    m.RotateAt(dynamicAngle, new PointF(currentX, Y));
                    m.TransformPoints(pts);
                }
            }
            path.AddPolygon(pts);
            return path;
        }

        public void Draw(Graphics g)
        {
            using (var path = GetPath())
            {
                RectangleF bounds = path.GetBounds();
                if (bounds.Width <= 0) return;

                using (LinearGradientBrush lgb = new LinearGradientBrush(
                    new PointF(bounds.Left, 0),
                    new PointF(bounds.Right, 0),
                    Color.Transparent,
                    Color.Transparent))
                {
                    Color blendColor = Color.FromArgb((int)(255 * CurrentAlpha), Color.White);

                    ColorBlend cb = new ColorBlend();
                    cb.Colors = new Color[] {
                Color.Transparent,
                blendColor,
                blendColor,
                Color.Transparent
            };

                    cb.Positions = new float[] { 0f, 0.15f, 0.85f, 1f };
                    lgb.InterpolationColors = cb;

                    // Рисуем сам луч
                    g.FillPath(lgb, path);

                    // Добавляем "ядро" — еще более плотную полоску в самом центре для сочности
                    using (Pen corePen = new Pen(Color.FromArgb((int)(100 * CurrentAlpha), Color.White), bounds.Width * 0.3f))
                    {
                        float centerX = bounds.Left + bounds.Width / 2f;
                        g.DrawLine(corePen, centerX, bounds.Top, centerX, bounds.Bottom);
                    }
                }
            }
        }
    }
    public class MainController
    {
        public LightSource Light { get; private set; }
        public Player Player { get; private set; }
        public LevelManager LevelManager { get; private set; }
        public GameState State { get; set; } = GameState.MainMenu;

        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        public PointF FinishPoint { get; set; } = new PointF(1850, 500);
        public List<Rectangle> Obstacles => LevelManager.GetCurrentLevel()?.Obstacles ?? new List<Rectangle>();
        public List<MovingObstacle> MovingPlatforms => LevelManager.GetCurrentLevel()?.MovingPlatforms ?? new List<MovingObstacle>();
        public List<LightRift> Rifts { get; set; } = new List<LightRift>();

        public float CandleLife { get; set; } = 100f;

        public MainController()
        {
            LevelManager = new LevelManager();
            var level = LevelManager.GetCurrentLevel();
            Light = new LightSource(400, 300);
            Player = (level != null) ? new Player(level.SpawnPoint.X, level.SpawnPoint.Y) : new Player(100, 500);

            InitializeRifts();
        }

        private void InitializeRifts()
        {
            Rifts.Clear();
            Rifts.Add(new LightRift(500, -100, 80, 1500, -5));
            Rifts.Add(new LightRift(1000, -100, 100, 1500, 3));
            Rifts.Add(new LightRift(1500, -100, 120, 1500, 12));
        }

        public void KeyUp(Keys key) => pressedKeys.Remove(key);
        public void UpdateLightPosition(float x, float y) { Light.X = (int)x; Light.Y = (int)y; }

        private bool CheckIfInLightRift()
        {
            PointF p = new PointF(Player.Position.X + Player.Width / 2, Player.Position.Y + Player.Height / 2);
            foreach (var rift in Rifts)
            {
                using (var path = rift.GetPath())
                {
                    if (IsPointInPolygon(path.PathPoints, p)) return true;
                }
            }
            return false;
        }

        public void Update()
        {
            var currentLevel = LevelManager.GetCurrentLevel();
            if (currentLevel != null)
            {
                if (Player.Bounds.IntersectsWith(currentLevel.FinishZone))
                {
                    // Идем на следующий уровень
                    LevelManager.NextLevel();

                    ResetToLevelSpawn();

                    return; // Выходим из Update, чтобы начать новый уровень с чистого листа
                }
            }

            if (State != GameState.Playing) return;

            // 1. Двигаем платформы
            foreach (var platform in MovingPlatforms) platform.Update();

            // 2. Проверка падения за экран (Респаун)
            if (Player.Position.Y > 1200) ResetToLevelSpawn();

            foreach (var rift in Rifts) rift.Update();

            CandleLife -= 0.05f;
            if (CandleLife <= 0) ResetToLevelSpawn();

            bool isInShadow = CheckIfFeetInShadow();
            bool isInRift = CheckIfInLightRift();

            if (isInShadow)
            {
                if (Player.VelocityY > 8.0f) Player.VelocityY = 8.0f;
                Player.CurrentAlpha = Math.Min(1.0f, Player.CurrentAlpha + 0.05f);
                ApplyNormalGravity();
            }
            else if (isInRift)
            {
                CandleLife -= 0.8f;
                Player.CurrentAlpha = Math.Max(0.2f, Player.CurrentAlpha - 0.03f);
                ApplyNormalGravity();
            }
            else
            {
                Player.CurrentAlpha = Math.Max(0.2f, Player.CurrentAlpha - 0.01f);
                ApplyNormalGravity();
            }

            Player.Position = new PointF(Player.Position.X, Player.Position.Y + Player.VelocityY);
            HandleHorizontalMovement();
            HandleCollisions();
            Player.Update();
        }

        private void ApplyNormalGravity() => Player.VelocityY += Player.Gravity;

        private void HandleHorizontalMovement()
        {
            float moveX = 0;
            if (pressedKeys.Contains(Keys.A)) moveX -= Player.Speed * 1.3f;
            if (pressedKeys.Contains(Keys.D)) moveX += Player.Speed * 1.3f;
            Player.Position = new PointF(Player.Position.X + moveX, Player.Position.Y);
            if (moveX != 0) CheckWallCollision(moveX > 0 ? 1 : -1);
        }

        private void HandleCollisions()
        {
            Player.IsGrounded = false;
            Rectangle feet = new Rectangle((int)Player.Position.X + 40, (int)Player.Position.Y + (int)Player.Height - 15, (int)Player.Width - 80, 20);

            // Проверка обычных препятствий
            foreach (var obs in Obstacles)
            {
                if (feet.IntersectsWith(obs) && Player.VelocityY >= 0)
                {
                    Player.Position = new PointF(Player.Position.X, obs.Top - Player.Height + 10);
                    Player.VelocityY = 0; Player.IsGrounded = true; return;
                }
            }

            // 3. Коллизия с движущимися книгами-лифтами
            foreach (var mp in MovingPlatforms)
            {
                Rectangle platformRect = Rectangle.Round(mp.Bounds);
                if (feet.IntersectsWith(platformRect) && Player.VelocityY >= 0)
                {
                    Player.Position = new PointF(Player.Position.X, platformRect.Top - Player.Height + 10);
                    Player.VelocityY = 0; Player.IsGrounded = true; return;
                }
            }
        }

        private bool CheckIfFeetInShadow()
        {
            var level = LevelManager.GetCurrentLevel();
            if (level == null) return false;

            // Точка под ногами духа для проверки
            PointF pt = new PointF(Player.Position.X + Player.Width / 2, Player.Position.Y + Player.Height - 5);

            // 1. Сначала объединяем все объекты, которые могут отбрасывать тень
            // Берем обычные полки и добавляем к ним прямоугольники движущихся книг
            var allShadowCasters = level.Obstacles
                .Concat(MovingPlatforms.Select(p => Rectangle.Round(p.Bounds)))
                .ToList();

            foreach (var rect in allShadowCasters)
            {
                // Проверяем, стоит ли дух прямо внутри книги 
                if (rect.Contains((int)pt.X, (int)pt.Y)) return true;

                // Генерируем тень для каждого объекта (включая движущиеся)
                var poly = ShadowEngine.GetShadowPolygon(rect, Light, 3000, 3000);

                if (poly != null && poly.Length > 2)
                {
                    if (IsPointInPolygon(poly, pt)) return true;
                }
            }
            return false;
        }

        private bool IsPointInPolygon(PointF[] polygon, PointF testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if ((polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y) || (polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y))
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                        result = !result;
                }
                j = i;
            }
            return result;
        }

        private void CheckWallCollision(int dir)
        {
            foreach (var obs in Obstacles)
                if (Rectangle.Round(Player.Bounds).IntersectsWith(obs))
                    Player.Position = new PointF(Player.Position.X - dir, Player.Position.Y);
        }

        public void ResetToLevelSpawn()
        {
            var l = LevelManager.GetCurrentLevel();
            if (l == null) return;
            Player.Position = new PointF(l.SpawnPoint.X, l.SpawnPoint.Y);
            Player.VelocityY = 0;
            Player.CurrentAlpha = 1.0f;
            CandleLife = 100f;
        }

        public void KeyDown(Keys key)
        {
            if (!pressedKeys.Contains(key)) pressedKeys.Add(key);
            if (key == Keys.Space)
            {
                if (Player.IsGrounded) { Player.VelocityY = Player.JumpForce; Player.IsGrounded = false; }
                else if (CheckIfFeetInShadow()) Player.VelocityY = Player.JumpForce * 0.7f;
            }
        }
    }
}