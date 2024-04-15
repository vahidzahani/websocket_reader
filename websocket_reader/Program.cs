using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;


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
    }
   

    public class PrinterManager
    {
        public static List<string> GetInstalledPrinters()
        {
            List<string> printers = new List<string>();

            // ساخت یک نمونه از کلاس PrintDocument
            using (PrintDocument printDocument = new PrintDocument())
            {
                // اضافه کردن نام هر پرینتر به لیست
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    printers.Add(printer);
                }
            }

            return printers;
        }
    }

}
