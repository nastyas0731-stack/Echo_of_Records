using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Echo_of_Records.Controllers;
using Echo_of_Records.Models;
using Echo_of_Records.Utils;
using System.Linq;
using System.Drawing.Imaging;

namespace Echo_of_Records
{
    public partial class Form1 : Form
    {
        // --- ПОЛЯ КОНТРОЛЛЕРОВ И РЕСУРСОВ ---
        private MainController _controller;
        private Bitmap? ghostBmpStandRight, ghostBmpStandLeft, ghostBmpJumpRight, ghostBmpJumpLeft;
        private Image? backgroundImage, bookSprite, candleSprite;
        private Bitmap? noiseTexture; // Текстура для эффекта "помех" или старой пленки

        // --- ИГРОВАЯ ЛОГИКА И ВИЗУАЛ ---
        private System.Windows.Forms.Timer _gameTimer;
        private List<PointF> dustParticles = new List<PointF>(); // Частицы пыли в воздухе
        private Random rnd = new Random();
        private bool _isLookingLeft = false; // Направление спрайта игрока
        private Dictionary<int, Image> _levelBackgrounds = new Dictionary<int, Image>();

        // --- ПЕРЕМЕННЫЕ АНИМАЦИИ ---
        private float _introAlpha = 0f;  // Прозрачность для плавного появления меню
        private float _waveAngle = 0f;   // Глобальный угол для математических эффектов (качание, пульсация)
        private float _portalLife = 1.0f; // Состояние входного портала (1.0 - виден, 0.0 - исчез)

        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            _controller = new MainController();

            LoadResources();            // Загрузка картинок
            GenerateNoiseTexture();     // Создание процедурного шума

            this.DoubleBuffered = true; // Защита от мерцания при рисовании
            this.WindowState = FormWindowState.Maximized;

            // Заполняем мир частицами пыли
            for (int i = 0; i < 120; i++)
                dustParticles.Add(new PointF(rnd.Next(2500), rnd.Next(1500)));

            // Подписка на события ввода
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.KeyPreview = true;

            // Игровой цикл (60 FPS примерно)
            _gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _gameTimer.Tick += (s, e) => {
                _controller.Update();    // Логика перемещений
                _waveAngle += 0.07f;     // Обновление фазы анимаций
                this.Invalidate();       // Перерисовка формы
            };
            _gameTimer.Start();
        }

        // Создает маленькую текстуру с точками для эффекта зернистости
        private void GenerateNoiseTexture()
        {
            noiseTexture = new Bitmap(250, 250);
            using (Graphics g = Graphics.FromImage(noiseTexture))
            {
                for (int i = 0; i < 1500; i++)
                {
                    int x = rnd.Next(noiseTexture.Width);
                    int y = rnd.Next(noiseTexture.Height);
                    int a = rnd.Next(5, 45);
                    noiseTexture.SetPixel(x, y, Color.FromArgb(a, Color.White));
                }
            }
        }

