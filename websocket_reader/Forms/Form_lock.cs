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
    public partial class Form_lock : Form
    {
        public Form_lock()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textPass.Text=="his@admin" || textPass.Text=="0000")
            {
                Form1.adminpass =true; 
                
            }
            this.Close();

        }

        private void Form_lock_Load(object sender, EventArgs e)
        {

        }

        private void Form_lock_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void textPass_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar==(char)Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }
    }
}
