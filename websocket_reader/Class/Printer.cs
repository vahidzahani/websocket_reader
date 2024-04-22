using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace websocket_reader
{
    public class Printer
    {
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



    }


}
