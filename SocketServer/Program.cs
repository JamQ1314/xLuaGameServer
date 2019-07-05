
using SocketServer.NetServer;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NetServer.NetServer serv = new NetServer.NetServer();
            serv.Start(5566);

            NetMessageCenter.Ins.Init();
            SqlManager.Ins.Init();
            LobbyServer.Ins.Init();
            //NiuNiuServer.Ins.Init();
            
            while (true) { }
        }
    }
}
