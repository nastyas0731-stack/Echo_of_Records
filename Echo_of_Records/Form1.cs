using Echo_of_Records.Controllers;
using Echo_of_Records.Models;
using Echo_of_Records.Views;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using WMPLib;


namespace Echo_of_Records
{
    public partial class Form1 : Form
    {
        private string[] _introPhrases = {
    "Архив №404. Объект: 'Эхо'.",
  };
        private int _currentPhraseIndex = 0;
        private bool _isIntroStoryShown = false;
        private float _subtitleAlpha = 0f;
        private bool _isSubtitleVisible = false;
        private int _subtitleTimer = 0;
        private Random _glitchRnd = new Random();

        private bool _wasNpcSoundPlayed = false;
        private Random _rng = new Random();
        private Image? noteSprite;
 
        private MainController _controller;
        private Bitmap? noiseTexture;
        private WMPLib.WindowsMediaPlayer _backgroundMusic;

        private System.Windows.Forms.Timer _gameTimer;
        private List<PointF> dustParticles = new List<PointF>();
        private Random rnd = new Random();
        private bool _isLookingLeft = false;

        private float _introAlpha = 0f;
        private float _waveAngle = 0f;
        private float _portalLife = 1.0f;

        private float _glitchIntensity = 0f;
        private bool _isTransitioningLevels = false;

        private string _activeNoteText = "";
        private float _noteUIAlpha = 0f;
        private System.Windows.Forms.Timer _noteTimer;

        private bool _isShowingLevelLore = false;
        private string _transitionText = ""; 
        private System.Windows.Forms.Timer _loreTimer;

        private float _fadeAlpha = 0f;
        private int _transitionState = 0;

        private Image npcSprite;
        private bool npcTriggered = false;
        private int npcDisplayCounter = 0;
        private const int NPC_LIFETIME = 180;


        private PointF npcPosition = new PointF(520, 230);
        private Image _ghostImage;

        private GameRenderer _renderer;

