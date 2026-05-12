using Echo_of_Records.Models;
using Echo_of_Records.Controllers;
using System.Drawing;
using System.Linq;
using Xunit;
using System;

namespace Echo_of_Records.Tests
{


    public class MovementTests
    {
        // ПРОВЕРКА: Инициализируется ли игрок в правильных координатах.
        [Fact]
        public void Player_ShouldStartAtSpawnPoint()
        {
            var player = new Player(100f, 200f);

            Assert.Equal(100f, player.Position.X);
            Assert.Equal(200f, player.Position.Y);
        }

        // ПРОВЕРКА: Работает ли визуальный эффект «парения» духа.
        [Fact]
        public void Player_Update_ShouldChangeVisualY()
        {
            var player = new Player(100, 200);
            float initialVisualY = player.VisualY;

            player.Update();

            Assert.NotEqual(initialVisualY, player.VisualY);
        }

        // ПРОВЕРКА: Генерирует ли движок теней полигон из 4 точек.
        [Fact]
        public void GetShadowPolygon_ShouldReturnCorrectPoints()
        {
            var rect = new Rectangle(100, 100, 50, 50);
            var light = new LightSource(0, 0);

            var polygon = ShadowEngine.GetShadowPolygon(rect, light, 800, 600);

            Assert.NotNull(polygon);
            Assert.Equal(4, polygon.Length);
        }

        // ПРОВЕРКА: Не ломается ли движок, если источник света оказался внутри объекта.
        [Fact]
        public void GetShadowPolygon_ShouldReturnEmpty_WhenLightInsideObstacle()
        {
            var rect = new Rectangle(100, 100, 100, 100);
            var light = new LightSource(150, 150);

            var polygon = ShadowEngine.GetShadowPolygon(rect, light, 800, 600);

            Assert.Empty(polygon);
        }

        // ПРОВЕРКА: Улетают ли лучи тени на заданное расстояние
        [Fact]
        public void Shadow_ShouldExtendFarAway()
        {
            var rect = new Rectangle(100, 100, 10, 10);
            var light = new LightSource(50, 50);
            float expectedDist = 4000f;

            var polygon = ShadowEngine.GetShadowPolygon(rect, light, 800, 600);

            var p1 = polygon[0];
            var f1 = polygon[1];

            double actualDist = Math.Sqrt(Math.Pow(f1.X - p1.X, 2) + Math.Pow(f1.Y - p1.Y, 2));

            Assert.Equal(expectedDist, (float)actualDist, 1);
        }

        // ПРОВЕРКА: Находится ли игрок под воздействием света, если его ничего не закрывает.
        [Fact]
        public void Player_IsExposedToLight_WhenNoShadowsPresent()
        {
            // Arrange
            var playerPos = new PointF(400f, 300f);
            var light = new LightSource(0f, 0f);
            var obstacles = new List<Rectangle>();

            bool isUnderLight = !obstacles.Any(rect =>
                IsPointInPolygon(playerPos, ShadowEngine.GetShadowPolygon(rect, light, 800, 600)));

            Assert.True(isUnderLight, "Игрок должен быть освещен, если нет препятствий");
        }

        // ПРОВЕРКА: Проверка базовой геометрии тени.
        [Fact]
        public void Shadow_ShouldBeGeneratedCorrectly()
        {
            var rect = new Rectangle(100, 100, 50, 50);
            var light = new LightSource(0f, 0f);

            var polygon = ShadowEngine.GetShadowPolygon(rect, light, 800, 600);

            Assert.NotNull(polygon);
            Assert.Equal(4, polygon.Length);
        }

        // ПРОВЕРКА: Проверка плавного визуального эффекта (свечения) игрока.
        [Fact]
        public void Player_GlowAlpha_ShouldStayWithinBounds()
        {
            var player = new Player(100f, 100f);

            player.CurrentAlpha = 0.5f;
            player.Update();

            Assert.True(player.GlowAlpha >= 0f && player.GlowAlpha <= 1.0f);
        }

        // Вспомогательный метод для проверки попадания в тень 
        private bool IsPointInPolygon(PointF point, PointF[] polygon)
        {
            if (polygon.Length == 0) return false;
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < point.Y && polygon[j].Y >= point.Y || polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
                {
                    if (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < point.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
    }
}
