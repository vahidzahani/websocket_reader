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
            try
            {
                HttpListener listener = new HttpListener();
                int port = 1988;
                port = Getport2();
                listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                listener.Start();
                instance.vahidConsole($"Listening for {port} ");

                // برای شروع گوش دادن به درخواست‌ها از یک رویداد متغیر استفاده می‌کنیم
                listener.BeginGetContext(ListenerCallback, listener);

            }
            catch (Exception ex)
            {
                Logger.SaveErrorLog(ex.Message);
            }
        }

        static void ListenerCallback(IAsyncResult result)
        {
            try
            {
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


                                /***********************************************/
                                try
                                {
                                    string filePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\data_print_tmp.txt";
                                    File.WriteAllText(filePath, requestBody);
                                }
                                catch (Exception ex)
                                {
                                    Logger.SaveErrorLog($"Error to write error file code 800: {ex.Message}");
                                }
                                /***********************************************/


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


                                string input = data.visibleContent;
                                string pattern2 = @"<iframe\s+[^>]*>[\s\S]*?</iframe>";//remove iframe section from html data with regex
                                data.visibleContent = Regex.Replace(input, pattern2, "", RegexOptions.Multiline);

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
            catch (Exception ex)
            {
                Logger.SaveErrorLog(ex.Message);
            }
        }

        private static string get_IPaddress(string serveraddress)
        {
            try
            {
                string pattern = @"^(http://[\d\.]+)";
                Match match = Regex.Match(serveraddress, pattern);
                if (match.Success)
                    return match.Groups[1].Value;
            
            }
            catch (Exception ex)
            {
                Logger.SaveErrorLog(ex.Message);
            }
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

            form_mydialogshow m = new form_mydialogshow();
            m.defaultPrintername = Printer.GetDefaultPrinterName();

            //MessageBox.Show(Printer.GetPrinterQueueStatus(m.defaultPrintername).ToString() + m.defaultPrintername);
            while (Printer.GetPrinterQueueStatus(m.defaultPrintername) >= 5)
            {
                Application.DoEvents();
            }

            //webBrowser1.Print();
            if (isDirect)
            {
                webTMP.Print();
            }
            else
            {
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

                    //Printer.SetDefaultPrinter(m.printername);
                    Printer.SetDefaultPrinterWithCopies(m.printername,3);

                                       

                    for (int i = 0; i < m.numberOfPrint; i++)
                    {
                        webTMP.Print();
                        
                    }

                }

            }

            HttpListenerResponse response = instance.context.Response;

            try
            {
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
                string responseString = "{\"status\": \"success\", \"message\": \"" + str_message + "\"}";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception)
            {

            }

        }


       
        private void button1_Click_1(object sender, EventArgs e)
        {
           
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
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
            
           

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }
        private void button5_Click(object sender, EventArgs e)
        {
           
            
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



            //Logger.SaveErrorLog("vahidzahani");
            //Console.WriteLine("Printer select >>>>>>>>>> " + get_full_printer("   hp       "));

            string printerName = "label"; // نام پرینتر خود را وارد کنید
            int status = Printer.GetPrinterQueueStatus(printerName);
            MessageBox.Show(status.ToString());

            //string htmlContent = "<html><body><h1>Hello, World!</h1></body></html>";
            //// فرض کنید تعداد کپی مورد نظر 3 است
            //printer.PrintHTML(htmlContent, 3);

        }
     

        private void button6_Click_1(object sender, EventArgs e)
        {
            

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HideMainForm(false);//true is hide my form
            
        }

        private void button7_Click(object sender, EventArgs e)
        {


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Updater UP = new Updater();

            if (UP.ExistUpdate())
            {
                notifyIcon1.ShowBalloonTip(1000, "نسخه : " + Application.ProductVersion,"نسخه جدید یافت شد",ToolTipIcon.Info);
            }
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            HideMainForm(true);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            adminpass = false;
            Form_lock form_Lock = new Form_lock();
            form_Lock.ShowDialog();
            //MessageBox.Show(adminpass.ToString());
            toolStripButton6.Enabled = adminpass;
            toolStripButton5.Enabled = adminpass;
            toolStripButton4.Enabled = adminpass;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("");//webBrowser1.DocumentText = "";
            vahidConsole("", true);
            
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (frmConfigInstance == null || frmConfigInstance.IsDisposed)
            {
                frmConfigInstance = new form_config();
            }

            // نمایش یا مخفی کردن فرم
            frmConfigInstance.TopMost = true;
            frmConfigInstance.ShowDialog();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            HideMainForm(true);
            //HideMainForm(false);//mean showw this form
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("آیا مطمئن هستید که می‌خواهید از برنامه خارج شوید؟", "تأیید خروج", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = "<h2>thi is test.</h2><h4>this is Ttext.</h4>";
            webBrowser1.ShowPrintDialog();
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {

            if (!panel1.Visible)
            {
                panel1.Visible = true;
                panel1.Width = textBox1.Width = this.Width / 2;
                panel1.Height = textBox1.Height;
                panel1.Top = textBox1.Top;
                panel1.Left = textBox1.Right;
            }
            else
            {
                panel1.Visible = false;
                textBox1.Width = this.Width;
            }
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            //textBox1.Text += "Checking for updates . . ." + "\r\n";
            vahidConsole("Checking for updates", showTime: true);
            Application.DoEvents();
            Updater updater = new Updater();



            if (updater.IsUserAdmin() == false)
            {
                MessageBox.Show("دسترسی سطح ادمین نیاز است");
                vahidConsole("access denied", showTime: true);
                return;
            }





            toolStripButton9.Enabled = false;



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

            toolStripButton9.Enabled = true;
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            Forms.AboutBox1 ab=new Forms.AboutBox1();
            ab.ShowDialog();
            
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }

    
    
}

   

