using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using card;
using user;
using System.Threading;

namespace SocketServer.NetServer
{
    public class NiuNiuGame
    {
        public int GameID;

        private int hostid;

        private int[] cardcolor = {1, 2, 3, 4};
        private int[] cardpoint = {1, 2, 3, 4,5,6,7,8,9,10,11,12,13};

        private int[] cards;

        private int Score = 100; //底分为 100;

        private int GamePlayerNumber = 6;
        private bool isPlaying = false; //代表游戏是否开始

        private Dictionary<int, NiuNiuPlayer> dictNNPlayers;//int 是座位id

        public NiuNiuGame(int gameid)
        {
            GameID = gameid;

            cards = new int[cardcolor.Length * cardpoint.Length];
            for (int i = 0; i < cardcolor.Length; i++)
            {
                for (int j = 0; j < cardpoint.Length; j++)
                {
                    cards[cardpoint.Length * i + j] = cardcolor[i] * 100 + cardpoint[j];
                }
            }

            dictNNPlayers = new Dictionary<int, NiuNiuPlayer>();

            hostid = 0;
        }

        public void RoomCreate(Conn client)
        {
            var userid = client.ClientUseID;
            var userinfo = LobbyServer.Ins.GetUserInfo(userid);
            NiuNiuPlayer player = new NiuNiuPlayer(0,userinfo,client);
            dictNNPlayers.Add(0, player);

            simpledata.SimpleInt simpleInt = new simpledata.SimpleInt();
            simpleInt.simple = GameID;
            client.SendAsync((ushort) MainID.NiuNiu, (ushort) NiuNiuID.CreateSuccess,NetUtil.ProtobufSerialize(simpleInt));
        }
        public void RoomJion(Conn client)
        {
            
            if (isPlaying)
            {
                simpledata.SimpleString simpleStr = new simpledata.SimpleString();
                simpleStr.simple = "加入房间失败：游戏已开始，请稍后。";
                byte[] data = NetUtil.ProtobufSerialize(simpleStr);
                client.SendAsync((ushort) MainID.NiuNiu, (ushort) NiuNiuID.JionFailure, data);
                //游戏开始 加入失败
                return;
            }

            if (dictNNPlayers.Count == GamePlayerNumber)
            {
                simpledata.SimpleString simpleStr = new simpledata.SimpleString();
                simpleStr.simple = "加入房间失败：游戏人数已满。";
                byte[] data = NetUtil.ProtobufSerialize(simpleStr);
                client.SendAsync((ushort)MainID.NiuNiu, (ushort)NiuNiuID.JionFailure, data);
                //游戏开始 加入失败
                return;
            }


            NiuNiuPlayer newNNPlayer = null;

            for (int i = 0; i < GamePlayerNumber; i++)
            {
                if (!dictNNPlayers.ContainsKey(i))
                {
                    var userid = client.ClientUseID;
                    var userinfo = LobbyServer.Ins.GetUserInfo(userid);
                    newNNPlayer = new NiuNiuPlayer(i, userinfo, client);
                    dictNNPlayers.Add(i, newNNPlayer);
                    break;
                }
            }
            //所有玩家数据（包括新加入的玩家）,发送给新加入的玩家
            GameSeatedUPlayers otherSeatedPlayers = new GameSeatedUPlayers();

            //刚加入玩家的数据，发给已经加入的玩家
            GamePlayer newGamePlayer = new GamePlayer();
            newGamePlayer.seatid = newNNPlayer.SeatID;
            newGamePlayer.user = newNNPlayer.UserInfo;
            byte[] newPlayerData = NetUtil.ProtobufSerialize(newGamePlayer);

            foreach (var temp in dictNNPlayers.Values)
            {
                GamePlayer player = new GamePlayer();
                player.seatid = temp.SeatID;
                player.state = temp.GState;
                player.user = temp.UserInfo;
                otherSeatedPlayers.players.Add(player);
                otherSeatedPlayers.roomid = GameID;
                otherSeatedPlayers.hostid = hostid;

                if (temp.SeatID != newNNPlayer.SeatID)
                {
                    temp.conn.SendAsync((ushort) MainID.NiuNiu, (ushort) NiuNiuID.PlayerJion, newPlayerData);
                }
            }

            LogUtil.LogInfo(string.Format("****** 用户ID{0}    加入房间{1}     座位{2}", client.ClientUseID,GameID, newNNPlayer.SeatID));
            byte[] allPlayerData = NetUtil.ProtobufSerialize(otherSeatedPlayers);
            client.SendAsync((ushort) MainID.NiuNiu, (ushort) NiuNiuID.JionSuccess, allPlayerData);
        }

