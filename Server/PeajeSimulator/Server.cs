using Common;
using Common.Buffer;
using Common.MessageHandlers;
using Common.Messages;

using System.Net;
using System.Net.Sockets;

namespace SocketServer
{
    public class Server : MessageSender, IDisposable
    {
        readonly Socket _serverSocket;
        readonly IPEndPoint _localEndPoint;
        readonly IPAddress _ipAddress;

        public Server(int port, int maxConnections = 10)
        {
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses
            IPHostEntry host = Dns.GetHostEntry("localhost");
            _ipAddress = host.AddressList[0];
            _localEndPoint = new IPEndPoint(_ipAddress, port);

            try
            {
                _serverSocket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method
                _serverSocket.Bind(_localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.
                // We will listen 10 requests at a time
                _serverSocket.Listen(maxConnections);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Start()
        {
            Console.WriteLine($"Listening for a connection at {_localEndPoint}");

            while (true)
            {
                ReceiveConnections();
            }
        }

        public void ReceiveConnections()
        {
            Socket socket = _serverSocket.Accept();

            Thread thread = new Thread(() =>
            {
                // Incoming data from the client.
                byte[] bytes;
                ByteBuffer receive;

                while (socket.Connected)
                {
                    try
                    {
                        bytes = new byte[8192];
                        int bytesRec = socket.Receive(bytes);
                        if (bytesRec == 0)
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{socket.RemoteEndPoint} : {e.Message}");
                        continue;
                    }

                    receive = new ByteBuffer(bytes);
                    MessageHandle.HandleMessage(bytes, socket);
                }

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            });
            thread.Start();
        }

        public void Send(byte[] buffer, Socket clientSocket)
        {
            // Send the data through the socket.
            clientSocket.Send(buffer);
        }

        public void Dispose()
        {
            // Release the socket.
            _serverSocket.Shutdown(SocketShutdown.Both);
            _serverSocket.Close();
        }
    }
}
