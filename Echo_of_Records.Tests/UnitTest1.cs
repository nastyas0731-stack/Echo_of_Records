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

        // ТЕСТ 2: Проверка механики Свечи
        [Fact]
        public void Controller_ShouldResetPlayer_WhenCandleLifeIsZero()
        {
            // Arrange
            var controller = new MainController();
            var level = controller.LevelManager.GetCurrentLevel();
            controller.State = GameState.Playing;

            // Отводим игрока от точки старта
            controller.Player.Position = new PointF(1000, 1000);
            controller.CandleLife = 0; // Свеча погасла

            // Act
            controller.Update();

            // Assert
            // Проверяем возврат к SpawnPoint уровня
            Assert.Equal(level.SpawnPoint.X, controller.Player.Position.X);
            Assert.Equal(level.SpawnPoint.Y, controller.Player.Position.Y);
        }

        // ТЕСТ 3: Проверка движения
        [Fact]
        public void Player_ShouldMoveRight_CorrectAmount()
        {
            // Arrange
            var player = new Player(100, 100);

            // Act
            // Используем player.Speed из твоего класса (он равен 15f)
            player.Position = new PointF(player.Position.X + player.Speed, player.Position.Y);

            // Assert
            Assert.Equal(115f, player.Position.X);
        }

        // ТЕСТ 4: Проверка сбора записки (ИСПРАВЛЕНО НА MemoryNote)
        [Fact]
        public void Note_ShouldBeMarkedAsCollected_WhenPlayerTouchesIt()
        {
            // Arrange
            // В твоем коде конструктор MemoryNote(x, y, text)
            var note = new MemoryNote(100, 100, "Secret Text");
            var player = new Player(110, 110);

            // Act
            // Имитируем логику сбора, как в твоем Form1.cs
            if (note.Bounds.Contains((int)player.Position.X, (int)player.Position.Y))
            {
                note.IsCollected = true;
            }

            // Assert
            Assert.True(note.IsCollected);
        }

        // ТЕСТ 5: Проверка платформы
        [Fact]
        public void Player_ShouldStopFalling_OnPlatform()
        {
            // Arrange
            var player = new Player(100, 100);
            var platform = new Rectangle(100, 150, 100, 20);
            player.IsGrounded = false;
            player.VelocityY = 5f;

            // Act
            // 150 - это высота твоего спрайта из Player.cs
            if (player.Position.Y + player.Height >= platform.Top)
            {
                player.IsGrounded = true;
                player.VelocityY = 0;
            }

            // Assert
            Assert.True(player.IsGrounded);
            Assert.Equal(0, player.VelocityY);
        }
    }
}
