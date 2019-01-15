using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace SocketLibrary
{
    public delegate void BufferNotifyEventHandler(byte[] CaptureData,IPEndPoint FromIP);
    /// <summary> 
    /// ----名称:UDP通讯类 
    /// ----建立:niefei 
    /// ----建立时间:2004-12-6 
    /// </summary> 
    /// <remarks> 
    /// ----使用说明与定义: 
    /// ----接到字符串 "NeedDownCards" 表示需要调用卡下载功能 
    /// </remarks> 
    public class UDPSocket
    {
        public class ClientEndPoint 
        {
            public ClientEndPoint() { }
            public ClientEndPoint(int cID, string remoteIP, int remotePort) 
            {
                CID = cID;
                RemoteIP = remoteIP;
                RemotePort = remotePort;
            }

            public ClientEndPoint(int cID, string remoteIP, int remotePort,int flag)
            {
                CID = cID;
                RemoteIP = remoteIP;
                RemotePort = remotePort;
                Flag = flag;
            }
            public int CID;
            public string RemoteIP = "192.168.0.255";
            public int RemotePort = 9999;
            public int Flag = 0;
        }

        public UDPPackage.UdpData PulseData;
        #region 内部变量区
        private IPEndPoint m_SendToIpEndPoint = null;
        /// <summary>
        /// 为了不让定义的事件在没有附加接收函数时出错，需要先加载一个空函数
        /// </summary>
        /// <param name="Msg"></param>
        protected void EmptyFunction(string Msg) { }
        protected ArrayList m_computers;
        /// <summary> 
        /// 发送命令文本常量 
        /// </summary> 
        protected string m_sendText;
        /// <summary> 
        /// 默认发送的字符串 
        /// </summary> 
        protected const string m_sendStr = "Hello!Server Is Running!";
        /// <summary> 
        /// Udp对象 
        /// </summary> 
        protected UdpClient m_Client;
        /// <summary> 
        /// Udp对象 发送
        /// </summary> 
        protected UdpClient m_Client2;
        /// <summary> 
        /// 本地通讯端口(默认8888) 
        /// </summary> 
        protected int m_LocalPort;
        /// <summary>
        /// 本一IP(默认127.0.0.1)
        /// </summary>
        protected string m_LocalIP;
        /// <summary> 
        /// 对方IP 
        /// </summary> 
        protected string m_SendToIP ="";
        /// <summary> 
        /// 远程通讯端口(默认8000)
        /// </summary> 
        protected int m_RemotePort=9999;
        /// <summary>
        /// 当有多台客户机需要与服务器通讯时，这里记录了所有客户机的信息
        /// </summary>
        protected List<ClientEndPoint> m_ClientList = new List<ClientEndPoint>();
        /// <summary> 
        /// 跟踪是否退出程序 
        /// </summary> 
        protected bool m_Done;
        /// <summary> 
        /// 设置是否要发送 
        /// </summary> 
        protected bool m_flag;
        #endregion

        #region 属性区
        /// <summary> 
        /// 定义委托 
        /// </summary> 
        public delegate void SOCKETDelegateArrive(string sReceived);
        /// <summary> 
        /// 定义一个消息接收事件 
        /// </summary> 
        public SOCKETDelegateArrive SOCKETEventArrive;
        /// <summary> 
        /// 定义一个接受线程 
        /// </summary> 
        public Thread recvThread;
        /// <summary> 
        /// 定义一个检测发送线程 
        /// </summary> 
        public Thread checkSendThread;
        /// <summary> 
        /// 下载标志 
        /// </summary> 
        public bool flag
        {
            set { this.m_flag = value; }
            get { return this.m_flag; }
        }
        /// <summary> 
        /// 设置通讯端口 
        /// </summary> 
        public int LocalPort
        {
            set { m_LocalPort = value; }
            get { return m_LocalPort; }
        }
        /// <summary>
        /// 设置本地IP
        /// </summary>
        public string LocalIP 
        {
            set { m_LocalIP = value; }
            get{return m_LocalIP;}
        }
        /// <summary> 
        /// 设置对方IP地址 ,m_SendToIP
        /// </summary> 
        public string RemoteIP
        {
            set
            {
                m_SendToIP = value;
                m_SendToIpEndPoint =
                    new IPEndPoint(IPAddress.Parse(this.m_SendToIP), m_RemotePort);
            }
            get { return m_SendToIP; }
        }
        /// <summary> 
        /// 远程通讯端口 
        /// </summary> 
        public int RemotePort
        {
            set
            {
                m_RemotePort = value;
                if (m_SendToIP != "")
                {
                    m_SendToIpEndPoint =
                       new IPEndPoint(IPAddress.Parse(this.m_SendToIP), m_RemotePort);
                }
            }
            get { return m_RemotePort; }
        }
        /// <summary>
        /// 设置多个远程接收端
        /// </summary>
        public List<ClientEndPoint> ClientList
        {
            get { return m_ClientList; }
            set { m_ClientList = value; }
        }
        /// <summary> 
        /// 设置要发送的岗位对象 
        /// </summary> 
        public ArrayList computers
        {
            set { this.m_computers = value; }
            get { return this.m_computers; }
        }

        /// <summary> 
        /// 收发开关，false为正常工作，true为关闭收发  
        /// </summary> 
        public bool Done
        {
            set { m_Done = value; }
            get { return m_Done; }
        }
        #endregion
        /// <summary> 
        /// 构造函数设置各项默认值 
        /// </summary> 
        public UDPSocket()
        {
            m_sendText = string.Empty;
            m_computers = new ArrayList();
            m_Done = false;
            m_flag = false;
        }
        /// <summary> 
        /// 初始化 
        /// </summary> 
        public void Init()
        {
            //初始化UDP对象 
            try
            {
                //Dispose();
                //SOCKETEventArrive += this.EmptyFunction;
                if (m_LocalIP != null && m_LocalIP != "")
                {
                    m_Client = new UdpClient(new IPEndPoint(IPAddress.Parse(m_LocalIP), m_LocalPort)); ;
                }
                else 
                {
                    m_Client = new UdpClient(m_LocalPort);
                }
                
                //m_Client = new UdpClient(m_LocalPort);
                //SOCKETEventArrive("Initialize succeed by " + m_LocalPort.ToString() + " port");
            }
            catch
            {

                //SOCKETEventArrive("Initialize failed by " + m_LocalPort.ToString() + " port");
            }
        }
        /// <summary> 
        /// 析构函数 
        /// </summary> 
        ~UDPSocket() { Dispose(); }
        /// <summary> 
        /// 关闭对象 
        /// </summary> 
        public void Dispose()
        {
            ClientList.Clear();
            DisConnection();
            m_computers = null;
        }
        
        /// <summary> 
        /// 关闭UDP对象 
        /// </summary> 
        public void DisConnection()
        {
            try
            {
                if (m_Client != null)
                {
                    this.Done = true;
                    if (recvThread != null)
                    {
                        this.recvThread.Abort();
                    }
                    if (checkSendThread != null)
                    {
                        this.checkSendThread.Abort();
                    }
                    if (recvThread != null)
                    {
                        this.recvThread.Abort();
                    }
                    if (checkSendThread != null)
                    {
                        this.checkSendThread.Abort();
                    }
                    m_Client.Close();
                    m_Client = null;

                }
            }
            catch
            {
                this.Done = true;
                m_Client.Close();
                m_Client = null;
            }
            finally
            {
                this.Done = true;
                if (m_Client != null) 
                {
                    m_Client.Close();
                    m_Client = null;
                }
                if (m_Client2 != null) 
                {
                    m_Client2.Close();
                    m_Client2 = null;
                }
                
            }
        }


        

        #region 接收区
        public event BufferNotifyEventHandler BufferNotify;
        /// <summary> 
        /// 侦听线程 
        /// </summary> 
        public void StartRecvThreadListener()
        {
            try
            {
                // 启动等待连接的线程 
                recvThread = new Thread(new ThreadStart(Received));
                recvThread.Priority = ThreadPriority.Normal;
                recvThread.IsBackground = false;
                recvThread.Name = "UDPReceiveThread";
                recvThread.Start();
                //SOCKETEventArrive("[Received]Thread Start....");
            }
            catch(Exception exp)
            {
                //SOCKETEventArrive("[Received]Thread Start failed!"+exp.Message);
            }
        }
        /// <summary> 
        /// 循环接收，收到数据引发BufferNotifyEventHandler事件
        /// </summary> 
        private void Received()
        {
            while (!m_Done)
            {
                //接收数据   
                try
                {
                    IPEndPoint endpoint = null;
                    if (m_Client != null && recvThread.IsAlive)
                    {

                        m_Client.Client.Blocking = true ;
                        Byte[] CaptureData = m_Client.Receive(ref endpoint);
                        if (CaptureData != null)
                        {
                            BufferNotify(CaptureData, endpoint/*m_Client.Client.LocalEndPoint*/);
                        }

                    }
                    else if (!recvThread.IsAlive)
                    {
                        recvThread.Resume();
                    }
                    if (this.checkSendThread != null) //顺便检查发送线程是否工作正常
                    {
                        if (this.checkSendThread.ThreadState == System.Threading.ThreadState.Aborted
                            || this.checkSendThread.ThreadState == System.Threading.ThreadState.Stopped)
                        {

                            checkSendThread.Abort();
                            checkSendThread = null;
                            checkSendThread = new Thread(new ThreadStart(ChekSendListener));
                            checkSendThread.IsBackground = false;
                            checkSendThread.Start();
                        }
                    }
                }
                catch (Exception exp)
                {
                    //Trace.WriteLine("2222222222222222222");
                    //SOCKETEventArrive("ReceiveData:CaptureData. Nullerror"+exp.Message);
                }
                finally { }
                Thread.Sleep(10); //防止系统资源耗尽 
            }
        }
        #endregion

        #region 发送区
        public Queue<byte[]> CaptureDataQueue = new Queue<byte[]>();
        /// <summary>
        /// 用于接收音频数据的入口
        /// </summary>
        /// <param name="CaptureData"></param>
        public int ReceiveSound(byte[] CaptureData)
        {
            if (!m_Done)
            {
                lock (CaptureDataQueue)
                {
                    CaptureDataQueue.Enqueue(CaptureData);
                }
            }
            return CaptureData.Length;
        }
        /// <summary> 
        /// 启动检测发送侦听线程 
        /// </summary> 
        public void StartCheckSendListenerThread()
        {
            try
            {
                checkSendThread = new Thread(new ThreadStart(ChekSendListener));
                checkSendThread.Priority = ThreadPriority.Normal;
                checkSendThread.IsBackground = false;
                checkSendThread.Name = "UDPSendThread";
                checkSendThread.Start();

                ////SOCKETEventArrive("[ChekSendListener]Thread Start...");
            }
            catch
            {
                //SOCKETEventArrive("[ChekSendListener]Thread Start failed!");
            }
        }
        /// <summary> 
        /// 如果当前发送队列中有数据，就启动发送 
        /// </summary> 
        private void ChekSendListener()
        {
            lock (CaptureDataQueue)
            {
                CaptureDataQueue.Clear();
            }
            while (!m_Done)
            {
                try
                {
                    if (CaptureDataQueue.Count > 0)
                    {
                        lock (CaptureDataQueue)
                        {
                            this.sendData(CaptureDataQueue.Dequeue());
                        }
                    }
                    if (this.recvThread != null) 
                    {
                        if (this.recvThread.ThreadState == System.Threading.ThreadState.Aborted
                            || this.recvThread.ThreadState == System.Threading.ThreadState.Stopped) 
                        {

                            recvThread.Abort();
                            recvThread = null;
                            recvThread = new Thread(new ThreadStart(Received));
                            recvThread.IsBackground = false;
                            recvThread.Start();
                        }
                    }
                }
                catch { }
                finally { }
                Thread.Sleep(1); //防止系统资源耗尽 
            }
        }

        
        #region 二进制发送区
        /// <summary>
        /// 发送字节流数据
        /// </summary>
        /// <param name="CaptureData"></param>
        public void sendData(byte[] CaptureData/*UDPPackage.UDPData UdpData*/)
        {
            try
            {
                if (m_Client == null)
                {
                    m_Client = new UdpClient(new IPEndPoint(IPAddress.Parse(m_LocalIP), m_LocalPort ));
                }
                //m_Client.Connect(this.m_SendToIP, m_RemotePort);
                //byte[] bytReceivedData = UdpData.ToByte();// new byte[CaptureData.Length];
                //System.Buffer.BlockCopy(CaptureData, 0, bytReceivedData, 0, CaptureData.Length);
                // 连接后传送一个消息给ip主机 
                //m_Client.Send(bytReceivedData, bytReceivedData.Length);
                if (0 != this.m_ClientList.Count)
                {
                    for (int i = 0; i < this.m_ClientList.Count; i++)
                    {
                        int m = m_Client.Send(CaptureData, CaptureData.Length,
                            new IPEndPoint(IPAddress.Parse(this.m_ClientList[i].RemoteIP), this.m_ClientList[i].RemotePort));
                    }
                }
                else
                {
                    if (m_SendToIpEndPoint != null)
                    {
                        int i = m_Client.Send(CaptureData, CaptureData.Length, m_SendToIpEndPoint);

                    }
                }

            }
            catch
            {

            }
            finally
            {
                //m_Client.Close();
                //m_Client = null;
            }
        }
        /// <summary>
        /// 向指定的IP的端口发送数据
        /// </summary>
        /// <param name="CaptureData"></param>
        /// <param name="mIP"></param>
        /// <param name="mPort"></param>
        /// <returns></returns>
        public int sendData(byte[] CaptureData,string mIP,int mPort) 
        {
            IPEndPoint mSendToIpEndPoint =
                    new IPEndPoint(IPAddress.Parse(mIP), mPort);
            int i = 0;
            try
            {
                if (m_Client == null)
                {
                    m_Client = new UdpClient(new IPEndPoint(IPAddress.Parse(m_LocalIP), m_LocalPort));
                }
                i = m_Client.Send(CaptureData, CaptureData.Length, mSendToIpEndPoint);


            }
            catch
            {
                return 0;
            }
            return i;

        }
        #endregion
        #endregion

        #region 周期信号区
        Thread PualseThread;
        /// <summary>
        /// 向服务器发送脉冲信号
        /// </summary>
        public void sendPulseSignal()
        {
            try
            {
                PualseThread = new Thread(new ThreadStart(PulseSingnal));
                PualseThread.Priority = ThreadPriority.Normal;
                PualseThread.Start();

                //SOCKETEventArrive("[PulseSignal]Thread Start...");
            }
            catch
            {
                //SOCKETEventArrive("[PulseSignal]Thread Start failed!");
            }
        }
        private void PulseSingnal()
        {
            while (true)
            {
                if (this.PulseData.EXP  == 1)
                {
                    this.sendData(UDPPackage.StructToBytes(this.PulseData));
                }

                Thread.Sleep(5000); //每五秒钟一次脉冲
            }
        }
        #endregion

    }
}
