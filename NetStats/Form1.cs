using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;
using System.Timers;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Win32;
using Chart;
namespace Chart
{
    public partial class NETwork : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
         );

        DirectoryInfo dir = Directory.CreateDirectory("C:\\Users\\" + Environment.UserName + "\\NETwork");
        String r = "C:\\Users\\" + Environment.UserName + "\\NETwork\\setting.txt";
        int z = 0; bool log;
        StreamWriter sw = new StreamWriter("C:\\Users\\" + Environment.UserName + "\\NETwork\\speed.csv", true);
        
        public NETwork()
        {
            InitializeComponent();
            StreamWriter yu = new StreamWriter(r, true);
            yu.Close();
            StreamReader sr = new StreamReader(r, true);
            string min = sr.ReadLine();
            sr.Close();
            label7.Width = 0;
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0,0,Width,Height,5,5));
            if (min == "true")
            {
                this.WindowState = FormWindowState.Minimized;
                sr.Close();
            }
            notifyIcon1.Visible = true;
            notifyIcon1.MouseClick += delegate
            {
                this.WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
            };
            ShowInTaskbar = false;
            net();
        }

        long down = 0, upload = 0;
        //
        int d = 0;
        double unid = 0.000, uniu = 0.000; 
        long[] dspd = new long[21];
        long[] uspd = new long[21];

        private void button1_Click(object sender, EventArgs e)
        {
            sw.Close();
            this.Close();
        }

        protected override void WndProc(ref Message m)//move the form
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }

            base.WndProc(ref m);
        }

        public void net()
        {
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 20;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            z = ping();
            //MessageBox.Show(z.ToString()); 
            System.Windows.Forms.Timer tm = new System.Windows.Forms.Timer();
            tm.Tick += new EventHandler(OnTimedEvent);
            tm.Interval = 1000;
            tm.Start();
        }

        void OnTimedEvent(Object myObject, EventArgs myEventArgs)
        {
            int z1=0;
            foreach(NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                z1++;
                if (z1 == z)
                {
                    if (d < 20)
                        d++;
                    label7.Text = ni.Description;
                    long y1 = 0, y2 = 0;
                    y1 = ni.GetIPv4Statistics().BytesReceived; y1 = y1 / 1024;
                    y2 = ni.GetIPv4Statistics().BytesSent; y2 = y2 / 1024;

                    for (int y = 0; y < d; y++)
                    {
                        dspd[y + 1] = dspd[y];
                        uspd[y + 1] = uspd[y];
                    }
                    if (d != 1)
                    {
                        dspd[0] = y1 - down;
                        uspd[0] = y2 - upload;
                    }
                    else
                    {
                        dspd[0] = 0;
                        uspd[0] = 0;
                    }
                    down = y1; upload = y2;
                    chart1.Series["DLSpeed"].Points.Add(dspd[0]);
                    chart1.Series["ULSpeed"].Points.Add(uspd[0]);
                    label9.Text = "Download Speed : " + dspd[0].ToString() + " kB/s";
                    label10.Text = "Upload Speed : " + uspd[0].ToString() + " kB/s";
                    if (d > 19)
                    {
                        chart1.Series["DLSpeed"].Points.RemoveAt(0);
                        chart1.Series["ULSpeed"].Points.RemoveAt(0);
                        chart1.ResetAutoValues();
                    }
                    unid = unid + dspd[0] / 1024.0;
                    uniu = uniu + uspd[0] / 1024.0;
                    label2.Text = string.Format("{0:0.000}", unid);
                    label4.Text = string.Format("{0:0.000}", uniu);
                    if (log == true)
                    {
                        //StreamWriter sw = new StreamWriter(r, true);
                        sw.Write(DateTime.Now.ToString("dd/MM/yy") + ',' + DateTime.Now.ToString("hh:mm:ss") + ',' + dspd[0].ToString() + ',' + uspd[0].ToString() + "\r\n");
                        sw.Flush();
                    }
                }
            }
        }
            
        public int ping()
        {
            //MessageBox.Show("sgsgsg");
            int z = 1;
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                //MessageBox.Show(ni.Description+"  "+ ni.OperationalStatus);
                if (ni.OperationalStatus == OperationalStatus.Up && ni.Description!="Microsoft Hosted Network Virtual Adapter")
                    return z;
                else
                    z++;
            } return 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
          
            string caption = "";
            DialogResult dialogResult = MessageBox.Show("Start App at Startup ?", caption, MessageBoxButtons.YesNoCancel);
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (dialogResult == DialogResult.Yes)
            {
                rk.SetValue("NETwork", System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                if (dialogResult == DialogResult.No)
                    rk.DeleteValue("NETwork", false);
                else
                { }
            }

            DialogResult dialogResult2 = MessageBox.Show("Start App Minimized ?", caption, MessageBoxButtons.YesNoCancel);
            if (dialogResult2 == DialogResult.Yes)
            {
                System.IO.File.WriteAllText(r, String.Empty);
                StreamWriter fg = new StreamWriter(r, true);
                fg.Write("true");
                fg.Close();
            }
            else
            {
                if (dialogResult2 == DialogResult.No)
                {
                    System.IO.File.WriteAllText(r, String.Empty);
                    StreamWriter fg = new StreamWriter(r, true);
                    fg.Write("false");
                    fg.Close();
                }
                else
                { }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                System.IO.File.WriteAllText(r, String.Empty);
                log = true;
                StreamWriter fg = new StreamWriter(r, true);
                fg.Write("true");
                fg.Close();
            }
            else
            {
                System.IO.File.WriteAllText(r, String.Empty);
                StreamWriter fg = new StreamWriter(r, true);
                log = false;
                fg.Write("false");
                fg.Close();
            }
        }
    }
}
