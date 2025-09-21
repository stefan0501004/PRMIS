using System;

namespace SpaceInvaders.Common.Models
{
    [Serializable]
    public class Command
    {
        public Command(int playerId, CommandType commandType)
        {
            PlayerId = playerId;
            CommandType = commandType;
        }
        
        public int PlayerId { get; set; }
        public CommandType CommandType { get; set; }
    }

    [Serializable]
    public enum CommandType
    {
        MoveLeft,
        MoveRight,
        Shoot
    }
}