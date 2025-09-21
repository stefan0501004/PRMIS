using System;
using System.Net;
using System.Net.Sockets;
using SpaceInvaders.Network.Utils;

namespace SpaceInvaders.Network
{
    public abstract class NetworkComponent
    {
        protected static void SendMessageTcp(Socket clientSocket, object message)
        {
            // Convert the object to a byte array using serialization
            var messageBytes = SerializationUtils.SerializeObject(message);

            // Send the bytes through the TCP socket
            clientSocket.Send(messageBytes);
        }

        protected static T ReceiveMessageTcp<T>(Socket clientSocket)
        {
            // Create a buffer to store incoming data
            var buffer = new byte[1024];

            // Receive bytes from the TCP socket
            clientSocket.Receive(buffer);

            // Convert received bytes back to object of type T
            return SerializationUtils.DeserializeObject<T>(buffer);
        }

        protected static void SendMessageUdp(Socket clientSocket, object message, EndPoint remoteEndPoint)
        {
            // Convert the object to a byte array using serialization
            var messageBytes = SerializationUtils.SerializeObject(message);

            // Send the bytes through UDP socket to specified endpoint
            clientSocket.SendTo(messageBytes, remoteEndPoint);
        }

        protected static T ReceiveMessageUdp<T>(Socket clientSocket, ref EndPoint remoteEndPoint,
            int? timeoutMilliseconds = null)
        {
            if (timeoutMilliseconds.HasValue)
                // Check if data is available within timeout period (converting ms to μs)
                if (!clientSocket.Poll(timeoutMilliseconds.Value * 1000, SelectMode.SelectRead))
                    return default;

            // Create a buffer for incoming data (UDP typically needs larger buffer)
            var buffer = new byte[8192];

            // Receive data and get actual number of bytes read
            var bytesRead = clientSocket.ReceiveFrom(buffer, ref remoteEndPoint);

            // Create a new array with exact size of received data
            var actualData = new byte[bytesRead];

            // Copy only the received bytes to avoid extra null bytes
            Array.Copy(buffer, actualData, bytesRead);

            // Convert received bytes back to object of type T
            return SerializationUtils.DeserializeObject<T>(actualData);
        }
    }
}