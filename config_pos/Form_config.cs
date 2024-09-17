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

namespace config_pos
{
    public partial class Form_config : Form
    {
        public string pos_name;
        public string pos_ip;
        public string pos_port;
        public string pos_model;
        public bool savedata=false;
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
            if (TextBox_IP.Text.Trim()=="" || Textbox_port.Text.Trim() == "")
            {
                MessageBox.Show("مقادری نباید خالی باشد","مقادری آیپی و پورت",MessageBoxButtons.OK,MessageBoxIcon.Hand);
                return;
            }
            if (comboBox1.Text == "fanava")
            {
                Form_configpos frm = new Form_configpos();
                string res = frm.Fn_send_to_POS("fanava",textBox1.Text, TextBox_IP.Text, int.Parse(Textbox_port.Text));
                MessageBox.Show(res);

            } else if (comboBox1.Text=="omidpay") {


                Form_configpos frm = new Form_configpos();
                string res = frm.Fn_send_to_POS("omidpay",textBox1.Text, TextBox_IP.Text, int.Parse(Textbox_port.Text));
                MessageBox.Show(res);

                //OmidPayPcPosClass omid =new OmidPayPcPosClass();
                //ResponseJson res;
                //res=omid.DoTcpTransaction(TextBox_IP.Text, int.Parse(Textbox_port.Text), textBox1.Text);
            }
            else if (comboBox1.Text == "behpardakht")
            {
                Transaction.Connection Connect = new Transaction.Connection();
                Result retCode = new Result();
                Connect.CommunicationType = "TCP/IP";
                Connect.POSPC_TCPCOMMU_SocketRecTimeout = 60000;
                Connect.POS_PORTtcp = Convert.ToInt16(Textbox_port.Text);//1024
                //Connect.POS_IP ="127.0.0.1";
                Connect.POS_IP = TextBox_IP.Text;//"192.168.1.241";

                //POS_PC_v3.Result.return_codes retCode = POS_PC_v3.Result.return_codes.ERR_POS_PC_OTHER;
                Transaction TXN = new Transaction(Connect);
                retCode = TXN.Debits_Goods_And_Service("1", "1", textBox1.Text, "500", "hello", "good");

                //POS_PC_v3.Transaction.return_codes retCode = Transaction.return_codes.ERR_POS_PC_OTHER;
                //POS_PC_v3.Transaction TXN = new POS_PC_v3.Transaction(Connect);
                /* Debit */
                //            retCode = TXN.Debits_Goods_And_Service(RequestID, "1", Amount, PayerID, MerchantMsg,
                //            MerchantadditionalData);
                //            OR /* Debit with AutoSettle */
                //            retCode = TXN.Debits_Goods_And_Service(RequestID, "1", Amount, PayerID, MerchantMsg,
                //            MerchantadditionalData, false);
                //            OR /* BillPayment */
                //            retCode = TXN.Bill_Payment_Service(RequestID, "1", strBillId, strPayCode, strAmount, strMerchantMsg,
                //            strMerchantAddit);
                //            OR /* Payment */
                //            retCode = TXN.Payment(RequestID, "1", strAmount, strPayerId, strAcountId, strMerchantAddit);
                //            OR /*MultiPayment */
                //            retCode = TXN.MultiPayment(RequestID, "1", strTotalAmount, RequestList, PrintDetail,
                //            strMerchantadditionalData)
                //OR /* Inquery */
                //retCode = TXN.Inquery_Service(RequestID, "1", MerchantadditionalData);
                //            OR /* Last_Inquery */
                //            retCode = TXN.Last_Inquery_Service("1", MerchantadditionalData);
                //            OR /* Get_Card */
                //            retCode = TXN.Get_Card_Service("20", "1", MerchantadditionalData);
                //            OR /* Get_Card_Debits */
                //            retCode = TXN.Get_Card_Debits_Goods_And_Service(RequestID, "1", Amount, PayerID, MerchantMsg,
                //            MerchantadditionalData);


                POS_PC_v3.Result.return_codes returncode = (POS_PC_v3.Result.return_codes)retCode.ReturnCode;
                MessageBox.Show(returncode.ToString());
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var a=read_registry(comboBox1.Text);
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
        private string read_registry(string valuename) {
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
    }
    public class RegData
    {
       public string pos_ip { get; set; }
        public string pos_port { get; set; } 
        public string pos_name { get; set; }

    }
}
