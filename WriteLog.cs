using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MegreVideo
{
    public static class WriteLog
    {
        private static BlockingCollection<LogItem> logList = new BlockingCollection<LogItem>();

        static WriteLog()
        {
            new Thread(WriteFunc).Start();
        }

        private static void WriteFunc()
        {
            foreach (var item in logList.GetConsumingEnumerable())
            {
                try
                {
                    string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogFiles", item.Type);
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);
                    string logpath = Path.Combine(dirPath, $"{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
                    if (!System.IO.File.Exists(logpath))
                    {
                        var file = System.IO.File.Create(logpath);
                        file.Close();
                    }
                    string content = string.Format("{0} {1} \r\n", item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), item.Msg);
                    content += "------------------------------------------------------------\r\n";
                    System.IO.File.AppendAllText(logpath, content);

                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(1000);
                    logList.Add(item);
                }
            }
        }

        public static void AddLog(string str, string type = "log")
        {
            logList.Add(new LogItem { Msg = str, Type = type });
        }

    }

    public class LogItem
    {
        public LogItem()
        {
            _createTime = DateTime.Now;
        }


        private DateTime _createTime;
        public DateTime CreateTime
        {
            get { return _createTime; }
        }

        private string _Type;
        public string Type
        {
            get { return _Type; }
            set { _Type = value; ; }
        }


        private string _msg;

        public string Msg
        {
            get { return _msg; }
            set { _msg = value; }
        }

    }
}