        public bool RoomLeave(Conn client)
        {
            if (isPlaying)
            {
                simpledata.SimpleString simpleStr = new simpledata.SimpleString();
                simpleStr.simple = "离开房间失败：游戏已开始，请稍后。";
                byte[] data = NetUtil.ProtobufSerialize(simpleStr);
                client.SendAsync((ushort)MainID.NiuNiu, (ushort)NiuNiuID.LeaveFailure, data);
                //游戏开始 加入失败
                return false;
            }
            int userid = client.ClientUseID;

            int seatid = 0;

            foreach (var kvp in dictNNPlayers)
            {
                if (kvp.Value.UserInfo.id == userid)
                {
                    seatid = kvp.Key;
                    break;
                }
            }



            //客户端更新数据
//            var userinfo = dictNNPlayers[seatid].UserInfo;
//            byte[] data = NetUtil.ProtobufSerialize(userinfo);
//            client.SendAsync((ushort)MainID.NiuNiu, (ushort)NiuNiuID.LeaveSuccess, data);

            //移除玩家
            

            foreach (var v in dictNNPlayers.Values)
            {
                simpledata.SimpleInt msg = new simpledata.SimpleInt();
                msg.simple = seatid;
                byte[] data = NetUtil.ProtobufSerialize(msg);
                v.conn.SendAsync((ushort)MainID.NiuNiu, (ushort)NiuNiuID.LeaveSuccess, data);
            }
            dictNNPlayers.Remove(seatid);
            //判断是否还有玩家，没有就解散
            if (dictNNPlayers.Count == 0)
                return true;
            //若果离开的是坐庄的 则换下一个
            if (hostid == seatid)
            {
                for (int i = 1; i <GamePlayerNumber; i++)
                {
                    int nhostid = (seatid + 1)% GamePlayerNumber;
                    if (dictNNPlayers.ContainsKey(nhostid))
                    {
                        SetHost(nhostid);
                        break;
                    }
                }
            }
            return false;
        }


        public void PlayerReady(Conn client)
        {
            var userid = client.ClientUseID;
            int seatid = 0;
            foreach (var kvp in dictNNPlayers)
            {
                if (kvp.Value.UserInfo.id == userid)
                {
                    seatid = kvp.Key;
                    kvp.Value.GState = 1;//1表示准备状态
                    break;
                }
            }

            simpledata.SimpleInt msg = new simpledata.SimpleInt();
            msg.simple = seatid;
            byte[] data = NetUtil.ProtobufSerialize(msg);


            bool isAllReady = true;

            for (int i = 0; i < GamePlayerNumber; i++)
            {
                if (dictNNPlayers.ContainsKey(i))
                {
                    var p = dictNNPlayers[i];
                    if (p.GState == 0)
                        isAllReady = false;
                    p.conn.SendAsync((ushort)MainID.NiuNiu, (ushort)NiuNiuID.ReadySuccess, data);
                }
            }

            if (dictNNPlayers.Count >= 2)
            {
                if (isAllReady)
                {
                    //游戏开始
                    Thread t = new Thread(GameStart);
                    t.Start();
                }
            }
        }

        private void SetHost(int nHostID)
        {
            LogUtil.LogInfo(string.Format("****** 庄家转移至 SeateID : " + nHostID));
            if (hostid == nHostID)
                return;

            
            foreach (var v in dictNNPlayers.Values)
            {
                simpledata.SimpleInt msg = new simpledata.SimpleInt();
                msg.simple = nHostID;
                byte[] data = NetUtil.ProtobufSerialize(msg);
                v.conn.SendAsync((ushort)MainID.NiuNiu, (ushort)NiuNiuID.TurnHost, data);
            }

            hostid = nHostID;
        }


     

        public void GameStart()
        {
            Thread.Sleep(200);
            isPlaying = true;
            int[] mixcards = ShuffleCards();

            for (int i = 0; i < GamePlayerNumber; i++)
            {
                if (dictNNPlayers.ContainsKey(i))
                {
                    var player = dictNNPlayers[i];
                    OnePlayerCards onecards = new OnePlayerCards();
                    onecards.seatid = i;
                    onecards.nntype = 0;
                    onecards.xgold = 0;
                    string logstr = "******   玩家 " + i + " 的牌为 :";
                    for (int j = 0; j < player.NNCards.Length; j++)
                    {
                        var card = mixcards[player.NNCards.Length * i + j];
                        player.NNCards[j] = card;
                        onecards.cards.Add(card);

                        logstr += mixcards[player.NNCards.Length * i + j] + "  ";
                    }

                    player.NNType = NiuNiuHelper.CalcNNType(player.NNCards);
                    player.MaxCard = NiuNiuHelper.GetMaxCard(player.NNCards);
                    logstr += string.Format("   牛牛点数为:{0}   最大的牌为:{1}", player.NNType, player.MaxCard);
                    byte[] data = NetUtil.ProtobufSerialize(onecards);
                    player.conn.SendAsync((ushort) MainID.NiuNiu, (ushort) NiuNiuID.GameDeal, data);

                    LogUtil.LogInfo(logstr);
                }
            }

            //倒计时
            int nDelayTime = 3;
            Thread.Sleep(200);
            TimeCount(nDelayTime);

            Thread.Sleep((nDelayTime +1)*1000);
            GameLayCards();

            Thread.Sleep((dictNNPlayers.Count + 1) * 1000);
            //判断是否换庄家，游戏初始化

        }


