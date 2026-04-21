using System.Drawing;
using System.Linq;

namespace Echo_of_Records.Utils
{
    public static class Physics
    {
        public static bool IsPointInPolygon(PointF p, PointF[] poly)
        {
            bool isInside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].Y > p.Y) != (poly[j].Y > p.Y)) &&
                (p.X < (poly[j].X - poly[i].X) * (p.Y - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X))
                {
                    isInside = !isInside;
                }
            }
            return isInside;
        }
    }
}