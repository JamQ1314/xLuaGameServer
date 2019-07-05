using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SocketServer.Utils;
using user;
using System.Timers;

namespace SocketServer.NetServer
{
    public class SqlManager
    {
        private static SqlManager ins;

        System.Timers.Timer myTimer;

        private Queue<string> qGetCmd = new Queue<string>();
        private Queue<string> qTmpCmd = new Queue<string>();

        private MySqlConnection conn = null;

        private MySqlDataReader dr;

        public static SqlManager Ins
        {
            get
            {
                if (ins == null)
                    ins = new SqlManager();
                return ins;
            }
        }

        public void Init()
        {
            String connetStr = "server=127.0.0.1;port=3306;user=root;password=123456; database=calc;";
            conn = new MySqlConnection(connetStr);

            myTimer = new System.Timers.Timer();
            myTimer.Interval = 1000;
            myTimer.AutoReset = true;
            myTimer.Enabled = true;
            myTimer.Elapsed += new ElapsedEventHandler(DoVoidSqlCmd);
            
        }

        private void DoVoidSqlCmd(object sender, ElapsedEventArgs e)
        {
            if (qTmpCmd.Count > 0) //上一次存储没结束
                return;
            if (qGetCmd.Count == 0) //没有消息
                return;

            qTmpCmd = qGetCmd;
            qGetCmd.Clear();

            conn.Open();
            while (qTmpCmd.Count>0)
            {
                string cmd = qGetCmd.Dequeue();
                LogUtil.LogInfo("执行cmd ：" + cmd);
                MySqlCommand mySqlCmd = new MySqlCommand(cmd, conn);
                mySqlCmd.ExecuteNonQuery();
                mySqlCmd.Dispose();
            }
            conn.Close();
            conn.Dispose();
            qTmpCmd.Clear();
        }


        public void AddSqlCmd(string cmd)
        {
            qGetCmd.Enqueue(cmd);
        }


        public bool SqlMatch(string cmd)
        {
            conn.Open();
            MySqlCommand mySqlCmd = new MySqlCommand(cmd, conn);
            var o = mySqlCmd.ExecuteScalar();
            conn.Close();
            return o != null;
        }

        public int SqlInsert(string cmd)
        {
            conn.Open();
            MySqlCommand mySqlCmd = new MySqlCommand(cmd, conn);
            mySqlCmd.ExecuteNonQuery();
            long lastid = mySqlCmd.LastInsertedId;
            mySqlCmd.Dispose();
            conn.Close();
            conn.Dispose();
            return (int)lastid;
        }
       

        public MySqlDataReader SqlSelect(string cmd)
        {
            conn.Open();
            MySqlCommand mySqlCmd = new MySqlCommand(cmd, conn);
            dr = mySqlCmd.ExecuteReader();
            return dr;
        }

        public void CloseMySqlDataReader()
        {
            dr.Close();
            conn.Close();
            conn.Dispose();
        }
        public DataTable SqlSelectFunction2(string cmd)
        {
            //建立DataSet对象(相当于建立前台的虚拟数据库)
            DataSet Ds = new DataSet();

 
            //打开连接
            conn.Open();


            MySqlCommand MysqlCmd = new MySqlCommand(cmd, conn);
            MySqlDataAdapter Msda = new MySqlDataAdapter(MysqlCmd);

            //将查询的结果存到虚拟数据库Ds中的虚拟表StudentInfo中
            Msda.Fill(Ds, "Temp");

            //将数据表StudentInfo的数据复制到DataTable对象中
            return Ds.Tables["Temp"];

        }

        private void ASDA()
        {
//            MySqlCommand cmd = new MySqlCommand();
//            cmd.CommandText = "SELECT *FROM student1";
//            MySqlDataAdapter adap = new MySqlDataAdapter(cmd);
//            DataSet ds = new DataSet();
//            adap.Fill(ds);
        }
    }
}
