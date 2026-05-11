using System.Collections.Generic;
using System.Drawing;
using Echo_of_Records.Models;

namespace Echo_of_Records.Controllers
{
    public class LevelManager
    {
        private List<LevelData> levels;
        public int CurrentLevelIndex { get; private set; } = 0;

        public LevelManager()
        {
            levels = new List<LevelData>();
            InitializeLevels();
        }

        private void InitializeLevels()
        {
            // --- УРОВЕНЬ 1 ---
            var level1 = new LevelData("Уровень 1", new PointF(100, 500), new Rectangle(1800, 400, 100, 200));
            level1.Obstacles.Add(new Rectangle(300, 600, 200, 50));
            level1.Obstacles.Add(new Rectangle(1200, 500, 250, 50));
            level1.MovingPlatforms.Add(new MovingObstacle(new RectangleF(600, 450, 200, 50), 1000f, 3f));
            levels.Add(level1);

            // --- УРОВЕНЬ 2 (Гранд-Библиотека) ---
            var level2 = new LevelData("Гранд-Библиотека", new PointF(100, 750), new Rectangle(1850, 150, 120, 120));

            // 1. БЕЗОПАСНЫЕ ПЛАТФОРМЫ
            level2.Obstacles.Add(new Rectangle(20, 800, 150, 25));   // Стартовая
            level2.Obstacles.Add(new Rectangle(250, 700, 100, 20));  // Первая ступенька
            level2.Obstacles.Add(new Rectangle(300, 500, 120, 25));  // Прыжок выше
            level2.Obstacles.Add(new Rectangle(700, 450, 150, 25));  // Центр
            level2.Obstacles.Add(new Rectangle(1100, 550, 150, 25)); // Дальняя
            level2.Obstacles.Add(new Rectangle(1750, 300, 200, 30)); // Перед финишем

            // 2. ТЕ САМЫЕ ДВЕ ОПАСНЫЕ ПЛАТФОРМЫ (Бледно-желтые с уроном)
            // Платформа 1: Ловушка на пути к центру
            Rectangle lava1 = new Rectangle(100, 600, 120, 20);
            level2.Obstacles.Add(lava1);           // Добавляем физику
            level2.GlowingPlatforms.Add(lava1);    // Добавляем желтый свет и урон

            // Платформа 2: Опасный уступ чуть выше
            Rectangle lava2 = new Rectangle(400, 600, 120, 20);
            level2.Obstacles.Add(lava2);           // Добавляем физику
            level2.GlowingPlatforms.Add(lava2);    // Добавляем желтый свет и урон

           

            levels.Add(level2);

            // Положим записку на ту самую опасную нежно-желтую платформу для риска
            level2.Notes.Add(new MemoryNote(440, 550, "День 42. Книги начали шептать. Или это просто сквозняк из обсерватории?"));

            // Внутри InitializeLevels для Level 2:

            // Правильный вариант для LevelManager
            level2.Notes.Add(new MemoryNote(870, 480, "День 56. Старые чертежи обсерватории врут..."));
            level2.Notes.Add(new MemoryNote(1820, 250, "Запись без даты. Я слышу, как слитки гудят в тишине..."));

            level1.FinishPoint = new PointF(1800, 500);


            // Если хочешь выход справа, как на 1 уровне:
            var level3 = new LevelData("Обсерватория", new PointF(450, 750), new RectangleF(1600, 250, 150, 100));

            // ИСПРАВЛЕНИЕ: Используем Obstacles (как в других уровнях), если в LevelData нет поля Platforms
            level3.Obstacles.Add(new Rectangle(300, 850, 150, 40)); // ПОЛ: Магический круг
            level3.Obstacles.Add(new Rectangle(50, 620, 150, 20));  // ПЕРВЫЙ ЯРУС
            level3.Obstacles.Add(new Rectangle(600, 480, 150, 20)); // ВТОРОЙ ЯРУС
            level3.Obstacles.Add(new Rectangle(400, 320, 200, 20)); // ТРЕТИЙ ЯРУС
            level3.Obstacles.Add(new Rectangle(350, 150, 200, 20)); // ВЫХОД

            level3.Obstacles.Add(new Rectangle(1550, 350, 200, 25));

            /// --- УРОВЕНЬ 3 (Обсерватория) ---
            // Отдельная книга-алтарь справа
            level3.Obstacles.Add(new Rectangle(1200, 400, 100, 20));

            // ИСПРАВЛЕНО: Передаем координаты X, Y и Текст отдельно, как того требует конструктор MemoryNote
            level3.Notes.Add(new MemoryNote(1220, 340, "«Всё, что ты видишь — лишь запись в чьём-то дневнике. \nТы — не призрак, ты — воспоминание, которое не хочет исчезать»."));
            level3.Notes.Add(new MemoryNote(435, 90,
            "«Ты ищешь выход из библиотеки, но посмотри на свои руки... они из чернил. \n" +
            "Ты — не гость в этой истории, ты — её последний, недописанный абзац. \n" +
            "Если ты уйдёшь сейчас, книга закроется навсегда»."));
            levels.Add(level3);

            var level4 = new LevelData("Залы Пустоты", new PointF(100, 800), new RectangleF(1750, 750, 150, 150));

            // Добавим пол, чтобы игрок не падал в бесконечность сразу при появлении
            level4.Obstacles.Add(new Rectangle(0, 900, 2000, 50));
            level4.Obstacles.Add(new Rectangle(50, 850, 200, 20)); // Стартовая платформа

            // Добавляем тот самый свиток, который мы ждем в контроллере
            level4.Notes.Add(new MemoryNote(600,790, "Ты зашел слишком далеко..."));

            levels.Add(level4);

            // --- УРОВЕНЬ 5 (Сердце Архива) ---
            // Начало внизу слева, финиш вверху справа
            var level5 = new LevelData("Сердце Архива", new PointF(100, 850), new RectangleF(1700, 150, 120, 120));

            // 1. Стартовая опора
            level5.Obstacles.Add(new Rectangle(50, 900, 250, 30));

            // 2. Разбросанные в темноте платформы (игроку придется прыгать "в никуда", ища их светом)
            level5.Obstacles.Add(new Rectangle(400, 750, 150, 20));  // Платформа повыше
            level5.Obstacles.Add(new Rectangle(150, 600, 120, 20));  // Платформа слева
            level5.Obstacles.Add(new Rectangle(500, 450, 180, 25));  // Центральный мостик
            level5.Obstacles.Add(new Rectangle(900, 550, 150, 20));  // Далекий остров
            level5.Obstacles.Add(new Rectangle(1300, 400, 120, 20)); // Путь к финалу
            level5.Obstacles.Add(new Rectangle(1650, 250, 200, 30)); // Площадка перед выходом

            // 3. Движущаяся платформа (сюрприз в темноте)
            level5.MovingPlatforms.Add(new MovingObstacle(new RectangleF(800, 300, 150, 20), 1200f, 4f));

            // 4. Финальная записка
            level5.Notes.Add(new MemoryNote(1700, 200, "«Свет — это не то, что ты несешь в руках. \nСвет — это то, что остается, когда ты закрываешь глаза. \nТы дошел до конца своей записи...»"));

            levels.Add(level5);
        }


        public LevelData GetCurrentLevel() => levels[CurrentLevelIndex];

        public void NextLevel()
        {
            if (CurrentLevelIndex < levels.Count - 1) CurrentLevelIndex++;
        }
    }
}