        private void LoadResources()
        {
            try
            {
                // Загрузка спрайтов призрака
                ghostBmpStandRight = LoadSmartBitmap("ghost.png") ?? LoadSmartBitmap("ghost.jpg");
                ghostBmpJumpRight = LoadSmartBitmap("ghost_jump.png") ?? LoadSmartBitmap("ghost_jump.jpg");

                // Создание зеркальных копий для движения влево
                if (ghostBmpStandRight != null)
                {
                    ghostBmpStandLeft = (Bitmap)ghostBmpStandRight.Clone();
                    ghostBmpStandLeft.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
                if (ghostBmpJumpRight != null)
                {
                    ghostBmpJumpLeft = (Bitmap)ghostBmpJumpRight.Clone();
                    ghostBmpJumpLeft.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }

                // Загрузка объектов окружения
                if (System.IO.File.Exists("book_platform.png")) bookSprite = Image.FromFile("book_platform.png");
                if (System.IO.File.Exists("light.png")) candleSprite = Image.FromFile("light.png");

                // Загрузка фонов уровней
                if (System.IO.File.Exists("background.jpg")) _levelBackgrounds[0] = Image.FromFile("background.jpg");
                if (System.IO.File.Exists("background1.jpg")) _levelBackgrounds[1] = Image.FromFile("background1.jpg");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка ресурсов: " + ex.Message); }
        }

        // Метод для загрузки с автоматической прозрачностью для JPG
        private Bitmap? LoadSmartBitmap(string fileName)
        {
            if (!System.IO.File.Exists(fileName)) return null;
            Bitmap bmp = new Bitmap(fileName);
            if (fileName.ToLower().EndsWith(".jpg") || fileName.ToLower().EndsWith(".jpeg"))
            {
                bmp.MakeTransparent(Color.Black);
                bmp.MakeTransparent(Color.White);
            }
            return bmp;
        }

        // ГЛАВНЫЙ МЕТОД ОТРИСОВКИ
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias; // Мягкие края объектов

            if (_controller.State == GameState.MainMenu)
            {
                Cursor.Show();
                DrawMainMenu(g);
            }
            else if (_controller.State == GameState.Playing)
            {
                Cursor.Hide();
                DrawWorld(g);
                DrawHUD(g); // Интерфейс (стабильность эфира)
            }
        }

        private void DrawMainMenu(Graphics g)
        {
            if (_introAlpha < 1.0f) _introAlpha += 0.01f;

            // Темный фон меню
            int bgVal = (int)(15 * _introAlpha);
            g.Clear(Color.FromArgb(bgVal, bgVal, bgVal + 10));

            // Наложение шума
            if (noiseTexture != null)
            {
                using (TextureBrush tb = new TextureBrush(noiseTexture))
                {
                    Matrix m = new Matrix();
                    m.Scale(3.0f, 3.0f);
                    m.Translate(rnd.Next(100), rnd.Next(100));
                    tb.Transform = m;
                    g.FillRectangle(tb, this.ClientRectangle);
                }
            }

            using (Font titleFont = new Font("Courier New", 65, FontStyle.Bold))
            using (Font buttonFont = new Font("Courier New", 26, FontStyle.Bold))
            {
                string titleText = "ECHO OF RECORDS";
                Size titleSize = TextRenderer.MeasureText(titleText, titleFont);
                float centerX = Width / 2 - titleSize.Width / 2;
                float centerY = Height / 3;

                // Эффект мерцания заголовка
                float flicker = (float)Math.Abs(Math.Sin(_waveAngle * 2.5f));
                int alphaVal = Math.Clamp((int)(255 * _introAlpha * flicker), 0, 255);

                // Глитч-эффект (красные и синие тени)
                if (rnd.Next(10) > 7)
                {
                    float shift = (float)Math.Sin(_waveAngle * 10f) * 5f;
                    using (Brush redBrush = new SolidBrush(Color.FromArgb(alphaVal / 2, Color.Red)))
                        g.DrawString(titleText, titleFont, redBrush, centerX - shift, centerY);

                    using (Brush blueBrush = new SolidBrush(Color.FromArgb(alphaVal / 2, Color.Blue)))
                        g.DrawString(titleText, titleFont, blueBrush, centerX + shift, centerY);
                }

                // Основной текст заголовка с легкой тряской
                using (Brush titleBrush = new SolidBrush(Color.FromArgb(alphaVal, Color.White)))
                {
                    float shakeX = (rnd.Next(5) == 0) ? rnd.Next(-10, 11) : 0;
                    g.DrawString(titleText, titleFont, titleBrush, centerX + shakeX, centerY);
                }

                // Отрисовка кнопки "ИССЛЕДОВАТЬ"
                Rectangle btnRect = new Rectangle(Width / 2 - 190, Height / 2 + 100, 380, 90);
                bool hovered = btnRect.Contains(PointToClient(Cursor.Position));

                if (hovered) btnRect.Offset(rnd.Next(-3, 4), rnd.Next(-3, 4));

                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    using (Pen p = new Pen(hovered ? Color.White : Color.FromArgb(120, Color.Cyan), hovered ? 3 : 1))
                    {
                        if (hovered && rnd.Next(5) == 0) p.DashStyle = DashStyle.Dash;
                        g.DrawRectangle(p, btnRect);

                        string btnText = "ИССЛЕДОВАТЬ";
                        if (hovered && rnd.Next(20) == 0) btnText = "ПОТЕРЯНО...";

                        g.DrawString(btnText, buttonFont, hovered ? Brushes.White : Brushes.Cyan, btnRect, sf);
                    }
                }
            }
        }

