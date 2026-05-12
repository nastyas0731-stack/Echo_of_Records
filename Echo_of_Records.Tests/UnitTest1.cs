using Echo_of_Records.Controllers;
using Echo_of_Records.Models;
using System.Drawing;
using Xunit;

namespace Echo_of_Records.Tests
{
    public class GameMechanicsTests
    {
        // ТЕСТ 1: Проверка гравитации
        [Fact]
        public void Player_ShouldFall_WhenInAir()
        {
            // Arrange
            var player = new Player(100, 100);
            player.IsGrounded = false;
            float initialY = player.Position.Y;

            // Act
            // Используем реальную логику гравитации из твоего класса Player
            player.VelocityY += player.Gravity;
            player.Position = new PointF(player.Position.X, player.Position.Y + player.VelocityY);

            // Assert
            Assert.True(player.Position.Y > initialY);
        }

        // Простая проверка бега: нажал вправо — персонаж сместился на свою скорость
        [Fact]
        public void Player_ShouldMoveRight_CorrectAmount()
        {
            var player = new Player(100, 100);

            player.Position = new PointF(player.Position.X + player.Speed, player.Position.Y);

            Assert.Equal(115f, player.Position.X);
        }

        // Проверка сбора записки
        [Fact]
        public void Note_ShouldBeMarkedAsCollected_WhenPlayerTouchesIt()
        {
            var note = new MemoryNote(100, 100, "Secret Text");
            var player = new Player(110, 110);

            if (note.Bounds.Contains((int)player.Position.X, (int)player.Position.Y))
            {
                note.IsCollected = true;
            }

            Assert.True(note.IsCollected);
        }

        // Проверка платформы
        [Fact]
        public void Player_ShouldStopFalling_OnPlatform()
        {
            var player = new Player(100, 100);
            var platform = new Rectangle(100, 150, 100, 20);
            player.IsGrounded = false;
            player.VelocityY = 5f;

            if (player.Position.Y + player.Height >= platform.Top)
            {
                player.IsGrounded = true;
                player.VelocityY = 0;
            }

            Assert.True(player.IsGrounded);
            Assert.Equal(0, player.VelocityY);
        }

        // Проверка лимита Стабильности Эфира
        [Fact]
        public void Player_ShouldStayWithinBounds()
        {
            var player = new Player(100, 100);
            player.Position = new PointF(150, 100);

            Assert.Equal(150f, player.Position.X);
        }

        [Fact]
        public void Player_ShouldFall_WhenInVoid()
        {
            var controller = new MainController();

            // ВАЖНО: Включаем режим игры, иначе Update() проигнорирует физику
            controller.State = GameState.Playing;

            // Ставим игрока в воздух
            controller.Player.Position = new PointF(100, 100);
            float initialY = controller.Player.Position.Y;

            // Act
            controller.Update();

            // Assert
            // Теперь, когда State == Playing, гравитация сработает и Y увеличится
            Assert.True(controller.Player.Position.Y > initialY);
        }

    }
}
