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

        // В LevelData.cs добавь новый список
        public List<Rectangle> GlowingPlatforms { get; set; } = new List<Rectangle>();

        // В классе, где ты описываешь уровни (LevelData)
        public List<Rectangle> LavaPlatforms { get; set; } = new List<Rectangle>();

        public PointF FinishPoint { get; set; }
        public List<MemoryNote> Notes { get; set; } = new List<MemoryNote>();

        // И GlowingPlatforms тоже проверь на всякий случай
        public List<Rectangle> Platforms { get; set; } = new List<Rectangle>();


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