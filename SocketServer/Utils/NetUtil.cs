using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ProtoBuf;

[StructLayoutAttribute(LayoutKind.Sequential,CharSet= CharSet.Unicode,Pack =1)]
public struct TCP_Buffer
{
    public TCP_Head TCPHead;
    [MarshalAs(UnmanagedType.ByValArray,SizeConst = NetUtil.SOCKET_TCP_BUFFER)]
    public byte[] Buffer;
}

public struct TCP_Info
{
    public uint Buffer_Size;
    public uint Check_ID;
}
public struct TCP_Commend
{
    public uint Main_ID;
    public uint Sub_ID;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
public class TCP_Head
{
    public TCP_Info Info;
    public TCP_Commend Cmd;
}


public class NetUtil
{
    public const int SOCKET_TCP_BUFFER = 4096 * 3;

    public const int TCP_HEAD_SIZE = 16;

    public const string ImgPath = "d:/res/calc/headico/";

    public static byte[] StringToBytes(string str)
    {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes; 
    }

    public static string BytesToString(byte[] bytes)
    {
        char[] chars = new char[bytes.Length / sizeof(char)];
        System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        return new string(chars);
    }

    //结构体转字节数组
    public static byte[] StructToBytes(object structObj, int size = 0)
    {
        if (size == 0)
        {
            size = Marshal.SizeOf(structObj);
        }
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structObj, buffer, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(buffer, bytes, 0, size);
            return bytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine("struct to bytes error:" + ex);
            return null;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    //字节数组转结构体
    public static object BytesToStruct<T>(byte[] bytes, int nSize)
    {

        if (bytes == null)
        {
            Console.WriteLine("null bytes!!!!!!!!!!!!!");
        }
        int size = Marshal.SizeOf(typeof(T));
        IntPtr buffer = Marshal.AllocHGlobal(nSize);
        //Console.WriteLine.LogError("Type: " + strcutType.ToString() + "---TypeSize:" + size + "----packetSize:" + nSize);
        try
        {
            Marshal.Copy(bytes, 0, buffer, nSize);
            return Marshal.PtrToStructure(buffer, typeof(T));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Type: " + typeof(T).ToString() + "---TypeSize:" + size + "----packetSize:" + nSize + " error"+ ex.ToString());
            return null;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }


    public static byte[] ProtobufSerialize<T>(T pbStruct)
    {
        MemoryStream ms = new MemoryStream();
        Serializer.Serialize<T>(ms, pbStruct);
        return ms.ToArray();
    }

    public static T ProtobufDeserialize<T>(byte[] bytes)
    {
        MemoryStream ms = new MemoryStream(bytes);
        return Serializer.Deserialize<T>(ms);
    }
}
