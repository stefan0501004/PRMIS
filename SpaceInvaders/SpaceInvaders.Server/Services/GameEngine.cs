using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SpaceInvaders.Common.Game;
using SpaceInvaders.Common.Models;
using SpaceInvaders.Network;

namespace SpaceInvaders.Server.Services
{
    public class GameEngine : NetworkComponent
    {
        public GameEngine(GameConfiguration gameConfiguration, GameState gameState)
        {
            GameConfiguration = gameConfiguration;
            GameState = gameState;
            Clients = new List<(Socket Socket, EndPoint EndPoint)>();
            ProcessingFrame = false;
        }

        public GameState GameState { get; }
        public GameConfiguration GameConfiguration { get; }
        private List<(Socket Socket, EndPoint EndPoint)> Clients { get; }
        private bool ProcessingFrame { get; set; }

        public void StartGame()
        {
            // Send game configuration to clients
            foreach (var (socket, endPoint) in Clients) SendMessageUdp(socket, GameConfiguration, endPoint);

            GameState.IsGameStarted = true;
            StartGameLoop();
        }

        public void AddPlayer(Player player, Socket udpSocket, EndPoint clientEndPoint)
        {
            GameState.Players.Add(player);
            Clients.Add((udpSocket, clientEndPoint));
        }

        private void StartGameLoop()
        {
            // Start background threads to accept commands from each client
            foreach (var (socket, endPoint) in Clients)
            {
                var thread = new Thread(() => AcceptCommands(socket, endPoint));
                thread.Start();
            }

            // Game loop
            while (GameState.IsGameStarted)
            {
                ProcessingFrame = true;
                ProcessFrame();
                ProcessingFrame = false;
                Thread.Sleep(GameConfiguration.FrameRate);
            }
        }

        private void ProcessFrame()
        {
            // Check for win
            CheckForWin();

            // Update Game State
            UpdateProjectilePositions();
            UpdateObstaclePositions();
            GenerateNewObstacles();
            CheckCollisions();
            CheckForGameOver();

            // Network Updates
            SendGameState();

            // Control game speed
            Thread.Sleep(GameConfiguration.FrameRate);
        }

        private void CheckForWin()
        {
            if (GameState.Players.Any(p => p.Points >= GameConfiguration.RequiredPoints))
                GameState.IsGameStarted = false;
        }

        private void UpdateProjectilePositions()
        {
            foreach (var projectile in GameState.Projectiles)
            {
                // Update projectile map coverage
                projectile.MapCoverage += GameConfiguration.ProjectileMapCoveragePerFrame;

                // Update projectile position - It moves up the map
                var newY = GameConfiguration.MapHeight -
                           (int)Math.Floor(GameConfiguration.MapHeight * projectile.MapCoverage);
                projectile.Coordinates.Y = newY;
            }
        }

        private void UpdateObstaclePositions()
        {
            foreach (var obstacle in GameState.Obstacles)
            {
                // Update obstacle map coverage
                obstacle.MapCoverage += GameConfiguration.ObstacleMapCoveragePerFrame;

                // Update obstacle position - It moves down the map
                var newY = (int)Math.Floor(GameConfiguration.MapHeight * obstacle.MapCoverage);
                obstacle.Coordinates.Y = newY;
            }
        }

        private void GenerateNewObstacles()
        {
            var random = new Random();
            var roll = random.NextDouble();
            int numObstacles;

            switch (roll)
            {
                case double r when r < 0.75: // 75% chance
                    numObstacles = 0;
                    break;
                case double r when r < 0.85: // 10% chance
                    numObstacles = 1;
                    break;
                case double r when r < 0.95: // 10% chance
                    numObstacles = 2;
                    break;
                default: // 5% chance
                    numObstacles = 3;
                    break;
            }

            var rowsWithObstacles = new List<int>();
            for (var i = 0; i < numObstacles; i++)
            {
                // Random X position across the top row
                var x = random.Next(GameConfiguration.MapWidth);

                // Ensure no duplicate obstacles in the same row
                while (rowsWithObstacles.Contains(x)) x = random.Next(GameConfiguration.MapWidth);
                rowsWithObstacles.Add(x);

                // Random obstacle shape
                var shape = random.Next(2) == 0 ? ObstacleShape.Square : ObstacleShape.Circle;

                // Create new obstacle at top of map (y=0)
                var obstacle = new Obstacle(new Coordinates(x, 0), shape);

                GameState.Obstacles.Add(obstacle);
            }
        }