        public Form1()
        {

            _controller = new MainController();
            _renderer = new GameRenderer();
            InitializeComponent();

            _renderer.LoadResources();
            _backgroundMusic = new WMPLib.WindowsMediaPlayer();

            _backgroundMusic.URL = "background_music.mp3";


            _backgroundMusic.settings.volume = 50;

            _backgroundMusic.settings.setMode("loop", true);

            _backgroundMusic.controls.play();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.StartPosition = FormStartPosition.CenterScreen;
            GenerateNoiseTexture();

            this.WindowState = FormWindowState.Maximized;

            for (int i = 0; i < 120; i++)
                dustParticles.Add(new PointF(rnd.Next(2500), rnd.Next(1500)));

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.KeyPreview = true;

            _gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _gameTimer.Tick += (s, e) =>
            {
                _controller.Update();

                if (_controller.LevelManager.CurrentLevelIndex < 2 && !_isIntroStoryShown)
                {
                    _currentPhraseIndex = 0;
                    _subtitleAlpha = 0f;
                    _subtitleTimer = 250;
                    _isSubtitleVisible = true;
                    _isIntroStoryShown = true;
                }

                if (_isSubtitleVisible)
                {
                    if (_subtitleTimer > 0)
                    {
                        _subtitleTimer--;

                        if (_subtitleTimer > 200) _subtitleAlpha += 0.02f;
                        else if (_subtitleTimer < 50) _subtitleAlpha -= 0.02f;

                        if (_subtitleAlpha > 1) _subtitleAlpha = 1;
                        if (_subtitleAlpha < 0) _subtitleAlpha = 0;
                    }
                    else
                    {
                        _currentPhraseIndex++;
                        _subtitleAlpha = 0;

                        if (_currentPhraseIndex < _introPhrases.Length)
                        {
                            _subtitleTimer = 250;
                        }
                        else
                        {
                            _isSubtitleVisible = false;
                        }
                    }
                }

                if (_isSubtitleVisible)
                {
                    if (_subtitleTimer > 40) _subtitleAlpha = Math.Min(1f, _subtitleAlpha + 0.05f);
                    else _subtitleAlpha = Math.Max(0f, _subtitleAlpha - 0.05f);

                    _subtitleTimer--;
                    if (_subtitleTimer <= 0) _isSubtitleVisible = false;
                }

                var currentLevel = _controller.LevelManager.GetCurrentLevel();
                if (currentLevel != null)
                {
                    var player = _controller.Player;
                    float playerCenterX = player.Position.X + 60;
                    float playerCenterY = player.Position.Y + 75;

                    if (!_noteTimer.Enabled && _noteUIAlpha > 0)
                    {
                        _noteUIAlpha -= 0.01f;
                    }

                    foreach (var note in currentLevel.Notes.Where(n => !n.IsCollected))
                    {
                        playerCenterX = player.Position.X + 60;
                        playerCenterY = player.Position.Y + 75;

                        float noteCenterX = note.Bounds.X + note.Bounds.Width / 2;
                        float noteCenterY = note.Bounds.Y + note.Bounds.Height / 2;

                        float dx = playerCenterX - noteCenterX;
                        float dy = playerCenterY - noteCenterY;
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance < 100)
                        {
                            note.IsCollected = true;

                            _activeNoteText = note.Content;
                            _noteUIAlpha = 1.0f;

                            _noteTimer.Stop();
                            _noteTimer.Start();

                            _controller.TriggerNpc();

                            Console.WriteLine($"Собрал слиток! Текст: {_activeNoteText}");
                        }
                    }

                    if (currentLevel.FinishPoint != null)
                    {
                        float dxF = playerCenterX - currentLevel.FinishPoint.X;
                        float dyF = playerCenterY - currentLevel.FinishPoint.Y;
                        double distToFinish = Math.Sqrt(dxF * dxF + dyF * dyF);
                        bool isLevelInitialized = false;

                        if (distToFinish < 100 && _controller.LevelManager.CurrentLevelIndex == 0)
                        {
                            _controller.LevelManager.NextLevel();
                            _controller.ResetToLevelSpawn();

                            _transitionText = "«Некоторые вещи лучше оставлять забытыми.\nНо любопытство всегда было сильнее страха».";
                            _isShowingLevelLore = true;
                            _loreTimer.Stop();
                            _loreTimer.Start();
                        }
                    }
                }
                this.Invalidate();

                _waveAngle += 0.07f;
                this.Invalidate();
            };

            _noteTimer = new System.Windows.Forms.Timer { Interval = 4000 };
            _noteTimer.Tick += (s, e) => _noteTimer.Stop();

            _loreTimer = new System.Windows.Forms.Timer { Interval = 1200 };
            _loreTimer.Tick += (s, e) =>
            {
                _loreTimer.Stop();
                _isShowingLevelLore = false;
            };

            _gameTimer.Start();
        }

