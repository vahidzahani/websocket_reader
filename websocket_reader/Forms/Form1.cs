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
using System.Text.RegularExpressions;
using System.Drawing.Text;

namespace websocket_reader
{
    
    public partial class Form1 : Form
    {
        private bool isDragging = false;
        private Point offset;
        private  HttpListenerContext context;
        private form_config frmConfigInstance;
        public static Boolean adminpass=false;
        public static Form1 instance;
        private static bool isDirect=false;
        public static string strConfigFile = Path.GetDirectoryName(Application.ExecutablePath) + "\\config.ini";
        public string defaultPrinterName;
        static readonly Mutex mutex = new Mutex(true, "c676b1d7-c868-4e9a-8409-135cec4dff43");

        public void vahidConsole(string str,bool cls =false,bool showTime =false)
        {
            if (showTime)
            {
                str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+ " " +str; 
            }
            textBox1.BeginInvoke((MethodInvoker)delegate {
                if (cls)
                    textBox1.Text = "ver : "+ Application.ProductVersion + "\r\n";
                textBox1.Text += $"{str}\r\n";
            });
            Logger.SaveLog(str);
        }

        public Form1()
        {
            InitializeComponent();
            InitializeNotifyIcon();

            instance = this;

            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;

        }
        private void InitializeNotifyIcon()
        {
            notifyIcon1.Text=Application.ProductName + " ver : " + Application.ProductVersion;
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


        private void Form1_Load(object sender, EventArgs e)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("this program already running.");
                Application.Exit();
            }

            Updater updater = new Updater();

            if (updater.IsUserAdmin() == true)
            {
                pic_super.Visible = true;
            }

            try
            {
                if (File.Exists("tmpupdate.exe"))
                {
                    File.Delete("tmpupdate.exe");
                }
            }
            catch (IOException ioe)
            {
                Logger.SaveErrorLog(ioe.Message);
            }
            
            textBox1.Text = "";
            vahidConsole($"ver : {Application.ProductVersion}");
            Program.Myversion();
            StartListener();
            HideMainForm(true);//true is hide my form
            SetStartup();
            this.TopMost = true;
        }

        static void StartListener()
        {
            HttpListener listener = new HttpListener();
            int port = 1988;
            port = Getport2();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            listener.Start();
            Console.WriteLine("Listening for requests...");
            instance.vahidConsole($"Listening for {port} ");

            // برای شروع گوش دادن به درخواست‌ها از یک رویداد متغیر استفاده می‌کنیم
            listener.BeginGetContext(ListenerCallback, listener);
            
        }


        static void ListenerCallback(IAsyncResult result)
        {
            //Form1 frm = new Form1();
            //frm.textBox1.Text += "ListenerCallback";

            HttpListener listener = (HttpListener)result.AsyncState;

            // دریافت درخواست و ادامه گوش دادن به درخواست‌های جدید
            instance.context = listener.EndGetContext(result);
            HttpListenerRequest request = instance.context.Request;

                
            
            Console.WriteLine("request.HttpMethod:"+request.HttpMethod);

            //if (request.HttpMethod == "POST" || request.HttpMethod == "OPTIONS")
            if (request.HttpMethod == "POST" || request.HttpMethod == "OPTIONS")
            {

                if (request.HttpMethod == "POST")
                {
                    using (StreamReader reader = new StreamReader(request.InputStream,  Encoding.UTF8))
                    {
                        string requestBody = reader.ReadToEnd();

                        try
                        {

                            var data = JsonConvert.DeserializeObject<Data_For_Print>(requestBody);
                            var printsetting = JsonConvert.DeserializeObject<print_setting>(data.print_setting);

                            if (data.rootPath == null) data.rootPath = "nullnull";
                            if (data.serverAddress == null){
                                data.serverAddress = "nullnull";
                            }else
                            {
                                string fileServertxt = data.serverAddress;
                                string executablePath = AppDomain.CurrentDomain.BaseDirectory;

                                IniFile iniFile = new IniFile(strConfigFile);
                                iniFile.SetValue("Settings", "server", fileServertxt);
                            }

                            isDirect = printsetting.is_direct == "1";//true for 1 and false for 0

                            string printername = printsetting.printer_name;
                            printername = Printer.Get_full_printer(printername);

                            Printer.SetDefaultPrinter(printername);

                            Printer.SetHeaderFooter("footer", printsetting.footer);
                            Printer.SetHeaderFooter("header", printsetting.header);
                            Printer.SetHeaderFooter("margin_bottom", printsetting.margin_bottom);
                            Printer.SetHeaderFooter("margin_left", printsetting.margin_left);
                            Printer.SetHeaderFooter("margin_right", printsetting.margin_right);
                            Printer.SetHeaderFooter("margin_top", printsetting.margin_top);
                            Printer.SetHeaderFooter("Print_Background", "no");
                            Printer.SetHeaderFooter("Shrink_To_Fit", "yes");

                            string pattern = @"(\.\.\/)+";
                            data.visibleContent = Regex.Replace(data.visibleContent, pattern, match =>
                            {
                                if (match.Value == data.rootPath)//"../../"
                                {
                                    return data.serverAddress + "/";
                                }
                                else
                                {
                                    return  get_IPaddress( data.serverAddress )+ "/";
                                }
                            });
                            
                            instance.vahidConsole( " > print to : " + printername, showTime: true);

                            instance.Invoke((MethodInvoker)delegate
                            {
                                WebBrowser WBB = new WebBrowser();
                                WBB.DocumentCompleted += WebBrowser1_DocumentCompleted;
                                WBB.Dock = DockStyle.Fill;
                                instance.panel1.Controls.Add(WBB);
                                WBB.DocumentText = data.visibleContent;
                                instance.webBrowser1.DocumentText = data.visibleContent;

                            });



                        }
                        catch (WebSocketException ex)
                        {
                            Logger.SaveErrorLog(ex.Message);
                        }
                    }
                }
                else if (request.HttpMethod == "OPTIONS")
                {
                    HttpListenerResponse response =instance.context.Response;
                    response.StatusCode = 200; // موفقیت‌آمیز
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
                    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
                    response.OutputStream.Close(); // بستن جریان خروجی
                }
            }

            listener.BeginGetContext(ListenerCallback, listener);
        }

