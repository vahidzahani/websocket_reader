﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.Principal;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace websocket_reader
{
    internal class Updater
    {
        // Import the AddFontResourceEx function from the gdi32.dll
        [DllImport("gdi32.dll")]
        private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

        private const string UpdaterBatPath = "updater.bat";
        public string serverAddress = "http://192.168.1.200/care2";//is Default

        static void InstallFont(string fontFileSource)
        {

           PrivateFontCollection fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(fontFileSource);

            int result = AddFontResourceEx(fontFileSource, 0, IntPtr.Zero);
            Console.WriteLine(fontFileSource);
            if (fontCollection.Families.Length > 0)
            {
                //Console.WriteLine("FAMILI OKKKKKK");
                byte[] fontData = File.ReadAllBytes(fontFileSource);
                IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
                fontCollection.AddMemoryFont(fontPtr, fontData.Length);
                Marshal.FreeCoTaskMem(fontPtr);
            }
            else
            {
                //Console.WriteLine("FAMILI NOOOOOOO");
            }

            string fileName = Path.GetFileName(fontFileSource);
            string fontDestination = Environment.GetFolderPath(Environment.SpecialFolder.Fonts).ToString() + @"\" + fileName;
            if (!File.Exists(fontDestination))
                File.Copy(fontFileSource, fontDestination);

        }


        public string GetServerAddress() {
            //Form1 form1 = new Form1();
            IniFile iniFile = new IniFile(Form1.strConfigFile);
            string tmp = iniFile.GetValue("Settings", "server");
            if (tmp == null)
                iniFile.SetValue("Settings", "server", serverAddress);
            else
                serverAddress = tmp;
            return serverAddress;
        }



        static string[] ExtractTTFFiles(string htmlContent)
        {
            // Regular expression pattern to match .ttf file links
            string pattern = @"<a\s+(?:[^>]*?\s+)?href=([""'])(.*?)\1";
            // Match the pattern against the HTML content
            MatchCollection matches = Regex.Matches(htmlContent, pattern);
            // Extract .ttf file names from matches
            List<string> ttfFiles = new List<string>();
            foreach (Match match in matches)
            {
                string link = match.Groups[2].Value;
                if (link.EndsWith(".ttf"))
                {
                    // Remove any directory paths from the link
                    string fileName = link.Split('/').Last();
                    ttfFiles.Add(fileName);
                }
            }
            return ttfFiles.ToArray();
        }
        public  bool IsUserAdmin()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(currentUser);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public bool ExistUpdate()
        {
            string localVerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ver.txt");

            string localVersion = ReadVersion(localVerFilePath);
            string webVersion = DownloadString(serverAddress + "/printsoft/ver.txt");

            if (webVersion == localVersion || webVersion == "notfoundnewversion")  
                return false;//application is Update or not found Update
            else
                return true;//application is Not Update and there is update


        }
        public void UpdateFonts()
        {
            serverAddress = GetServerAddress();
            string url = serverAddress + "/printsoft/fonts/";
            // Directory to save the downloaded font files
            string downloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadedFonts");
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(downloadDirectory);
            using (WebClient client = new WebClient())
            {
                // Download the HTML content of the directory
                string htmlContent = client.DownloadString(url);
                string[] ttfFiles = ExtractTTFFiles(htmlContent);
                // Download and copy each .ttf file
                foreach (string ttfFile in ttfFiles)
                {
                    try
                    {
                        string ttfUrl = url + ttfFile;
                        string downloadedFilePath = Path.Combine(downloadDirectory, ttfFile);
                        if (File.Exists(downloadedFilePath) == false)
                        {
                            client.DownloadFile(ttfUrl, downloadedFilePath);
                            string fontsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), ttfFile);
                            //File.Copy(downloadedFilePath, fontsDirectory,false);
                        }
                        InstallFont(downloadedFilePath);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Logger.SaveErrorLog(ex.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
        public string GetUpdate()
        {
            
            string localVerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ver.txt");

            string setupExeUrl = serverAddress + "/printsoft/websocketprinter.exe";

            string localVersion = ReadVersion(localVerFilePath);
            string webVersion = DownloadString(serverAddress + "/printsoft/ver.txt");

            if (webVersion== "notfoundnewversion") { return (webVersion); }

            if (CompareVersions(localVersion, webVersion) < 0)
            {
                Console.WriteLine("new version is downloading .....");

                DownloadFile(setupExeUrl, "tmpupdate.exe");


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
                Console.WriteLine($"خطا در نوشتن محتوای فایل bat: {ex.Message}");
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