    using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
namespace WindowsService2
{
    internal static class Program
    {
        //static void Main()
        //{
        //    ServiceBase[] ServicesToRun;
        //    ServicesToRun = new ServiceBase[]
        //    {
        //        new Service1()
        //    };
        //    ServiceBase.Run(ServicesToRun);
        //}


        public static void Main(string[] args)
        {
            //if (Environment.UserInteractive)
            //{
            //    // حالت کنسولی
            //    RunAsConsole(args);
            //}
            //else
            //{
            //    // حالت سرویس ویندوز
            //    RunAsService();
            //}



            // Specify the URI to listen on
            string uri = "http://localhost:2024/";

            // Create a new HttpListener instance
            HttpListener listener = new HttpListener();

            // Add the URI to the listener
            listener.Prefixes.Add(uri);

            // Start listening for incoming requests
            listener.Start();

            Console.WriteLine("Listening for requests on " + uri);

            // Start an asynchronous operation to handle requests
            listener.BeginGetContext(ListenerCallback, listener);

            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // Stop listening for requests
            listener.Stop();
            listener.Close();




        }

        static void ListenerCallback(IAsyncResult result)
        {
            // Get the HttpListener instance
            HttpListener listener = (HttpListener)result.AsyncState;

            // End the asynchronous operation and get the context of the incoming request
            HttpListenerContext context = listener.EndGetContext(result);

            // Handle the request
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Read the request data
            using (Stream stream = request.InputStream)
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestData = reader.ReadToEnd();
                Console.WriteLine("Received request data: " + requestData);

                // Send a response back to the client
                string responseString = "Hello from HttpListener!";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }

            // Close the response
            response.Close();

            // Continue listening for next request
            listener.BeginGetContext(ListenerCallback, listener);
        }

        private static void RunAsConsole(string[] args)
        {
            Service1 service = new Service1();
            service.Start(args);

            Console.WriteLine("Press Enter to terminate ...");
            Console.ReadLine();

            service.Stop();
        }
        private static void RunAsService()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
            new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }



    }
}
