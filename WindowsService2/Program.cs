    using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService2
{
    internal static class Program
    {
        //static void Main()
        //{
        //    ServiceBase[] ServicesToRun;
        //    ServicesToRun = new ServiceBase[]
        //    {
        //        new Service1()
        //    };
        //    ServiceBase.Run(ServicesToRun);
        //}


        public static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                // حالت کنسولی
                RunAsConsole(args);
            }
            else
            {
                // حالت سرویس ویندوز
                RunAsService();
            }
        }
        private static void RunAsConsole(string[] args)
        {
            Service1 service = new Service1();
            service.Start(args);

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            service.Stop();
        }
        private static void RunAsService()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
            new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }



    }
}
