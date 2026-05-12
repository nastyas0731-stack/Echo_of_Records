using Echo_of_Records.Controllers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Echo_of_Records.Views
{
    public partial class GameRenderer
    {
        private MainController _controller;

        private Image? noteSprite;
        private Image? backgroundImage, bookSprite, candleSprite, npcSprite, _ghostImage;
        public Bitmap? ghostBmpStandRight, ghostBmpStandLeft, ghostBmpJumpRight, ghostBmpJumpLeft;
        private Bitmap? noiseTexture;
        private Dictionary<int, Image> _levelBackgrounds = new Dictionary<int, Image>();

        private float _waveAngle = 0f;
        private float _portalLife = 1.0f;
        private Random _glitchRnd = new Random();
        private Random _rng = new Random();
        private bool _isLookingLeft = false;

        public int Width { get; set; } = 800;
        public int Height { get; set; } = 450;

        private bool _wasNpcSoundPlayed = false;
        private bool npcTriggered = false;
        private PointF npcPosition = new PointF(520, 230);

        private List<PointF> dustParticles = new List<PointF>();

        public void Render(Graphics g, MainController controller, Size clientSize)
        {
            this._controller = controller;

            this.Width = clientSize.Width;
            this.Height = clientSize.Height;

            DrawWorld(g);
            DrawNpc(g);
            DrawPlayer(g);

           
        }

        private void DrawGlow(Graphics g, Rectangle rect, Color color, float intensity)
        {
            rect.Inflate((int)intensity, (int)intensity);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(Math.Clamp((int)(110 * intensity), 0, 255), color);
                    brush.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(brush, path);
                }
            }
        }

        private void PlayDusSpawnSound()
        {
            _controller.PlayNpcSpawnSound();
        }

        public void LoadResources()
        {
            if (File.Exists("Ingot.png"))
            {
                noteSprite = Image.FromFile("Ingot.png");
            }

            try
            {
                ghostBmpStandRight = LoadSmartBitmap("ghost.png") ?? LoadSmartBitmap("ghost.jpg");
                ghostBmpJumpRight = LoadSmartBitmap("ghost_jump.png") ?? LoadSmartBitmap("ghost_jump.jpg");

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

                if (File.Exists("ghost_two.png"))
                    _ghostImage = Image.FromFile("ghost_two.png");

                if (File.Exists("book_platform.png")) bookSprite = Image.FromFile("book_platform.png");
                if (File.Exists("light.png")) candleSprite = Image.FromFile("light.png");
                npcSprite = Image.FromFile("ghost_two.png");

                if (File.Exists("background.jpg")) _levelBackgrounds[0] = Image.FromFile("background.jpg");
                if (File.Exists("background1.jpg")) _levelBackgrounds[1] = Image.FromFile("background1.jpg");
                if (File.Exists("background2.jpg")) _levelBackgrounds[2] = Image.FromFile("background2.jpg");
                if (File.Exists("background3.jpg")) _levelBackgrounds[3] = Image.FromFile("background3.jpg");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка ресурсов: " + ex.Message); }
        }

        public Bitmap? LoadSmartBitmap(string fileName)
        {
            if (!File.Exists(fileName)) return null;
            Bitmap bmp = new Bitmap(fileName);
            if (fileName.ToLower().EndsWith(".jpg") || fileName.ToLower().EndsWith(".jpeg"))
            {
                bmp.MakeTransparent(Color.Black);
                bmp.MakeTransparent(Color.White);
            }
            return bmp;
        }

    }
}