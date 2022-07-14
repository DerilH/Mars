using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Mars.Connection
{
    internal class Host
    {
        private Connection listeningConnection;
        private List<Connection> connections = new List<Connection>();
        private Dictionary<Connection, Thread> receivingThreads = new Dictionary<Connection, Thread>();
        private CancellationTokenSource cancellationTokenSource;

        public Host(string ip, int port)
        {
            listeningConnection = new Connection(ConnectionType.Host);
            listeningConnection.Bind(ip, port);
        }

        public void StartListen()
        {
            cancellationTokenSource = new CancellationTokenSource();
            Thread listenThread = new Thread(() =>
            {
                listeningConnection.Listen();
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Connection acceptedConnection = listeningConnection.Accept();
                    connections.Add(acceptedConnection);
                    Thread receivingThread = new Thread(() =>
                    {
                        while (true)
                        {
                            byte[] data = acceptedConnection.Receive((int)DataUnit.MB);
                            Thread.Sleep(16);
                        }
                    });
                    receivingThread.Start();
                    receivingThreads.Add(acceptedConnection, receivingThread);

                    acceptedConnection.Closing += () =>
                    {
                        if (connections.Contains(acceptedConnection))
                        {
                            connections.Remove(acceptedConnection);
                            receivingThread.Abort();
                            receivingThreads.Remove(acceptedConnection);
                        }
                    };
                }
            });
            listenThread.Start();
        }

        public void SendAll(byte[] data)
        {
            foreach (Connection connection in connections)
            {
                connection.SendCovenant(data);
            }
        }
    }
}