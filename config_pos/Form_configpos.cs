﻿using Newtonsoft.Json;
using OmidPayPcPos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace config_pos
{
    public partial class Form_configpos : Form
    {
        int posDefaultIndex = 0;
        string serviceName = "WebServiceLoggerPOS";
        string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configwebservice.ini");
        int servicePort;
        public Form_configpos()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {


        }
        public bool IsUserAdmin()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(currentUser);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void ControlService(string serviceName, ServiceControllerStatus desiredStatus)
        {
            try
            {
                ServiceController sc = new ServiceController(serviceName);

                if (desiredStatus == ServiceControllerStatus.Running)
                {
                    if (sc.Status != ServiceControllerStatus.Running)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        //MessageBox.Show($"سرویس '{serviceName}' با موفقیت شروع به کار کرد.");
                    }
                    else
                    {
                        //MessageBox.Show($"سرویس '{serviceName}' در حال حاضر در حال اجرا است.");
                    }
                }
                else if (desiredStatus == ServiceControllerStatus.Stopped)
                {
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        //MessageBox.Show($"سرویس '{serviceName}' با موفقیت متوقف شد.");
                    }
                    else
                    {
                        //MessageBox.Show($"سرویس '{serviceName}' در حال حاضر متوقف است.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا: {ex.Message}");
            }
        }
        private int ReadPortFromConfig()
        {
            int port = 2024; // مقدار پیش‌فرض

            if (File.Exists(configFilePath))
            {
                string[] lines = File.ReadAllLines(configFilePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("port="))
                    {
                        string portValue = line.Substring("port=".Length);
                        if (int.TryParse(portValue, out int parsedPort))
                        {
                            port = parsedPort;
                        }
                    }
                }
            }

            return port;
        }
        public static void CheckAndCopyDll()
        {
            // مسیر فایل DLL در کنار برنامه
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dllFilePath = Path.Combine(currentDirectory, "RJCP.SerialPortStream.dll");

            // مسیر فایل DLL در پوشه bk
            string backupDirectory = Path.Combine(currentDirectory, "bk");
            string backupDllFilePath = Path.Combine(backupDirectory, "RJCP.SerialPortStream.dll");

            // بررسی وجود فایل DLL در کنار برنامه
            if (!File.Exists(dllFilePath))
            {
                // اگر فایل در کنار برنامه نبود، از پوشه bk کپی کن
                if (File.Exists(backupDllFilePath))
                {
                    File.Copy(backupDllFilePath, dllFilePath);
                    //Console.WriteLine("فایل RJCP.SerialPortStream.dll از پوشه bk کپی شد.");
                }
                else
                {
                    //Console.WriteLine("فایل RJCP.SerialPortStream.dll در پوشه bk یافت نشد.");
                }
            }
            else
            {
                //Console.WriteLine("فایل RJCP.SerialPortStream.dll در کنار برنامه وجود دارد.");
            }
        }

        private void Form_configpos_Load(object sender, EventArgs e)
        {


            servicePort = ReadPortFromConfig();
            textBox1.Text = servicePort.ToString();
            CheckAndCopyDll();
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                if (args[1] == "getAllDevices")
                {

                    // مسیر فایل config.dat که در کنار برنامه قرار دارد
                    string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.dat");

                    if (File.Exists(configFilePath))
                    {
                        // خواندن تمام خطوط فایل config.dat
                        var lines = File.ReadAllLines(configFilePath);

                        // لیستی برای ذخیره اطلاعات JSON
                        var devices = new List<object>();

                        foreach (var line in lines)
                        {
                            // جدا کردن مقادیر با استفاده از کاما
                            var parts = line.Split(',');

                            if (parts.Length == 6)
                            {
                                // اضافه کردن هر دستگاه به لیست
                                devices.Add(new
                                {
                                    id = parts[0],
                                    devicename = parts[1],
                                    postype = parts[2],
                                    ip = parts[3],
                                    port = parts[4],
                                    isDeafult = parts[5],
                                    icon = Program.imagesBase64.ContainsKey(parts[2]) ? Program.imagesBase64[parts[2]] : "NOICON"

                                });
                            }
                        }

                        // تبدیل لیست به JSON
                        string jsondata = JsonConvert.SerializeObject(devices, Formatting.Indented);

                        // نوشتن JSON به فایل response.json
                        string responseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");
                        File.WriteAllText(responseFilePath, jsondata);
                    }

                }
                if (args[1] == "sendToPos")
                {
                    string amount = args[3];
                    string id = args[2];

                    List<string> deviceInfo = GetDeviceInfo(id);
                    //List<string> deviceInfo = GetDeviceInfo("fanava");
                    if (deviceInfo.Count > 0)
                    {
                        string ip = deviceInfo[1];
                        string port = deviceInfo[2];
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");
                        string resJsonString = "";
                        if (deviceInfo[0] == "fanava")
                        {
                            resJsonString = Fn_send_to_POS("fanava", amount, ip, int.Parse(port));
                        }
                        if (deviceInfo[0] == "omidpay")
                        {
                            resJsonString = Fn_send_to_POS("omidpay", amount, ip, int.Parse(port));
                        }
                        File.WriteAllText(filePath, resJsonString);
                    }
                    else
                    {
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");
                        File.WriteAllText(filePath, "\"Error\":\"Failed to get a valid response from the server.\"");
                    }
                }
                Application.Exit();
            }

            btn_start_Click_1(sender, e);
            //this.Text += $" ver : {Application.ProductVersion} ";
            if (IsUserAdmin() == true)
            {
                this.Text += " : Administrator";
            }
            else
            {
                //MessageBox.Show("جهت اجرای صحیح برنامه نباز به دسترسی سطح کاربر مدیر می باشد");
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
        public void InstallService(string executablePath, string serviceName)
        {
            try
            {
                string installUtilPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe";

                if (File.Exists(installUtilPath))
                {
                    // اگر InstallUtil.exe وجود دارد، از آن برای نصب سرویس استفاده کنید
                    InstallUsingInstallUtil(executablePath, installUtilPath);
                }
                else
                {
                    // اگر InstallUtil.exe وجود ندارد، از دستور sc create استفاده کنید
                    InstallUsingScCreate(executablePath, serviceName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error installing service: " + ex.Message);
            }
        }
        private void InstallUsingInstallUtil(string executablePath, string installUtilPath)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = installUtilPath,
                    Arguments = $"/i \"{executablePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Failed to install service using InstallUtil. Error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error installing service using InstallUtil: " + ex.Message);
            }
        }

        private void InstallUsingScCreate(string executablePath, string serviceName)
        {
            try
            {
                string command = $"create {serviceName} binPath= \"{executablePath}\"";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Failed to install service using sc create. Error: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error installing service using sc create: " + ex.Message);
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            servicePort = ReadPortFromConfig();
            ControlService(serviceName, ServiceControllerStatus.Running);
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            ControlService(serviceName, ServiceControllerStatus.Stopped);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                bool serviceExists = services.Any(s => s.ServiceName == serviceName);

                if (!serviceExists)
                {
                    label4.Text = "Service does not exist";
                    button7.Visible = true;
                    return;
                }
                else
                {
                    button7.Visible = false;

                }

                ServiceController sc = new ServiceController(serviceName);
                string str = (sc.Status.ToString() == "Running") ? "Service is Running on port : " + servicePort : "Service is Stopped";
                label4.Text = str;
            }
            catch (Exception ex)
            {
                // Handle other exceptions if necessary
                label4.Text = "Error: " + ex.Message;
            }
        }


        private void btn_start_Click_1(object sender, EventArgs e)
        {

            try
            {
                listView1.Clear();

                listView1.View = View.Details;
                listView1.Columns.Add("id", 20);
                listView1.Columns.Add("عنوان دستگاه", 140);
                listView1.Columns.Add("مدل", 100);
                listView1.Columns.Add("IP", 100);
                listView1.Columns.Add("Port", 50);

                listView1.SmallImageList = imageList1;

                // مسیر فایل را مشخص کنید
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.dat");

                // بررسی وجود فایل
                if (File.Exists(filePath))
                {
                    // خواندن محتویات فایل
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        string line;
                        // خواندن هر خط از فایل
                        while ((line = sr.ReadLine()) != null)
                        {
                            // تقسیم خط به عباراتی که با کاما جدا شده‌اند
                            string[] parts = line.Split(',');

                            // بررسی تعداد آیتم‌ها در آرایه
                            if (parts.Length >= 6)  // اطمینان از وجود حداقل 6 جزء
                            {
                                ListViewItem item = new ListViewItem(parts[0]);  // id
                                item.SubItems.Add(parts[1]); // devicename
                                item.SubItems.Add(parts[2]); // مدل
                                item.SubItems.Add(parts[3]); // IP
                                item.SubItems.Add(parts[4]); // Port

                                // بررسی مقدار isDefault
                                if (parts[5] == "1")
                                {
                                    item.ImageIndex = 1;
                                    posDefaultIndex = listView1.Items.Count;  // ذخیره اندیس برای دستگاه پیش‌فرض
                                }
                                else
                                {
                                    item.ImageIndex = 0;
                                }

                                listView1.Items.Add(item);
                            }
                            else
                            {
                                // گزارش خطا در صورت نادرست بودن تعداد آیتم‌ها
                                Console.WriteLine("خطای داده: تعداد مقادیر کافی نیست.");
                            }
                        }
                    }
                }
                else
                {
                    //MessageBox.Show("فایل config.dat یافت نشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // لاگ کردن اطلاعات استثنا برای اشکال‌زدایی
                MessageBox.Show($"خطا رخ داد: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }






        }


        private void button4_Click(object sender, EventArgs e)
        {
            Form_config frm = new Form_config();
            //save data
            frm.ShowDialog();

            if (frm.pos_name != null)
            {

                // بدست آوردن بزرگ‌ترین id از ستون اول (id)
                int maxId = 0; // شروع با صفر
                foreach (ListViewItem item1 in listView1.Items)
                {
                    int currentId;
                    if (int.TryParse(item1.Text, out currentId)) // چک کردن اینکه مقدار id قابل تبدیل به عدد باشد
                        maxId = Math.Max(maxId, currentId);
                }

                // حالا maxId بزرگ‌ترین مقدار موجود است، آیتم جدید یک واحد بیشتر خواهد بود
                int newId = maxId + 1;


                ListViewItem item = new ListViewItem(newId.ToString());
                item.SubItems.Add(frm.pos_name);
                item.SubItems.Add(frm.pos_model);
                item.SubItems.Add(frm.pos_ip);
                item.SubItems.Add(frm.pos_port);
                item.ImageIndex = 0;

                listView1.Items.Add(item);
                button1.Enabled = true;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                // باز کردن یک فایل برای نوشتن
                using (StreamWriter writer = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.dat")))
                {
                    // پیمایش تمام سطرهای ListView و نوشتن هر سطر به فایل
                    foreach (ListViewItem item in listView1.Items)
                    {
                        for (int i = 0; i < item.SubItems.Count; i++)
                        {
                            // اضافه کردن مقدار هر ستون به فایل، با استفاده از فاصله به عنوان جداکننده
                            writer.Write(item.SubItems[i].Text);
                            if (i < item.SubItems.Count - 1)
                                writer.Write(","); // اضافه کردن فاصله بین ستون‌ها
                        }
                        if (item.Index == posDefaultIndex)
                            writer.Write(",1"); // اضافه کردن برای پیش فرض
                        else
                            writer.Write(",0"); // اضافه کردن برای پیش فرض

                        writer.WriteLine(); // پایان هر سطر
                    }
                }

                MessageBox.Show("Items saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                button1.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void button5_Click(object sender, EventArgs e)
        {
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Form_config frm = new Form_config();
                frm.editdata = true;
                ListViewItem item = listView1.SelectedItems[0];

                frm.pos_name = item.SubItems[1].Text;
                frm.pos_model = item.SubItems[2].Text;
                frm.pos_ip = item.SubItems[3].Text;
                frm.pos_port = item.SubItems[4].Text;

                frm.ShowDialog();

                listView1.Items[listView1.SelectedIndices[0]].SubItems[1].Text = frm.pos_name;
                listView1.Items[listView1.SelectedIndices[0]].SubItems[2].Text = frm.pos_model;
                listView1.Items[listView1.SelectedIndices[0]].SubItems[3].Text = frm.pos_ip;
                listView1.Items[listView1.SelectedIndices[0]].SubItems[4].Text = frm.pos_port;
                button1.Enabled = true;
            }
            else
            {
                MessageBox.Show("No item selected.");
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {

                listView1.Items[listView1.SelectedIndices[0]].Remove();
                button1.Enabled = true;

            }
        }





        public static void SendTextToTcpPort(string textToSend, string IP, int port)
        {
            try
            {
                // Connect to the remote TCP server
                TcpClient client = new TcpClient(IP, port);

                // Get the network stream from the TCP client
                NetworkStream stream = client.GetStream();

                // Convert the text to bytes
                byte[] data = Encoding.ASCII.GetBytes(textToSend);

                // Send the data to the TCP server
                stream.Write(data, 0, data.Length);
                Console.WriteLine($"Sent '{textToSend}' to port {port}");

                // Buffer to store the response bytes.
                byte[] responseData = new byte[256];

                // Read the response from the stream
                int bytes = stream.Read(responseData, 0, responseData.Length);
                string response = Encoding.ASCII.GetString(responseData, 0, bytes);

                // Show the response in a MessageBox
                MessageBox.Show(response, "Response from Server");

                // Close the network stream and the client
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception occurred: {ex.Message}", "Error");
            }
        }
        public string Fn_send_to_omidpay(string amount, string ipAddress, int port)
        {
            // ایجاد یک نمونه از کلاس OmidPayPcPosClass
            OmidPayPcPosClass omid = new OmidPayPcPosClass();

            // فراخوانی تابع DoTcpTransaction و دریافت پاسخ
            ResponseJson response = omid.DoTcpTransaction(ipAddress, port, amount);

            // تبدیل شیء response به رشته JSON
            string jsonResponse = JsonConvert.SerializeObject(new
            {
                TermNo = response.TermNo,
                Date = response.Date,
                Time = response.Time,
                SpentAmount = response.SpentAmount,
                RRN = response.RRN,
                TraceNo = response.TraceNo,
                CardNo = response.CardNo,
                CardName = response.CardName,
                ResponseCode = response.ResponseCode,
                Result = response.Result
            });

            // بازگشت JSON به عنوان خروجی
            return jsonResponse;
        }

        public string Fn_send_to_POS(string postype, string amount, string ipAddress, int port)
        {
            if (postype == "omidpay")
            {
                // ایجاد یک نمونه از کلاس OmidPayPcPosClass
                OmidPayPcPosClass omid = new OmidPayPcPosClass();

                // فراخوانی تابع DoTcpTransaction و دریافت پاسخ
                ResponseJson response = omid.DoTcpTransaction(ipAddress, port, amount);

                // تبدیل شیء response به رشته JSON
                string jsonResponse = JsonConvert.SerializeObject(new
                {
                    TermNo = response.TermNo,
                    Date = DateConverter.ConvertToPersianDate(response.Date),
                    Time = DateConverter.ConvertToFormattedTime(response.Time),
                    SpentAmount = response.SpentAmount,
                    RRN = response.RRN,
                    TraceNo = response.TraceNo,
                    CardNo = response.CardNo,
                    CardName = response.CardName,
                    ResponseCode = response.ResponseCode == "00" ? "200" : response.ResponseCode,
                    Result = response.Result
                });


                // بازگشت JSON به عنوان خروجی
                return jsonResponse;
            }

            if (postype == "fanava")
            {

                string firstMessage = "{\"STX\":\"02\",\"Message Len\":\"0072\",\"Message ID\":\"88\",\"ETX\":\"03\",\"LRC\":\"$\"}";
                byte[] firstData = Encoding.ASCII.GetBytes(firstMessage);
                string secondMessage = $"{{\"STX\":\"02\",\"Message Len\":\"0393\",\"Message ID\":\"89\",\"Request Type\":\"01\",\"Processing Code\":\"000000\",\"Code Page\":\"01\",\"Print Type\":\"02\",\"Service Code\":\"01\",\"Spent Amount\":\"{amount}\",\"Discount Amount\":\"\",\"Invoice Count\":\"\",\"Invoice No\":\"\",\"Bill ID\":\"\",\"Payment ID\":\"\",\"Tel\":\"\",\"National ID\":\"\",\"Name\":\"\",\"Acc NO\":\"\",\"Charge ID\":\"41\",\"Other\":\"\",\"Print Description\":\"F1=,F2=10000,\",\"ETX\":\"03\",\"LRC\":\"$\"}}";
                byte[] secondData = Encoding.ASCII.GetBytes(secondMessage);
                byte[] ackData = new byte[] { 0x06 };  // ASCII code for ACK is \u0006

                try
                {
                    using (TcpClient client = new TcpClient(ipAddress, port))
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(firstData, 0, firstData.Length);

                        byte[] responseData = new byte[256];
                        int bytes = stream.Read(responseData, 0, responseData.Length);
                        string responseMessage = Encoding.ASCII.GetString(responseData, 0, bytes);

                        if (responseMessage == "\u0006")
                        {
                            stream.Write(secondData, 0, secondData.Length);
                            bytes = stream.Read(responseData, 0, responseData.Length);
                            string responseMessage2 = Encoding.ASCII.GetString(responseData, 0, bytes);

                            if (responseMessage2 == "\u0006")
                            {
                                string completeMessage = "";
                                bool messageComplete = false;

                                while (!messageComplete)
                                {
                                    bytes = stream.Read(responseData, 0, responseData.Length);
                                    string partMessage = Encoding.ASCII.GetString(responseData, 0, bytes);
                                    completeMessage += partMessage;

                                    if (completeMessage.Contains("\u0003"))
                                    {
                                        messageComplete = true;
                                    }
                                }

                                // پردازش کاراکترهای کنترلی
                                completeMessage = completeMessage.Replace("\u0015", "")
                                                                 .Replace("\u0002", "")
                                                                 .Replace("\u0003", "")
                                                                 .Replace("\u0000", "");

                                // اصلاح نام‌های کلید برای مطابقت با خواص کلاس
                                completeMessage = completeMessage.Replace("Message Len", "MessageLen")
                                                                 .Replace("Message ID", "MessageID")
                                                                 .Replace("Processing Code", "ProcessingCode")
                                                                 .Replace("Term No", "TermNo")
                                                                 .Replace("Merchant No", "MerchantNo")
                                                                 .Replace("Spent Amount", "SpentAmount")
                                                                 .Replace("Response Code", "ResponseCode")
                                                                 .Replace("Card No", "CardNo")
                                                                 .Replace("Card Name", "CardName")
                                                                 .Replace("Response Desc", "ResponseDesc")
                                                                 .Replace("Used Discount", "UsedDiscount")
                                                                 .Replace("Used Wage", "UsedWage")
                                                                 .Replace("Used Loyality", "UsedLoyality")
                                                                 .Replace("Discount Amount", "DiscountAmount")
                                                                 .Replace("Tip Amount", "TipAmount")
                                                                 .Replace("Charge ID", "ChargeID")
                                                                 .Replace("Charge Serial", "ChargeSerial");

                                // تبدیل JSON به شیء
                                ResponseMessage parsedResponseMessage = JsonConvert.DeserializeObject<ResponseMessage>(completeMessage);
                                if (parsedResponseMessage.ResponseDesc == "Swipe Card Fail")
                                    parsedResponseMessage.ResponseDesc = "کارت کشیده نشد";
                                if (parsedResponseMessage.ResponseCode == "00")
                                    parsedResponseMessage.ResponseDesc = "عملیات موفق";

                                // ساختن JSON با اطلاعات برگشتی
                                var responseObject = new
                                {
                                    parsedResponseMessage.TermNo,
                                    Date = DateConverter.ConvertToPersianDate(parsedResponseMessage.Date),
                                    Time=DateConverter.ConvertToFormattedTime(parsedResponseMessage.Time),
                                    parsedResponseMessage.SpentAmount,
                                    parsedResponseMessage.RRN,
                                    parsedResponseMessage.TraceNo,
                                    parsedResponseMessage.CardNo,
                                    parsedResponseMessage.CardName,
                                    ResponseCode=parsedResponseMessage.ResponseCode=="00"?"200":parsedResponseMessage.ResponseCode,
                                    parsedResponseMessage.ResponseDesc
                                };

                                // تبدیل به JSON
                                string jsonResponse = JsonConvert.SerializeObject(responseObject);

                                // بازگشت JSON به عنوان خروجی
                                return jsonResponse;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // نمایش پیام خطا در صورت بروز مشکل در ارتباط با دستگاه
                    MessageBox.Show($"خطا در ارتباط با دستگاه: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }


            // در صورت بروز خطا یا پاسخ نامناسب، می‌توانید یک پیام خطا برگردانید
            return "{\"Error\":\"Failed to get a valid response from the server.\"}";
        }


        private List<string> GetDeviceInfo(string id)
        {
            // براساس نوع دستگاه یک آیپی و پورت برمیگردونه
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.dat");

            if (File.Exists(configFilePath))
            {
                // خواندن تمام خطوط فایل config.dat
                string[] lines = File.ReadAllLines(configFilePath);

                // پردازش هر خط از فایل
                foreach (string line in lines)
                {
                    // جدا کردن مقادیر خط با استفاده از کاما
                    string[] parts = line.Split(',');

                    //if (parts.Length == 6 && parts[0].Trim() == id)
                    if (parts[0].Trim() == id)
                    {
                        // برگرداندن اطلاعات دستگاهی که postype آن برابر با ورودی است
                        return new List<string> { parts[2], parts[3], parts[4] }; // parts[3]: IP, parts[4]: Port
                    }
                }

                // اگر دستگاه پیدا نشد، لیست خالی برگردان
                return new List<string>();
            }
            else
            {
                // اگر فایل پیدا نشد، لیست خالی برگردان
                return new List<string>();
            }
        }


        private void پیشفرضToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // بررسی اینکه آیتمی در ListView انتخاب شده است یا خیر
            if (listView1.SelectedItems.Count > 0)
            {
                // دریافت اولین آیتم انتخاب شده
                listView1.Items[posDefaultIndex].ImageIndex = 0;

                ListViewItem selectedItem = listView1.SelectedItems[0];
                posDefaultIndex = selectedItem.Index;
                selectedItem.ImageIndex = 1;
                // چاپ شماره آیتم (ایندکس)
                //MessageBox.Show("Selected item index: " + selectedItem.Index);
            }
            else
            {
                MessageBox.Show("No item selected.");
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // بررسی اینکه آیا آیتمی در زیر مکان کلیک قرار دارد یا خیر
                var hitTest = listView1.HitTest(e.Location);
                if (hitTest.Item != null)
                {
                    // نمایش ContextMenuStrip در موقعیت کلیک
                    contextMenuStrip1.Show(listView1, e.Location);
                }
            }
        }

        private void ویرایشToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1_DoubleClick(sender, e);

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string serviceExecutablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebServiceLoggerPOS.exe");
            InstallService(serviceExecutablePath, "WebServiceLoggerPOS");
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            if (!IsUserAdmin())
            {
                MessageBox.Show("برای تغییر پورت نیاز به دسترسی ادمین به برنامه را دارید", "دسترسی ادمین");
                return;
            }
            try
            {
                // بررسی اینکه مقدار داخل TextBox1 یک عدد صحیح است
                if (int.TryParse(textBox1.Text, out int portNumber))
                {
                    // بررسی اینکه مقدار داخل محدوده مجاز پورت‌ها (200 تا 9999) باشد
                    if (portNumber >= 200 && portNumber <= 9999)
                    {
                        string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configwebservice.ini");

                        // خواندن محتوای فعلی فایل
                        string[] lines = File.ReadAllLines(configFilePath);

                        // جستجوی خط حاوی "port=" و جایگزینی آن با مقدار جدید
                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (lines[i].StartsWith("port="))
                            {
                                lines[i] = "port=" + portNumber.ToString();
                                break;
                            }
                        }

                        // نوشتن مقادیر جدید به فایل
                        File.WriteAllLines(configFilePath, lines);

                        MessageBox.Show("پورت با موفقیت ذخیره شد ، لطفا سرویس را راه اندازی مجدد کنید");
                    }
                    else
                    {
                        MessageBox.Show("مقدار پورت باید بین 200 تا 9999 باشد.");
                    }
                }
                else
                {
                    MessageBox.Show("لطفاً یک عدد صحیح معتبر برای پورت وارد کنید.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ذخیره پورت: " + ex.Message);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form_configpos_FormClosing(object sender, FormClosingEventArgs e)
        {
            // نمایش پیام برای تأیید بستن برنامه
            if (button1.Enabled == true)
            {
                DialogResult result = MessageBox.Show("تغییرات را ذخیره کنم ؟", "تأیید بستن", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    button1_Click_1(sender, e);
                    //e.Cancel = true;
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
    public class ResponseMessage
    {
        public string STX { get; set; }
        public string MessageLen { get; set; }
        public string MessageID { get; set; }
        public string ProcessingCode { get; set; }
        public string TermNo { get; set; }
        public string MerchantNo { get; set; }
        public string SpentAmount { get; set; }
        public string RRN { get; set; }
        public string TraceNo { get; set; }
        public string ResponseCode { get; set; }
        public string CardNo { get; set; }
        public string CardName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string ResponseDesc { get; set; }
        public string UsedDiscount { get; set; }
        public string UsedWage { get; set; }
        public string UsedLoyality { get; set; }
        public string DiscountAmount { get; set; }
        public string TipAmount { get; set; }
        public string ChargeID { get; set; }
        public string ChargeSerial { get; set; }
        public string Other { get; set; }
        public string ETX { get; set; }
        public string LRC { get; set; }
    }

}
