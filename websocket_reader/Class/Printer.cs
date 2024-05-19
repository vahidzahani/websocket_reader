using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Management;


namespace websocket_reader
{
    public class Printer
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PRINTER_DEFAULTS
        {
            public string pDatatype;
            public IntPtr pDevMode;
            public int DesiredAccess;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct JOB_INFO_1
        {
            public int JobId;
            public string pPrinterName;
            public string pMachineName;
            public string pUserName;
            public string pDocument;
            public string pDatatype;
            public string pStatus;
            public int Status;
            public int Priority;
            public int Position;
            public int TotalPages;
            public int PagesPrinted;
            public System.Runtime.InteropServices.ComTypes.FILETIME Submitted;
        }
        private const int PRINTER_ACCESS_USE = 0x00000008;

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, ref PRINTER_DEFAULTS pDefault);

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool EnumJobs(IntPtr hPrinter, int FirstJob, int NoJobs, int Level, IntPtr pJob, int cbBuf, ref int pcbNeeded, ref int pcReturned);
        public static int GetPrinterQueueStatus(string printerName)
        {
            IntPtr hPrinter;
            PRINTER_DEFAULTS pd = new PRINTER_DEFAULTS();
            pd.pDatatype = null;
            pd.pDevMode = IntPtr.Zero;
            pd.DesiredAccess = PRINTER_ACCESS_USE;

            if (OpenPrinter(printerName, out hPrinter, ref pd))
            {
                try
                {
                    int bytesNeeded = 0;
                    int jobsReturned = 0;
                    int level = 1;

                    // First call to EnumJobs to get the required buffer size
                    EnumJobs(hPrinter, 0, 10, level, IntPtr.Zero, 0, ref bytesNeeded, ref jobsReturned);

                    if (bytesNeeded == 0)
                    {
                        //return $"No print jobs found for printer {printerName}.";
                        return 0;
                    }

                    IntPtr buffer = Marshal.AllocHGlobal(bytesNeeded);

                    try
                    {
                        if (EnumJobs(hPrinter, 0, 10, level, buffer, bytesNeeded, ref bytesNeeded, ref jobsReturned))
                        {
                            int totalJobs = jobsReturned;
                            //return $"Printer {printerName} has {totalJobs} jobs in queue. Buffer size: {bytesNeeded} bytes.";
                            return totalJobs;
                        }
                        else
                        {
                            //return $"Failed to enumerate print jobs for printer {printerName}.";
                            return -1;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
                finally
                {
                    ClosePrinter(hPrinter);
                }
            }
            else
            {
                //return $"Failed to open printer {printerName}.";
                return -1;
            }

        }


        public static string GetDefaultPrinterName()
        {
            PrinterSettings settings = new PrinterSettings();
            string defaultPrinterName = settings.PrinterName;
            return defaultPrinterName;
        }
        
        public static void SetDefaultPageSizeA5(string printerName)
        {
            string registryPath = @"Software\Microsoft\Windows NT\CurrentVersion\Devices";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath, true))
            {
                if (key != null)
                {
                    string currentValue = key.GetValue(printerName) as string;

                    if (!string.IsNullOrEmpty(currentValue))
                    {
                        string newValue = currentValue.Replace("WINA4", "WINA5");

                        key.SetValue(printerName, newValue);
                        Console.WriteLine("Default page size set to A5: " + printerName);
                    }
                    else
                    {
                        Console.WriteLine("Printer setting not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Registry key not found: " + registryPath);
                }
            }
        }

        public static void SetHeaderFooter(string keyname, string keyvalue)
        {
            string keyPath = @"SOFTWARE\Microsoft\Internet Explorer\PageSetup";
            bool bolWritable = true;
            RegistryKey oKey = Registry.CurrentUser.OpenSubKey(keyPath, bolWritable);
            Console.Write(keyPath);
            oKey.SetValue(keyname, keyvalue);
            oKey.Close();
        }
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
        public static string Get_full_printer(string printername)
        {

            List<string> installedPrinters = GetInstalledPrinters();


            foreach (string printer in installedPrinters)
            {
                if (printername.ToLower().Trim() == printer.ToLower().Trim())
                    return printer;
            }

            foreach (string printer in installedPrinters)
            {
                if (printer.ToLower().Trim().Contains(printername.ToLower().Trim()))
                    return printer;
            }


            return printername;

        }


        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string Name);

        public static void SetDefaultPrinterWithCopies(string printerName, int copies)
        {
            // تعریف یک نمونه از کلاس PrinterSettings
            PrinterSettings settings = new PrinterSettings
            {
                PrinterName = printerName,
                Copies = (short)copies
            };

            // اعمال تغییرات
            settings.SetHdevmode(settings.GetHdevmode()); // این خط برای ذخیره تغییرات لازم است
            settings.SetHdevnames(settings.GetHdevnames()); // این خط برای ذخیره تغییرات لازم است

            // تنظیم پرینتر پیش‌فرض
            SetDefaultPrinter(printerName);
        }


    }


}
