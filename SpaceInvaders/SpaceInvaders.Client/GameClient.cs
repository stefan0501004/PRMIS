using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SpaceInvaders.Client.UI;
using SpaceInvaders.Common.Game;
using SpaceInvaders.Common.Models;
using SpaceInvaders.Network;
using SpaceInvaders.Network.Services;

namespace SpaceInvaders.Client
{
    public class GameClient : NetworkComponent
    {
        private const int Port = 5000;
        private const string ServerIp = "127.0.0.1";

        private readonly Socket _clientSocket =
            new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private Player _player;

        private EndPoint _serverEndPoint;

        private Socket _udpSocket;

        private UiRenderer _uiRenderer;

        public void Start()
        {
            try
            {
                // TCP Connection and Registration
                _clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ServerIp), Port));
                ClientUi.ShowConnectionStatus("Connected to server");

                var (name, surname) = ClientUi.GetPlayerInfo();
                var player = new Player(name, surname);
                SendMessageTcp(_clientSocket, player);

                _player = ReceiveMessageTcp<Player>(_clientSocket);
                ClientUi.ShowPlayerRegistration(_player);

                // Setup UDP Connection
                _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _udpSocket.Bind(new IPEndPoint(IPAddress.Any, 0)); // Let OS assign port

                // Send UDP port to server via TCP
                var localEndPoint = (IPEndPoint)_udpSocket.LocalEndPoint;
                SendMessageTcp(_clientSocket, localEndPoint.Port);

                // Get server UDP endpoint
                var serverUdpPort = ReceiveMessageTcp<int>(_clientSocket);
                _serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), serverUdpPort);

                // Close TCP connection as it's no longer needed
                _clientSocket.Close();

                // Receive game configuration via UDP
                var gameConfiguration = ReceiveMessageUdp<GameConfiguration>(_udpSocket, ref _serverEndPoint);
                _uiRenderer = new UiRenderer(gameConfiguration.MapHeight, gameConfiguration.MapWidth);

                // Start background threads for game state updates, rendering, and input handling
                var receiveThread = new Thread(ReceiveGameStateUpdates);
                var uiThread = new Thread(_uiRenderer.Render);
                var inputThread = new Thread(HandlePlayerInput); // Add new thread for input

                receiveThread.Start();
                uiThread.Start();
                inputThread.Start(); // Start the input thread

                ClientUi.WaitForExit();

                // Cleanup threads
                receiveThread.Join();
                uiThread.Join();
                inputThread.Join(); // Join the input thread
            }
            catch (Exception ex)
            {
                ClientUi.ShowError(ex.Message);
            }
            finally
            {
                // Cleanup sockets
                _clientSocket.Close();
                _udpSocket?.Close();
            }
        }

        private void ReceiveGameStateUpdates()
        {
            do
            {
                _uiRenderer.GameState = ReceiveMessageUdp<GameState>(_udpSocket, ref _serverEndPoint);
            } while (_uiRenderer.GameState.IsGameStarted);
        }

        private void HandlePlayerInput()
        {
            // Wait for game to start
            if (_uiRenderer.GameState == null || !_uiRenderer.GameState.IsGameStarted)
                while (_uiRenderer.GameState == null || !_uiRenderer.GameState.IsGameStarted)
                    Thread.Sleep(50);

            while (_uiRenderer.GameState.IsGameStarted)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    Command command = null;

                    switch (key.Key)
                    {
                        case ConsoleKey.LeftArrow:
                            command = new Command(_player.Id, CommandType.MoveLeft);
                            break;
                        case ConsoleKey.RightArrow:
                            command = new Command(_player.Id, CommandType.MoveRight);
                            break;
                        case ConsoleKey.Spacebar:
                            command = new Command(_player.Id, CommandType.Shoot);
                            break;
                    }

                    if (command != null) SendMessageUdp(_udpSocket, command, _serverEndPoint);
                }

                Thread.Sleep(100);
            }
        }
    }
}