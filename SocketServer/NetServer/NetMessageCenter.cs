using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketServer.Utils;

namespace SocketServer.NetServer
{
    public class NetMessageCenter
    {

        #region  singleton
        private static NetMessageCenter ins = null;
        public static NetMessageCenter Ins
        {
            get
            {
                if (ins == null)
                    ins = new NetMessageCenter();
                return ins;
            }
        }
        #endregion

        private Dictionary<int, IGame> dictServs;

        public void Init()
        {
            dictServs = new Dictionary<int, IGame>();
        }


        public void RegisterServer(int main_id,IGame serv)
        {
            if (dictServs.ContainsKey(main_id))
            {
                LogUtil.LogError(string.Format("Server with key {0} already exsits!",main_id));
                return;
            }
            dictServs.Add(main_id, serv);
            LogUtil.LogInfo(string.Format("服务注册 ID：{0} 名称：{1}",main_id, serv.ToString()));
        }


        public void DispatchMsg(Conn client,int main_id,int sub_id,byte[] buff)
        {
            if (dictServs.ContainsKey(main_id))
            {
                var serv = dictServs[main_id];
                serv.DispatchMsg(client,sub_id, buff);
            }
        }
    }
}