        public void StartIntroStory()
        {
            _currentPhraseIndex = 0;
            _isSubtitleVisible = true;
            _subtitleAlpha = 0;
            _subtitleTimer = 250;
            _isIntroStoryShown = true;
        }
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

       
        private void PlayDusSpawnSound()
        {
            try
            {
                using (System.Media.SoundPlayer sp = new System.Media.SoundPlayer("dus_spawn.wav"))
                {
                    sp.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка звука: " + ex.Message);
            }
        }
        private void DrawInterLevelText(Graphics g)
        {
            if (noiseTexture != null)
            {
                using (TextureBrush tb = new TextureBrush(noiseTexture))
                {
                    Matrix m = new Matrix();
                    m.Scale(6.0f, 6.0f);

                    float offsetX = (float)(Math.Sin(_waveAngle * 10) * 100);
                    float offsetY = (float)(Math.Cos(_waveAngle * 10) * 100);
                    m.Translate(offsetX, offsetY);

                    tb.Transform = m;

                    using (Brush noiseBrush = new SolidBrush(Color.FromArgb((int)(200 * _glitchIntensity), 10, 10, 20)))
                    {
                        g.FillRectangle(tb, this.ClientRectangle);
                    }
                }
            }

            using (Font f = new Font("Courier New", 18, FontStyle.Italic | FontStyle.Bold))

            using (Brush textBrush = new SolidBrush(Color.FromArgb((int)(240 * _glitchIntensity), Color.Azure)))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                Rectangle textRect = Rectangle.Inflate(this.ClientRectangle, -100, -100);
                g.DrawString(_transitionText, f, textBrush, textRect, sf);
            }
        }
        private void DrawMainMenu(Graphics g)
        {
            if (_introAlpha < 1.0f) _introAlpha += 0.01f;
            int bgVal = (int)(15 * _introAlpha);
            g.Clear(Color.FromArgb(bgVal, bgVal, bgVal + 10));

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

                float flicker = (float)Math.Abs(Math.Sin(_waveAngle * 2.5f));
                int alphaVal = Math.Clamp((int)(255 * _introAlpha * flicker), 0, 255);

                if (rnd.Next(10) > 7)
                {
                    float shift = (float)Math.Sin(_waveAngle * 10f) * 5f;
                    using (Brush redBrush = new SolidBrush(Color.FromArgb(alphaVal / 2, Color.Red)))
                        g.DrawString(titleText, titleFont, redBrush, centerX - shift, centerY);
                }

                using (Brush titleBrush = new SolidBrush(Color.FromArgb(alphaVal, Color.White)))
                {
                    float shakeX = (rnd.Next(5) == 0) ? rnd.Next(-10, 11) : 0;
                    g.DrawString(titleText, titleFont, titleBrush, centerX + shakeX, centerY);
                }

                Rectangle btnRect = new Rectangle(Width / 2 - 190, Height / 2 + 100, 380, 90);
                bool hovered = btnRect.Contains(PointToClient(Cursor.Position));
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    using (Pen p = new Pen(hovered ? Color.White : Color.FromArgb(120, Color.Cyan), hovered ? 3 : 1))
                    {
                        g.DrawRectangle(p, btnRect);
                        g.DrawString("ИССЛЕДОВАТЬ", buttonFont, hovered ? Brushes.White : Brushes.Cyan, btnRect, sf);
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_renderer == null || _controller == null || _controller.Player == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            _renderer.Render(e.Graphics, _controller, this.ClientSize);

            if (_controller.State == GameState.MainMenu)
            {
                DrawMainMenu(e.Graphics);
            }
            else
            {
                DrawHUD(e.Graphics);     
                DrawNoteUI(e.Graphics);  
            }
        }
        private void DrawSubtitles(Graphics g)
        {

        }        

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
                
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
        private void DrawNoteUI(Graphics g)
        {
            if (_noteUIAlpha <= 0 || string.IsNullOrEmpty(_activeNoteText)) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Font font = new Font("Courier New", 14, FontStyle.Italic))
            {
                Size textSize = TextRenderer.MeasureText(_activeNoteText, font);
                Rectangle cloudRect = new Rectangle(
                    Width / 2 - textSize.Width,
                    Height - 220,
                    textSize.Width * 2,
                    textSize.Height + 100
                );

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(cloudRect);

                    using (PathGradientBrush shadowBrush = new PathGradientBrush(path))
                    {
                        int centralAlpha = (int)(200 * _noteUIAlpha);
                        shadowBrush.CenterColor = Color.FromArgb(centralAlpha, 5, 5, 10);
                        shadowBrush.SurroundColors = new Color[] { Color.Transparent };

                        g.FillPath(shadowBrush, path);
                    }
                }

                int textAlpha = (int)(255 * _noteUIAlpha);
                using (Brush textBrush = new SolidBrush(Color.FromArgb(textAlpha, Color.GhostWhite)))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(_activeNoteText, font, textBrush,
                        new Rectangle(0, Height - 210, Width, 100), sf);
                }
            }
        }
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A) _isLookingLeft = true;
            if (e.KeyCode == Keys.D) _isLookingLeft = false;
            _controller.KeyDown(e.KeyCode);
        }

        
        private void Form1_KeyUp(object? sender, KeyEventArgs e) => _controller.KeyUp(e.KeyCode);
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

