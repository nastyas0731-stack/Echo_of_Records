using System.Drawing;

namespace Echo_of_Records.Models
{
    public class Obstacle
    {
        // Прямоугольник, который описывает границы объекта
        public RectangleF Rect { get; set; }

        public Obstacle(float x, float y, float width, float height)
        {
            Rect = new RectangleF(x, y, width, height);
        }

       
        public PointF[] GetVertices()
        {
            return new PointF[]
            {
                new PointF(Rect.Left, Rect.Top),     // Левый верхний
                new PointF(Rect.Right, Rect.Top),    // Правый верхний
                new PointF(Rect.Right, Rect.Bottom), // Правый нижний
                new PointF(Rect.Left, Rect.Bottom)   // Левый нижний
            };
        }
    }
}