        private void DrawWorld(Graphics g)
        {
            var currentLevel = _controller.LevelManager.GetCurrentLevel();
            if (currentLevel == null) return;

            // 1. Отрисовка фона уровня
            int currentIdx = _controller.LevelManager.CurrentLevelIndex;
            if (_levelBackgrounds.ContainsKey(currentIdx)) g.DrawImage(_levelBackgrounds[currentIdx], 0, 0, Width, Height);
            else g.Clear(Color.FromArgb(5, 5, 15));

            // 2. Портал появления (спавн)
            DrawSpawnPortal(g, currentLevel.SpawnPoint);

            // 3. Динамические тени от всех объектов (платформы + движущиеся объекты)
            var allShadowObjects = currentLevel.Obstacles.Concat(_controller.MovingPlatforms.Select(p => Rectangle.Round(p.Bounds))).ToList();
            DrawUnifiedSoftShadow(g, allShadowObjects, _controller.Light);

            // 4. Портал выхода (Черная дыра)
            DrawBlackHole(g, _controller.FinishPoint);

            // Эффекты разломов/трещин
            DrawRiftsEffect(g);

            // 5. Отрисовка платформ-книг
            foreach (Rectangle rect in currentLevel.Obstacles) DrawPlatform(g, rect);
            foreach (var platform in _controller.MovingPlatforms) DrawPlatform(g, Rectangle.Round(platform.Bounds));

            // 6. Игрок
            DrawPlayer(g);

            // Пыль в воздухе
            DrawAtmosphere(g);

            // 7. Свеча (источник света в руках игрока/мышки)
            if (candleSprite != null)
            {
                float flick = (float)Math.Sin(_waveAngle * 5) * 5;
                Rectangle glowRect = new Rectangle((int)_controller.Light.X - 180, (int)_controller.Light.Y - 180, 360, 360);
                DrawGlow(g, glowRect, Color.LightYellow, 1.1f + flick / 100f);
                g.DrawImage(candleSprite, _controller.Light.X - 40, _controller.Light.Y - 40, 80, 80);
            }
        }

        // Эффект плавного исчезновения портала спавна при удалении игрока
        private void DrawSpawnPortal(Graphics g, PointF pos)
        {
            var p = _controller.Player;
            float dx = p.Position.X - pos.X;
            float dy = p.Position.Y - pos.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (dist < 10 && _portalLife < 0.5f) _portalLife = 1.0f;
            if (dist > 40 && _portalLife > 0) _portalLife -= 0.015f;

            if (_portalLife <= 0.05f) return;

            int baseSize = (int)(130 * _portalLife);
            if (baseSize < 5) return;

            RectangleF fogRect = new RectangleF(pos.X - baseSize / 2f, pos.Y - baseSize / 2f, baseSize, baseSize);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(fogRect);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    int alpha = (int)(160 * _portalLife);
                    brush.CenterColor = Color.FromArgb(alpha, Color.LightSlateGray);
                    brush.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(brush, path);
                }

