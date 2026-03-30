using Echo_of_Records.Models;

namespace Echo_of_Records.Controllers
{
    public class MainController
    {
        public LightSource Light { get; private set; }
        public List<Obstacle> Obstacles { get; private set; }

        public MainController()
        {
            Light = new LightSource(400, 300);
            Obstacles = new List<Obstacle>();

            // Добавим пару объектов для теста
            Obstacles.Add(new Obstacle(200, 200, 120, 40));
            Obstacles.Add(new Obstacle(500, 350, 60, 180));
        }

        public void UpdateLightPosition(float x, float y)
        {
            Light.X = x;
            Light.Y = y;
        }
    }
}