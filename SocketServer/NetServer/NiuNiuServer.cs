using SocketServer.Utils;
using System;
using System.Collections.Generic;

namespace SocketServer.NetServer
{
    public class NiuNiuServer : IGame
    {
        #region  singleton
        private static NiuNiuServer ins = null;
        public static NiuNiuServer Ins
        {
            get
            {
                if (ins == null)
                    ins = new NiuNiuServer();
                return ins;
            }
        }
        #endregion

        private Dictionary<int, NiuNiuGame> nnGames;
        public void DispatchMsg(Conn client, int Sub_ID, byte[] buffer)
        {
            switch ((NiuNiuID)Sub_ID)
            {
                case NiuNiuID.Create:
                    DoCreate(client,buffer);
                    break;
                case NiuNiuID.Jion:
                    DoJion(client, buffer);
                    break;
                case NiuNiuID.Leave:
                    DoLeave(client, buffer);
                    break;
                case NiuNiuID.Ready:
                    DoReady(client, buffer);
                    break;
            }
        }

        public void Init()
        {
            NetMessageCenter.Ins.RegisterServer((int) MainID.NiuNiu, this);

            nnGames = new Dictionary<int, NiuNiuGame>();

        }

        public void DoCreate(Conn client, byte[] buffer)
        {
            int roomid = new Random().Next(1000, 10000);

            while (nnGames.ContainsKey(roomid))
            {
                roomid = new Random().Next(1000, 10000);
            }

            
            var newGame = new NiuNiuGame(roomid);
            newGame.RoomCreate(client);
            nnGames.Add(roomid, newGame);
            LogUtil.LogInfo("******  创建房间 ：" + roomid);
        }

        public void DoJion(Conn client, byte[] buffer)
        {
            var simpleInt = NetUtil.ProtobufDeserialize<simpledata.SimpleInt>(buffer);
            int jroomid = simpleInt.simple;
            

            if (!nnGames.ContainsKey(jroomid))
            {
                var simpleStr = new simpledata.SimpleString();
                simpleStr.simple = string.Format("加入房间失败：房间号{0}不存在！",jroomid);
                byte[] data = NetUtil.ProtobufSerialize(simpleStr);
                client.SendAsync((ushort) MainID.NiuNiu, (ushort)NiuNiuID.JionFailure, data);
                return;
            }

            NiuNiuGame jGame = nnGames[jroomid];
            jGame.RoomJion(client);
        }

        public void DoLeave(Conn client, byte[] buffer)
        {
            var simpleInt = NetUtil.ProtobufDeserialize<simpledata.SimpleInt>(buffer);
            int roomid = simpleInt.simple;

            NiuNiuGame nngame = nnGames[roomid];

            if (nngame.RoomLeave(client))
            {
                nnGames.Remove(roomid);
                LogUtil.LogInfo("******  房间解散 ：" + roomid);
            }
        }

        public void DoReady(Conn client, byte[] buffer)
        {
            var simpleInt = NetUtil.ProtobufDeserialize<simpledata.SimpleInt>(buffer);
            int roomid = simpleInt.simple;
            NiuNiuGame game = nnGames[roomid];
            game.PlayerReady(client);

        }
    }
}