        public void TimeCount(int seconds)
        {
            simpledata.SimpleInt timemsg = new simpledata.SimpleInt();
            timemsg.simple = seconds;
            byte[] data = NetUtil.ProtobufSerialize(timemsg);
            foreach (var v in dictNNPlayers.Values)
            {
                v.conn.SendAsync((ushort)MainID.NiuNiu, (ushort)NiuNiuID.TimeCount, data);
            }
        }
        /// <summary>
        /// 游戏开牌
        /// </summary>
        public void GameLayCards()
        {

            var hostPlayer = dictNNPlayers[hostid];
            int xhostgold = 0;

            AllPlayersCards allcards = new AllPlayersCards();

            foreach (var v in dictNNPlayers.Values)
            {
                if (v.SeatID != hostid)
                {
                    OnePlayerCards onecards = new OnePlayerCards();
                    onecards.seatid = v.SeatID;
                    onecards.nntype = (int)v.NNType;
                    for (int i = 0; i < v.NNCards.Length; i++)
                    {
                        onecards.cards.Add(v.NNCards[i]);
                    }
                    //计算得分  数据库得更新
                    onecards.xgold = NiuNiuHelper.CalcPower(v, hostPlayer) * Score;
                    v.UserInfo.gold += onecards.xgold;
                    LobbyServer.Ins.SaveUserInfo(v.UserInfo);

                    xhostgold += (-onecards.xgold); //庄家得分等于所有玩家相加的相反数
                    LogUtil.LogInfo("玩家 : " +onecards.seatid +" 牛牛 : "+onecards.nntype + " 输赢 : " +onecards.xgold);
                    allcards.oneplayercards.Add(onecards);
                }
            }

            //把庄家也加进去
            OnePlayerCards hostcards = new OnePlayerCards();
            hostcards.seatid = hostid;
            hostcards.xgold = xhostgold;
            hostcards.nntype = (int)hostPlayer.NNType;
            for (int i = 0; i < hostPlayer.NNCards.Length; i++)
            {
                hostcards.cards.Add(hostPlayer.NNCards[i]);
            }
            allcards.oneplayercards.Add(hostcards);

            byte[] data = NetUtil.ProtobufSerialize(allcards);

            foreach (var v in dictNNPlayers.Values)
            {
                v.conn.SendAsync((ushort)MainID.NiuNiu, (ushort)NiuNiuID.GameLay, data);
            }
        }

        public void  NextGame()
        {
            //判断是否需要切换庄家
            var hplayer = dictNNPlayers[hostid];
            for (int i = 0; i < GamePlayerNumber; i++)
            {
                if (dictNNPlayers.ContainsKey(i))
                {
                    var p = dictNNPlayers[i];
                    if((int)p.NNType>= (int)NiuNiuType.牛牛)
                    {
                       if( NiuNiuHelper.CalcPower(p,hplayer)>0)
                        {
                            hplayer = p;
                        }
                    }
                }
            }
            SetHost(hplayer.SeatID);

            //数据初始化
            for (int i = 0; i < GamePlayerNumber; i++)
            {
                if(dictNNPlayers.ContainsKey(i))
                {
                    dictNNPlayers[i].NNCards = new int[5];
                    dictNNPlayers[i].NNType = 0;
                    dictNNPlayers[i].GState = 0;
                }
            }

        }
        public int[] ShuffleCards()
        {
            List<int> listIndex = cards.ToList();
            int[] MixCards = new int[cards.Length];
            Random r = new Random();
            for (int i = 0; i < cards.Length; i++)
            {
                int randIndex = r.Next(0, listIndex.Count);
                MixCards[i] = listIndex[randIndex];
                listIndex.RemoveAt(randIndex);

            }
            return MixCards;
        }



        public void ShowCards()
        {
            string strCards = "";
            for (int i = 0; i < cards.Length; i++)
            {
                strCards += cards[i] + " ";
            }
            LogUtil.LogInfo(strCards);
        }
    }
}
