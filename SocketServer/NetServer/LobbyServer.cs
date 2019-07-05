using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acc;
using ProtoBuf;
using SocketServer.Utils;
using user;

namespace SocketServer.NetServer
{
    public class LobbyServer:IGame
    {

        #region  singleton
        private static LobbyServer ins = null;
        public static LobbyServer Ins
        {
            get
            {
                if (ins == null)
                    ins = new LobbyServer();
                return ins;
            }
        }
        #endregion

        private Dictionary<int, User> dictUsers;

        private Dictionary<string, Account> dicAccouts;
        public void Init()
        {
            dictUsers = new Dictionary<int, User>();
            dicAccouts = new Dictionary<string, Account>();
            NetMessageCenter.Ins.RegisterServer((int)MainID.Lobby, this);
        }

        public void DispatchMsg(Conn client, int Sub_ID, byte[] buff)
        {
            switch ((LobbyID)Sub_ID)
            {
                case LobbyID.AccLogin:
                    DoAccLogin(client,buff);
                    break;
                case LobbyID.AccRegister:
                    DoAccRegister(client,buff);
                    break;
                case LobbyID.UploadHead:
                    DoUploadHead(client, buff);
                    break;
                case LobbyID.DownloadHead:
                    DoDownloadHead(client, buff);
                    break;
                case LobbyID.WxLogin:
                    break;
                case LobbyID.VisitorLogin:
                    DoVistorLogin(client, buff);
                    break;
            }
        }

        

        public void DoAccLogin(Conn client,byte[] buff)
        {
            MemoryStream ms = new MemoryStream(buff);
            Account acc = Serializer.Deserialize<Account>(ms);
            LogUtil.LogInfo(string.Format("收到登陆消息,账号：{0}  密码：{1}", acc.acc, acc.pwd));
        }

        public void DoAccRegister(Conn client, byte[] buff)
        {
            MemoryStream ms = new MemoryStream(buff);
            Account acc = Serializer.Deserialize<Account>(ms);
            LogUtil.LogInfo(string.Format("收到注册消息,账号：{0}  密码：{1}", acc.acc, acc.pwd));

            if (dicAccouts.ContainsKey(acc.acc))
            {
                simpledata.SimpleString simpleStr = new simpledata.SimpleString();
                simpleStr.simple = "账号创建失败：已存在。";
                byte[] data = NetUtil.ProtobufSerialize(simpleStr);
                client.SendAsync((ushort)MainID.Lobby, (ushort)LobbyID.RegisterFailure, data);
            }
            else
            {
                string cmd = string.Format("select * from accounts where acc = '{0}' ", acc.acc);
                if (SqlManager.Ins.SqlMatch(cmd))
                {
                    simpledata.SimpleString simpleStr = new simpledata.SimpleString();
                    simpleStr.simple = "账号创建失败：已存在。";
                    byte[] data = NetUtil.ProtobufSerialize(simpleStr);
                    client.SendAsync((ushort)MainID.Lobby, (ushort)LobbyID.RegisterFailure, data);
                }
                else
                {
                    //数据库不存在，插入
//                    string cmd1 = string.Format("insert into accounts(acc,pwd) values('{0}','{1}')", acc.acc, acc.pwd);
//                    acc.id = SqlManager.Ins.SqlInsert(cmd1);
//                    dicAccouts.Add(acc.acc, acc);

                    System.Threading.Thread chThread = new System.Threading.Thread(ThradSavePng);
                    chThread.Start(acc);
                }
            }
        }

        private void ThradSavePng(object o)
        {
            var acc = (Account) o;
            string path = NetUtil.ImgPath + acc.id +".png";
            File.WriteAllBytes(path, acc.ico);
        }
        public void DoUploadHead(Conn client, byte[] buff)
        {
            
        }

        public void DoDownloadHead(Conn client, byte[] buff)
        {

        }

        public void DoAccWxLogin(Conn client, byte[] buff)
        {

        }
        #region 游客登录
        public void DoVistorLogin(Conn client, byte[] buff)
        {
            MemoryStream recvms = new MemoryStream(buff);
            simpledata.SimpleInt simpleInt = Serializer.Deserialize<simpledata.SimpleInt>(recvms);
            int vid = simpleInt.simple;
            User tempUser  = new User();
            if (vid == 0)
            {
                //
                tempUser.name = GenerateRandom(8);
                tempUser.gold = 100000;
                tempUser.sex = 0;
                string cmd = string.Format("insert into user(name,sex,gold) values('{0}',{1},{2})",tempUser.name,tempUser.sex,tempUser.gold);
                tempUser.id = SqlManager.Ins.SqlInsert(cmd);
                dictUsers.Add(tempUser.id, tempUser);
            }
            else
            {
                if (dictUsers.ContainsKey(vid))
                {
                    tempUser = dictUsers[vid];
                }
                else
                {
                    string cmd = string.Format("select * from user where id = {0} ", vid);
                    var dr = SqlManager.Ins.SqlSelect(cmd);
                    dr.Read();
                    tempUser.id = vid;
                    tempUser.name = (string)dr["name"];
                    tempUser.sex = (int)dr["sex"];
                    tempUser.gold = (int)dr["gold"];
                    dictUsers.Add(tempUser.id, tempUser);
                    SqlManager.Ins.CloseMySqlDataReader();
                }
            }

            client.ClientUseID = tempUser.id;

            //返回客户端数据
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize<User>(ms, tempUser);
            byte[] sendBytes = ms.ToArray();
            client.SendAsync((ushort)MainID.Lobby, (ushort)LobbyID.LoginSuccess, sendBytes);
        }


        private static string GenerateRandom(int Length)
        {
            char[] constant =
            {
                '0','1','2','3','4','5','6','7','8','9',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
            };

            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }

        public User GetUserInfo(int userid)
        {
            if (dictUsers.ContainsKey(userid))
                return dictUsers[userid];
            return null;
        }

        public void SaveUserInfo(User info)
        {
            //数据库保存
            string cmd = string.Format("update user set gold={0} where userid={1}", info.gold, info.id);
            SqlManager.Ins.AddSqlCmd(cmd);
        }
        
    }
    #endregion
}
