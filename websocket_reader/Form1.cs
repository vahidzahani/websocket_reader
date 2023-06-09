﻿using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing.Printing;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace websocket_reader
{

    public partial class Form1 : Form
    {
        private static Form1 instance;


        //public Form mainForm = Application.OpenForms["Form1"];
        

        public static void SetDefaultPrinter(string printerName)
        {
            const string printerRegKey = @"Software\Microsoft\Windows NT\CurrentVersion\Windows";

            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(printerRegKey, true))
            {
                if (regKey != null)
                {
                    regKey.SetValue("Device", printerName);
                }
            }
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

        public static void setHeaderFooter(string keyname,string keyvalue)
            {
            string keyPath = @"SOFTWARE\Microsoft\Internet Explorer\PageSetup";
            bool bolWritable = true;
            RegistryKey oKey = Registry.CurrentUser.OpenSubKey(keyPath, bolWritable);
            Console.Write(keyPath);
            oKey.SetValue(keyname, keyvalue);
            oKey.Close();
        }

        public Form1()
        {
            InitializeComponent();
            instance = this;
        }
        static readonly Mutex mutex = new Mutex(true, "c676b1d7-c868-4e9a-8409-135cec4dff43");

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("this program already running.");
                Application.Exit();
            }
            myversion();
            //#showprintersocket   "Copy this phrase to the clipboard when the app is running."
            HttpListener listener = new HttpListener();
            int port = 8080;
            port = getport();
            listener.Prefixes.Add("http://127.0.0.1:"+port+"/");
            listener.Start();
            Console.WriteLine("Listening connections ["+port+"] ...");
            textBox1.Text += "\nListening connections ["+port+"] ...";

            ThreadPool.QueueUserWorkItem(ProcessWebSocketRequests, listener);

            HideMainForm(true);//true is hide my form
            label1.Text = "ver : " + Application.ProductVersion;

            setHeaderFooter("footer", "");
            setHeaderFooter("header", "");
            setHeaderFooter("margin_bottom", "0.196670");
            setHeaderFooter("margin_left", "0.196670");
            setHeaderFooter("margin_right", "0.196670");
            setHeaderFooter("margin_top", "0.196670");
            setHeaderFooter("Print_Background", "no");
            setHeaderFooter("Shrink_To_Fit", "yes");


        }
        private int getport()
        {
            int port = 3270; // default value

            if (File.Exists("config.txt"))
            {
                string[] lines = File.ReadAllLines("config.txt");
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2 && parts[0].Trim() == "port")
                    {
                        if (int.TryParse(parts[1].Trim(), out int parsedPort))
                        {
                            port = parsedPort;
                            break;
                        }
                    }
                }
            }

            return(port);
        }
        private void HideMainForm(bool mode)
        {
            if (mode)
            {
                Visible = false; // Hide form window.
                ShowInTaskbar = false; // Remove from taskbar.
                Opacity = 0;
            }
            else {
                Visible = true; // Hide form window.
                ShowInTaskbar = true; // Remove from taskbar.
                Opacity = 1;

            }
        }

        static void ProcessWebSocketRequests(object state)
        {
            HttpListener listener = (HttpListener)state;
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                if (context.Request.IsWebSocketRequest)
                {
                    ProcessWebSocketRequest(context,instance);
                }
            }
        }
        static async void ProcessWebSocketRequest(HttpListenerContext context,Form1 form)
        {
            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket;

            try
            {
                byte[] buffer = new byte[4096];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    //SetLabel1Text("vahid");


                    form.Invoke(new Action(() =>
                    {
                        receivedMessage= receivedMessage.Replace("<body>", "");
                        receivedMessage=receivedMessage.Replace("</body>", "");
                        receivedMessage=receivedMessage.Replace("<head>", "");
                        receivedMessage=receivedMessage.Replace("</head>", "");

                        string mystyle = "";//"<style> @media print { @page { size: auto; margin: 0; } @page: first { header: none; footer: none; } }</style>";
                        form.webBrowser1.DocumentText = "<html><head>"+mystyle+"</head><body>" + receivedMessage.Split('|')[1] + "</body></html>";


                        //string printername = "Microsoft Print to PDF";
                        string printername = receivedMessage.Split('|')[0];
                        SetDefaultPrinter(printername);
                        SetDefaultPageSizeA5(printername);
                        //DisableHeaderFooter(printername);

                    }));

                    //back to response
                    string response = "ok";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                    //Console.WriteLine("Message: " + receivedMessage);

                    buffer = new byte[2048];
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine("WebSocket exception: " + ex.Message);
            }
            finally
            {
                webSocket.Dispose();
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.Document.RightToLeft = true;
            //webBrowser1.Print();
            webBrowser1.Print();
            //webBrowser1.ShowPrintPreviewDialog();



        }
        static void myversion()
        {
            // Get the version of your application
            Version version = typeof(Program).Assembly.GetName().Version;

            // Create the file path
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ver.txt");

            // Write the version into the file
            File.WriteAllText(filePath, version.ToString());

        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            HideMainForm(true);
            //HideMainForm(false);//mean showw this form
        }
        private void button2_Click_1(object sender, EventArgs e)
        {
            Application.Restart();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();

                if (clipboardText.Contains("#showprintersocket"))
                {
                    textBox1.Text += Clipboard.GetText();
                    HideMainForm(false);//show this form
                    Clipboard.Clear();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
