using System;

namespace SpaceInvaders.Common.Models
{
    [Serializable]
    public class Player
    {
        public Player(string name, string surname, Coordinates coordinates = null)
        {
            Name = name;
            Surname = surname;
            Coordinates = coordinates == null ? new Coordinates() : new Coordinates(coordinates);
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Points { get; set; } = 0;
        public int Lives { get; set; } = 3;
        public Coordinates Coordinates { get; set; }
    }
}