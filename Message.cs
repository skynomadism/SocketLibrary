using System;

namespace SocketLibrary
{
	public class Message
	{
		public Connection SendToOrReceivedFrom;
		public int MessageLength; 
        public byte[] ByteMessageBody;//字节型的数据体

        public bool DataValidity;//专用于标记接收到的数据是否有效（有时侯因为粘包现像导致数据错位，在容错时会把不完整的无效数据也接收过来并通知）
        public string Remark = "";
        #region 字符型数据初始化区
        public Message()
		{
			SendToOrReceivedFrom = null;
            DataValidity = false;
		}
        #endregion
        #region 字节型数据初始化区
        public Message(byte[] bytemessageBody)
            : this()
        {
            this.ByteMessageBody = bytemessageBody;
        }
        #endregion

        public byte[] ToBytes()
        {
            return this.ByteMessageBody.Length >0? this.ByteMessageBody:new byte[]{0};
        }
		
        /// <summary>
        /// 从一个连接里得到的字节数据，重新组装成一个消息
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static Message Parse(Connection connection,byte StartByte=0x7E,byte EndByte=0x7E) {
            Message message = new Message();
            if (connection.NetworkStream.DataAvailable)
            {
                message.SendToOrReceivedFrom = connection;
                //首先检查即将读到的数据是否是数据头如果不是如果不是，将这一包无效数据取出（最长取1024）
                byte[] headmark = new byte[1];
                int count = connection.NetworkStream.Read(headmark, 0, 1);
                if (count > 0)
                {
                    if (headmark[0] != StartByte)
                    {
                        byte[] temBuffer = new byte[1024];
                        int ReadCount = 1;
                        temBuffer[0] = headmark[0];
                        for (int i = 1; i < 1024; i++)
                        {
                            if (connection.NetworkStream.DataAvailable)
                            {
                                count = connection.NetworkStream.Read(headmark, 0, 1);
                                if (count > 0)
                                {
                                    temBuffer[i] = headmark[0];
                                    ReadCount++;
                                    if (headmark[0] == EndByte) { break; }
                                }
                                else { break; }
                            }
                            else { break; }
                        }
                        message.ByteMessageBody = new byte[ReadCount];
                        System.Buffer.BlockCopy(temBuffer, 0, message.ByteMessageBody, 0, ReadCount);
                        message.DataValidity = false;
                        message.MessageLength = ReadCount;
                        message.Remark = "发现没有起始符的残缺数据";
                    }
                    else
                    {
                        byte[] temBuffer = new byte[1024];
                        int ReadCount = 1;
                        temBuffer[0] = headmark[0];
                        for (int i = 1; i < 1024; i++)
                        {
                            if (connection.NetworkStream.DataAvailable&&connection.NetworkStream.CanRead)
                            {
                                count = connection.NetworkStream.Read(headmark, 0, 1);
                                if (count > 0)
                                {
                                    temBuffer[i] = headmark[0];
                                    ReadCount++;
                                    if (headmark[0] == EndByte) { break; }
                                }
                                else { break; }
                            }
                            else { break; }
                        }
                        message.ByteMessageBody = new byte[ReadCount];
                        System.Buffer.BlockCopy(temBuffer, 0, message.ByteMessageBody, 0, ReadCount);
                        
                        message.MessageLength = ReadCount;
                        if (message.ByteMessageBody[ReadCount-1] == 0x7E&&ReadCount >1)
                        {
                            message.DataValidity = true;
                            message.Remark = "正确读到数据";
                        }
                        else
                        { 
                            message.DataValidity = false;
                            message.Remark = "数据没有正确的结束符";
                        }
                    }
                }
                else { message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "数据起始标识符没有正确取出"; }
            }
            else { message.SendToOrReceivedFrom = null; message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "网络链接不可读取"; }
            return message;
		}

        public static Message Parse(Connection connection, int ReadSize) 
        {
            Message message = new Message();
            if (connection.NetworkStream.DataAvailable) 
            {
                message.SendToOrReceivedFrom = connection;
                byte[] temBuffer = new byte[ReadSize];
                int ReadCount = connection.NetworkStream.Read(temBuffer, 0, ReadSize);
                if (ReadCount > 0)
                {
                    message.ByteMessageBody = new byte[ReadCount];
                    System.Buffer.BlockCopy(temBuffer, 0, message.ByteMessageBody, 0, ReadCount);
                    message.MessageLength = ReadCount;
                    message.DataValidity = true;
                    message.Remark = "正确读到数据";
                }
                else { message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "端口中未能读取到数据"; }

            }
            else { message.SendToOrReceivedFrom = null; message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "网络链接不可读取"; }
            return message;
        }
        public static Message ReceiveFrom(Connection connection) 
        {
            Message message = new Message();
            if (connection.NetworkStream.DataAvailable)
            {
                message.SendToOrReceivedFrom = connection;
                byte[] temBuffer = new byte[connection.Client.Available];
                int ReadCount = connection.Client.Receive(temBuffer);
                if (ReadCount > 0)
                {
                    message.ByteMessageBody = new byte[ReadCount];
                    System.Buffer.BlockCopy(temBuffer, 0, message.ByteMessageBody, 0, ReadCount);
                    message.MessageLength = ReadCount;
                    message.DataValidity = true;
                    message.Remark = "正确读到数据";
                }
                else { message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "端口中未能读取到数据"; }

            }
            else { message.SendToOrReceivedFrom = null; message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "网络链接不可读取"; }
            return message;
        }
    }
}
