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
            // Уровень 1
            var level1 = new LevelData("Уровень 1", new PointF(100, 500), new Rectangle(1800, 400, 100, 200));
            level1.Obstacles.Add(new Rectangle(300, 600, 200, 50));
            level1.Obstacles.Add(new Rectangle(1200, 500, 250, 50));
            level1.MovingPlatforms.Add(new MovingObstacle(new RectangleF(600, 450, 200, 50), 1000f, 3f));
            levels.Add(level1);

            // Уровень 2 (Гранд-Библиотека)
            var level2 = new LevelData("Гранд-Библиотека", new PointF(100, 700), new Rectangle(1850, 500, 100, 200));
            level2.Obstacles.Add(new Rectangle(0, 850, 2000, 100)); // Пол
            levels.Add(level2);
        }

        public LevelData GetCurrentLevel() => levels[CurrentLevelIndex];

        public void NextLevel()
        {
            if (CurrentLevelIndex < levels.Count - 1) CurrentLevelIndex++;
        }
    }
}