using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Windows.Forms;
namespace websocket_reader.Class
{
    public class PrintingExample
    {
        private WebBrowser webBrowser;
        public PrintingExample()
        {
            webBrowser = new WebBrowser();
            // تنظیم رویداد برای اتمام بارگذاری صفحه
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebBrowserDocumentCompletedHandler);
        }

        public void PrintHTML(string html, int numberOfCopies)
        {
            // بارگذاری HTML
            webBrowser.DocumentText = html;
            // ذخیره تعداد کپی برای استفاده در رویداد چاپ
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += (sender, e) => PrintPageHandler(sender, e, numberOfCopies);
            // چاپ صفحه HTML
            printDocument.Print();
        }

        private void WebBrowserDocumentCompletedHandler(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // صفحه HTML به طور کامل بارگذاری شده است
        }

        private void PrintPageHandler(object sender, PrintPageEventArgs e, int numberOfCopies)
        {
            // تنظیم تعداد کپی
            e.PageSettings.PrinterSettings.Copies = (short)numberOfCopies;
            // چاپ صفحه HTML
            webBrowser.Print();
        }

    }
}
