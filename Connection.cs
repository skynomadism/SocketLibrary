using System;
using System.Net;
using System.Net.Sockets;

namespace SocketLibrary
{
	/// <summary>
	/// Connection 的摘要说明。
	/// </summary>
	public class Connection
	{
		public NetworkStream NetworkStream {
			get{return _networkStream;}
			set{_networkStream = value;}
		}
		private NetworkStream _networkStream;
		public string ConnectionName {
			get{return _connectionName;}
			set{_connectionName = value;}
		}
		private string _connectionName;
        private Socket _client;
        public Socket Client{
            get { return _client; }
            set { _client = value; }
        }
        public Connection(TcpClient client)
        {
            this._client = client.Client;
            this._networkStream = client.GetStream();
            this._connectionName = client.Client.LocalEndPoint.ToString();
            this.LastReceiveOrSendDataTime = DateTime.Now;
        }
        public DateTime LastReceiveOrSendDataTime { get; set; }
        //public Connection(NetworkStream networkStream,string connectionName)
        //{
        //    this._networkStream = networkStream;
        //    this._connectionName = connectionName;
        //}
        //public Connection(NetworkStream networkStream):this(networkStream,string.Empty) {
        //}
	}
}