        private static string get_IPaddress(string serveraddress)
        {
            string pattern = @"^(http://[\d\.]+)";
            Match match = Regex.Match(serveraddress, pattern);
            if (match.Success)
                return match.Groups[1].Value;
            
            return "";

        }
        private void SetStartup()
        {
            try
            {
                string keys = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run";
                string value = "websocket";

                if (Registry.GetValue(keys, value, null) == null)
                {
                    // if key doesn't exist
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        key.SetValue("websocket", Path.GetDirectoryName(Application.ExecutablePath) + "\\" + Path.GetFileName(Application.ExecutablePath));
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
                Logger.SaveErrorLog(e.Message);
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
            string str_message = "Data received successfully!";

            if (webTMP.DocumentText == "") { return; }
            webTMP.Document.RightToLeft = true;
            //webBrowser1.Print();
            if (isDirect)
            {
                webTMP.Print();
            }
            else
            {
                form_mydialogshow m = new form_mydialogshow();
                m.defaultPrintername = Printer.GetDefaultPrinterName();
                m.ShowDialog();
                m.Activate();
                                
                if(m.printername == "##SHOWDIALOG##")
                {
                    str_message = "print preview";
                    webTMP.ShowPrintDialog();
                }else if (m.printername == "")
                {
                    str_message = "cancel";

                }
                else if(m.printername !="")
                {
                    str_message = "print";

                    //MessageBox.Show(instance.defaultPrinterName);
                    Printer.SetDefaultPrinter(m.printername);
                    webTMP.Print();
                }

            }

            HttpListenerResponse response = instance.context.Response;

            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
            string responseString = "{\"status\": \"success\", \"message\": \""+str_message+"\"}";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();

        }
        

        private void button1_Click_1(object sender, EventArgs e)
        {
            HideMainForm(true);
            //HideMainForm(false);//mean showw this form
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("آیا مطمئن هستید که می‌خواهید از برنامه خارج شوید؟", "تأیید خروج", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("");//webBrowser1.DocumentText = "";
            vahidConsole("",true);
        }

        public static int Getport2()
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
            
            //textBox1.Text += "Checking for updates . . ." + "\r\n";
            vahidConsole("Checking for updates",showTime:true);
            Application.DoEvents();
            Updater updater = new Updater();



            if (updater.IsUserAdmin() == false)
            {
                MessageBox.Show("دسترسی سطح ادمین نیاز است");
                vahidConsole("access denied", showTime: true);
                return;
            }





            button4.Enabled = false;

            

            updater.UpdateFonts();
            if (updater.ExistUpdate() == true)
            {
                MessageBox.Show(@"پس از بروز رسانی برنامه بارگزاری مجدد می شود");
                string s = updater.GetUpdate();
                //MessageBox.Show("نسخه جدید جهت بروز رسانی یافت نشد");
                //vahidConsole("new version not found", showTime: true);
            }
            else
            {
                MessageBox.Show("نسخه جدید جهت بروز رسانی یافت نشد");
                vahidConsole("new version not found", showTime: true);

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
            adminpass = false;
            Form_lock form_Lock = new Form_lock();  
            form_Lock.ShowDialog();
            //MessageBox.Show(adminpass.ToString());
            button3.Enabled = adminpass;
            btnClear.Enabled = adminpass;
            button6.Enabled = adminpass;
            
        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width <= 450)
                this.Width = 450;
            if (this.Height <= 400)
                this.Height = 400;
        }


        private void btnTest_Click(object sender, EventArgs e)
        {

            Logger.SaveErrorLog("vahidzahani");
            //Console.WriteLine("Printer select >>>>>>>>>> " + get_full_printer("   hp       "));
        }
     

        private void button6_Click_1(object sender, EventArgs e)
        {
            if (frmConfigInstance == null || frmConfigInstance.IsDisposed)
            {
                frmConfigInstance = new form_config();
            }

            // نمایش یا مخفی کردن فرم
            frmConfigInstance.TopMost = true;
            frmConfigInstance.ShowDialog();

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HideMainForm(false);//true is hide my form
            
        }

        private void button7_Click(object sender, EventArgs e)
        {

            if (!panel1.Visible)
            {
                panel1.Visible = true;
                panel1.Width = textBox1.Width = this.Width / 2;
                panel1.Height = textBox1.Height;
                panel1.Top= textBox1.Top;
                panel1.Left = textBox1.Right;
            }
            else
            {
                panel1.Visible = false;
                textBox1.Width = this.Width;
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Updater UP = new Updater();

            if (UP.ExistUpdate())
            {
                notifyIcon1.ShowBalloonTip(1000, "نسخه : " + Application.ProductVersion,"نسخه جدید یافت شد",ToolTipIcon.Info);
            }
        }
    }

    
    
}

   

