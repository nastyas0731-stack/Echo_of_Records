using System.Drawing;

namespace Echo_of_Records.Models
{
    public class MovingObstacle
    {
        public RectangleF Bounds;
        public float StartX;
        public float EndX;
        public float Speed;
        private int _direction = 1;

        public PointF CurrentVelocity { get; private set; }

        public MovingObstacle(RectangleF rect, float endX, float speed)
        {
            Bounds = rect;
            StartX = rect.X;
            EndX = endX;
            Speed = speed;
            CurrentVelocity = new PointF(0, 0);
        }

        public void Update()
        {
            float moveStep = Speed * _direction;
            CurrentVelocity = new PointF(moveStep, 0);

            Bounds.X += moveStep;

            if (Bounds.X >= EndX)
            {
                Bounds.X = EndX;
                _direction = -1;
            }
            else if (Bounds.X <= StartX)
            {
                Bounds.X = StartX;
                _direction = 1;
            }
        }
    }
}