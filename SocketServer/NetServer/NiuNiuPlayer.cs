using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.NetServer
{
    public enum NiuNiuType : int
    {
        没牛 = 0,
        牛一 = 1,
        牛二,
        牛三,
        牛四,
        牛五,
        牛六,
        牛七,
        牛八,
        牛九,
        牛牛,
        五花牛,
        五小牛,
    }
    public class NiuNiuPlayer
    {
        public int SeatID;//座位号
        public int GState; //游戏状态 0 未准备 1 准备
        public user.User UserInfo; //角色信息
        public Conn conn; //socket
        public int[] NNCards;
        public NiuNiuType NNType = NiuNiuType.没牛;
        public int MaxCard; //最大的牌

        public NiuNiuPlayer(int _seatid, user.User _userinfo, Conn _conn)
        {
            SeatID = _seatid;
            GState = 0;
            UserInfo = _userinfo;
            conn = _conn;
            NNCards = new int[5];
        }
    }
}
