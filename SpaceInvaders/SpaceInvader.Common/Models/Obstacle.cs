using System;

namespace SpaceInvaders.Common.Models
{
    [Serializable]
    public class Obstacle
    {
        public Obstacle(Coordinates coordinates, ObstacleShape shape = ObstacleShape.Square)
        {
            Coordinates = new Coordinates(coordinates);
            Shape = shape;
            MapCoverage = 0;
        }

        public Coordinates Coordinates { get; set; }
        public ObstacleShape Shape { get; set; }
        public bool IsDestroyed { get; set; }
        public double MapCoverage { get; set; }

        public string GetDisplaySymbol()
        {
            switch (Shape)
            {
                case ObstacleShape.Square:
                    return "[[]]";
                case ObstacleShape.Circle:
                    return "()";
                default:
                    return "[[]]";
            }
        }
    }

    [Serializable]
    public enum ObstacleShape
    {
        Square,
        Circle
    }
}