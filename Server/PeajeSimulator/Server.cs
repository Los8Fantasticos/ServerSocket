using Common;
using Common.Buffer;
using Common.MessageHandlers;
using Common.Messages;

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketServer
{
    public class Server : MessageSender, IDisposable
    {
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<Socket> _clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private static byte[] _buffer = new byte[BUFFER_SIZE];
        readonly IPAddress _ipAddress;
        readonly IPEndPoint _localEndPoint;

        public Server(int port, int maxConnections = 10)
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            _ipAddress = host.AddressList[1];
            _localEndPoint = new IPEndPoint(_ipAddress, port);
            SetupServer(maxConnections);
        }
        
        public void SetupServer(int maxConnections)
        {
            Console.WriteLine("Setting up server...");
            
            _serverSocket.Bind(_localEndPoint);
            _serverSocket.Listen(maxConnections);
            _serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;
            try
            {
                socket = _serverSocket.EndAccept(AR);
            }
            catch (Exception)
            {

                return;
            }
            _clientSockets.Add(socket);           
            socket.BeginReceive(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected");
            _serverSocket.BeginAccept(AcceptCallback, null);
        }
        
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int received;
            try
            {
                received = socket.EndReceive(AR);
                
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                socket.Close();
                _clientSockets.Remove(socket);
                return;
            }
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);
            string text = Encoding.ASCII.GetString(dataBuf);
            
            MessageHandle.HandleMessage(_buffer, socket);

            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, socket);
        }
    

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
