using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace InstallScanServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WiaDotNet.WiaManager mgr = new WiaDotNet.WiaManager();
            for (int i = 0; i < mgr.Devices.Count; i++)
            {
                selectscaner.Items.Add(mgr.Devices[i].Name);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string autchsql = "";
            if (checkBox1.Checked)
            {
                autchsql = "Integrated Security=True";
            }
            else
            {

                autchsql = string.Format("User ID={0};Password={1};", login.Text, password.Text);
            }

            System.Data.SqlClient.SqlConnection sc = new System.Data.SqlClient.SqlConnection(string.Format("Data Source={0};Initial Catalog={1};  {2} ", serverinst.Text, textBox5.Text, autchsql));
            svDataSetTableAdapters.ScanerTableAdapter scan = new svDataSetTableAdapters.ScanerTableAdapter();
            scan.Connection = sc;
            scan.Insert(textBox1.Text, selectscaner.SelectedIndex, "Color-Grayscale-Text", textBox2.Text, textBox3.Text, int.Parse(textBox4.Text), Environment.MachineName);
            StreamWriter wr = new StreamWriter(Environment.GetEnvironmentVariable("SYSTEMDRIVE") + "\\ScanServerSetting\\Setting.scan",false);
            wr.WriteLine(serverinst.Text);
                wr.WriteLine(textBox5.Text);
                wr.Close();
            Close();
        }
    }
}
