using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.WebSockets;


namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        private Thread workerThread;
        private bool isRunning;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start(args);
        }
        public void Start(string[] args)
        {
            //isRunning = true;
            //workerThread = new Thread(DoWork);
            //workerThread.Start();
            isRunning = true;
            workerThread = new Thread(async () => await RunWebSocketServer());
            workerThread.Start();
        }

        protected override void OnStop()
        {
            Stop();
        }
        public void Stop()
        {
            isRunning = false;
            workerThread.Join();
        }

        private async Task RunWebSocketServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:2024/");
            listener.Start();
            Console.WriteLine("WebSocket server started on ws://localhost:2024");

            while (isRunning)
            {
                HttpListenerContext context = await listener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = wsContext.WebSocket;

                    await HandleWebSocketCommunication(webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        private async Task HandleWebSocketCommunication(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open && isRunning)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Received: " + message);

                    byte[] responseBuffer = Encoding.UTF8.GetBytes("Message received");
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private void DoWork()
        {
            while (isRunning)
            {
                // کدهای اصلی سرویس در اینجا اجرا می‌شود
                Console.WriteLine("Service is running...");
                Thread.Sleep(1000);
            }
        }
    }
}
