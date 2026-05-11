using Echo_of_Records.Controllers;
using Echo_of_Records.Models;
using Echo_of_Records.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WMPLib;          // Для фоновой музыки (нужно добавить через COM выше)
using System.Media;    // Для звуков выстрелов/сбора (стандартная)
using System.IO;


namespace Echo_of_Records
{
    public partial class Form1 : Form
    {
        // --- СИСТЕМА ФАЗОВОГО ИНТРО ---
        private string[] _introPhrases = {
    "Архив №404. Объект: 'Эхо'.",
  };
        private int _currentPhraseIndex = 0;    // Номер текущей фразы
        private bool _isIntroStoryShown = false; // Флаг запуска
        private float _subtitleAlpha = 0f;      // Прозрачность
        private bool _isSubtitleVisible = false;
        private int _subtitleTimer = 0;         // Таймер одной фразы
        private Random _glitchRnd = new Random();

        private bool _wasNpcSoundPlayed = false;
        private Random _rng = new Random();
        private Image? noteSprite;
        // --- ПОЛЯ КОНТРОЛЛЕРОВ И РЕСУРСОВ ---
        private MainController _controller;
        private Bitmap? ghostBmpStandRight, ghostBmpStandLeft, ghostBmpJumpRight, ghostBmpJumpLeft;
        private Image? backgroundImage, bookSprite, candleSprite;
        private Bitmap? noiseTexture;
        private WMPLib.WindowsMediaPlayer _backgroundMusic;

        // --- ИГРОВАЯ ЛОГИКА И ВИЗУАЛ ---
        private System.Windows.Forms.Timer _gameTimer;
        private List<PointF> dustParticles = new List<PointF>();
        private Random rnd = new Random();
        private bool _isLookingLeft = false;
        private Dictionary<int, Image> _levelBackgrounds = new Dictionary<int, Image>();

        // --- ПЕРЕМЕННЫЕ АНИМАЦИИ ---
        private float _introAlpha = 0f;
        private float _waveAngle = 0f;
        private float _portalLife = 1.0f;

        private float _glitchIntensity = 0f;
        private bool _isTransitioningLevels = false;

        private string _activeNoteText = "";     // Текст, который мы сейчас показываем
        private float _noteUIAlpha = 0f;         // Прозрачность панели (0 - скрыта, 1 - видна)
        private System.Windows.Forms.Timer _noteTimer; // Таймер, чтобы текст исчезал сам через 5 секунд

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

      // Счетчик времени показа


