using System;
using System.Net;
using System.Net.Sockets;

namespace SocketLibrary
{

	public class Client:SocketBase
	{
        private string ServerIP;
        private int ServerPort;
        public bool IsConnectServer 
        {
            get { return this._connections.Count>0? this._connections[0].Client.Connected:false; } 
            private set { } }
		public const int CONNECTTIMEOUT = 50;
		public Client(string socketname="DefaultClient")
		{
            this.SocketName = socketname;
		}

		public void StartClient(string serIP,int serPort) {

            this.ServerIP = serIP;
            this.ServerPort = serPort;
            this.StartListenAndSend();
            this._isStop = false;
		}
		public void StopClient() {
			this.EndListenAndSend();
            this._isStop = true;
		}

        public bool ConnectServer() 
        {
            try
            {
                if (this._connections.Count > 0) 
                {
                    this._connections.RemoveAt(0);
                }
                TcpClient client = new TcpClient();
                //UdpClient client = new UdpClient();
                client.SendTimeout = CONNECTTIMEOUT;
                client.ReceiveTimeout = CONNECTTIMEOUT;
                client.Connect(IPAddress.Parse(this.ServerIP), this.ServerPort);
                this._connections.Add(new Connection(client));
                return true;
            }
            catch { return false; }
            

        }

        public void SendBuffer(byte[] Buffer) 
        {
            Message msg = new Message(Buffer);
            this.messageQueue.Enqueue(msg);
        }
	}
}
