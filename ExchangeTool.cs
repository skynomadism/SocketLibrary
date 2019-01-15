using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SocketLibrary
{
    public class ExchangeTool
    {
        /// <summary>
        /// 将一个实例转化志一个内存流
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static System.IO.MemoryStream SerializeBinary(object request)
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
        /// <summary>
        /// 利用模板函数实现的从结构体到字节数据的转化（不知道是否存在内存泄露）
        /// 用于UDP和TCP等的网络通讯
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Struct2Bytes<T>(T obj)
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
        public static T Bytes2Struct<T>(byte[] bytes)
        {
            IntPtr arrPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            return (T)Marshal.PtrToStructure(arrPtr, typeof(T));
        }
    }
}
