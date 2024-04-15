using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace websocket_reader
{
    public partial class form_mydialogshow : Form
    {
        public form_mydialogshow()
        {
            InitializeComponent();
        }
        public string printername { get; set; }

        private void form_mydialogshow_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("asasa");
            printername = "";
            List<string> installedPrinters = PrinterManager.GetInstalledPrinters();
            foreach (string printer in installedPrinters)
            {
                listBox1.Items.Add(printer);
            }
            if(listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }
            listBox1.Focus();
        
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            printername= listBox1.SelectedItem.ToString();
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
