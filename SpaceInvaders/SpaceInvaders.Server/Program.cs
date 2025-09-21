using System;
using SpaceInvaders.Common.Game;
using SpaceInvaders.Server.UI;
using Spectre.Console;

namespace SpaceInvaders.Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var gameConfiguration = new GameConfiguration(40, 20, 30, GameMode.SinglePlayer, 30, 1, 2);
            var server = new GameServer(gameConfiguration);

            try
            {
                server.Start();
            }
            catch (Exception ex)
            {
                AnsiConsole.Clear();
                ServerUi.ShowError(ex.Message);
            }
        }
    }
}