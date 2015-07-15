using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace LanTribune
{
    abstract class ClassNetworkServerBroadcast
    {
        private class UdpState
        {
            public IPEndPoint E;
            public UdpClient U;
        }

        private bool _messageReceived;

        private Thread _serverThread;

        private Thread _clientThread;

        private Boolean _run;

        public Boolean Run
        {
            get { return _run; }
            set
            {
                _run = value;
                if (!_run)
                {
                    _serverThread.Abort();
                }
            }
        }

        protected ClassNetworkServerBroadcast()
        {
            Run = true;
        }

        public void LaunchServer()
        {
            _serverThread = new Thread(ServerThread);

            _serverThread.Start();

            while (!_serverThread.IsAlive){}
        }

        private void ServerReceiveCallback(IAsyncResult ar)
        {

            string returnData = "";
            try
            {
                UdpClient udpClient = ((UdpState)(ar.AsyncState)).U;
                IPEndPoint remoteIpEndPoint = ((UdpState)(ar.AsyncState)).E;
                Byte[] receiveBytes = udpClient.EndReceive(ar, ref remoteIpEndPoint);
                //remoteIpEndPoint.Address  <- adresse du serveur distant
                returnData = Encoding.ASCII.GetString(receiveBytes);

                ServerAction(returnData);
            }
            catch (Exception e)
            {
                MessageBox.Show(e + "\r\n\r\n" + returnData);

            }
            _messageReceived = true;
        }

        private void ServerThread()
        {
            UdpClient udpClient = new UdpClient(8123);
            while (_run)
            {
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                UdpState s = new UdpState
                {
                    E = remoteIpEndPoint,
                    U = udpClient
                };
                udpClient.BeginReceive(ServerReceiveCallback, s);

                while (!_messageReceived)
                {
                    Thread.Sleep(10);
                }

                _messageReceived = false;
            }
        }

        protected abstract void ServerAction(string data);

        public void Send(string data)
        {
            _clientThread = new Thread(SendThread);

            _clientThread.Start(data);

            while (!_clientThread.IsAlive) { }
        }


        private void SendThread(object data)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Broadcast, 8123);
            Byte[] senddata = Encoding.ASCII.GetBytes(data.ToString());
            udpClient.Send(senddata, senddata.Length);
            udpClient.Close();   
        }
    }
}
