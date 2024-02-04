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
    public partial class form_config : Form
    {
        public form_config()
        {
            InitializeComponent();
        }

        private void form_config_Load(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            textport.Text= form1.Getport2().ToString();
            Updater up = new Updater();
            textserver.Text=up.GetServerAddress().ToString();


        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("بعد از انجام تنظیمات برنامه راه اندازی مجدد می شود، ادامه می دهید؟", "تأیید ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                
                Form1 form1 = new Form1();
                IniFile iniFile = new IniFile(form1.strConfigFile);
                iniFile.SetValue("Settings", "server", textserver.Text);
                iniFile.SetValue("Settings", "port", textport.Text);
            
                Application.Restart();
            }


        }
    }
}
