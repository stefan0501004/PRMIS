using System;
using System.Collections.Generic;
using SpaceInvaders.Common.Models;

namespace SpaceInvaders.Common.Game
{
    [Serializable]
    public class GameState
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Obstacle> Obstacles { get; set; } = new List<Obstacle>();
        public List<Projectile> Projectiles { get; set; } = new List<Projectile>();
        public bool IsGameStarted { get; set; }

        public void Update(GameState gameState)
        {
            Players = gameState.Players;
            Obstacles = gameState.Obstacles;
            Projectiles = gameState.Projectiles;
            IsGameStarted = gameState.IsGameStarted;
        }
    }
}