using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace websocket_reader
{
    
    public partial class form_mydialogshow : Form
    {
        Timer mini_timer;
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        public form_mydialogshow()
        {
            InitializeComponent();
        }
        public string printername { get; set; }
        public string defaultPrintername { get; set; }

        private void form_mydialogshow_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("asasa");
            
            mini_timer = new Timer();
            mini_timer.Interval = 1000;
            mini_timer.Tick += Mini_timer_Tick1;
            mini_timer.Start();

            printername = "";
            List<string> installedPrinters = Printer.GetInstalledPrinters();
            
            foreach (string printer in installedPrinters)
            {
                listBox1.Items.Add(printer);
                //dataGridView1.Rows.Add(printer);
            }

            int index = listBox1.FindStringExact(defaultPrintername);
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }

            if (index != -1)
            {
                listBox1.SelectedIndex = index;
            }

            SetForegroundWindow(this.Handle);
            listBox1.Focus();

            /////////////////////////////////////////////////////////







        }

        private void Mini_timer_Tick1(object sender, EventArgs e)
        {
            this.Activate();
            mini_timer.Stop();
        }

       

        private void btn_ok_Click(object sender, EventArgs e)
        {
            printername = listBox1.SelectedItem.ToString();
            //MyPrinters.SetDefaultPrinter(printername);
            this.Close();
        }
        private void listBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) {
                //printername = listBox1.SelectedItem.ToString();
                btn_ok_Click(sender, e);  
            }
        }

        private void btn_no_Click(object sender, EventArgs e)
        {
            printername = "";
            this.Close();
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            

        }

        private void btn_showdialog_Click(object sender, EventArgs e)
        {
            printername = listBox1.SelectedItem.ToString();
            Printer.SetDefaultPrinter(printername);

            printername = "##SHOWDIALOG##";//run webbrowser printdialog
            this.Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            btn_ok_Click(sender, e);
        }

        private void form_mydialogshow_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
