using System;

namespace SpaceInvaders.Common.Game
{
    [Serializable]
    public class GameConfiguration
    {
        public GameConfiguration(int mapWidth, int mapHeight, int requiredPoints, GameMode gameMode, int fps,
            double projectileSecondsToCrossMap, double obstacleSecondsToCrossMap)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            RequiredPoints = requiredPoints;
            GameMode = gameMode;
            Fps = fps;
            FrameRate = 1000 / Fps;

            ProjectileSecondsToCrossMap = projectileSecondsToCrossMap;
            // var projectileSpeed = (int)(ProjectileSecondsToCrossMap * 1000) / MapHeight; // 1s to cross the map
            ProjectileMapCoveragePerFrame = FrameRate * 2 / (ProjectileSecondsToCrossMap * 1000);

            ObstacleSecondsToCrossMap = obstacleSecondsToCrossMap;
            // var obstacleSpeed = (int)(ObstacleSecondsToCrossMap * 1000) / MapHeight; // 5s to cross the map
            ObstacleMapCoveragePerFrame = FrameRate * 2 / (ObstacleSecondsToCrossMap * 1000);
        }

        public int MapWidth { get; }
        public int MapHeight { get; }
        public int RequiredPoints { get; set; }
        public GameMode GameMode { get; set; }
        private double ProjectileSecondsToCrossMap { get; }
        public double ProjectileMapCoveragePerFrame { get; }
        private double ObstacleSecondsToCrossMap { get; }
        public double ObstacleMapCoveragePerFrame { get; }
        private int Fps { get; } // Frames per second
        public int FrameRate { get; } // Milliseconds needed for one frame
    }

    [Serializable]
    public enum GameMode
    {
        SinglePlayer,
        MultiPlayer
    }
}