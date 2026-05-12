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
        public void DrawWorld(Graphics g)
        { 
            var currentLevel = _controller.LevelManager.GetCurrentLevel();
            if (currentLevel == null || _controller.LevelManager == null) return;

            int currentIdx = _controller.LevelManager.CurrentLevelIndex;
            if (_levelBackgrounds.ContainsKey(currentIdx)) g.DrawImage(_levelBackgrounds[currentIdx], 0, 0, Width, Height);
            else g.Clear(Color.FromArgb(5, 5, 15));

            DrawSpawnPortal(g, currentLevel.SpawnPoint);

            var allShadowObjects = currentLevel.Obstacles.Concat(_controller.MovingPlatforms.Select(p => Rectangle.Round(p.Bounds))).ToList();
            DrawUnifiedSoftShadow(g, allShadowObjects, _controller.Light);

            foreach (Rectangle rect in currentLevel.Obstacles) DrawPlatform(g, rect);
            foreach (var platform in _controller.MovingPlatforms) DrawPlatform(g, Rectangle.Round(platform.Bounds));

            DrawNotes(g);

            if (currentLevel.FinishPoint != null)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                DrawBlackHole(g, currentLevel.FinishPoint);
            }
            DrawRiftsEffect(g);

            foreach (Rectangle rect in currentLevel.Obstacles) DrawPlatform(g, rect);
            foreach (var platform in _controller.MovingPlatforms) DrawPlatform(g, Rectangle.Round(platform.Bounds));

            DrawPlayer(g);
            DrawAtmosphere(g);

            if (candleSprite != null)
            {
                float flick = (float)Math.Sin(_waveAngle * 5) * 5;
                Rectangle glowRect = new Rectangle((int)_controller.Light.X - 180, (int)_controller.Light.Y - 180, 360, 360);
                DrawGlow(g, glowRect, Color.LightYellow, 1.1f + flick / 100f);
                g.DrawImage(candleSprite, _controller.Light.X - 40, _controller.Light.Y - 40, 80, 80);
            }

        }

        private void DrawPlatform(Graphics g, Rectangle rect)
        {
            var currentLevel = _controller.LevelManager.GetCurrentLevel();
            if (currentLevel == null) return;

            bool isLava = currentLevel.GlowingPlatforms.Any(p => p.X == rect.X && p.Y == rect.Y);
            float pulse = (float)(Math.Sin(_waveAngle * 3f) * 0.2f + 0.8f);

            using (GraphicsPath path = new GraphicsPath())
            {
                RectangleF rectF = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
                path.AddEllipse(RectangleF.Inflate(rectF, 50, 50));

                using (PathGradientBrush pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = isLava ?
                        Color.FromArgb((int)(170 * pulse), Color.LemonChiffon) :
                        Color.FromArgb((int)(100 * pulse), Color.Cyan);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }

            if (bookSprite != null)
            {
                g.DrawImage(bookSprite, rect);
            }
        }

        private void DrawSpawnPortal(Graphics g, PointF pos)
        {
            if (_portalLife <= 0.05f) return;
            var p = _controller.Player;
            float dist = (float)Math.Sqrt(Math.Pow(p.Position.X - pos.X, 2) + Math.Pow(p.Position.Y - pos.Y, 2));

            if (dist > 5 && _portalLife > 0) _portalLife -= 0.03f;
            int baseSize = (int)(130 * _portalLife);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(pos.X - baseSize / 2f, pos.Y - baseSize / 2f, baseSize, baseSize);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb((int)(160 * _portalLife), Color.LightSlateGray);
                    brush.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(brush, path);
                }
            }
        }

        private void DrawNotes(Graphics g)
        {
            var currentLevel = _controller.LevelManager.GetCurrentLevel();
            if (currentLevel?.Notes == null) return;

            float floatAnim = (float)Math.Sin(_waveAngle * 2f) * 10f;

            foreach (var note in currentLevel.Notes.Where(n => !n.IsCollected))
            {
                int fixWidth = 60;
                int fixHeight = 60;

                float x = note.Bounds.X + (note.Bounds.Width / 2) - (fixWidth / 2);
                float y = note.Bounds.Y + (note.Bounds.Height / 2) - (fixHeight / 2) + floatAnim;

                RectangleF drawRect = new RectangleF(x, y, fixWidth, fixHeight);

                DrawGlow(g, Rectangle.Round(drawRect), Color.FromArgb(150, Color.Gold), 1.2f);

                if (noteSprite != null)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(noteSprite, drawRect);
                }
                else
                {
                    g.FillRectangle(Brushes.Gold, drawRect);
                }
            }
        }

        private void DrawBlackHole(Graphics g, PointF pos)
        {
            float distPulse = (float)Math.Sin(_waveAngle * 2f) * 10;
            int baseCoreRadius = 55;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(pos.X - 135, pos.Y - 135, 270 + distPulse, 270 + distPulse);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    brush.InterpolationColors = new ColorBlend
                    {
                        Colors = new Color[] { Color.Transparent, Color.FromArgb(90, Color.GhostWhite), Color.Transparent },
                        Positions = new float[] { 0f, 0.45f, 1f }
                    };
                    g.FillPath(brush, path);
                }
            }

            using (GraphicsPath corePath = new GraphicsPath())
            {
                corePath.AddEllipse(pos.X - baseCoreRadius, pos.Y - baseCoreRadius, baseCoreRadius * 2, baseCoreRadius * 2);
                using (PathGradientBrush coreBrush = new PathGradientBrush(corePath))
                {
                    coreBrush.CenterColor = Color.Black;
                    coreBrush.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(coreBrush, corePath);
                }
            }
        }
    }
}
