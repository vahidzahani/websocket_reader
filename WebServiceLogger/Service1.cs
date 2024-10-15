using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WebServiceLogger
{
    public partial class Service1 : ServiceBase
    {
        private HttpListener listener;
        private Thread listenerThread;
        private string url;
        string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configwebservice.ini");
        private string IPServer = "localhost";

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // خواندن پورت از فایل INI
            int port = ReadPortFromConfig();
            url = $"http://+:{port}/";

            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();

            listenerThread = new Thread(Listen);
            listenerThread.Start();
        }
        private int ReadPortFromConfig()
        {
            int port = 2024; // default

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
        private void Listen()
        {
            while (listener.IsListening)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                IPServer = ExtractAddress(request.Url.ToString());
                LogToFile("URL", IPServer);

                if (request.HttpMethod == "OPTIONS")
                {
                    HandleOptionsRequest(context);
                }
                else if (request.HttpMethod == "POST" && request.ContentType == "application/json")
                {
                    HandlePostRequest(context);
                }
                else if (request.HttpMethod == "GET")
                {
                    HandleGetRequest(context);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    context.Response.Close();
                }
            }
        }
        public static string ExtractAddress(string url)
        {
            // الگوی رگولار اکسپرشن برای استخراج IP یا نام سرور
            string pattern = @"^(?:http://)?([^:/\s]+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "127.0.0.1";
        }

        private void HandleOptionsRequest(HttpListenerContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
        }

        private void HandlePostRequest(HttpListenerContext context)
        {
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                string jsonData = reader.ReadToEnd();
                var jsonObject = JObject.Parse(jsonData);

                // Log the request
                LogToFile("Received POST request", jsonData);

                // Build and execute the command
                string arguments = BuildArguments(jsonObject);
                string result = ExecuteExternalProcess(arguments);

                // Send the result to the client
                SendJsonResponse(context, result);
            }
        }

        private void HandleGetRequest(HttpListenerContext context)
        {
            string requestPath = context.Request.Url.AbsolutePath;

            if (requestPath == "/test")
            {
                ServeHtmlFile(context, "webservice_test.html");
            }
            else if (requestPath == "/devices")
            {
                // آرگومان‌های اجرای فایل
                string arguments = "getAllDevices";

                // اجرای فایل config_pos.exe
                ExecuteExternalProcess(arguments);
                HandleResponse(context);
            }
            else if (requestPath == "/showtransactions")// all transaction from tbl_transactions_mini
            {
                // آرگومان‌های اجرای فایل
                string arguments = "showtransactions";

                // اجرای فایل config_pos.exe
                ExecuteExternalProcess(arguments);
                HandleResponse(context);
            }
            else if (requestPath.StartsWith("/showtransaction"))//selected a transaction from table tbl_transactions_mini
            {
                string amount = context.Request.QueryString["amount"];
                string batchnr = context.Request.QueryString["batchnr"];
                string sanadyear = context.Request.QueryString["sanadyear"];
                string sandoghnr = context.Request.QueryString["sandoghnr"];

                var arguments = "showtransaction";

                if (!string.IsNullOrEmpty(batchnr) && !string.IsNullOrEmpty(sanadyear) && !string.IsNullOrEmpty(sandoghnr) && !string.IsNullOrEmpty(amount))
                {
                    arguments += $" amount:{amount}";
                    arguments += $" batchnr:{batchnr}";
                    arguments += $" sanadyear:{sanadyear}";
                    arguments += $" sandoghnr:{sandoghnr}";
                }
                
                ExecuteExternalProcess(arguments);

                string responseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");
                Response responseObject = new Response();

                if (File.Exists(responseFilePath))
                {
                    responseObject.response_code = 200; // موفقیت
                    responseObject.response_data = JsonConvert.DeserializeObject(File.ReadAllText(responseFilePath));
                }
                else
                {
                    responseObject.response_code = 404; // خطا
                    responseObject.response_data = "not find file"; // رشته در حالت خطا
                }

                // ساخت JSON پاسخ و ارسال آن
                ServeJsonResponse(context, JsonConvert.SerializeObject(responseObject, Formatting.Indented));
            }
            else if (requestPath.StartsWith("/deleteTransactions"))
            {
                // دریافت مقدارهای اختیاری از QueryString
                string idString = context.Request.QueryString["id"];
                string amountString = context.Request.QueryString["amount"];
                string batchnrString = context.Request.QueryString["batchnr"];
                string sanadyearString = context.Request.QueryString["sanadyear"];
                string sandoghnrString = context.Request.QueryString["sandoghnr"];

                long id = 0;
                decimal amount = 0;
                int batchnr = 0, sanadyear = 0, sandoghnr = 0;

                // ساختن آرگومان‌ها برای ارسال به فایل اجرایی
                var arguments = "deleteTransactions";

                if (!string.IsNullOrEmpty(idString) && long.TryParse(idString, out id))
                {
                    arguments += $" id:{id}";
                }
                if (!string.IsNullOrEmpty(amountString) && decimal.TryParse(amountString, out amount))
                {
                    arguments += $" amount:{amount}";
                }
                if (!string.IsNullOrEmpty(batchnrString) && int.TryParse(batchnrString, out batchnr))
                {
                    arguments += $" batchnr:{batchnr}";
                }
                if (!string.IsNullOrEmpty(sanadyearString) && int.TryParse(sanadyearString, out sanadyear))
                {
                    arguments += $" sanadyear:{sanadyear}";
                }
                if (!string.IsNullOrEmpty(sandoghnrString) && int.TryParse(sandoghnrString, out sandoghnr))
                {
                    arguments += $" sandoghnr:{sandoghnr}";
                }

                // اجرای فایل config_pos.exe با آرگومان‌های ساخته شده
                ExecuteExternalProcess(arguments);

                string responseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");
                Response responseObject = new Response();

                if (File.Exists(responseFilePath))
                {
                    responseObject.response_code = 200; // موفقیت
                    responseObject.response_data = JsonConvert.DeserializeObject(File.ReadAllText(responseFilePath));
                }
                else
                {
                    responseObject.response_code = 404; // خطا
                    responseObject.response_data = "not find file"; // رشته در حالت خطا
                }

                // ساخت JSON پاسخ و ارسال آن
                ServeJsonResponse(context, JsonConvert.SerializeObject(responseObject, Formatting.Indented));
            }

            
            else
            {

                string htmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "webservice_main.html");

                if (File.Exists(htmlFilePath))
                {
                    // خواندن محتوای فایل HTML
                    string htmlMessage = File.ReadAllText(htmlFilePath);

                    // ارسال پاسخ
                    ServeSimpleResponse(context, htmlMessage);
                }
                else
                {
                    // اگر فایل پیدا نشد، یک پیام خطا نمایش داده می‌شود
                    string errorMessage = "<html><body><h1>Error</h1><p>File not found: webservice_main.html</p></body></html>";
                    ServeSimpleResponse(context, errorMessage);
                }




            }
        }
        public void HandleResponse(HttpListenerContext context)
        {
            try
            {
                string responseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");
                Response responseObject = new Response();
                if (File.Exists(responseFilePath))
                {
                    string jsonResponse = File.ReadAllText(responseFilePath);
                    var responseData = JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse);
                    responseObject.response_code = 200; // موفقیت
                    responseObject.response_data = responseData; // آرایه بازگردانده می‌شود
                }
                else
                {
                    responseObject.response_code = 404; // خطا
                    responseObject.response_data = "هیچ دستگاهی ثبت نشده است"; // رشته در حالت خطا
                }
                string jsonResponseOutput = JsonConvert.SerializeObject(responseObject, Formatting.Indented);
                ServeJsonResponse(context, jsonResponseOutput);
            }
            catch (Exception ex)
            {
                LogToFile("ERROR HandleResponse", ex.Message);
            }
        }



        private string BuildArguments(JObject jsonObject)
        {
            string id = jsonObject["id"].ToString();
            string amount = jsonObject["amount"].ToString();
            string batchnr = jsonObject["batchnr"].ToString();
            string sanadyear = jsonObject["sanadyear"].ToString();
            string sandoghnr = jsonObject["sandoghnr"].ToString();

            return $"sendToPos \"{id}\" \"{amount}\" \"{batchnr}\" \"{sanadyear}\" \"{sandoghnr}\"";
        }

        private string ExecuteExternalProcess(string arguments)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_pos.exe");
            LogToFile("Executing process", exePath +" "+ arguments);

            ProcessStartInfo processStartInfo = new ProcessStartInfo(exePath, arguments)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return result;
            }
        }
        private void ServeJsonResponse(HttpListenerContext context, string jsonResponse)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.ContentType = "application/json";
            byte[] responseBuffer = Encoding.UTF8.GetBytes(jsonResponse);
            context.Response.ContentLength64 = responseBuffer.Length;
            context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
            context.Response.OutputStream.Close();
        }

        //private void SendJsonResponse(HttpListenerContext context, string result)
        //{
        //    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");


        //    if (File.Exists(filePath))
        //    {
        //        string jsonResponse = File.ReadAllText(filePath);
        //        LogToFile("Sending JSON response", jsonResponse);

        //        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        //        context.Response.ContentType = "application/json";
        //        byte[] responseBuffer = Encoding.UTF8.GetBytes(jsonResponse);
        //        context.Response.ContentLength64 = responseBuffer.Length;
        //        context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
        //        context.Response.OutputStream.Close();
        //    }
        //    else
        //    {
        //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //        string errorMessage = "Response file not found.";
        //        LogToFile("Error", errorMessage);
        //        byte[] errorBuffer = Encoding.UTF8.GetBytes(errorMessage);
        //        context.Response.ContentLength64 = errorBuffer.Length;
        //        context.Response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
        //        context.Response.OutputStream.Close();
        //    }
        //}
        //private void SendJsonResponse(HttpListenerContext context, string result)
        //{
        //    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");

        //    // ایجاد شیء پاسخ از کلاس Response
        //    var responseObj = new Response();

        //    if (File.Exists(filePath))
        //    {
        //        string jsonResponse = File.ReadAllText(filePath);

        //        // تنظیم کد موفقیت و داده‌ها
        //        responseObj.response_code = 200;
        //        responseObj.response_data = JsonConvert.DeserializeObject(jsonResponse); // داده‌ها از فایل JSON

        //        LogToFile("Sending JSON response", jsonResponse);
        //    }
        //    else
        //    {
        //        // تنظیم کد خطا و پیام خطا
        //        responseObj.response_code = 404;
        //        responseObj.response_data = "Response file not found.";

        //        LogToFile("Error", "Response file not found.");
        //    }

        //    // تبدیل شیء responseObj به JSON
        //    string jsonResponseOutput = JsonConvert.SerializeObject(responseObj);
        //    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        //    context.Response.ContentType = "application/json";
        //    byte[] responseBuffer = Encoding.UTF8.GetBytes(jsonResponseOutput);
        //    context.Response.ContentLength64 = responseBuffer.Length;
        //    context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
        //    context.Response.OutputStream.Close();
        //}
        private void SendJsonResponse(HttpListenerContext context, string result)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");

            // ایجاد شیء پاسخ از کلاس Response
            var responseObj = new Response();
            string sssssss = "";
            if (File.Exists(filePath))
            {
                string jsonResponse = File.ReadAllText(filePath);
                sssssss = jsonResponse;

                // Deserialize کردن محتوای JSON به یک شیء داینامیک
                dynamic parsedResponse = JsonConvert.DeserializeObject(jsonResponse);

                // بررسی و تنظیم کد ResponseCode از فایل JSON
                if (parsedResponse != null && parsedResponse.ResponseCode != null)
                {
                    responseObj.response_code = int.Parse(parsedResponse.ResponseCode.ToString());
                }
                else
                {
                    responseObj.response_code = 500; // مقدار پیش‌فرض در صورت عدم وجود ResponseCode
                }

                // تنظیم داده‌ها
                responseObj.response_data = parsedResponse; // داده‌ها از فایل JSON

                LogToFile("Sending JSON response", jsonResponse);
            }
            else
            {
                // تنظیم کد خطا و پیام خطا
                responseObj.response_code = 404;
                responseObj.response_data = "Response file not found.";

                LogToFile("Error", "Response file not found.");
            }

            // تبدیل شیء responseObj به JSON
            string jsonResponseOutput = JsonConvert.SerializeObject(responseObj);//به صورت آرایه در آرایه
            //string jsonResponseOutput = (sssssss);
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.ContentType = "application/json";
            byte[] responseBuffer = Encoding.UTF8.GetBytes(jsonResponseOutput);
            context.Response.ContentLength64 = responseBuffer.Length;
            context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
            context.Response.OutputStream.Close();
        }


        private void ServeHtmlFile(HttpListenerContext context, string fileName)
        {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string htmlFilePath = Path.Combine(exeDirectory, fileName);

            if (File.Exists(htmlFilePath))
            {
                string htmlContent = File.ReadAllText(htmlFilePath);

                // جایگزینی "vahid" با "mamad"
                htmlContent = htmlContent.Replace("127.0.0.1", IPServer);

                LogToFile("Serving HTML file", htmlFilePath);

                context.Response.ContentType = "text/html";
                byte[] responseBuffer = Encoding.UTF8.GetBytes(htmlContent);
                context.Response.ContentLength64 = responseBuffer.Length;
                context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                string errorMessage = "File not found.";
                LogToFile("Error", errorMessage);
                byte[] errorBuffer = Encoding.UTF8.GetBytes(errorMessage);
                context.Response.ContentLength64 = errorBuffer.Length;
                context.Response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
            }

            context.Response.OutputStream.Close();
        }


        private void ServeSimpleResponse(HttpListenerContext context, string message)
        {
            // تنظیم هدر Access-Control-Allow-Origin برای اجازه به درخواست‌های CORS (در صورت نیاز)
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            // تنظیم نوع محتوا به HTML
            context.Response.ContentType = "text/html";

            // تبدیل پیام به بایت‌ها
            byte[] responseBuffer = Encoding.UTF8.GetBytes(message);

            // تنظیم طول محتوا
            context.Response.ContentLength64 = responseBuffer.Length;

            // نوشتن داده‌ها به جریان خروجی
            context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);

            // بستن جریان خروجی
            context.Response.OutputStream.Close();
        }


        private void LogToFile(string title, string message)
        {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logFilePath = Path.Combine(exeDirectory, "webservice_log.txt");

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {title}");
                writer.WriteLine(message);
                writer.WriteLine("------------------------------------------------------");
            }
        }

        protected override void OnStop()
        {
            listener.Stop();
            listenerThread.Abort();
        }
    }
    public class Response
    {
        public int response_code { get; set; }
        public object response_data { get; set; }
    }

}