        private void CheckCollisions()
        {
            var obstaclesToRemove = new List<Obstacle>();
            var projectilesToRemove = new List<Projectile>();
            foreach (var obstacle in GameState.Obstacles)
            {
                // If the obstacle crossed the whole map, remove it
                if (obstacle.MapCoverage >= 1) obstaclesToRemove.Add(obstacle);

                // Check for collisions with players in the same row
                if (obstacle.Coordinates.Y == GameConfiguration.MapHeight - 1)
                {
                    var hitPlayer = GameState.Players.FirstOrDefault(p => p.Coordinates.X == obstacle.Coordinates.X);
                    if (hitPlayer != null)
                    {
                        // Take damage
                        hitPlayer.Lives -= 1;

                        // Remove player from game
                        if (hitPlayer.Lives == 0)
                            GameState.Players.Remove(hitPlayer);
                    }
                }

                // Check for collisions with projectiles in the same row
                var projectilesInTheSameRow =
                    GameState.Projectiles.Where(p => p.Coordinates.X == obstacle.Coordinates.X).ToList();
                foreach (var projectile in projectilesInTheSameRow)
                {
                    // Detect collision between projectile and obstacle
                    var heightDifference = projectile.Coordinates.Y - obstacle.Coordinates.Y;
                    if (heightDifference <= -3 || heightDifference >= 0) continue;
                    if (projectilesToRemove.Contains(projectile)) continue;

                    // Award points to shooting player
                    var player = GameState.Players.FirstOrDefault(p => p.Id == projectile.SenderId);
                    if (player != null) player.Points += 1;

                    // Remove obstacle and projectile
                    obstaclesToRemove.Add(obstacle);
                    projectilesToRemove.Add(projectile);
                    break;
                }
            }

            foreach (var obstacle in obstaclesToRemove) GameState.Obstacles.Remove(obstacle);
            foreach (var projectile in projectilesToRemove) GameState.Projectiles.Remove(projectile);
        }

        private void CheckForGameOver()
        {
            if (GameState.Players.Count == 0) GameState.IsGameStarted = false;
        }

        private void SendGameState()
        {
            foreach (var (socket, endPoint) in Clients) SendMessageUdp(socket, GameState, endPoint);
        }

        private void AcceptCommands(Socket socket, EndPoint endPoint)
        {
            while (GameState.IsGameStarted)
            {
                var command = ReceiveMessageUdp<Command>(socket, ref endPoint, 10);

                if (command == null) continue;

                while (ProcessingFrame) Thread.Sleep(10);
                switch (command.CommandType)
                {
                    case CommandType.MoveLeft:
                    case CommandType.MoveRight:
                        MovePlayer(command.PlayerId, command.CommandType);
                        break;
                    case CommandType.Shoot:
                        ShootProjectile(command.PlayerId);
                        break;
                }
            }
        }

        private void MovePlayer(int playerId, CommandType commandType)
        {
            var player = GameState.Players.FirstOrDefault(p => p.Id == playerId);
            Player otherPlayer;
            if (player == null) return;

            switch (commandType)
            {
                case CommandType.MoveLeft:
                    // If player is at the left edge, do nothing
                    if (player.Coordinates.X <= 0) return;
                    // This means we are not at the edge

                    // If single player, move left
                    if (GameConfiguration.GameMode == GameMode.SinglePlayer)
                    {
                        player.Coordinates.X -= 1;
                        return;
                    }
                    // This means we are in multiplayer mode

                    // If multiplayer, check if the other players position is to the left of the current player
                    otherPlayer = GameState.Players.FirstOrDefault(p => p.Id != playerId);

                    // If other player is dead, move left anyways
                    if (otherPlayer == null)
                    {
                        player.Coordinates.X -= 1;
                        return;
                    }
                    // This means other player is alive

                    // If other player is alive, and their position is not to the right of the current player, move left
                    if (otherPlayer.Coordinates.X > player.Coordinates.X)
                    {
                        player.Coordinates.X -= 1;
                        return;
                    }
                    // This means the other players position is to the left of the current player

                    // If other player is not at the edge, move left
                    if (otherPlayer.Coordinates.X > 0) player.Coordinates.X -= 1;
                    // This means the other player is at the edge so we can't move there
                    break;
                case CommandType.MoveRight:
                    // If player is at the right edge, do nothing
                    if (player.Coordinates.X >= GameConfiguration.MapWidth - 1) return;
                    // This means we are not at the edge

                    // If single player, move right
                    if (GameConfiguration.GameMode == GameMode.SinglePlayer)
                    {
                        player.Coordinates.X += 1;
                        return;
                    }
                    // This means we are in multiplayer mode

                    // If multiplayer, check if the other players position is to the right of the current player
                    otherPlayer = GameState.Players.FirstOrDefault(p => p.Id != playerId);

                    // If other player is dead, move right anyways
                    if (otherPlayer == null)
                    {
                        player.Coordinates.X += 1;
                        return;
                    }
                    // This means other player is alive

                    // If other player is alive, and their position is not to the right of the current player, move right
                    if (otherPlayer.Coordinates.X < player.Coordinates.X)
                    {
                        player.Coordinates.X += 1;
                        return;
                    }
                    // This means the other players position is to the right of the current player

                    // If other player is not at the edge, move right
                    if (otherPlayer.Coordinates.X < GameConfiguration.MapWidth - 1) player.Coordinates.X += 1;
                    // This means the other player is at the edge so we can't move there
                    break;
            }
        }

        private void ShootProjectile(int playerId)
        {
            var player = GameState.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null) GameState.Projectiles.Add(new Projectile(player, player.Coordinates));
        }
    }
}