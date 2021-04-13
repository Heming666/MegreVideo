using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MegreVideo
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }
        private void btn_start_Click(object sender, EventArgs e)
        {
            var directoryPath = AppDomain.CurrentDomain.BaseDirectory;//程序的根目录
            //存放临时文件的文件夹里不能重名的文件，否则会导致ffmpeg程序卡死，无法执行后面的命令，进程也不会被被结束，需要手动在任务管理器杀进程
            string tsOne = Guid.NewGuid().ToString() + ".ts";
            string tstwo = Guid.NewGuid().ToString() + ".ts";
            //生成临时文件
            string command = $"-i {Path.Combine(directoryPath, "Video\\360p.mp4")} -vcodec copy -acodec copy -vbsf h264_mp4toannexb  {Path.Combine(directoryPath, tsOne)} -y";
            this.Execute(command);
            command = $" -i {Path.Combine(directoryPath, "Video\\360p2.mp4")} -vcodec copy -acodec copy -vbsf h264_mp4toannexb  {Path.Combine(directoryPath, tstwo)} -y";
            this.Execute(command);
            //合并临时文件并输出新的视频
            string outputFile = Guid.NewGuid().ToString() + ".mp4";
            string megreCommand = $"-i concat:{Path.Combine(directoryPath,tsOne)}|{Path.Combine(directoryPath, tstwo)} -acodec copy -vcodec copy -absf aac_adtstoasc {Path.Combine(directoryPath, outputFile)} -y";
            //合并放到后面执行是因为 ts文件转换需要时间，如果三行命令一起执行的话可能会导致ts文件还未转换完毕就开始合并ts文件，这会导致报找不到ts文件的错
            this.Execute(megreCommand);
            //删除生成的ts文件
            Task.Run(() =>
            {
                if (File.Exists(tsOne)) File.Delete(Path.Combine(directoryPath, tsOne));
                if (File.Exists(tstwo)) File.Delete(Path.Combine(directoryPath, tstwo));
            });

        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">命令</param>
        private void Execute(string command)
        {
            Process p = new Process();//建立外部调用线程
            p.StartInfo.FileName =AppDomain.CurrentDomain.BaseDirectory+ "\\ffmpeg-4.3.2-2021-02-27-essentials_build\\bin\\ffmpeg.exe";//要调用外部程序的绝对路径
            p.StartInfo.Arguments = command;//程序参数，字符串形式输入
            p.StartInfo.UseShellExecute = false;//不使用操作系统外壳程序启动线程(一定为FALSE,详细的请看MSDN)
            p.StartInfo.RedirectStandardError = true;//把外部程序错误输出写到StandardError流中(这个一定要注意,FFMPEG的所有输出信息,都为错误输出流,用StandardOutput是捕获不到任何消息的..
            p.StartInfo.CreateNoWindow = true;//不需要创建窗口 
            p.ErrorDataReceived += new DataReceivedEventHandler(Output);//外部程序(这里是FFMPEG)输出流时候产生的事件,这里是把流的处理过程转移到下面的方法中
            p.Start();//启动线程
            p.BeginErrorReadLine();//开始异步读取
            p.WaitForExit();//阻塞等待进程结束
            p.Close();//关闭进程
            p.Dispose();
        }

        /// <summary>
        /// 输出消息
        /// </summary>
        /// <param name="sendProcess"></param>
        /// <param name="output"></param>
        private void Output(object sendProcess, DataReceivedEventArgs output)
        {
            if (!string.IsNullOrEmpty(output.Data))
            {
                WriteLog.AddLog(output.Data);
            }
        }
    }
}
