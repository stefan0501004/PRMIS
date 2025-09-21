using System;
using System.Net;
using System.Net.Sockets;
using SpaceInvaders.Common.Game;
using SpaceInvaders.Common.Models;
using SpaceInvaders.Network;
using SpaceInvaders.Server.Services;
using SpaceInvaders.Server.UI;

namespace SpaceInvaders.Server
{
    public class GameServer : NetworkComponent
    {
        private const int Port = 5000;
        private readonly GameEngine _gameEngine;
        private readonly Socket _serverSocket;
        private readonly Socket _udpSocket;

        public GameServer(GameConfiguration gameConfiguration)
        {
            var gameState = new GameState();
            _gameEngine = new GameEngine(gameConfiguration, gameState);

            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Bind UDP socket to any available port
            _udpSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
        }

        public void Start()
        {
            try
            {
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                _serverSocket.Listen(2);
                ServerUi.ShowServerStart(Port);

                while (true)
                {
                    ServerUi.ShowWaitingForPlayers();
                    var clientSocket = _serverSocket.Accept();
                    HandlePlayerRegistration(clientSocket);
                }
            }
            catch (Exception ex)
            {
                ServerUi.ShowError(ex.Message);
            }
        }

        private void HandlePlayerRegistration(Socket clientSocket)
        {
            try
            {
                // Receive player data via TCP
                var player = ReceiveMessageTcp<Player>(clientSocket);

                // Set initial position
                player.Coordinates.X = _gameEngine.GameConfiguration.MapWidth / 2;
                player.Coordinates.Y = _gameEngine.GameConfiguration.MapHeight - 1;
                player.Id = _gameEngine.GameState.Players.Count + 1;

                // Send confirmation back via TCP
                SendMessageTcp(clientSocket, player);

                // Receive client's UDP port
                var clientUdpPort = ReceiveMessageTcp<int>(clientSocket);
                var clientIp = ((IPEndPoint)clientSocket.RemoteEndPoint).Address;
                var clientUdpEndPoint = new IPEndPoint(clientIp, clientUdpPort);

                // Send server's UDP port
                var serverUdpPort = ((IPEndPoint)_udpSocket.LocalEndPoint).Port;
                SendMessageTcp(clientSocket, serverUdpPort);

                // Close TCP connection
                clientSocket.Close();

                // Add player and UDP endpoint to game engine
                _gameEngine.AddPlayer(player, _udpSocket, clientUdpEndPoint);
                ServerUi.ShowPlayerRegistered(player);

                // Start game
                _gameEngine.StartGame();
            }
            catch (Exception ex)
            {
                ServerUi.ShowError(ex.Message);
            }
        }
    }
}