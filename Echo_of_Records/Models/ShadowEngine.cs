using System.Drawing;

namespace Echo_of_Records.Models
{
    public static class ShadowEngine
    {
        public static void DrawShadow(Graphics g, LightSource light, Obstacle obs)
        {
            var vertices = obs.GetVertices();

            for (int i = 0; i < vertices.Length; i++)
            {
                PointF v1 = vertices[i];
                PointF v2 = vertices[(i + 1) % vertices.Length];

                PointF dir1 = new PointF(v1.X - light.X, v1.Y - light.Y);
                PointF dir2 = new PointF(v2.X - light.X, v2.Y - light.Y);

                PointF[] shadowQuad = new PointF[]
                {
                    v1,
                    v2,
                    new PointF(v2.X + dir2.X * 2000, v2.Y + dir2.Y * 2000),
                    new PointF(v1.X + dir1.X * 2000, v1.Y + dir1.Y * 2000)
                };

                // Тень делаем глубокой черной
                g.FillPolygon(Brushes.Black, shadowQuad);
            }
        }
    }
}