        Image background4;
        public Form1()
        {
            InitializeComponent();


        // Создаем экземпляр плеера
            _backgroundMusic = new WMPLib.WindowsMediaPlayer();

            // Указываем путь к файлу (если он в папке с .exe, то просто имя)
            _backgroundMusic.URL = "background_music.mp3";

            // Настраиваем громкость (от 0 до 100)
            _backgroundMusic.settings.volume = 50;

            // Зацикливаем мелодию
            _backgroundMusic.settings.setMode("loop", true);

            // Запускаем
            _backgroundMusic.controls.play();

            // ПРАВИЛЬНАЯ НАСТРОЙКА БУФЕРИЗАЦИИ (от мигания)
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.StartPosition = FormStartPosition.CenterScreen;
            _controller = new MainController();

            LoadResources();
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

                // 1. Запуск самой первой фразы при входе на 1 уровень
                if (_controller.LevelManager.CurrentLevelIndex < 2 && !_isIntroStoryShown)
                {
                    _currentPhraseIndex = 0;
                    _subtitleAlpha = 0f;
                    _subtitleTimer = 250; // Время на одну фразу
                    _isSubtitleVisible = true;
                    _isIntroStoryShown = true;
                }

                // Внутри метода Tick таймера
                if (_isSubtitleVisible)
                {
                    if (_subtitleTimer > 0)
                    {
                        _subtitleTimer--;

                        // Плавное появление в начале (первые 50 тиков)
                        if (_subtitleTimer > 200) _subtitleAlpha += 0.02f;
                        // Плавное исчезновение в конце (последние 50 тиков)
                        else if (_subtitleTimer < 50) _subtitleAlpha -= 0.02f;

                        // Ограничиваем альфу от 0 до 1
                        if (_subtitleAlpha > 1) _subtitleAlpha = 1;
                        if (_subtitleAlpha < 0) _subtitleAlpha = 0;
                    }
                    else
                    {
                        // Когда таймер фразы кончился:
                        _currentPhraseIndex++; // Переходим к следующей фразе
                        _subtitleAlpha = 0;    // Сбрасываем прозрачность

                        // Если фразы еще есть — перезапускаем таймер для следующей
                        if (_currentPhraseIndex < _introPhrases.Length)
                        {
                            _subtitleTimer = 250;
                        }
                        else
                        {
                            // Если фразы кончились — выключаем субтитры
                            _isSubtitleVisible = false;
                        }
                    }
                }

                // Логика плавного появления и исчезновения текста
                if (_isSubtitleVisible)
                {
                    if (_subtitleTimer > 40) _subtitleAlpha = Math.Min(1f, _subtitleAlpha + 0.05f); // Проявление
                    else _subtitleAlpha = Math.Max(0f, _subtitleAlpha - 0.05f); // Исчезновение в конце

                    _subtitleTimer--;
                    if (_subtitleTimer <= 0) _isSubtitleVisible = false;
                }

                var currentLevel = _controller.LevelManager.GetCurrentLevel();
                if (currentLevel != null)
                {
                    var player = _controller.Player;
                    float playerCenterX = player.Position.X + 60;
                    float playerCenterY = player.Position.Y + 75;

                    // Плавное затухание текста записки
                    if (!_noteTimer.Enabled && _noteUIAlpha > 0)
                    {
                        _noteUIAlpha -= 0.01f;
                    }

                    // Логика сбора слитков
                    foreach (var note in currentLevel.Notes.Where(n => !n.IsCollected))
                    {
                        // Центр игрока
                        playerCenterX = player.Position.X + 60;
                        playerCenterY = player.Position.Y + 75;

                        // Центр слитка
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

                            // Перезапускаем таймер отображения текста
                            _noteTimer.Stop();
                            _noteTimer.Start();

                            // Активируем духа, если он еще не был вызван
                            _controller.TriggerNpc();

                            // Добавь это для отладки в консоль (потом можно удалить)
                            Console.WriteLine($"Собрал слиток! Текст: {_activeNoteText}");
                        }
                    }

                    // МГНОВЕННЫЙ ПЕРЕХОД МЕЖДУ УРОВНЯМИ
                    if (currentLevel.FinishPoint != null)
                    {
                        float dxF = playerCenterX - currentLevel.FinishPoint.X;
                        float dyF = playerCenterY - currentLevel.FinishPoint.Y;
                        double distToFinish = Math.Sqrt(dxF * dxF + dyF * dyF);
                        bool isLevelInitialized = false; // Поле в классе формы

                        // Если подошли к порталу на 1 уровне
                        if (distToFinish < 100 && _controller.LevelManager.CurrentLevelIndex == 0)
                        {
                            // 1. Сразу меняем уровень
                            _controller.LevelManager.NextLevel();
                            _controller.ResetToLevelSpawn();

                            // 2. Включаем короткую надпись
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

            // Настройки таймеров
            _noteTimer = new System.Windows.Forms.Timer { Interval = 4000 };
            _noteTimer.Tick += (s, e) => _noteTimer.Stop();

            _loreTimer = new System.Windows.Forms.Timer { Interval = 1200 }; // Надпись исчезнет быстро
            _loreTimer.Tick += (s, e) =>
            {
                _loreTimer.Stop();
                _isShowingLevelLore = false;
            };

            _gameTimer.Start();
        }

        public void StartIntroStory()
        {
            // Вместо записи в одну строку, просто сбрасываем индекс на начало
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

        private void LoadResources()
        {
            // Добавь это внутрь блока try в методе LoadResources
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

        private Bitmap? LoadSmartBitmap(string fileName)
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



        private void PlayDusSpawnSound()
        {
            try
            {
                // Используем SoundPlayer для мгновенного звука без задержек
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
            // Экран уже черный (благодаря case 1), рисуем только шум и текст.

            // 1. ДВИЖУЩИЙСЯ ШУМ
            if (noiseTexture != null)
            {
                using (TextureBrush tb = new TextureBrush(noiseTexture))
                {
                    Matrix m = new Matrix();
                    m.Scale(6.0f, 6.0f);

                    // ОЖИВЛЯЕМ ШУМ:
                    // Вместо rnd.Next используем Math.Sin от waveAngle, чтобы смещение менялось плавно и быстро
                    float offsetX = (float)(Math.Sin(_waveAngle * 10) * 100);
                    float offsetY = (float)(Math.Cos(_waveAngle * 10) * 100);
                    m.Translate(offsetX, offsetY);

                    tb.Transform = m;

                    // Рисуем шум с высокой прозрачностью (0.6 - 0.8)
                    using (Brush noiseBrush = new SolidBrush(Color.FromArgb((int)(200 * _glitchIntensity), 10, 10, 20)))
                    {
                        g.FillRectangle(tb, this.ClientRectangle);
                    }
                }
            }

            // 2. ПЛАВНЫЙ ТЕКСТ (Azure, как ты хотела)
            using (Font f = new Font("Courier New", 18, FontStyle.Italic | FontStyle.Bold))
            // Используем fadeAlpha, чтобы текст появлялся/исчезал вместе с затемнением мира
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

        private void DrawWorld(Graphics g)
        {

            var currentLevel = _controller.LevelManager.GetCurrentLevel();
            if (currentLevel == null) return;

            int currentIdx = _controller.LevelManager.CurrentLevelIndex;
            if (_levelBackgrounds.ContainsKey(currentIdx)) g.DrawImage(_levelBackgrounds[currentIdx], 0, 0, Width, Height);
            else g.Clear(Color.FromArgb(5, 5, 15));

            DrawSpawnPortal(g, currentLevel.SpawnPoint);

            var allShadowObjects = currentLevel.Obstacles.Concat(_controller.MovingPlatforms.Select(p => Rectangle.Round(p.Bounds))).ToList();
            DrawUnifiedSoftShadow(g, allShadowObjects, _controller.Light);

            foreach (Rectangle rect in currentLevel.Obstacles) DrawPlatform(g, rect);
            foreach (var platform in _controller.MovingPlatforms) DrawPlatform(g, Rectangle.Round(platform.Bounds));

            // И в конце записки, чтобы они были поверх теней
            DrawNotes(g);

            if (currentLevel.FinishPoint != null)
            {
                // Если FinishPoint — это PointF, пишем просто currentLevel.FinishPoint
                // Если FinishPoint — это PointF?, пишем currentLevel.FinishPoint.Value
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

            // --- Отрисовка Духа (NPC) ---
            // Находим место отрисовки NPC в Form1.cs
            // --- Отрисовка Духа (NPC) ---

        }

        // --- ОБЪЕДИНЕННЫЙ И ИСПРАВЛЕННЫЙ МЕТОД ONPAINT ---
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_controller.LevelManager.CurrentLevelIndex == 4)
            {
                DrawDarknessEffect(g, _controller.Player.Position);
            }

            if (_controller.State == GameState.MainMenu)
            {
                DrawMainMenu(g);
                return;
            }

            if (_controller.State == GameState.Playing)
            {
                if (_isTransitioningLevels && _transitionState == 1)
                {
                    g.Clear(Color.Black);
                    DrawInterLevelText(g);
                    return;
                }

                DrawWorld(g); // Здесь теперь чисто, маленького духа нет

                // Оставляем ТОЛЬКО этот вызов — он рисует большого и лагающего
                DrawNpc(g);

                DrawHUD(g);
                DrawNoteUI(g);

                if (_isSubtitleVisible && _currentPhraseIndex < _introPhrases.Length)
                {
                    using (Font subFont = new Font("Consolas", 14, FontStyle.Italic))
                    {
                        Rectangle bgRect = new Rectangle(0, Height - 180, Width, 120);
                        int alpha = (int)(255 * _subtitleAlpha);
                        TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

                        // ВАЖНО: Берем текст из МАССИВА по текущему индексу
                        string currentText = _introPhrases[_currentPhraseIndex];

                        TextRenderer.DrawText(g, currentText, subFont, bgRect,
                            Color.FromArgb(alpha, Color.Azure), flags);
                    }
                }
            }

            // Вставь это в самом конце OnPaint
            if (_isSubtitleVisible && _currentPhraseIndex < _introPhrases.Length)
            {
                using (Font subFont = new Font("Consolas", 14, FontStyle.Italic))
                {
                    Rectangle bgRect = new Rectangle(0, Height - 180, Width, 120);
                    int alpha = (int)(255 * _subtitleAlpha);
                    var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

                    string currentText = _introPhrases[_currentPhraseIndex];

                    // 1. Рисуем ТЕНЬ (черную), чтобы текст не терялся
                    TextRenderer.DrawText(g, currentText, subFont,
                        new Rectangle(bgRect.X + 2, bgRect.Y + 2, bgRect.Width, bgRect.Height),
                        Color.FromArgb(alpha, Color.Black), flags);

                    // 2. Рисуем основной текст (Azure)
                    TextRenderer.DrawText(g, currentText, subFont, bgRect,
                        Color.FromArgb(alpha, Color.Azure), flags);
                }
            }

            if (_controller.LevelManager.CurrentLevelIndex == 4)
            {
                // 1. Шанс жесткого "лага" (замирание кадра)
                if (_glitchRnd.Next(100) > 97) return;

                // 2. Смещение целых блоков экрана (разрыв реальности)
                for (int i = 0; i < _glitchRnd.Next(1, 5); i++)
                {
                    int startY = _glitchRnd.Next(0, Height);
                    int h = _glitchRnd.Next(10, 100);
                    int offset = _glitchRnd.Next(-30, 31);

                    // Рисуем полосу со сдвигом
                    g.CopyFromScreen(this.PointToScreen(new Point(0, startY)),
                                     new Point(offset, startY),
                                     new Size(Width, h));
                }

                // 3. Цветовые артефакты (вспышки)
                if (_glitchRnd.Next(100) > 90)
                {
                    Color glitchColor = _glitchRnd.Next(2) == 0 ? Color.FromArgb(50, Color.Red) : Color.FromArgb(50, Color.Lime);
                    using (SolidBrush b = new SolidBrush(glitchColor))
                    {
                        g.FillRectangle(b, 0, 0, Width, Height); // Вспышка на весь экран
                    }
                }

                // 4. "Битые пиксели"
                for (int i = 0; i < 10; i++)
                {
                    g.FillRectangle(Brushes.White, _glitchRnd.Next(Width), _glitchRnd.Next(Height), 2, 2);
                }
            }
        }

        private void DrawSubtitles(Graphics g)
        {

        }
        // Вспомогательный метод для NPC (вынесен из OnPaint для чистоты)
        // Вспомогательный метод для NPC (вынесен из OnPaint для чистоты)
        private void DrawNpc(Graphics g)
        {
            // 1. ПРОВЕРКА УРОВНЯ
            if (_controller.LevelManager.CurrentLevelIndex != 3) return;

            // Сбрасываем флаг звука, если NPC исчез или еще не появился, 
            // чтобы при следующем появлении звук сработал снова
            if (!_controller.IsNpcTriggered || _controller.IsNpcVanished)
            {
                _wasNpcSoundPlayed = false;
                return;
            }

            // 2. ЛОГИКА ЗВУКА (Срабатывает один раз при появлении)
            if (_controller.IsNpcTriggered && !_wasNpcSoundPlayed)
            {
                PlayDusSpawnSound(); // Вызов метода проигрывания
                _wasNpcSoundPlayed = true;
            }

            // 3. ОТРИСОВКА NPC
            if (_controller.IsNpcTriggered && !_controller.IsNpcVanished)
            {
                float alpha = _controller.GetNpcAlpha();
                Image sprite = _ghostImage;

                if (sprite != null)
                {
                    // --- ЭФФЕКТ УКАЧИВАНИЯ (Swaying) ---
                    double time = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    float offsetX = (float)Math.Sin(time * 2.0) * 15;
                    float offsetY = (float)Math.Cos(time * 1.5) * 10;

                    // --- ЭФФЕКТ ЛАГАНИЯ (Glitch) ---
                    if (_rng.Next(0, 100) > 90)
                    {
                        offsetX += _rng.Next(-20, 20);
                        alpha *= 0.5f;
                    }

                    // --- РАЗМЕР ---
                    int npcWidth = 450;
                    int npcHeight = (int)((float)npcWidth / sprite.Width * sprite.Height);

                    float drawX = _controller.NpcPosition.X + offsetX;
                    float drawY = _controller.NpcPosition.Y + offsetY;

                    using (var ia = new System.Drawing.Imaging.ImageAttributes())
                    {
                        // --- ЭФФЕКТ ТУМАННОСТИ (Прозрачность + Наложение) ---
                        var matrixLow = new System.Drawing.Imaging.ColorMatrix { Matrix33 = alpha * 0.3f };
                        ia.SetColorMatrix(matrixLow);

                        // "Призрачное эхо"
                        g.DrawImage(sprite,
                            new Rectangle((int)drawX - 15, (int)drawY, npcWidth, npcHeight),
                            0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, ia);

                        // Основное тело
                        var matrixFull = new System.Drawing.Imaging.ColorMatrix { Matrix33 = alpha };
                        ia.SetColorMatrix(matrixFull);

                        g.DrawImage(sprite,
                            new Rectangle((int)drawX, (int)drawY, npcWidth, npcHeight),
                            0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, ia);
                    }

                    // --- ТЕКСТ (Дрожащий) ---
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

        private void DrawPlatform(Graphics g, Rectangle rect)
        {
            var currentLevel = _controller.LevelManager.GetCurrentLevel();
            if (currentLevel == null) return;

            // Сравнение для лавы (теперь по расположению)
            bool isLava = currentLevel.GlowingPlatforms.Any(p => p.X == rect.X && p.Y == rect.Y);
            float pulse = (float)(Math.Sin(_waveAngle * 3f) * 0.2f + 0.8f);

            // Рисуем свечение с использованием дробных координат для плавности
            using (GraphicsPath path = new GraphicsPath())
            {
                // Используем RectangleF для более точного позиционирования свечения
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
                // ВАЖНО: Рисуем с сохранением качества при движении
                g.DrawImage(bookSprite, rect);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED — отрисовывает всё дерево элементов в буфер
                return cp;
            }
        }

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

        private void DrawAtmosphere(Graphics g)
        {
            foreach (var p in dustParticles)
                g.FillEllipse(Brushes.WhiteSmoke, p.X + (float)Math.Cos(_waveAngle * 0.5f) * 15, p.Y, 1.5f, 1.5f);
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

        private void DrawUnifiedSoftShadow(Graphics g, List<Rectangle> obstacles, LightSource light)
        {
            // Если список пуст, ничего не делаем
            if (obstacles == null || obstacles.Count == 0) return;

            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(75, 0, 0, 0)))
            {
                foreach (Rectangle rect in obstacles)
                {
                    // Вызываем твой ShadowEngine. Убедись, что передаешь верные параметры.
                    var poly = ShadowEngine.GetShadowPolygon(rect, light, Width, Height);
                    if (poly != null && poly.Length > 2)
                    {
                        g.FillPolygon(shadowBrush, poly);
                    }
                }
            }
        }

        private void DrawNotes(Graphics g)
        {
            var currentLevel = _controller.LevelManager.GetCurrentLevel();
            if (currentLevel?.Notes == null) return;

            // Плавная анимация парения (вверх-вниз)
            float floatAnim = (float)Math.Sin(_waveAngle * 2f) * 10f;

            foreach (var note in currentLevel.Notes.Where(n => !n.IsCollected))
            {
                // 1. Фиксированный размер слитка
                int fixWidth = 60;
                int fixHeight = 60;

                // 2. Центрируем и добавляем анимацию парения
                float x = note.Bounds.X + (note.Bounds.Width / 2) - (fixWidth / 2);
                float y = note.Bounds.Y + (note.Bounds.Height / 2) - (fixHeight / 2) + floatAnim;

                // Наша новая переменная, которую "видит" код ниже
                RectangleF drawRect = new RectangleF(x, y, fixWidth, fixHeight);

                // 3. Рисуем свечение вокруг фиксированного размера
                // Rectangle.Round конвертирует дробные координаты в целые для DrawGlow
                DrawGlow(g, Rectangle.Round(drawRect), Color.FromArgb(150, Color.Gold), 1.2f);

                if (noteSprite != null)
                {
                    // Рисуем картинку Ingot.png в границах drawRect
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(noteSprite, drawRect);
                }
                else
                {
                    // Запасной вариант, если картинка не загрузилась
                    g.FillRectangle(Brushes.Gold, drawRect);
                }
            }
        }
        private void DrawNoteUI(Graphics g)
        {
            if (_noteUIAlpha <= 0 || string.IsNullOrEmpty(_activeNoteText)) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Font font = new Font("Courier New", 14, FontStyle.Italic))
            {
                Size textSize = TextRenderer.MeasureText(_activeNoteText, font);
                // Делаем область облака значительно больше текста для мягкого размытия
                Rectangle cloudRect = new Rectangle(
                    Width / 2 - textSize.Width,
                    Height - 220,
                    textSize.Width * 2,
                    textSize.Height + 100
                );

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(cloudRect); // Облако будет эллипсом

                    using (PathGradientBrush shadowBrush = new PathGradientBrush(path))
                    {
                        // Настраиваем прозрачность в зависимости от _noteUIAlpha
                        int centralAlpha = (int)(200 * _noteUIAlpha);
                        shadowBrush.CenterColor = Color.FromArgb(centralAlpha, 5, 5, 10); // Почти черный
                        shadowBrush.SurroundColors = new Color[] { Color.Transparent }; // Края полностью прозрачные

                        g.FillPath(shadowBrush, path);
                    }
                }

                // Рисуем текст поверх облака
                int textAlpha = (int)(255 * _noteUIAlpha);
                using (Brush textBrush = new SolidBrush(Color.FromArgb(textAlpha, Color.GhostWhite)))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    // Рисуем в чуть меньшем прямоугольнике внутри облака
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

        private void DrawDarknessEffect(Graphics g, PointF playerPos)
        {
            using (Bitmap mask = new Bitmap(this.ClientSize.Width, this.ClientSize.Height))
            {
                using (Graphics maskG = Graphics.FromImage(mask))
                {
                    // Черный фон маски
                    maskG.Clear(Color.FromArgb(252, 10, 10, 15));

                    // Эффект дрожания свечи
                    float flicker = (float)Math.Sin(Environment.TickCount * 0.005) * 8;
                    float lightRadius = 280f + flicker;

                    using (GraphicsPath path = new GraphicsPath())
                    {
                        // Рисуем круг света вокруг переданной позиции игрока
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

