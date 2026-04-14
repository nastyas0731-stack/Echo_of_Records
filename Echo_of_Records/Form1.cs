using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Echo_of_Records.Controllers;
using Echo_of_Records.Models;
using Echo_of_Records.Utils;

namespace Echo_of_Records
{
    public partial class Form1 : Form
    {
        private MainController _controller;
        private Image? ghostSprite;
        private Image? backgroundImage;
        private Image? bookSprite;
        private System.Windows.Forms.Timer _gameTimer;
        private List<PointF> dustParticles = new List<PointF>();
        private Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();

            // 1. Сначала создаем контроллер
            _controller = new MainController();

            // 2. Загружаем картинки
            LoadResources();

            // Настройка графики
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            this.WindowState = FormWindowState.Maximized;

            // Частицы пыли для атмосферы
            for (int i = 0; i < 100; i++)
                dustParticles.Add(new PointF(rnd.Next(2000), rnd.Next(2000)));

            // События клавиш
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.KeyPreview = true;

            // Игровой цикл
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 20;
            _gameTimer.Tick += (s, e) => {
                _controller.Update();
                this.Invalidate();
            };
            _gameTimer.Start();
        }

        private void LoadResources()
        {
            try
            {
                // Проверка духа (пробуем оба расширения)
                if (System.IO.File.Exists("ghost.png")) ghostSprite = Image.FromFile("ghost.png");
                else if (System.IO.File.Exists("ghost.jpg")) ghostSprite = Image.FromFile("ghost.jpg");

                // Проверка фона
                if (System.IO.File.Exists("background.jpg")) backgroundImage = Image.FromFile("background.jpg");
                else if (System.IO.File.Exists("background.png")) backgroundImage = Image.FromFile("background.png");

                // Проверка книги
                if (System.IO.File.Exists("book_platform.jpg")) bookSprite = Image.FromFile("book_platform.jpg");
                else if (System.IO.File.Exists("book_platform.png")) bookSprite = Image.FromFile("book_platform.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки файлов: " + ex.Message);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. ФОН
            if (backgroundImage != null)
                g.DrawImage(backgroundImage, 0, 0, this.Width, this.Height);

            var currentLevel = _controller.LevelManager.GetCurrentLevel();

            // --- 2. ТЕНИ ОТ ПЛАТФОРМ ---
            foreach (var rect in currentLevel.Obstacles)
            {
                // Делаем объект для тени чуть-чуть меньше самой книги (на 5 пикселей с краев)
                
                var shadowObs = new Obstacle(rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 5);
                ShadowEngine.DrawShadow(g, _controller.Light, shadowObs);
            }

            // 3. АТМОСФЕРА
            DrawAtmosphere(g, _controller.Light);

            // --- 4. КНИГИ-ПЛАТФОРМЫ ---
            foreach (var rect in currentLevel.Obstacles)
            {
                DrawBookGlow(g, rect); // Синее свечение

                if (bookSprite != null)
                {
                    using (var attr = new System.Drawing.Imaging.ImageAttributes())
                    {
                        // Вырезаем серый фон JPG
                        attr.SetColorKey(Color.FromArgb(180, 180, 180), Color.White);

                        // РИСУЕМ СТРОГО В RECT
                        // Теперь картинка книги идеально совпадает с тем, где мы стоим
                        g.DrawImage(bookSprite, rect, 0, 0, bookSprite.Width, bookSprite.Height, GraphicsUnit.Pixel, attr);
                    }
                }
            }

            // 5. ПЕРСОНАЖ (ПОВЕРХ КНИГ)
            DrawPlayer(g);

            // 6. СВЕЧА
            g.FillEllipse(Brushes.Orange, _controller.Light.X - 6, _controller.Light.Y - 6, 12, 12);
        }
        private void DrawPlayer(Graphics g)
        {
            var p = _controller.Player;

            // Если дух пропал, рисуем розовый квадрат, чтобы знать, где он
            if (ghostSprite == null)
            {
                g.FillRectangle(Brushes.HotPink, p.Position.X, p.VisualY, p.Width, p.Height);
                return;
            }

            int glitchX = rnd.Next(0, 100) > 97 ? rnd.Next(-1, 1) : 0;
            Rectangle destRect = new Rectangle((int)p.Position.X + glitchX, (int)p.VisualY + 15, (int)p.Width, (int)p.Height);

            // Аура духа
            using (GraphicsPath auraPath = new GraphicsPath())
            {
                float dynamicScale = 0.5f + (p.CurrentAlpha * 0.6f);
                float auraW = destRect.Width * dynamicScale;
                float auraH = destRect.Height * dynamicScale;
                auraPath.AddEllipse(destRect.X - (auraW - destRect.Width) / 2, destRect.Y - (auraH - destRect.Height) / 2, auraW, auraH);
                using (PathGradientBrush pgb = new PathGradientBrush(auraPath))
                {
                    pgb.CenterColor = Color.FromArgb((int)(120 * p.CurrentAlpha), 180, 220, 255);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, auraPath);
                }
            }

            // Сама картинка духа
            using (var attr = new System.Drawing.Imaging.ImageAttributes())
            {
                attr.SetColorKey(Color.Black, Color.FromArgb(30, 30, 30));
                var matrix = new System.Drawing.Imaging.ColorMatrix { Matrix33 = Math.Max(0.1f, p.CurrentAlpha) };
                attr.SetColorMatrix(matrix);
                g.DrawImage(ghostSprite, destRect, 0, 0, ghostSprite.Width, ghostSprite.Height, GraphicsUnit.Pixel, attr);
            }
        }

        private void DrawBookGlow(Graphics g, Rectangle rect)
        {
            using (GraphicsPath glowPath = new GraphicsPath())
            {
                RectangleF glowArea = new RectangleF(rect.X - 20, rect.Y + rect.Height - 10, rect.Width + 40, 30);
                glowPath.AddEllipse(glowArea);
                using (PathGradientBrush pgb = new PathGradientBrush(glowPath))
                {
                    pgb.CenterColor = Color.FromArgb(100, 100, 200, 255);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, glowPath);
                }
            }
        }

        private void DrawAtmosphere(Graphics g, LightSource light)
        {
            float radius = 280;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(light.X - radius, light.Y - radius, radius * 2, radius * 2);
                using (PathGradientBrush pgb = new PathGradientBrush(path))
                {
                    pgb.CenterPoint = new PointF(light.X, light.Y);
                    pgb.CenterColor = Color.FromArgb(130, 255, 240, 180);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }

            foreach (var p in dustParticles)
            {
                float dist = (float)Math.Sqrt(Math.Pow(p.X - light.X, 2) + Math.Pow(p.Y - light.Y, 2));
                if (dist < radius)
                {
                    int alpha = (int)(150 * (1 - dist / radius));
                    using (SolidBrush dustBrush = new SolidBrush(Color.FromArgb(alpha, Color.WhiteSmoke)))
                        g.FillEllipse(dustBrush, p.X, p.Y, 2, 2);
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) => _controller.KeyDown(e.KeyCode);
        private void Form1_KeyUp(object sender, KeyEventArgs e) => _controller.KeyUp(e.KeyCode);
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _controller.UpdateLightPosition(e.X, e.Y);
        }
    }
}