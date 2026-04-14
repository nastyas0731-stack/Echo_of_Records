using System.Collections.Generic;
using System.Drawing;

namespace Echo_of_Records.Models
{
    public class LevelData
    {
        // #Platforms: Список всех физических объектов (книг/полок)
        public List<Rectangle> Obstacles { get; set; } = new List<Rectangle>();

        // #SpawnPoint: Где дух появляется в начале
        public Point SpawnPoint { get; set; }

        // #ExitPoint: Зона перехода на следующий уровень
        public Rectangle ExitGate { get; set; }

        // #LevelName: Название для UI
        public string Name { get; set; }
    }
}