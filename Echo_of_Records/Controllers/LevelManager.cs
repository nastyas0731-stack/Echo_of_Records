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

            // --- УРОВЕНЬ 2  ---
            var level2 = new LevelData("Гранд-Библиотека", new PointF(100, 750), new Rectangle(1850, 150, 120, 120));

            level2.Obstacles.Add(new Rectangle(20, 800, 150, 25));
            level2.Obstacles.Add(new Rectangle(250, 700, 100, 20));
            level2.Obstacles.Add(new Rectangle(300, 500, 120, 25));
            level2.Obstacles.Add(new Rectangle(700, 450, 150, 25));
            level2.Obstacles.Add(new Rectangle(1100, 550, 150, 25));
            level2.Obstacles.Add(new Rectangle(1750, 300, 200, 30));


            Rectangle lava1 = new Rectangle(100, 600, 120, 20);
            level2.Obstacles.Add(lava1);          
            level2.GlowingPlatforms.Add(lava1);    

            Rectangle lava2 = new Rectangle(400, 600, 120, 20);
            level2.Obstacles.Add(lava2);           
            level2.GlowingPlatforms.Add(lava2);   

           

            levels.Add(level2);

            level2.Notes.Add(new MemoryNote(440, 550, "День 42. Книги начали шептать. Или это просто сквозняк из обсерватории?"));


            level2.Notes.Add(new MemoryNote(870, 480, "День 56. Старые чертежи обсерватории врут..."));
            level2.Notes.Add(new MemoryNote(1820, 250, "Запись без даты. Я слышу, как слитки гудят в тишине..."));

            level1.FinishPoint = new PointF(1800, 500);

            // --- УРОВЕНЬ 3  ---
            var level3 = new LevelData("Обсерватория", new PointF(450, 750), new RectangleF(1600, 250, 150, 100));

            level3.Obstacles.Add(new Rectangle(300, 850, 150, 40));
            level3.Obstacles.Add(new Rectangle(50, 620, 150, 20));
            level3.Obstacles.Add(new Rectangle(600, 480, 150, 20));
            level3.Obstacles.Add(new Rectangle(400, 320, 200, 20));
            level3.Obstacles.Add(new Rectangle(350, 150, 200, 20));

            level3.Obstacles.Add(new Rectangle(1550, 350, 200, 25));

            level3.Obstacles.Add(new Rectangle(1200, 400, 100, 20));

            level3.Notes.Add(new MemoryNote(1220, 340, "«Всё, что ты видишь — лишь запись в чьём-то дневнике. \nТы — не призрак, ты — воспоминание, которое не хочет исчезать»."));
            level3.Notes.Add(new MemoryNote(435, 90,
            "«Ты ищешь выход из библиотеки, но посмотри на свои руки... они из чернил. \n" +
            "Ты — не гость в этой истории, ты — её последний, недописанный абзац. \n" +
            "Если ты уйдёшь сейчас, книга закроется навсегда»."));
            levels.Add(level3);

            // --- УРОВЕНЬ 4  ---
            var level4 = new LevelData("Залы Пустоты", new PointF(100, 800), new RectangleF(1750, 750, 150, 150));

            level4.Obstacles.Add(new Rectangle(0, 900, 2000, 50));
            level4.Obstacles.Add(new Rectangle(50, 850, 200, 20));

            level4.Notes.Add(new MemoryNote(600,790, "Ты зашел слишком далеко..."));

            levels.Add(level4);

            // --- УРОВЕНЬ 5  ---
            var level5 = new LevelData("Сердце Архива", new PointF(100, 850), new RectangleF(1700, 150, 120, 120));

            level5.Obstacles.Add(new Rectangle(50, 900, 250, 30));

            level5.Obstacles.Add(new Rectangle(400, 750, 150, 20));
            level5.Obstacles.Add(new Rectangle(150, 600, 120, 20));
            level5.Obstacles.Add(new Rectangle(500, 450, 180, 25));
            level5.Obstacles.Add(new Rectangle(900, 550, 150, 20));
            level5.Obstacles.Add(new Rectangle(1300, 400, 120, 20));
            level5.Obstacles.Add(new Rectangle(1650, 250, 200, 30));

            level5.MovingPlatforms.Add(new MovingObstacle(new RectangleF(800, 300, 150, 20), 1200f, 4f));

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