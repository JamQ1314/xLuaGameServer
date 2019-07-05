using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace SocketServer.NetServer
{
    public interface IGame
    {
        void Init();
        void DispatchMsg(Conn client,int Sub_ID,byte[] buffer);
    }
}
