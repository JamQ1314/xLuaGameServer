using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.NetServer
{


    public enum MainID :ushort
    {
        Lobby = 1,
        NiuNiu = 2,
    }


    public enum LobbyID:ushort
    {
        AccLogin = 1,
        AccRegister = 2,
        WxLogin = 3,
        VisitorLogin=4,
        UploadHead = 50,
        DownloadHead = 51,



        //
        LoginSuccess = 101,
        LoginFaiure = 102,
        RegisterFailure= 103,
    }

    public enum NiuNiuID
    {
        Create = 1,
        Jion = 2,//自己加入房间
        Leave,
        Ready,
        

        //
        CreateSuccess = 101,
        JionSuccess = 102 ,
        JionFailure = 103,
        PlayerJion = 104, //其他玩家加入房间
        LeaveSuccess =105 ,
        LeaveFailure  = 106,
        ReadySuccess = 107,
        TurnHost = 108,//房主切换
        GameDeal = 109, //发牌
        TimeCount =110, //倒计时
        GameLay = 111, //亮牌
    }
    public class NetCmd
    {
    }
}
