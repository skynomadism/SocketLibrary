using System;

namespace SocketLibrary
{
	public class Message
	{
		public Connection SendToOrReceivedFrom;
		public int MessageLength; 
        public byte[] ByteMessageBody;//�ֽ��͵�������

        public bool DataValidity;//ר���ڱ�ǽ��յ��������Ƿ���Ч����ʱ����Ϊճ�����������ݴ�λ�����ݴ�ʱ��Ѳ���������Ч����Ҳ���չ�����֪ͨ��
        public string Remark = "";
        #region �ַ������ݳ�ʼ����
        public Message()
		{
			SendToOrReceivedFrom = null;
            DataValidity = false;
		}
        #endregion
        #region �ֽ������ݳ�ʼ����
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
        /// ��һ��������õ����ֽ����ݣ�������װ��һ����Ϣ
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static Message Parse(Connection connection,byte StartByte=0x7E,byte EndByte=0x7E) {
            Message message = new Message();
            if (connection.NetworkStream.DataAvailable)
            {
                message.SendToOrReceivedFrom = connection;
                //���ȼ�鼴�������������Ƿ�������ͷ�������������ǣ�����һ����Ч����ȡ�����ȡ1024��
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
                        message.Remark = "����û����ʼ���Ĳ�ȱ����";
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
                            message.Remark = "��ȷ��������";
                        }
                        else
                        { 
                            message.DataValidity = false;
                            message.Remark = "����û����ȷ�Ľ�����";
                        }
                    }
                }
                else { message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "������ʼ��ʶ��û����ȷȡ��"; }
            }
            else { message.SendToOrReceivedFrom = null; message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "�������Ӳ��ɶ�ȡ"; }
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
                    message.Remark = "��ȷ��������";
                }
                else { message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "�˿���δ�ܶ�ȡ������"; }

            }
            else { message.SendToOrReceivedFrom = null; message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "�������Ӳ��ɶ�ȡ"; }
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
                    message.Remark = "��ȷ��������";
                }
                else { message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "�˿���δ�ܶ�ȡ������"; }

            }
            else { message.SendToOrReceivedFrom = null; message.DataValidity = false; message.ByteMessageBody = null; message.MessageLength = 0; message.Remark = "�������Ӳ��ɶ�ȡ"; }
            return message;
        }
    }
}
