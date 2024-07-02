using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using POS_PC_v3;
using System.ServiceProcess;
using System.IO;
using System.Security.Principal;
using Newtonsoft.Json;

namespace config_pos
{
    public partial class Form_configpos : Form
    {
        string serviceName = "vahidservice";
        public Form_configpos()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           

        }
        public bool IsUserAdmin()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(currentUser);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void ControlService(string serviceName, ServiceControllerStatus desiredStatus)
        {
            try
            {
                ServiceController sc = new ServiceController(serviceName);

                if (desiredStatus == ServiceControllerStatus.Running)
                {
                    if (sc.Status != ServiceControllerStatus.Running)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        //MessageBox.Show($"سرویس '{serviceName}' با موفقیت شروع به کار کرد.");
                    }
                    else
                    {
                        //MessageBox.Show($"سرویس '{serviceName}' در حال حاضر در حال اجرا است.");
                    }
                }
                else if (desiredStatus == ServiceControllerStatus.Stopped)
                {
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        //MessageBox.Show($"سرویس '{serviceName}' با موفقیت متوقف شد.");
                    }
                    else
                    {
                        //MessageBox.Show($"سرویس '{serviceName}' در حال حاضر متوقف است.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا: {ex.Message}");
            }
        }

        private void Form_configpos_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            //configpos.exe behpardakht 2000
            if (args.Length > 1)
            {
                MessageBox.Show(args.Length.ToString());
            }
            btn_start_Click_1(sender, e);
            if (IsUserAdmin() == true)
            {
                this.Text+=" : Administrator";
            }
            else
            {
                MessageBox.Show("جهت اجرای صحیح برنامه نباز به دسترسی سطح کاربر مدیر می باشد");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            ControlService(serviceName, ServiceControllerStatus.Running);
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            ControlService(serviceName, ServiceControllerStatus.Stopped);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController(serviceName);
            string str = (sc.Status.ToString() == "Running") ?"در حال اجرا است":"متوقف است";
            label4.Text="وضعیت سرویس : " + str;
        }

        private void btn_start_Click_1(object sender, EventArgs e)
        {
            listView1.Clear();

            listView1.View = View.Details;
            listView1.Columns.Add("عنوان دستگاه", 150);
            listView1.Columns.Add("مدل", 100);
            listView1.Columns.Add("IP", 100);
            listView1.Columns.Add("Port", 80);

            // افزودن ImageList به ListView
            listView1.SmallImageList = imageList1;

            //AddRandomItemsToListView();



            // مسیر فایل را مشخص کنید
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");

            // بررسی وجود فایل
            if (File.Exists(filePath))
            {
                // خواندن محتویات فایل
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    // خواندن هر خط از فایل
                    while ((line = sr.ReadLine()) != null)
                    {
                        // تقسیم خط به عباراتی که با کاما جدا شده‌اند
                        string[] parts = line.Split(',');

                        // نمایش هر عبارت
                        foreach (string part in parts)
                        {
                            //MessageBox.Show(part);
                        }
                        ListViewItem item = new ListViewItem(parts[0]);
                        item.SubItems.Add(parts[1]);
                        item.SubItems.Add(parts[2]);
                        item.SubItems.Add(parts[3]);
                        item.ImageIndex = 0;

                        listView1.Items.Add(item);


                    }
                }
            }
            else
            {
                Console.WriteLine("فایل یافت نشد.");
            }





        }
      

        private void button4_Click(object sender, EventArgs e)
        {
            Form_config frm = new Form_config();
            //save data
            frm.ShowDialog();
            
            if (frm.pos_name != null)
            {
                ListViewItem item = new ListViewItem(frm.pos_name);
                item.SubItems.Add(frm.pos_model);
                item.SubItems.Add(frm.pos_ip);
                item.SubItems.Add(frm.pos_port);
                item.ImageIndex = 0;

                listView1.Items.Add(item);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                // باز کردن یک فایل برای نوشتن
                using (StreamWriter writer = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ,"config.txt")))
                {
                    // پیمایش تمام سطرهای ListView و نوشتن هر سطر به فایل
                    foreach (ListViewItem item in listView1.Items)
                    {
                        for (int i = 0; i < item.SubItems.Count; i++)
                        {
                            // اضافه کردن مقدار هر ستون به فایل، با استفاده از فاصله به عنوان جداکننده
                            writer.Write(item.SubItems[i].Text);
                            if (i < item.SubItems.Count - 1)
                                writer.Write(","); // اضافه کردن فاصله بین ستون‌ها
                        }
                        writer.WriteLine(); // پایان هر سطر
                    }
                }

                MessageBox.Show("Items saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        

        private void button5_Click(object sender, EventArgs e)
        {
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            Form_config frm=new Form_config();
            frm.editdata = true;
            ListViewItem item = listView1.SelectedItems[0];
           
            frm.pos_name = item.SubItems[0].Text;
            frm.pos_model = item.SubItems[1].Text;
            frm.pos_ip = item.SubItems[2].Text;
            frm.pos_port = item.SubItems[3].Text;

            frm.ShowDialog();

            listView1.Items[listView1.SelectedIndices[0]].SubItems[0].Text = frm.pos_name;
            listView1.Items[listView1.SelectedIndices[0]].SubItems[1].Text = frm.pos_model;
            listView1.Items[listView1.SelectedIndices[0]].SubItems[2].Text = frm.pos_ip;
            listView1.Items[listView1.SelectedIndices[0]].SubItems[3].Text = frm.pos_port;

//listView.Items[rowIndex].SubItems[columnIndex].Text = newValue;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedIndices.Count > 0)
            {
                listView1.Items[listView1.SelectedIndices[0]].Remove();
            }
        }
    }
}
