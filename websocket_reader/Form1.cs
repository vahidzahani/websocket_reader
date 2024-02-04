using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace websocket_reader
{
    
    public partial class Form1 : Form
    {
        private bool isDragging = false;
        private Point offset;

        private static Form1 instance;
        private static bool isDirect=false;

        public string strConfigFile = Path.GetDirectoryName(Application.ExecutablePath) + "\\config.ini";

        public string defaultPrinterName;
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

        public static void SetHeaderFooter(string keyname,string keyvalue)
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

            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;

        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                offset = new Point(e.X, e.Y);
            }
        }
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newLocation = this.PointToScreen(new Point(e.X, e.Y));
                newLocation.Offset(-offset.X, -offset.Y);
                this.Location = newLocation;
            }
        }
        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }



        private void SetStartup()
        {
            try
            {
                string keys =@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
                string value = "websocket";

                if (Registry.GetValue(keys, value, null) == null)
                {
                    // if key doesn't exist
                    using (RegistryKey key =Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        key.SetValue("websocket", Path.GetDirectoryName(Application.ExecutablePath)+"\\"+Path.GetFileName(Application.ExecutablePath));
                        key.Dispose();
                        key.Flush();
                    }
                }
                else
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        key.SetValue("websocket", Path.GetDirectoryName(Application.ExecutablePath) + "\\" + Path.GetFileName(Application.ExecutablePath));
                        key.Dispose();
                        key.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static readonly Mutex mutex = new Mutex(true, "c676b1d7-c868-4e9a-8409-135cec4dff43");


        private void Form1_Load(object sender, EventArgs e)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("this program already running.");
                Application.Exit();
            }

            try
            {
                if (File.Exists("tmpupdate.exe"))
                {
                    File.Delete("tmpupdate.exe");
                }
                textBox1.Text = "";
                textBox1.Text += "ver : " + Application.ProductVersion + "\r\n"; 
                myversion();
                //#showprintersocket   "Copy this phrase to the clipboard when the app is running."
                HttpListener listener = new HttpListener();
                int port = 1988;
                port = Getport2();
                listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
                listener.Start();
                
                Console.WriteLine("Listening connections [" + port + "] ...");
                textBox1.Text += "Listening connections [" + port + "] ..." + "\r\n";

                ThreadPool.QueueUserWorkItem(ProcessWebSocketRequests, listener);

                HideMainForm(true);//true is hide my form
                label1.Text = "ver : " + Application.ProductVersion;

                SetStartup();

                this.TopMost = true;


            }
            catch (Exception ex)
            {
                fnErrorToFile(ex.Message);
            }

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
        static void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            
            
            WebBrowser webTMP = (WebBrowser)sender;

            if (webTMP.DocumentText == "") { return; }
            webTMP.Document.RightToLeft = true;
            //webBrowser1.Print();
            if (isDirect)
            {
                webTMP.Print();
            }
            else
            {
                webTMP.ShowPrintDialog();
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
            //MessageBox.Show("TEST");
            //This Tls applies to versions of Windows 10 
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket;
            try
            {
                byte[] buffer = new byte[10485760];


                WebSocketReceiveResult result;
                string receivedMessage = "";
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    receivedMessage += Encoding.UTF8.GetString(buffer, 0, result.Count);
                    // پردازش داده دریافتی
                } while (!result.EndOfMessage);

               

                form.Invoke(new Action(() =>
                {
                    
                    

                    // خواندن JSON
                    try
                    {
                        var data = JsonConvert.DeserializeObject<Data_For_Print>(receivedMessage);
                        var printsetting = JsonConvert.DeserializeObject<print_setting>(data.print_setting);

                        if (data.rootPath == null) data.rootPath = "nullnull";
                        if (data.serverAddress == null){
                            data.serverAddress = "nullnull";
                        }
                        else
                        {
                            string fileServertxt = data.serverAddress;
                            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                            
                            //string fileServer = ;


                            File.WriteAllText(Path.Combine(executablePath, "server.txt"), fileServertxt);


                            //int intDefaultPort = 1988;
                            IniFile iniFile = new IniFile(form.strConfigFile);
                            iniFile.SetValue("Settings", "server",fileServertxt);
                            

                        }

                        isDirect = printsetting.is_direct == "1";//true for 1 and false for 0


                        string printername = printsetting.printer_name;

                        MyPrinters.SetDefaultPrinter(printername);
                        

                        SetHeaderFooter("footer", printsetting.footer);
                        SetHeaderFooter("header", printsetting.header);
                        SetHeaderFooter("margin_bottom", printsetting.margin_bottom);
                        SetHeaderFooter("margin_left", printsetting.margin_left);
                        SetHeaderFooter("margin_right", printsetting.margin_right);
                        SetHeaderFooter("margin_top", printsetting.margin_top);
                        SetHeaderFooter("Print_Background", "no");
                        SetHeaderFooter("Shrink_To_Fit", "yes");
                        
                        
                        
                        data.visibleContent = form.ReplaceAllOccurrences(data.visibleContent, data.rootPath, data.serverAddress + "/");
                        

                        WebBrowser WBB = new WebBrowser();
                        WBB.DocumentCompleted += WebBrowser1_DocumentCompleted;
                        WBB.Dock = DockStyle.Fill;
                        form.panel1.Controls.Add(WBB);


                        WBB.DocumentText = data.visibleContent;
                        form.webBrowser1.DocumentText = data.visibleContent;
                       
           

                    }
                    catch (WebSocketException ex)
                    {
                        Console.WriteLine("WebSocket exception: " + ex.Message);
                        fnErrorToFile(ex.Message);
                    }
                }));
                
                //back to response
                string response = "ok";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                //Console.WriteLine("Message: " + receivedMessage);

                buffer = new byte[2048];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine("WebSocket exception: " + ex.Message);
                fnErrorToFile(ex.Message);
            }
            finally
            {
                webSocket.Dispose();
            }
        }
       
        private string ReplaceAllOccurrences(string input, string oldValue, string newValue)
        {
            int index = input.IndexOf(oldValue);

            while (index != -1)
            {
                input = input.Remove(index, oldValue.Length).Insert(index, newValue);
                index = input.IndexOf(oldValue, index + newValue.Length);
            }

            return input;
        }
        private static void fnErrorToFile(string content)
        {
            try
            {
                string filePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\errorfilelog.txt" ; 
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                string er = $"Error to write error file code 800: {ex.Message}";
                //MessageBox.Show(er);
                Console.WriteLine(er);

            }
        }
        
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            
            /*
            


            if (webBrowser1.DocumentText == "") { return; }
            webBrowser1.Document.RightToLeft = true;
            //webBrowser1.Print();
            if (isDirect)
            {
                webBrowser1.Print();
            }
            else
            {
                webBrowser1.ShowPrintDialog();
            }
             */
  

            //webBrowser1.Refresh();
            //webBrowser1.ShowPrintPreviewDialog();
           

        }

        static void myversion()
        {
            // Get the version of your application
            Version version = typeof(Program).Assembly.GetName().Version;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ver.txt");
            // Write the version into the file
            File.WriteAllText(filePath, version.ToString());
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            HideMainForm(true);
            //HideMainForm(false);//mean showw this form
        }
       
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                if (clipboardText.Contains("#showprintersocket"))
                {
                    textBox1.Text += "Show config\r\n";
                    HideMainForm(false);//show this form
                    Clipboard.Clear();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("آیا مطمئن هستید که می‌خواهید از برنامه خارج شوید؟", "تأیید خروج", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

      

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = "";
        }

       

        public int Getport2()
        {
            int intDefaultPort =1988;
            IniFile iniFile = new IniFile(strConfigFile);
            string portValue = iniFile.GetValue("Settings", "port");
            if (portValue == null)
                iniFile.SetValue("Settings", "port", intDefaultPort.ToString());
            else
                return (int.Parse(portValue));

            return (1988);//this is default Port
        }

        private void button4_Click_2(object sender, EventArgs e)
        {
            
            textBox1.Text += "Checking for updates . . ." + "\r\n";
            button4.Enabled = false;

            Updater updater = new Updater();
            string s=updater.CheckAndUpdate();
            if (s == "notfoundnewversion")
            {
                MessageBox.Show("نسخه جدید جهت بروز رسانی یافت نشد");
                textBox1.Text += "new version not found" + "\r\n";
            }
                
            if (s == "isupdate")
                {
                MessageBox.Show("برنامه آخرین نسخه فعال است");
                textBox1.Text += "\nlast update is using" + "\r\n";
            }
            
            button4.Enabled = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = "<h2>thi is test.</h2><h4>this is Ttext.</h4>"; 
            webBrowser1.ShowPrintDialog();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = "<h2>thi is test.</h2><h4>this is Ttext.</h4>";
        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width <= 450)
                this.Width = 450;
            if (this.Height <= 400)
                this.Height = 400;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("TEST");


            // آدرس و پورت مقصد
            string serverAddress = "127.0.0.1";
            int port = 1988;


            // ایجاد یک اتصال TCP
            using (TcpClient client = new TcpClient(serverAddress, port))
            {
                // ارسال رشته به سرور
                string messageToSend = "salam";
                byte[] data = Encoding.UTF8.GetBytes(messageToSend);
                using (NetworkStream stream = client.GetStream())
                {
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("Message sent to server: " + messageToSend);
                }
            }
            
            
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            form_config frm = new form_config();
            frm.TopMost = true;
            frm.ShowDialog();
        }
    }

    public class print_setting
    {
        public string id {get ; set; }
        public string type {get ; set; }
        public string page_name {get ; set; }
        public string printer_name {get ; set; }
        public string margin_top {get ; set; }
        public string margin_right {get ; set; }
        public string margin_bottom {get ; set; }
        public string margin_left {get ; set; }
        public string header {get ; set; }
        public string footer {get ; set; }
        public string page_address { get; set; }
        public string is_direct{ get; set; }
    }
    public class Data_For_Print
    {
        public string print_setting { get; set; }
        public string visibleContent { get; set; }
        public string serverAddress { get; set; }
        public string rootPath { get; set; }
    }
    public static class MyPrinters
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string Name);
    }
}

   