                // Эффект "вьюги" внутри портала
                for (int i = 0; i < 6; i++)
                {
                    float angle = _waveAngle * 4 + (i * 1.5f);
                    float radius = (baseSize / 4f) + (float)Math.Sin(_waveAngle + i) * 10;
                    float px = pos.X + (float)Math.Cos(angle) * radius;
                    float py = pos.Y + (float)Math.Sin(angle) * radius;

                    using (Brush b = new SolidBrush(Color.FromArgb((int)(200 * _portalLife), Color.WhiteSmoke)))
                        g.FillEllipse(b, px, py, 3, 3);
                }
            }
        }

        // Отрисовка платформы с неоновым свечением
        private void DrawPlatform(Graphics g, Rectangle rect)
        {
            Rectangle glowRect = rect;
            glowRect.Inflate(40, 40); // Размер ауры свечения

            float pulse = (float)Math.Sin(_waveAngle * 2 + rect.X * 0.01f) * 0.15f + 0.85f;
            DrawGlow(g, glowRect, Color.Cyan, pulse);

            if (bookSprite != null) g.DrawImage(bookSprite, rect);
            else
            {
                using (Brush b = new SolidBrush(Color.FromArgb(35, 25, 20))) g.FillRectangle(b, rect);
                g.DrawRectangle(Pens.Cyan, rect);
            }
        }

        private void DrawPlayer(Graphics g)
        {
            var p = _controller.Player;

            // Дух постепенно материализуется после выхода из портала
            float appearanceFactor = 0.8f - (0.7f * _portalLife);
            float floatOffset = (float)Math.Sin(_waveAngle * 2f) * 4f; // Легкое покачивание в воздухе
            float visualY = p.VisualY + floatOffset;

            Bitmap? img = _isLookingLeft ? ghostBmpStandLeft : ghostBmpStandRight;
            if (!p.IsGrounded) img = _isLookingLeft ? (ghostBmpJumpLeft ?? ghostBmpStandLeft) : (ghostBmpJumpRight ?? ghostBmpStandRight);

            if (img != null)
            {
                using (var attr = new ImageAttributes())
                {
                    var matrix = new ColorMatrix { Matrix33 = appearanceFactor }; // Управление прозрачностью
                    attr.SetColorMatrix(matrix);
                    g.DrawImage(img, new Rectangle((int)p.Position.X, (int)visualY, 120, 150),
                        0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attr);
                }
            }
        }

        // Эффект магических разломов на уровне
        private void DrawRiftsEffect(Graphics g)
        {
            foreach (var rift in _controller.Rifts)
            {
                using (var path = rift.GetPath())
                using (var brush = new PathGradientBrush(path))
                {
                    int alpha = Math.Clamp((int)(160 * rift.CurrentAlpha), 0, 255);
                    brush.CenterColor = Color.FromArgb(alpha, Color.White);
                    brush.SurroundColors = new Color[] { Color.Transparent };
                    brush.FocusScales = new PointF(0.05f, 0.95f);
                    g.FillPath(brush, path);

                    // Текстурный шум внутри разлома
                    if (noiseTexture != null)
                    {
                        g.SetClip(path);
                        using (TextureBrush tb = new TextureBrush(noiseTexture))
                        {
                            Matrix m = new Matrix();
                            m.Translate(_waveAngle * 20, _waveAngle * 12);
                            tb.Transform = m;
                            g.FillPath(tb, path);
                        }
                        g.ResetClip();
                    }
                }
            }
        }

        // КРАСИВАЯ ЧЕРНАЯ ДЫРА (ТВОЙ НОВЫЙ ДИЗАЙН)
        private void DrawBlackHole(Graphics g, PointF pos)
        {
            float rotation = _waveAngle * 25f;
            float distPulse = (float)Math.Sin(_waveAngle * 2f) * 10;
            int baseCoreRadius = 55;

            var oldMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Внешняя размытая аура
            RectangleF distortionRect = new RectangleF(
                pos.X - baseCoreRadius - 80 - distPulse / 2,
                pos.Y - baseCoreRadius - 80 - distPulse / 2,
                (baseCoreRadius + 80) * 2 + distPulse,
                (baseCoreRadius + 80) * 2 + distPulse);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(distortionRect);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    ColorBlend cb = new ColorBlend();
                    cb.Colors = new Color[] { Color.Transparent, Color.FromArgb(90, Color.GhostWhite), Color.Transparent };
                    cb.Positions = new float[] { 0f, 0.45f, 1f };
                    brush.InterpolationColors = cb;
                    g.FillPath(brush, path);
                }
            }

            // 2. Частицы поглощаемой пыли
            for (int i = 0; i < 15; i++)
            {
                float angle = _waveAngle * 3 + (i * 1.5f);
                float radius = (baseCoreRadius + 30) + (float)Math.Sin(_waveAngle + i) * 15;
                float px = pos.X + (float)Math.Cos(angle) * radius;
                float py = pos.Y + (float)Math.Sin(angle) * radius;

                int particleAlpha = (int)Math.Clamp(255 * (1.0f - radius / (baseCoreRadius + 50)), 50, 220);
                using (Brush b = new SolidBrush(Color.FromArgb(particleAlpha, Color.AntiqueWhite)))
                    g.FillEllipse(b, px, py, 3, 3);
            }

            // 3. Мягкое черное ядро
            RectangleF coreRect = new RectangleF(pos.X - baseCoreRadius, pos.Y - baseCoreRadius, baseCoreRadius * 2, baseCoreRadius * 2);
            using (GraphicsPath corePath = new GraphicsPath())
            {
                corePath.AddEllipse(coreRect);
                using (PathGradientBrush coreBrush = new PathGradientBrush(corePath))
                {
                    coreBrush.CenterColor = Color.Black;
                    coreBrush.SurroundColors = new Color[] { Color.Transparent };
                    coreBrush.FocusScales = new PointF(0.3f, 0.3f);
                    g.FillPath(coreBrush, corePath);
                }
            }
            g.SmoothingMode = oldMode;
        }

        // Отрисовка полоски здоровья (Стабильность эфира)
        private void DrawHUD(Graphics g)
        {
            using (Font hudFont = new Font("Courier New", 12, FontStyle.Bold))
            {
                g.DrawString("СТАБИЛЬНОСТЬ ЭФИРА", hudFont, Brushes.White, 30, 30);
                g.DrawRectangle(Pens.Cyan, 30, 55, 200, 15);
                float fill = Math.Clamp(_controller.Player.CurrentAlpha, 0, 1);
                g.FillRectangle(Brushes.DarkCyan, 32, 57, (int)(fill * 196), 11);
            }
        }

        // Общая пыль, плавающая по всему экрану
        private void DrawAtmosphere(Graphics g)
        {
            foreach (var p in dustParticles)
                g.FillEllipse(Brushes.WhiteSmoke, p.X + (float)Math.Cos(_waveAngle * 0.5f) * 15, p.Y, 1.5f, 1.5f);
        }

        // Универсальный метод для рисования радиальных свечений
        private void DrawGlow(Graphics g, Rectangle rect, Color color, float intensity)
        {
            Rectangle glowRect = rect;
            int inf = (int)(intensity);
            glowRect.Inflate(inf, inf);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(glowRect);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    int a = Math.Clamp((int)(110 * intensity), 0, 255);
                    brush.CenterColor = Color.FromArgb(a, color);
                    brush.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(brush, path);
                }
            }
        }

        // Отрисовка теней, которые "отбрасывают" платформы от источника света
        private void DrawUnifiedSoftShadow(Graphics g, List<Rectangle> obstacles, LightSource light)
        {
            foreach (Rectangle rect in obstacles)
            {
                var poly = ShadowEngine.GetShadowPolygon(rect, light, Width, Height);
                if (poly != null && poly.Length > 2)
                    using (Brush b = new SolidBrush(Color.FromArgb(75, 0, 0, 0))) g.FillPolygon(b, poly);
            }
        }

        // ОБРАБОТКА ВВОДА
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A) _isLookingLeft = true;
            if (e.KeyCode == Keys.D) _isLookingLeft = false;
            _controller.KeyDown(e.KeyCode);
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e) => _controller.KeyUp(e.KeyCode);

        // Свет следует за мышкой
        private void Form1_MouseMove(object? sender, MouseEventArgs e) => _controller.UpdateLightPosition(e.X, e.Y);

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_controller.State == GameState.MainMenu)
            {
                Rectangle startBtnRect = new Rectangle(Width / 2 - 190, Height / 2 + 100, 380, 90);
                if (startBtnRect.Contains(e.Location))
                {
                    _controller.State = GameState.Playing;
                    _controller.ResetToLevelSpawn();
                    this.Focus();
                }
            }
        }
    }
}