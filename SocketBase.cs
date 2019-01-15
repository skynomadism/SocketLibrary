using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SocketLibrary
{
	/// <summary>
	/// SocketBase 的摘要说明。
	/// </summary>
	public class SocketBase
	{
        /// <summary>
        /// 创建的TCP实例的名称，用于进行各项管理
        /// </summary>
        public string SocketName { get; set; }

		public class MessageEventArgs:EventArgs {
			public Message Message;
			public Connection Connection;
			public MessageEventArgs(Message message,Connection connection) {
				this.Message = message;
				this.Connection = connection;
			}
		}
		public delegate void MessageEventHandler(object sender,MessageEventArgs e);

		public class ConnectionEventArgs:EventArgs {
			public Connection Connection;
			public Exception Exception;
			public ConnectionEventArgs(Connection connection,Exception exception) {
				this.Connection = connection;
				this.Exception = exception;
			}
		}
		public delegate void ConnectionHandler(object sender,ConnectionEventArgs e);

		public ConnectionCollection Connections {
			get{return _connections;}
			set{_connections = value;}
		}
		protected ConnectionCollection _connections;

        public Queue<Message> messageQueue
        {
            get { return _messageQueue; }
            set { _messageQueue = value; }
        }
        protected Queue<Message> _messageQueue;

        public bool isStop 
        {
            get { return _isStop; }
            set { _isStop = value; }
        }
        protected bool _isStop=true;
		public SocketBase()
		{
			this._connections = new ConnectionCollection();
            this._messageQueue = new Queue<Message>();
		}
		protected void Send(Message message) {
			this.Send(message,message.SendToOrReceivedFrom);	 
		}
		protected void Send(Message message,Connection connection) {
			byte[] buffer = message.ToBytes();
            try
            {
                lock (this)
                {
                    connection.NetworkStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch(Exception exp) { }
		}

		protected System.Threading.Thread _listenningthread;
		protected System.Threading.Thread _sendthread;
		protected virtual void Sendding() {

            while (true)
            {
                try
                {
                    bool isHaveDeadConnection = false;
                    System.Threading.Thread.Sleep(10);
                    while (messageQueue.Count>0)
                    {
                        Message myMessage = messageQueue.Dequeue();
                        if (myMessage.SendToOrReceivedFrom != null)
                        {
                            this.Send(myMessage);
                            this.OnMessageSent(this, new MessageEventArgs(myMessage, myMessage.SendToOrReceivedFrom));
                        }
                        else
                        {//对每一个连接都发送此消息
                            for (int j = 0; j < this.Connections.Count; j++)
                            {
                                if (this.Connections[j].Client.Connected)
                                {
                                    this.Send(myMessage, this.Connections[j]);
                                    this.Connections[j].LastReceiveOrSendDataTime = DateTime.Now;
                                    this.OnMessageSent(this, new MessageEventArgs(myMessage, this.Connections[j]));
                                    System.Threading.Thread.Sleep(1);
                                }
                                else 
                                {
                                    this.Connections[j].Client.Close();
                                    isHaveDeadConnection = true;
                                    this.OnConnectionClose(this, new ConnectionEventArgs(this.Connections[j], new Exception("连接失效"))); 
                                }
                                System.Threading.Thread.Sleep(1);
                            }
                        }
                        if (isHaveDeadConnection) 
                        {
                            for (int m = this.Connections.Count - 1; m >= 0; m--) //检查一些死去的连接，并把它们放弃。
                            {
                                if (!this.Connections[m].Client.Connected)
                                {
                                    this.Connections.RemoveAt(m);
                                }
                            }
                            isHaveDeadConnection = false;
                        }
                        
                        System.Threading.Thread.Sleep(1);
                        
                    }
                    //清除无效链接
                    for (int m = this.Connections.Count - 1; m >= 0; m--) //检查一些死去的连接，并把它们放弃。
                    {
                        if (Math.Abs((this.Connections[m].LastReceiveOrSendDataTime -DateTime.Now).TotalMinutes)>30)
                        {
                            var connect = this.Connections[m];
                            connect.Client.Close();
                            this.Connections.RemoveAt(m);
                            this.OnConnectionClose(this, new ConnectionEventArgs(connect, new Exception("连接失效")));
                        }
                        System.Threading.Thread.Sleep(1);
                    }

                }
                catch
                {
                }
            }
			
		}
		protected virtual void Listenning() {

            while (true)
            {
                try
                {
                    if (this._connections.Count == 0)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    foreach (Connection connection in this._connections)
                    {
                        if (connection.Client.Connected && connection.NetworkStream.CanRead && connection.NetworkStream.DataAvailable)
                        {
                            try
                            {
                                Message message = Message.ReceiveFrom(connection);
                                connection.LastReceiveOrSendDataTime = DateTime.Now;
                                this.OnMessageReceived(this, new MessageEventArgs(message, connection));
                            }
                            catch (Exception ex)
                            {
                                if (!connection.Client.Connected)
                                {
                                    connection.NetworkStream.Close();
                                    this.OnConnectionClose(this, new ConnectionEventArgs(connection, ex));
                                }
                            }
                        }
                        System.Threading.Thread.Sleep(1);
                    }
                }
                catch
                {
                }
            }
			
		}

		protected void StartListenAndSend() {
			_listenningthread = new System.Threading.Thread(new System.Threading.ThreadStart(Listenning));
            _listenningthread.IsBackground = true;
			_listenningthread.Start();
			_sendthread = new System.Threading.Thread(new System.Threading.ThreadStart(Sendding));
            _listenningthread.IsBackground = true;
			_sendthread.Start();
		}
		public void EndListenAndSend() {
            foreach (Connection connection in this._connections) 
            { 
                connection.NetworkStream.Close(); 
                this.OnConnectionClose(this, new ConnectionEventArgs(connection, new Exception("关闭网络连接"))); 
            }
            this._connections.Clear();
			_listenningthread.Abort();
			_sendthread.Abort();
            messageQueue.Clear();
            _isStop = true ;
		}

		public event MessageEventHandler MessageReceived;
		public event MessageEventHandler MessageSent;
		public event ConnectionHandler ConnectionClose;
		public event ConnectionHandler Connected;

		public void OnMessageReceived(object sender,MessageEventArgs e) {
			if(MessageReceived != null)
				this.MessageReceived(sender,e);
		}
		public void OnMessageSent(object sender,MessageEventArgs e) {
			if(MessageSent != null)
				this.MessageSent(sender,e);
		}
		public void OnConnectionClose(object sender,ConnectionEventArgs e) {
			if(ConnectionClose != null)
				this.ConnectionClose(sender,e);
		}
		public void OnConnected(object sender,ConnectionEventArgs e) {
			if(Connected != null)
				this.Connected(sender,e);
		}
	}
}
