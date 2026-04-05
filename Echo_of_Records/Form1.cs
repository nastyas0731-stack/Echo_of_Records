using Echo_of_Records.Controllers;
using Echo_of_Records.Models;
using System.Drawing.Drawing2D;

namespace Echo_of_Records
{
    public partial class Form1 : Form
    {
        private MainController _controller;
        // Добавим таймер прямо здесь, чтобы физика обновлялась плавно
        private System.Windows.Forms.Timer _gameTimer;

        public Form1()
        {
            InitializeComponent();
            _controller = new MainController();
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;

            // 1. ПОДПИСКА НА КЛАВИШИ
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.KeyPreview = true; // Чтобы форма ловила нажатия раньше кнопок

            // 2. ТАЙМЕР ДЛЯ ФИЗИКИ (чтобы игрок падал и бегал плавно)
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 20; // ~50 кадров в секунду
            _gameTimer.Tick += (s, e) => {
                _controller.Update(); // Считаем физику в контроллере
                this.Invalidate();    // Перерисовываем экран
            };
            _gameTimer.Start();
        }

        // ПЕРЕДАЕМ НАЖАТИЯ В КОНТРОЛЛЕР
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            _controller.KeyDown(e.KeyCode);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            _controller.KeyUp(e.KeyCode);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _controller.UpdateLightPosition(e.X, e.Y);
            // Invalidate() теперь делает таймер, здесь можно убрать для оптимизации
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            // 1. ФОН
            g.Clear(Color.FromArgb(65, 60, 55));

            // 2. ТЕНИ
            foreach (var obs in _controller.Obstacles)
            {
                ShadowEngine.DrawShadow(g, _controller.Light, obs);
            }

            // 3. МАЛЕНЬКОЕ СВЕЧЕНИЕ
            DrawFancyLight(g, _controller.Light);

            // 4. ОБЪЕКТЫ (КНИГИ)
            foreach (var obs in _controller.Obstacles)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(obs.Rect,
                        Color.SaddleBrown, Color.FromArgb(50, 25, 0), 45f))
                {
                    g.FillRectangle(brush, obs.Rect);
                }
                g.DrawRectangle(new Pen(Color.FromArgb(120, 212, 175, 55), 2),
                                obs.Rect.X, obs.Rect.Y, obs.Rect.Width, obs.Rect.Height);
            }

            // 5. ПЕРСОНАЖ (ДУХ)
            var p = _controller.Player;
            // Рисуем полупрозрачного белого духа
            using (SolidBrush playerBrush = new SolidBrush(Color.FromArgb(180, Color.White)))
            {
                g.FillEllipse(playerBrush, p.Position.X, p.Position.Y, p.Width, p.Height);
            }
            // Можно добавить легкое свечение вокруг духа
            g.DrawEllipse(Pens.LightBlue, p.Position.X - 2, p.Position.Y - 2, p.Width + 4, p.Height + 4);

            // 6. СВЕЧА (Курсор)
            var light = _controller.Light;
            g.FillEllipse(Brushes.Orange, light.X - 6, light.Y - 6, 12, 12);
            g.FillEllipse(Brushes.White, light.X - 3, light.Y - 3, 6, 6);
        }

        private void DrawFancyLight(Graphics g, LightSource light)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                float radius = 180;
                path.AddEllipse(light.X - radius, light.Y - radius, radius * 2, radius * 2);

                using (PathGradientBrush pgb = new PathGradientBrush(path))
                {
                    pgb.CenterPoint = new PointF(light.X, light.Y);
                    pgb.CenterColor = Color.FromArgb(90, 255, 220, 100);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }
        }
    }
}