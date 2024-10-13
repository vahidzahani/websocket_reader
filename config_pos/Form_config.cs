using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using POS_PC_v3;
using Newtonsoft.Json;
using OmidPayPcPos;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace config_pos
{
    public partial class Form_config : Form
    {
        public string pos_name;
        public string pos_ip;
        public string pos_port;
        public string pos_model;
        public bool savedata = false;
        public bool editdata = false;

        public Form_config()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //const string userRoot = "HKEY_CURRENT_USER";
            //const string subkey = "HIS\\POSCONFIG";
            //const string keyName = userRoot + "\\" + subkey;

            //var data = new RegData()
            //{
            //    pos_name = textBox_name.Text,
            //    pos_ip = TextBox_IP.Text,
            //    pos_port = Textbox_port.Text
            //};

            //string data_json = JsonConvert.SerializeObject(data);
            //try
            //{
            //    Registry.SetValue(keyName, comboBox1.Text, data_json);
            //    MessageBox.Show("ذخیره اطلاعات انجام شد");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"خطا: {ex.Message}");
            //}
            pos_name = textBox_name.Text;
            pos_ip = TextBox_IP.Text;
            pos_port = Textbox_port.Text;
            pos_model = comboBox1.Text;


            Close();
        }

        private void Form_config_Load(object sender, EventArgs e)
        {
            //MessageBox.Show(index_data.ToString());
            //var a = read_registry(comboBox1.Text);
            //register_json_reader(a);
            if (editdata == true)
            {
                textBox_name.Text = pos_name;
                TextBox_IP.Text = pos_ip;
                Textbox_port.Text = pos_port;
                comboBox1.Text = pos_model;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (TextBox_IP.Text.Trim() == "" || Textbox_port.Text.Trim() == "")
            {
                MessageBox.Show("مقادری نباید خالی باشد", "مقادری آیپی و پورت", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            string IP = TextBox_IP.Text;
            string PORT = Textbox_port.Text;
            string AMOUNT = textBox1.Text;



            string selectedProvider = comboBox1.Text;
            if (selectedProvider == "fanava" || selectedProvider == "omidpay" || selectedProvider == "behpardakht")
            {
                Form_configpos frm = new Form_configpos();
                string res = frm.Fn_send_to_POS(selectedProvider, AMOUNT, IP, int.Parse(PORT), "batchnrTEST", "sanadyearTEST", "sandoghnrTEST");
                MessageBox.Show(res);
            }
            else
            {
                MessageBox.Show("سرویس انتخاب‌شده نامعتبر و یا غیر فعال است");
            }




        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var a = read_registry(comboBox1.Text);
            register_json_reader(a);

        }
        private void register_json_reader(string json_data)
        {


            //var a =JsonConvert.DeserializeObject<RegData>(json_data);
            ////MessageBox.Show(a.pos_ip);

            //TextBox_IP.Text = a.pos_ip ?? "";
            //Textbox_port.Text = a.pos_port ?? "";
            //textBox_name.Text = a.pos_name ?? "";//check null
        }
        private string read_registry(string valuename)
        {
            //try
            //{
            //    const string userRoot = "HKEY_CURRENT_USER";
            //    const string subkey = "HIS\\POSCONFIG";
            //    const string keyName = userRoot + "\\" + subkey;
            //    var a = Registry.GetValue(keyName, valuename, "{'pos_name':'DeviceName','pos_ip':'127.0.0.1','pos_port':'1024'}");
            //    return (a.ToString() ?? "");
            //}
            //catch (Exception)
            //{

            //    throw;
            //}
            return "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // ایجاد یک شیء ProcessStartInfo برای تنظیمات اجرای فرآیند CMD
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",                // اجرای CMD
                Arguments = "/C ping " + TextBox_IP.Text + " -t ", // دستور ping با آرگومان
                RedirectStandardOutput = false,      // جلوگیری از ریدایرکت خروجی
                UseShellExecute = true,              // برای نمایش پنجره CMD
                CreateNoWindow = false               // پنجره CMD را نشان بده
            };

            // شروع فرآیند
            Process process = Process.Start(processInfo);
        }

    }
    public class RegData
    {
        public string pos_ip { get; set; }
        public string pos_port { get; set; }
        public string pos_name { get; set; }

    }
}
