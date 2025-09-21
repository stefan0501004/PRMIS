using System;

namespace SpaceInvaders.Common.Models
{
    [Serializable]
    public class Projectile
    {
        public Projectile(Player sender, Coordinates coordinates)
        {
            Coordinates = new Coordinates(coordinates);
            SenderId = sender.Id;
            DisplaySymbol = "^";
            MapCoverage = 0;
        }

        public Coordinates Coordinates { get; set; }
        public int SenderId { get; }
        public string DisplaySymbol { get; }
        public double MapCoverage { get; set; }
    }
}