using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SocketLibrary
{
    public class PackageStruct
    {
        /// <summary>
        /// 从数据流中得到数据头
        /// </summary>
        /// <param name="CaptureData"></param>
        /// <returns></returns>
        public static PackageStruct.Pro_Head BytesToHead(byte[] CaptureData)
        {
            if (CaptureData.Length > 10)
            {
                PackageStruct.Pro_Head Head = new PackageStruct.Pro_Head();
                Head.ver = CaptureData[1];
                Head.func = CaptureData[0];
                Head.addr = new PackageStruct.in_addr();
                Head.addr.addr1 = CaptureData[2];
                Head.addr.addr2 = CaptureData[3];
                Head.addr.addr3 = CaptureData[4];
                Head.addr.addr4 = CaptureData[5];
                Head.sin_port = (short)(CaptureData[7] * 256 + CaptureData[6]);
                Head.crc = (short)(CaptureData[9] * 255 + CaptureData[8]);
                return Head;
            }
            else
            {
                return null;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class in_addr
        {
            public in_addr() { }
            public in_addr(string[] ip) 
            {
                if (ip.Length == 4) 
                {
                    addr1 = (byte)int.Parse(ip[0]);
                    addr2 = (byte)int.Parse(ip[1]);
                    addr3 = (byte)int.Parse(ip[2]);
                    addr4 = (byte)int.Parse(ip[3]);
                }
            }
            public byte addr1 = 0;
            public byte addr2 = 0;
            public byte addr3 = 0;
            public byte addr4 = 0;

            //public byte[] a1=new byte[12];
        };
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Pro_Head
        {
            public Pro_Head() { }
            public Pro_Head(string IP, int Port, int Func) 
            {
                func = (byte)Func;
                addr = new in_addr(IP.Split('.'));
                sin_port = (short)Port;
            }
            public byte func;	//function number   [1]
            public  byte ver=3;	//version           [0]
            public in_addr addr;//用户名IP          [2,3,4,5]
            public short sin_port;//端口            [6,7]
            public short crc;	//check value       [8,9]

        };

        //数据请求
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LanMSG52
        {
            public LanMSG52(){}
            public LanMSG52(string IP, int port)
            {
                head = new Pro_Head(IP, port, 48);
            }
            public Pro_Head head;
            public short CID;
            public short DID;
            public byte req_code;
            //public short  port;
            //char 			IP[20];
        };
        //数据请求
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LanMSG49
        {
            public LanMSG49() { }
            public LanMSG49(string IP, int port)
            {
                head = new Pro_Head(IP, port, 49);
            }
            public Pro_Head head;
            public short CID;
            public short DID;
            public byte req_code;
            //public short  port;
            //char 			IP[20];
        };
        //数据请求
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LanMSG53:LanMSG49
        {
            public LanMSG53(){}
            public LanMSG53(string IP, int port)
            {
                head = new Pro_Head(IP, port, 53);
            }
        };
        //数据请求
        //操作数据
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LanMSG54
        {
            public LanMSG54(){}
            public LanMSG54(string IP, int port)
            {
                head = new Pro_Head(IP, port, 54);
            }
            public Pro_Head head;
            public short CID;	//Control_head_ID
            public short DID;	//device_radio_ID;
            public byte flag;	//0.write chmsg to com;
            //1.translate data then write collerent data to com
            public int opcode;	//Operate code define by 3G;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public int[] dwParam =new int[40];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] chmsg = new byte[256];
        };
        //操作数据
        //串口数据
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LanMSG55
        {
            public LanMSG55(){}
            public LanMSG55(string IP, int port)
            {
                head = new Pro_Head(IP, port, 55);
            }
            public Pro_Head head;
            public short CID;	//Control_head_ID
            public short DID;	//device_radio_ID;
            public byte flag;	//0.chmsg is raw data from com;unsigned char
            //1.translated data by device of radio; 
            public int msgcode;//Msg code define by 3G;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public int[] dwParam = new int[40];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] chmsg = new byte[256];
        };
        //串口数据
        //语音数据
        //client to server
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LanMSG56
        {
            public LanMSG56(){}
            public LanMSG56(string IP,int port) 
            {
                head = new Pro_Head(IP,port,56);
            }
            public Pro_Head head;
            public short CID;	//Control_head_ID
            public short DID;	//device_radio_ID;
            public byte vcode;	//语音编码
            public int len;	//数据长度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 800)]
            public byte[] buf; //数据缓存
        };
        // server to client
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LanMSG57
        {
            public LanMSG57(){}
            public LanMSG57(string IP, int port)
            {
                head = new Pro_Head(IP, port, 57);
            }
            public Pro_Head head;
            public short CID;	//Control_head_ID
            public short DID;	//device_radio_ID;
            public byte vcode;	//语音编码
            public int len;	//数据长度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 800)]
            public byte[] buf; //数据缓存
        };
        //语音数据
        /*UDP 协议定义-----结束*/


        //=========================================================================
        
        //public class in_addr {
        //union {
        //        class { u_char s_b1,s_b2,s_b3,s_b4; } S_un_b;
        //        class { u_short s_w1,s_w2; } S_un_w;
        //        u_long S_addr;
        //};
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class _Base
        {
            public byte Func;//Func Param
            public byte Ver;//版本编号

            public in_addr addr;//用户名IP
            public short sin_port;//端口

            public short CRC;

        };
//===============================END=========================================


//=========================================================================
        public class _RunParm
        {
            public byte cFormatZip;
            public byte cChannel;//声道数目，1--单声道；2--双声道
            public int cSample;//采样频率
            public byte cBit;//记录每个取样所需的位元数
            public short BuffLen;//回传字节数
            //51用
            public long sTGID;//本机电台组ID
            public long chCallerID;//主叫ID
            //53用
            public byte CCI;		//call instance
            public byte cDiscntCause;	//连接断开原因
        };
//===============================END=========================================



//=========================================================================
        public class _USerData//用户数据
        {
            public byte[] UserName;//16
            public in_addr addr;//本机IP
            public short sin_port;//本机端口
            public short PlayStart;

        };
//===============================END=======================================


//=========================================================================
        public class _Command2//下发--参数设置
        {
            public _Base Base;
            public _RunParm RunParm;

        };

//=========================================================================
        class _Command4//下发--读参数
        {
            public _Base Base;

        };

        public class _Command5//上传--读参数
        {
            public _Base Base;
            //	_USerData USerData;
            public _RunParm RunParm;

        };
//===============================END=======================================

//=========================================================================
        public class _Command6//下发--读运行参数
        {
            _Base Base;

        };

        public class _Command7//上发--读运行参数
        {
            public _Base Base;

        };
//===============================END=======================================



//=========================================================================
public  class _Command8//下发--com解析
{
	public _Base Base;
	public int ControlBit;		//1:解析；0:不解析

};

public  class _Command9//上发--com解析
{
	public _Base Base;

};

//===============================END=======================================

//=========================================================================
public  class _Command10//下发--写IO
{
	public _Base Base;

};

public  class _Command11//上发--写IO
{
	public _Base Base;

};

//===============================END=========================================

//=========================================================================
public  class _Command12//下发--读IO
{
	public _Base Base;

};
public  class _Command13//上发--读IO
{
	public _Base Base;

};
//===============================END=========================================
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



//=========================================================================
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
public  class _Command50//下发--启动音频
{
	public _Base	Base;
	public _USerData  USerData;//用户数据
		
};
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
public  class _Command51//上传--音频数据
{
	public _Base	              Base;			
	public byte   cFormatZip;
	public short	Len;		//本次传输字节数
	public long   sTGID;		//本机电台组ID
	public long   chCallerID;	//主叫ID
	public long   radioID;	//电台ID
	
};

//无_Command52
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
public  class _Command53//上传--音频数据
{
	_Base	Base;
	public  long        sTGID;//本机电台组ID
	public  long   chCallerID;//主叫ID
	public  long      radioID;//电台ID
	public  byte          CCI;//call instance
	public  byte cDiscntCause;//连接断开原因

};
//===============================END=========================================
    }
}
