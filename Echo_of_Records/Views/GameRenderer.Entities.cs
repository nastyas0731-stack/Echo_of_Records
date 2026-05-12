using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo_of_Records.Views
{
    public partial class GameRenderer
    {
        private void DrawPlayer(Graphics g)
        {
            var p = _controller.Player;
            float appearanceFactor = 0.8f - (0.7f * _portalLife);
            float visualY = p.VisualY + (float)Math.Sin(_waveAngle * 2f) * 4f;

            Bitmap? img = _isLookingLeft ? ghostBmpStandLeft : ghostBmpStandRight;
            if (!p.IsGrounded) img = _isLookingLeft ? (ghostBmpJumpLeft ?? ghostBmpStandLeft) : (ghostBmpJumpRight ?? ghostBmpStandRight);

            if (img != null)
            {
                using (var attr = new ImageAttributes())
                {
                    attr.SetColorMatrix(new ColorMatrix { Matrix33 = appearanceFactor });
                    g.DrawImage(img, new Rectangle((int)p.Position.X, (int)visualY, 120, 150),
                        0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attr);
                }
            }
        }

        public void DrawNpc(Graphics g)
        {
            if (_controller.LevelManager.CurrentLevelIndex != 3) return;

            if (!_controller.IsNpcTriggered || _controller.IsNpcVanished)
            {
                _wasNpcSoundPlayed = false;
                return;
            }

            if (_controller.IsNpcTriggered && !_wasNpcSoundPlayed)
            {
                PlayDusSpawnSound();
                _wasNpcSoundPlayed = true;
            }

            if (_controller.IsNpcTriggered && !_controller.IsNpcVanished)
            {
                float alpha = _controller.GetNpcAlpha();
                Image sprite = _ghostImage;

                if (sprite != null)
                {
                    double time = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    float offsetX = (float)Math.Sin(time * 2.0) * 15;
                    float offsetY = (float)Math.Cos(time * 1.5) * 10;

                    if (_rng.Next(0, 100) > 90)
                    {
                        offsetX += _rng.Next(-20, 20);
                        alpha *= 0.5f;
                    }

                    int npcWidth = 450;
                    int npcHeight = (int)((float)npcWidth / sprite.Width * sprite.Height);

                    float drawX = _controller.NpcPosition.X + offsetX;
                    float drawY = _controller.NpcPosition.Y + offsetY;

                    using (var ia = new System.Drawing.Imaging.ImageAttributes())
                    {
                        var matrixLow = new System.Drawing.Imaging.ColorMatrix { Matrix33 = alpha * 0.3f };
                        ia.SetColorMatrix(matrixLow);

                        g.DrawImage(sprite,
                            new Rectangle((int)drawX - 15, (int)drawY, npcWidth, npcHeight),
                            0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, ia);

                        var matrixFull = new System.Drawing.Imaging.ColorMatrix { Matrix33 = alpha };
                        ia.SetColorMatrix(matrixFull);

                        g.DrawImage(sprite,
                            new Rectangle((int)drawX, (int)drawY, npcWidth, npcHeight),
                            0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, ia);
                    }

                    string message = "НЕТ ВЫХОДА...";
                    float textGap = (_rng.Next(0, 100) > 95) ? _rng.Next(-5, 5) : 0;

                    using (Font font = new Font("Impact", 24, FontStyle.Bold))
                    {
                        float tX = drawX + 50 + textGap;
                        float tY = drawY - 60 + textGap;

                        g.DrawString(message, font, Brushes.DarkRed, tX + 2, tY + 2);
                        using (Brush b = new SolidBrush(Color.FromArgb((int)(255 * alpha), Color.White)))
                        {
                            g.DrawString(message, font, b, tX, tY);
                        }
                    }
                }
            }
        }

    }
}
