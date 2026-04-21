using System.Collections.Generic;
using System.Drawing;

namespace Echo_of_Records.Models
{
    public class LevelData
    {
        public string Name { get; set; }

        // Точка появления игрока
        public PointF SpawnPoint { get; set; }

        // Зона финиша (портал/черное пятно)
        public RectangleF FinishZone { get; set; }

        // Статические препятствия (книги, полки), отбрасывающие тень
        public List<Rectangle> Obstacles { get; set; }

        // Движущиеся платформы (летающие книги)
        public List<MovingObstacle> MovingPlatforms { get; set; }

        public LevelData(string name, PointF spawn, RectangleF finish)
        {
            Name = name;
            SpawnPoint = spawn;
            FinishZone = finish;

            Obstacles = new List<Rectangle>();
            MovingPlatforms = new List<MovingObstacle>();
        }
    }
}