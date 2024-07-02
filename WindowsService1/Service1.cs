using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        private string filePath = @"C:\a.txt";
        public Service1()
        {
            InitializeComponent();

            //this.ServiceName = "Service1";
            //this.CanStop = true;
            //this.CanPauseAndContinue = true;
            //this.AutoLog = true;

        }

        protected override void OnStart(string[] args)
        {
            timer = new Timer();
            timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }

        internal void Start(string[] args)
        {
            throw new NotImplementedException();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // تاریخ و ساعت فعلی
            string dateTimeString = DateTime.Now.ToString();

            // نوشتن تاریخ و ساعت به فایل
            StreamWriter sw = null;
            try
            {
                sw=new StreamWriter(AppDomain.CurrentDomain.BaseDirectory+"\\aaaaa.txt",true);
                sw.WriteLine(dateTimeString.ToString());
                sw.Flush();
                sw.Close();
                //File.WriteAllText(filePath, dateTimeString);
            }
            catch 
            {

            }
            
        }

        protected override void OnStop()
        {
            timer.Stop();
            timer.Dispose();
        }

        //public static void Main(string[] args)
        //{
        //    ServiceBase.Run(new Service1());
        //}
    }
}
