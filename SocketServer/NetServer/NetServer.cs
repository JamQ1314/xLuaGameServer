using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SocketServer.Utils;

namespace SocketServer.NetServer
{
    public class NetServer
    {

        public Socket m_Listen;
        private int maxClient;
        private Dictionary<ushort, Action<TCP_Buffer>> msgDispatcher;
        public NetServer(int max = 9999)
        {
            m_Listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            maxClient = max;
            msgDispatcher = new Dictionary<ushort, Action<TCP_Buffer>>();
        }

        public void Init(int max = 9999)
        {
            m_Listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            maxClient = max;
            msgDispatcher = new Dictionary<ushort, Action<TCP_Buffer>>();
        }

        public void Start(int port)
        {
            m_Listen.Bind(new IPEndPoint(IPAddress.Any, port));
            m_Listen.Listen(maxClient);
            m_Listen.BeginAccept(AcceptCB, null);
            LogUtil.LogInfo("服务已启动，端口" + port);
        }

        private void AcceptCB(IAsyncResult ar)
        {
            try
            {
                Socket client = m_Listen.EndAccept(ar);
                int ConnID = ConnPool.Ins.MatchConnID();
                if (ConnID == -1)
                    return;
                Conn conn = ConnPool.Ins.Conns[ConnID];
                conn.Init(client);
                LogUtil.LogInfo("ConnID：" + ConnID.ToString("00") + "  客户端" + conn.GetAddress() + "上线...... ");
                //conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCB,conn);
                conn.ReceiveAsync();

            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCB失败：" + e.Message);
            }
            finally
            {
                m_Listen.BeginAccept(AcceptCB, null);
            }
        }
    }
}
