using System.Collections.Generic;
using System.Drawing;
using Echo_of_Records.Models;

namespace Echo_of_Records.Controllers
{
    public class LevelManager
    {
        public List<LevelData> Levels { get; private set; }
        public int CurrentLevelIndex { get; private set; } = 0;

        public LevelManager()
        {
            Levels = new List<LevelData>();
            InitLevels();
        }

        private void InitLevels()
        {
            Levels.Clear();

            // УРОВЕНЬ 1
            Levels.Add(new LevelData
            {
                Name = "Level 1",
                SpawnPoint = new Point(100, 300),
                ExitGate = new Rectangle(700, 100, 60, 90),
                Obstacles = new List<Rectangle>
                {
                    new Rectangle(50, 450, 250, 80),
                    new Rectangle(400, 250, 200, 70)
                }
            });
        } 

        public LevelData GetCurrentLevel() => Levels[CurrentLevelIndex];

        public void NextLevel()
        {
            if (CurrentLevelIndex < Levels.Count - 1)
                CurrentLevelIndex++;
        }
    }
}