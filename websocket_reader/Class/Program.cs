using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;

namespace websocket_reader
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        public static void Myversion()
        {
            // Get the version of your application
            Version version = typeof(Program).Assembly.GetName().Version;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ver.txt");
            // Write the version into the file
            File.WriteAllText(filePath, version.ToString());
        }
    }


   

    public class print_setting
    {
        public string id { get; set; }
        public string type { get; set; }
        public string page_name { get; set; }
        public string printer_name { get; set; }
        public string margin_top { get; set; }
        public string margin_right { get; set; }
        public string margin_bottom { get; set; }
        public string margin_left { get; set; }
        public string header { get; set; }
        public string footer { get; set; }
        public string page_address { get; set; }
        public string is_direct { get; set; }
    }
    public class Data_For_Print
    {
        public string print_setting { get; set; }
        public string visibleContent { get; set; }
        public string serverAddress { get; set; }
        public string rootPath { get; set; }
    }




}
