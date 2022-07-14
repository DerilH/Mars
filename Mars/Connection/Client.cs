using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Mars.Connection
{
    internal class Client
    {
        private Connection connection;
        private Thread receivingThread;

        public delegate void DataReceivedEventHandler(byte[] data);

        public event DataReceivedEventHandler DataReceived = (data) => { };

        public Client()
        {
            connection = new Connection(ConnectionType.Client);
        }

        public void Connect(string hostIp, int hostPort)
        {
            connection.Connect(hostIp, hostPort);
            receivingThread = new Thread(() =>
            {
                while (!connection.Closed)
                {
                    byte[] data = connection.ReceiveCovenant();
                    DataReceived.Invoke(data);
                }
            });
            receivingThread.Start();
        }

        public void Send(byte[] data)
        {
            connection.Send(data);
        }
    }
}