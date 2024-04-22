using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;

namespace websocket_reader
{
    
    public class Logger
    {
        public static void SaveErrorLog(string content)
        {
            try
            {
                string filePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\errorfilelog.txt";
                File.AppendAllText(filePath,"\r\n"+content);
            }
            catch (Exception ex)
            {
                string er = $"Error to write error file code 800: {ex.Message}";
                Console.WriteLine(er);
            }
        }
        public static void SaveLog(string content)
        {
            try
            {
                string filePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\log.txt";
                File.AppendAllText(filePath, "\r\n" + content);
            }
            catch (Exception ex)
            {
                string er = $"Error to write error file code 800: {ex.Message}";
                Console.WriteLine(er);
            }
        }
    }

}
