using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.NetServer
{
    public class ConnPool
    {
        private static ConnPool ins;

        public static ConnPool Ins
        {
            get
            {
                if (ins == null)
                    ins = new ConnPool();
                return ins;
            }
        }

        public Conn[] Conns;                        //连接池
        public Dictionary<int, Conn> dictClients;      //客户端集合

        public ConnPool(int max = 999)
        {
            Conns = new Conn[max];
            for (int i = 0; i < max; i++)
            {
                Conns[i] = new Conn();
            }
        }

        public int MatchConnID()
        {
            for (int i = 0; i < Conns.Length; i++)
            {
                if (Conns[i].isUse == false)
                    return i;
            }
            return -1;
        }

    }
}
