using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SocketServer.Utils;

namespace SocketServer.NetServer
{
    public class Conn
    {
        public int ClientUseID; //标识，用户ID

        public const int BUFFER_SIZE = NetUtil.SOCKET_TCP_BUFFER;
        public const int TCPHEAD_SIZE = NetUtil.TCP_HEAD_SIZE;
        public Socket socket;
        public byte[] readBuff;         //缓存区
        public int buffCount;

        public bool isUse = false;

        private byte[] TmpRecvBuf = null;
        private uint TmpAllRecvSize = 0;
        private uint TmpHadRecvSize = 0;


        public Conn()
        {
            readBuff = new byte[BUFFER_SIZE];
            buffCount = 0;
        }

        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;


        }

        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }

        public string GetAddress()
        {
            return socket.RemoteEndPoint.ToString();
        }

        public void ReceiveAsync()
        {
            socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCB, this);
        }

        private uint idx = 0;
        private uint total = 0;

        private void ReceiveCB(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            try
            {
                int nBufferSize = conn.socket.EndReceive(ar);
                if (nBufferSize <= 0)
                {
                    LogUtil.LogWarm("客户端" + conn.GetAddress() + "断开连接！");
                    conn.Close();
                    return;
                }
                
                if (TmpAllRecvSize == 0)
                {
                    if (nBufferSize < TCPHEAD_SIZE)
                        return;         //异常处理
                    TmpAllRecvSize = BitConverter.ToUInt32(conn.readBuff, 0);//0 ：包头中的数据长度是第一个32位
                    TmpRecvBuf = new byte[TmpAllRecvSize];
                    LogUtil.LogInfo("准备接受数据：" + TmpAllRecvSize);
                }

                Array.Copy(conn.readBuff, 0, TmpRecvBuf, TmpHadRecvSize, nBufferSize);
                TmpHadRecvSize += (uint)nBufferSize;
                if (TmpHadRecvSize >= TmpAllRecvSize)
                {
                    LogUtil.LogInfo("准备处理数据：" + TmpRecvBuf.Length);
                    handleNetMsg(TmpRecvBuf);
                    TmpRecvBuf = null;
                    TmpAllRecvSize = 0;
                    TmpHadRecvSize = 0;
                }
                socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCB, this);
            }
            catch (Exception e)
            {
                Console.WriteLine("ReceiveCB失败：" + e.Message);
                conn.Close();
            }
        }

        private void handleNetMsg(byte[] msgBytes)
        {
            int nHeadSize = Marshal.SizeOf(typeof(TCP_Head));
            TCP_Head tcpHead = (TCP_Head)NetUtil.BytesToStruct<TCP_Head>(msgBytes, nHeadSize);
            int nBufferSize = msgBytes.Length - nHeadSize;

            byte[] buffer = new byte[nBufferSize];
            Array.Copy(msgBytes, nHeadSize, buffer, 0, nBufferSize);

            LogUtil.LogInfo(string.Format(">>>>>>>>>>    收到客户端数据长度：{0}   主命令：{1}  子命令：{2}",msgBytes.Length, tcpHead.Cmd.Main_ID, tcpHead.Cmd.Sub_ID));
            NetMessageCenter.Ins.DispatchMsg(this,(int)tcpHead.Cmd.Main_ID,(int) tcpHead.Cmd.Sub_ID, buffer);
        }

        public void SendAsync(ushort main_id, ushort sub_id, byte[] data = null, Action<Hashtable> callback = null, Hashtable hashtable = null)
        {
            TCP_Head tcpHead = new TCP_Head();
            tcpHead.Cmd.Main_ID = main_id;
            tcpHead.Cmd.Sub_ID = sub_id;
            int nHeadSize = Marshal.SizeOf(tcpHead);

            int nSendSize = nHeadSize;
            if (data != null)
                nSendSize += data.Length;
            tcpHead.Info.Buffer_Size = (ushort)nSendSize;

            byte[] SendBuffer = new byte[nSendSize];
            var Headbuffer = NetUtil.StructToBytes(tcpHead);
            Array.Copy(Headbuffer, SendBuffer, nHeadSize);
            if (data != null)
                Array.Copy(data, 0, SendBuffer, nHeadSize, data.Length);

            lock (socket)
            {
                socket.BeginSend(SendBuffer, 0, nSendSize, SocketFlags.None, (ar) =>
                {
                    try
                    {
                        socket.EndSend(ar);
                        callback?.Invoke(hashtable);
                        LogUtil.LogInfo(string.Format("<<<<<<<<<<    向客户端发送数据，长度 ：{0}   主命令：{1}   子命令：{2}", SendBuffer.Length,
                            main_id, sub_id));
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("Send失败：" + e.Message);
                        Close();
                    }
                }, socket);
            }
        }

        public void Send( ushort mainid, ushort subid, string msg)
        {
            simpledata.SimpleString simpleStr = new simpledata.SimpleString();
            simpleStr.simple = msg;
            byte[] data = NetUtil.ProtobufSerialize(simpleStr);
            SendAsync(mainid, subid, data);
        }

        public void Send(ushort mainid, ushort subid, int msg)
        {
            simpledata.SimpleInt simpleInt = new simpledata.SimpleInt();
            simpleInt.simple = msg;
            byte[] data = NetUtil.ProtobufSerialize(simpleInt);
            SendAsync(mainid, subid, data);
        }

        public void Send<T>(ushort mainid, ushort subid, T pStruct) where T : ProtoBuf.IExtensible
        {
           SendAsync(mainid, subid, NetUtil.ProtobufSerialize(pStruct));
        }

        public void Close()
        {
            socket.Close();
            isUse = false;
        }
    }
}
