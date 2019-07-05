using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Utils
{
    public class LogUtil
    {
        public static void LogInfo(string str)
        {
            string head = " ||Info|| ";
            Log(head, str);
        }

        public static void LogWarm(string str)
        {
            string head = "  ！！ ||Warm|| ";
            Log(head, str);
        }
        public static void LogError(string str)
        {
            string head = @"  XXXX  ||Error|| ";
            Log(head, str);
        }

        private static void Log(string head, string body)
        {
            Console.WriteLine(DateTime.Now.ToString() + head + body);
        }
    }
}
