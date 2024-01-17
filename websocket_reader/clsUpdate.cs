using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace websocket_reader
{
    internal class Updater
    {
        private const string UpdaterBatPath = "updater.bat";
        public string CheckAndUpdate()
        {
            string localVerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ver.txt");
            string fileServerConfig= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.txt");

            string serverAddress = "http://192.168.1.200/care2";//is Default
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string fileServer = Path.Combine(executablePath, "server.txt");

            if (File.Exists(fileServer)) {
                serverAddress= File.ReadAllText(fileServer);
            }
            string webVerFilePath = serverAddress + "/printsoft/ver.txt";
            string setupExeUrl = serverAddress + "/printsoft/websocketprinter.exe";

            string localVersion = ReadVersion(localVerFilePath);
            string webVersion = DownloadString(webVerFilePath);
            if (webVersion== "notfoundnewversion") { return (webVersion); }

            if (CompareVersions(localVersion, webVersion) < 0)
            {
                Console.WriteLine("new version is downloading .....");

                DownloadFile(setupExeUrl, "tmpupdate.exe");

                //File.Delete("vahid.exe");

                //File.Move("update.exe", "vahid.exe");

                Console.WriteLine("update completed.");
                WriteBatchFile();
                Thread.Sleep(2000);
                RunUpdaterBat();

                Process.GetCurrentProcess().Kill();
                return ("updateok");
            }
            else
            {
                Console.WriteLine("your APP is up to date.");
                return ("isupdate");
            }
        }



        static void WriteBatchFile()
        {
            try
            {
                string batchContent = @"
@echo off
set sourceFile=tmpupdate.exe
set targetFile=websocketprinter.exe
echo Updating ...
timeout /T 2 /nobreak >nul
copy /Y ""%sourceFile%"" ""%targetFile%""
start """" ""%targetFile%""
";
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string batchFilePath = Path.Combine(executablePath, "updater.bat");
                File.WriteAllText(batchFilePath, batchContent);
                //Console.WriteLine($"فایل rrrr.bat با موفقیت ایجاد شد در: {batchFilePath}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"خطا در نوشتن محتوای فایل bat: {ex.Message}");
            }
        }

        private string ReadVersion(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return "1.0.0";
        }

        private string DownloadString(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadString(url);
                }
            }
            catch (Exception)
            {

                return "notfoundnewversion";
            }
        }

        

        private void RunUpdaterBat()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = UpdaterBatPath,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                //WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(psi);
        }


        private void DownloadFile(string url, string destinationPath)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, destinationPath);
            }
        }

        private int CompareVersions(string version1, string version2)
        {
            Version v1 = Version.Parse(version1);
            Version v2 = Version.Parse(version2);
            return v1.CompareTo(v2);
        }

    }
}
