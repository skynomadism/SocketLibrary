using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SocketLibrary
{
    public class UDPPackage
    {
        public static int SoundBufferLenth = 512;
        /*新数据通讯协议数据结构*/
        public class UdpData 
        {
            public byte HEAD;
            public short LEN;
            public byte CID;
            public byte DID;
            public byte CMD;
            public byte EXP;
            public byte[] DATA;
            public byte END;
            public UdpData() { }
            public byte[] ToByte()
            {
                int dataLenth = DATA.Length + 8;
                byte[] buffer = new byte[dataLenth];
                buffer[0]= HEAD;
                BitConverter.GetBytes(LEN).CopyTo(buffer, 1);//将长度的2个字节写入到数组中。
                buffer[3] = CID;
                buffer[4] = DID;
                buffer[5] = CMD;
                buffer[6] = EXP;
                DATA.CopyTo(buffer, 7);
                buffer[dataLenth - 1] = END;
                
                return buffer;
            }
            public static UdpData Prase(byte[] netBuffer) 
            {
                if (netBuffer.Length > 8)
                {
                    try
                    {
                        UdpData data = new UdpData();
                        data.HEAD = netBuffer[0];
                        data.LEN = (short)(netBuffer[1] * 255 + netBuffer[2]);
                        data.CID = netBuffer[3];
                        data.DID = netBuffer[4];
                        data.CMD = netBuffer[5];
                        data.EXP = netBuffer[6];
                        data.DATA = new byte[netBuffer.Length - 8];
                        System.Buffer.BlockCopy(netBuffer, 7, data.DATA, 0, netBuffer.Length - 8);
                        data.END = netBuffer[netBuffer.Length - 1];
                        return data;
                    }
                    catch 
                    {
                        return null;
                    }
                }
                else 
                {
                    return null;
                }
            }

        }
        /*新数据通讯协议数据结构*/

        /*
        public class UDPData 
        {
            public Pro_Head Head;
            public byte[] CaptureData;
            public string Message;
            public string IP;
            public int DataLenth;
            /// <summary>
            /// 数据的类型0命令，1音频，2短信
            /// </summary>
            public int DataKindFlag;
            public UDPData() 
            {
                Head = new Pro_Head();
            }
            /// <summary>
            /// UDP所发送的数据包
            /// </summary>
            /// <param name="transceiverIP">目标IP</param>
            /// <param name="captureData">音频数据</param>
            /// <param name="msg">文字信息</param>
            /// <param name="dataFlag">数据的类型0命令，1音频，2短信</param>
            public UDPData(string transceiverIP, byte[] captureData, string msg, int dataFlag) 
            {
                this.IP = transceiverIP;
                this.CaptureData = captureData;
                this.Message = msg;
                this.DataKindFlag = dataFlag;
                Head = new Pro_Head();
            }
            public byte[] ToByte() 
            {
                int dataLenth = (this.DataKindFlag == 1 
                    ? this.CaptureData.Length 
                    : SocketFactory.DefaultEncoding.GetByteCount(this.Message))
                    + 16;
                byte[] buffer = new byte[dataLenth];
                //先将长度的4个字节写入到数组中。
                BitConverter.GetBytes(dataLenth).CopyTo(buffer, 0);
                string[] ipsplit = this.IP.Split('.');
                if (ipsplit.Length < 4)
                {
                    buffer[4] = (byte)192;
                    buffer[5] = (byte)168;
                    buffer[6] = (byte)0;
                    buffer[7] = (byte)82;
                }
                else 
                {
                    buffer[4] = (byte)int.Parse(ipsplit[0]);
                    buffer[5] = (byte)int.Parse(ipsplit[1]); ;
                    buffer[6] = (byte)int.Parse(ipsplit[2]); ;
                    buffer[7] = (byte)int.Parse(ipsplit[3]); ;
                }
                

                buffer[8] = (byte)this.DataKindFlag;
                buffer[9] = (byte)0;
                buffer[10] = (byte)0;
                buffer[11] = (byte)0;
                buffer[12] = (byte)0;
                buffer[13] = (byte)0;
                buffer[14] = (byte)0;
                buffer[15] = (byte)0;
                if (this.DataKindFlag == 1)
                {
                    CaptureData.CopyTo(buffer, 16);
                }
                else 
                {
                    SocketFactory.DefaultEncoding.GetBytes(this.Message).CopyTo(buffer, 16);
                }
                return buffer;
            }
            /// <summary>
            /// 把字符命令进行装包
            /// 0-15字节是头，其中4-7是IP地址
            /// 第8但是0表示送的是音频数据，1表示送的是字符串数据
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            public static byte[] ToByte(string _TransceiverIP, byte[] CaptureData, string Msg, int DataFlag)
            {
                return new UDPData(_TransceiverIP, CaptureData, Msg, DataFlag).ToByte();
            }

            /// <summary>
            /// 进行解包操作
            /// </summary>
            /// <param name="UdpByte"></param>
            /// <returns></returns>
            public static UDPData Prase(byte[] UdpByte)
            {
                UDPData UdpData = new UDPData();
                //先读出前十六个字节，即Message长度和它的一些参数（包头）
                byte[] buffer = new byte[4];
                //UdpData.DataLenth= int.Parse(SocketFactory.DefaultEncoding.GetString(UdpByte,0,4));
                buffer[0] = UdpByte[0];
                buffer[1] = UdpByte[1];
                buffer[2] = UdpByte[2];
                buffer[3] = UdpByte[3];
                
                UdpData.Head.addr1 = UdpByte[4];
                UdpData.Head.addr2 = UdpByte[5];
                UdpData.Head.addr3 = UdpByte[6];
                UdpData.Head.addr4 = UdpByte[7];
                UdpData.IP = ((int)UdpData.Head.addr1).ToString() + "."
                    + ((int)UdpData.Head.addr2).ToString() + "."
                    + ((int)UdpData.Head.addr3).ToString() + "."
                    + ((int)UdpData.Head.addr4).ToString();

                UdpData.Head.remark = UdpByte[8];
                UdpData.DataKindFlag = (int)UdpByte[8];
                UdpData.CaptureData = new byte[UdpByte.Length - 16];
                System.Buffer.BlockCopy(UdpByte, 16, UdpData.CaptureData, 0, UdpByte.Length - 16);
                if (UdpData.DataKindFlag != 1)
                {
                    UdpData.Message = SocketFactory.DefaultEncoding.GetString(UdpData.CaptureData, 0, UdpData.CaptureData.Length);
                }
                return UdpData;
            }
        }
        */

        /// <summary>
        /// 将一个实例转化志一个内存流
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public System.IO.MemoryStream SerializeBinary(object request)
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter serializer 
                = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.MemoryStream memStream = new System.IO.MemoryStream();
            serializer.Serialize(memStream, request);
            return memStream;
        }
        /// <summary>
        /// 结构体转byte数组（应当不存在内存泄露）
        /// </summary>
        /// <param name="structObj">要转换的结构体</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] StructToBytes(object structObj)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(structObj);
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }

        /// <summary>
        /// byte数组转结构体（应当不存在内存泄露）
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="type">结构体类型</param>
        /// <returns>转换后的结构体</returns>
        public static object BytesToStuct(byte[] bytes, Type type)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(type);
            //byte数组长度小于结构体的大小
            if (size > bytes.Length)
            {
                //返回空
                return null;
            }
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回结构体
            return obj;
        }























        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PaketHead
        {
            public UInt32 OPCode;
            public byte DiskFlag;
            public long DiskSize;
            public long OPOffSet;
            public long OPByteCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] Authentic;
            public byte Encrypt;
            public byte Ver;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] AddIn;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] Reserve;
            public byte Zero;
            public int SizeOfHead;
        }


        /// <summary>
        /// 利用模板函数实现的从结构体到字节数据的转化（不知道是否存在内存泄露）
        /// 用于UDP和TCP等的网络通讯
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected byte[] Struct2Bytes<T>(T obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] bytes = new byte[size];
            IntPtr arrPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            Marshal.StructureToPtr(obj, arrPtr, true);
            return bytes;

        }
        /// <summary>
        /// 利用模板函数实现的，从字节数据到结构体的转化（不知道是否存在内存泄露）
        /// 用于UDP和TCP等的网络通讯
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected T Bytes2Struct<T>(byte[] bytes)
        {
            IntPtr arrPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            return (T)Marshal.PtrToStructure(arrPtr, typeof(T));
        }


    }
}
