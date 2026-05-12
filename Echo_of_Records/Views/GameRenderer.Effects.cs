using Echo_of_Records.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo_of_Records.Views
{
    public partial class GameRenderer
    {
        private void DrawDarknessEffect(Graphics g, PointF playerPos)
        {
            using (Bitmap mask = new Bitmap(Width, Height))
            {
                using (Graphics maskG = Graphics.FromImage(mask))
                {
                    maskG.Clear(Color.FromArgb(252, 10, 10, 15));

                    float flicker = (float)Math.Sin(Environment.TickCount * 0.005) * 8;
                    float lightRadius = 280f + flicker;

                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddEllipse(playerPos.X - lightRadius + 50, playerPos.Y - lightRadius + 50, lightRadius * 2, lightRadius * 2);

                        using (PathGradientBrush pgb = new PathGradientBrush(path))
                        {
                            pgb.CenterColor = Color.Transparent;
                            pgb.SurroundColors = new Color[] { Color.FromArgb(252, 10, 10, 15) };

                            maskG.CompositingMode = CompositingMode.SourceCopy;
                            maskG.FillPath(pgb, path);
                        }
                    }
                }
                g.DrawImage(mask, 0, 0);
            }
        }

        private void DrawUnifiedSoftShadow(Graphics g, List<Rectangle> obstacles, LightSource light)
        {
            if (obstacles == null || obstacles.Count == 0) return;

            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(75, 0, 0, 0)))
            {
                foreach (Rectangle rect in obstacles)
                {
                    var poly = ShadowEngine.GetShadowPolygon(rect, light, Width, Height);
                    if (poly != null && poly.Length > 2)
                    {
                        g.FillPolygon(shadowBrush, poly);
                    }
                }
            }
        }

        private void DrawRiftsEffect(Graphics g)
        {
            foreach (var rift in _controller.Rifts)
            {
                using (var path = rift.GetPath())
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(Math.Clamp((int)(160 * rift.CurrentAlpha), 0, 255), Color.White);
                    brush.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawAtmosphere(Graphics g)
        {
            foreach (var p in dustParticles)
                g.FillEllipse(Brushes.WhiteSmoke, p.X + (float)Math.Cos(_waveAngle * 0.5f) * 15, p.Y, 1.5f, 1.5f);
        }
    }
}
