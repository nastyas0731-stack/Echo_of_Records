using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Echo_of_Records.Models
{
    public static class ShadowEngine
    {
        private static float dist = 4000f;

        public static PointF[] GetShadowPolygon(Rectangle rect, LightSource light, int screenWidth, int screenHeight)
        {
            if (light == null) return Array.Empty<PointF>();
            if (rect.Contains((int)light.X, (int)light.Y)) return Array.Empty<PointF>();

            PointF lPos = new PointF(light.X, light.Y);

            PointF[] corners = new PointF[]
            {
                new PointF(rect.Left, rect.Top),
                new PointF(rect.Right, rect.Top),
                new PointF(rect.Right, rect.Bottom),
                new PointF(rect.Left, rect.Bottom)
            };

            int minIdx = -1;
            int maxIdx = -1;
            float minAngle = float.MaxValue;
            float maxAngle = float.MinValue;

            foreach (var p in corners)
            {
                float angle = (float)Math.Atan2(p.Y - lPos.Y, p.X - lPos.X);

                if (angle < minAngle) minAngle = angle;
                if (angle > maxAngle) maxAngle = angle;
            }

            var sorted = corners
                .Select(c => new { Point = c, Angle = (float)Math.Atan2(c.Y - lPos.Y, c.X - lPos.X) })
                .OrderBy(c => c.Angle)
                .ToList();

            float maxGap = -1;
            int startIdx = 0;

            for (int i = 0; i < sorted.Count; i++)
            {
                int next = (i + 1) % sorted.Count;
                float diff = sorted[next].Angle - sorted[i].Angle;
                if (diff < 0) diff += (float)Math.PI * 2;

                if (diff > maxGap)
                {
                    maxGap = diff;
                    startIdx = next;
                }
            }

            PointF p1 = sorted[startIdx].Point;
            PointF p2 = sorted[(startIdx + sorted.Count - 1) % sorted.Count].Point;

            float a1 = (float)Math.Atan2(p1.Y - lPos.Y, p1.X - lPos.X);
            float a2 = (float)Math.Atan2(p2.Y - lPos.Y, p2.X - lPos.X);

            PointF f1 = new PointF(p1.X + (float)Math.Cos(a1) * dist, p1.Y + (float)Math.Sin(a1) * dist);
            PointF f2 = new PointF(p2.X + (float)Math.Cos(a2) * dist, p2.Y + (float)Math.Sin(a2) * dist);

            return new PointF[] { p1, f1, f2, p2 };
        }
    }
}