using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;

namespace Mars.Connection
{
    public enum DataUnit
    {
        KB = 1024,
        MB = 1048576,
        GB = 1073741824
    }

    public enum ConnectionType
    {
        Host,
        Client
    }

    internal class Connection
    {
        private Socket socket;
        public EndPoint LocalEndPoint => socket.LocalEndPoint;
        public EndPoint RemoteEndPoint => socket.RemoteEndPoint;
        public ConnectionType ConnectionType { get; private set; }

        public delegate void ConnectionClosingEventHandler();

        public event ConnectionClosingEventHandler Closing = () => { };

        public bool Closed { get; private set; } = false;

        public Connection(ConnectionType type)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public Connection(Socket socket, ConnectionType type)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];

            this.socket = socket;
        }

        public void Connect(string ip, int port)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                socket.Connect(endPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void Bind(string ip, int port)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                socket.Bind(endPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void Send(byte[] data, bool Async = false)
        {
            if (!socket.Connected)
            {
                MessageBox.Show("Doesn't connected");
                return;
            }
            try
            {
                if (Async)
                {
                    ArraySegment<byte> buffer = new ArraySegment<byte>(data);
                    socket.SendAsync(buffer, SocketFlags.None);
                }
                else socket.Send(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void SendCovenant(byte[] data)
        {
            if (!socket.Connected)
            {
                MessageBox.Show("Doesn't connected");
                return;
            }

            byte[] info = BitConverter.GetBytes(data.Length);
            socket.Send(info);
            socket.Send(data);
        }

        public byte[] Receive(int dataSize)
        {
            try
            {
                byte[] data = new byte[dataSize];
                int bytesCount = socket.Receive(data);
                return data;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private byte[] buffer = new byte[(int)DataUnit.MB * 4];
        private byte[] data;

        public byte[] ReceiveCovenant()
        {
            byte[] info = new byte[4];
            socket.Receive(info);
            int size = BitConverter.ToInt32(info, 0);
            if (data?.Length != size && size > 0)
            {
                data = new byte[size];
            }
            socket.Receive(data);
            return data;
        }

        public void Listen()
        {
            try
            {
                socket.Listen(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public Connection Accept()
        {
            try
            {
                Socket acceptedSocket = socket.Accept();
                Connection connection = new Connection(acceptedSocket, ConnectionType.Client);
                return connection;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public void Close()
        {
            Closing.Invoke();
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Closed = true;
        }
    }
}