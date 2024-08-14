using System.Text.RegularExpressions;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

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
        private void Listen()
        {
            while (listener.IsListening)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                IPServer = ExtractAddress(request.Url.ToString());
                LogToFile("URL",IPServer);
               
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

                // مسیر فایل response.json که باید وجود داشته باشد
                string responseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");

                if (File.Exists(responseFilePath))
                {
                    // خواندن محتوای فایل response.json
                    string jsonResponse = File.ReadAllText(responseFilePath);

                    // ارسال JSON به کلاینت
                    ServeJsonResponse(context, jsonResponse);
                }
                else
                {
                    // در صورتی که فایل response.json وجود نداشته باشد، خطا ارسال می‌شود
                    ServeSimpleResponse(context, "Error: response.json file not found.");
                }
            }
            else
            {
                string htmlMessage = @"
                                        <html>
                                        <head><title>Service Status</title></head>
                                        <body>
                                            <p>Service is running. <a href='/test'>/test</a> | <a href='/devices'>/devices</a></p>
                                        </body>
                                        </html>";

                // ارسال پاسخ
                ServeSimpleResponse(context, htmlMessage);

            }
        }


        private string BuildArguments(JObject jsonObject)
        {
            string postype = jsonObject["postype"].ToString();
            string amount = jsonObject["amount"].ToString();
            string batchnr = jsonObject["batchnr"].ToString();
            string sanadyear = jsonObject["sanadyear"].ToString();
            string sandoghnr = jsonObject["sandoghnr"].ToString();

            return $"\"{postype}\" \"{amount}\" \"{batchnr}\" \"{sanadyear}\" \"{sandoghnr}\"";
        }

        private string ExecuteExternalProcess(string arguments)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");
            if (File.Exists(filePath))
            { File.Delete(filePath); }

            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_pos.exe");
            LogToFile("Executing process", exePath + " - " + arguments);

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

        private void SendJsonResponse(HttpListenerContext context, string result)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "response.json");


            if (File.Exists(filePath))
            {
                string jsonResponse = File.ReadAllText(filePath);
                LogToFile("Sending JSON response", jsonResponse);

                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.ContentType = "application/json";
                byte[] responseBuffer = Encoding.UTF8.GetBytes(jsonResponse);
                context.Response.ContentLength64 = responseBuffer.Length;
                context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
                context.Response.OutputStream.Close();
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                string errorMessage = "Response file not found.";
                LogToFile("Error", errorMessage);
                byte[] errorBuffer = Encoding.UTF8.GetBytes(errorMessage);
                context.Response.ContentLength64 = errorBuffer.Length;
                context.Response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
                context.Response.OutputStream.Close();
            }
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
}
