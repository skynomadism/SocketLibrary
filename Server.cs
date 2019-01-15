using System;
using System.Net;
using System.Net.Sockets;

namespace SocketLibrary
{
	public class Server:SocketBase
	{
		
         
		private TcpListener _listener;
		public Server(string socketname="DefaultServer")
		{
            this.SocketName = socketname;
			this._connections = new ConnectionCollection();
		}
		protected System.Threading.Thread _listenConnection;

		public void StartServer(int port) {
			_listener = new TcpListener(IPAddress.Any, port);
			_listener.Start();

			_listenConnection = new System.Threading.Thread(new System.Threading.ThreadStart(Start));
            _listenConnection.IsBackground = true;
			_listenConnection.Start();
            
			this.StartListenAndSend();
            this._isStop = false;
		}
		public void StopServer() {
            try
            {
                _listenConnection.Abort();
                this.EndListenAndSend();
                _listener.Stop();
            } catch { }

            
			
		}
		private void Start() {
			try {
				while(true) {
					if(_listener.Pending()) {
						TcpClient client = _listener.AcceptTcpClient();

                        //NetworkStream stream = client.GetStream();
                        Connection connection = new Connection(client);
						this._connections.Add(connection);
                        this.OnConnected(this, new ConnectionEventArgs(connection, new Exception(client.Client.RemoteEndPoint.ToString() + "连接成功" )));
					}
					System.Threading.Thread.Sleep(200);
				}	
			}
			catch(Exception exp) {
				 }
		}
       
		
	}
}
