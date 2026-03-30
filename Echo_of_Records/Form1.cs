using Echo_of_Records.Controllers;
using Echo_of_Records.Models;
using System.Drawing.Drawing2D;

namespace Echo_of_Records
{
    public partial class Form1 : Form
    {
        private MainController _controller;

        public Form1()
        {
            InitializeComponent();
            _controller = new MainController();
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _controller.UpdateLightPosition(e.X, e.Y);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            // 1. ФОН: Светлее, чем был (серо-коричневый)
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

            // 5. СВЕЧА
            var light = _controller.Light;
            g.FillEllipse(Brushes.Orange, light.X - 6, light.Y - 6, 12, 12);
            g.FillEllipse(Brushes.White, light.X - 3, light.Y - 3, 6, 6);
        }

        private void DrawFancyLight(Graphics g, LightSource light)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                // Уменьшенный радиус свечения
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