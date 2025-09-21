using System;

namespace SpaceInvaders.Common.Models
{
    [Serializable]
    public class Coordinates
    {
        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Coordinates(Coordinates coordinates)
        {
            X = coordinates.X;
            Y = coordinates.Y;
        }

        public Coordinates()
        {
            X = 0;
            Y = 0;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public void UpdateCoordinates(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}