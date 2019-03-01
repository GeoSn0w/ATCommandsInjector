using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATCommandInjector
{
    public partial class WantSteak : Form
    {
        DateTime dateTime;
        String comPort;
        int baudRate;
        SerialPort devicePort;
        bool configDone = false;
        bool shouldLog = false;
        
        public WantSteak()
        {
            InitializeComponent();
        }

        private void WantSteak_Load(object sender, EventArgs e)
        {
            shouldLog = true;
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {
                com_port_unstripped_list.Items.Add(s);
            }

            SerialConsole.Text += "AT Commands Injector by GeoSn0w (@FCE365)\n\nReady!\n"; 
      
        }

        private void console_clear_btn_Click(object sender, EventArgs e)
        {
            SerialConsole.Clear();
            SerialConsole.Text += "AT Commands Injector by GeoSn0w (@FCE365)\n\nReady!\n";
        }

        private void save_com_and_open_Click(object sender, EventArgs e)
        {
         
            if (!configDone)
            {
                // <-- Variable populating logic -->
                // Yee, sanity checks.
                if (com_port_unstripped_list.SelectedItem == null)
                {
                    logStamper();
                    SerialConsole.Text += "[!] Invalid COM Port selected. Please select a valid port from the list.\n";
                    return;
                }
                if (baudField.Text == "")
                {
                    MessageBox.Show("The baud value cannot be empty!", "AT Commands Injector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                baudRate = int.Parse(baudField.Text);
                // <-- Port Opening logic -->
                // Get the com port from the populated list and open it.
                comPort = com_port_unstripped_list.SelectedItem.ToString();
                devicePort = new SerialPort(comPort, baudRate, Parity.None, 8);
                if (rts_toggle.Toggled == true) devicePort.RtsEnable = true;
                if (dts_toggle.Toggled == true) devicePort.DtrEnable = true;
                devicePort.WriteBufferSize = 3000;
                try
                {
                    devicePort.Open();
                }
                catch (Exception ex)
                {
                    logStamper();
                    SerialConsole.Text += "[!] An error has occured during the port opening:\n" + ex.Message;
                }
                if (devicePort.IsOpen)
                {
                    if (is_3g_modem.Toggled)
                    {
                        detect3GModemDetails();
                    }
                    logStamper();
                    SerialConsole.Text += "[+] Successfully opened COM Port: " + comPort + "\n";
                    configDone = true;
                    save_com_and_open.Text = "Close Port";
                }
               
            } else if (configDone)
            {
                try
                {
                    devicePort.Close();
                    if (!devicePort.IsOpen)
                    {
                        logStamper();
                        SerialConsole.Text += "[+] Successfully closed port: " + comPort + "\n";
                        save_com_and_open.Text = "Save & Open Port ";
                        configDone = false;
                    }
                }
                catch (Exception wtf)
                {
                    logStamper();
                    SerialConsole.Text += "[!] An error has occured during the port opening:\n" + wtf.Message + "\n";
                }
            }
        }

        private void baudField_TextChanged(object sender, EventArgs e)
        {
            int preChecks;
            if (!int.TryParse(baudField.Text, out preChecks) && baudField.Text != "")
            {
                MessageBox.Show("The baud can only be a number. Please do not type symbols or letters.", "AT Commands Injector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                baudField.Text = baudField.Text.Remove(baudField.Text.Length - 1);
                return;
            }
        }
        //"AT+CGSN" + "\r"
        private void SendCMD_Click(object sender, EventArgs e)
        {
            if (!configDone) {
                logStamper();
                SerialConsole.Text += "[!] No open ports! Please open a port to a serial device first.\n";  // Bail out
                return;
            }
            devicePort.Write(at_cmd_staging.Text);
            devicePort.Write("\r");
            Thread.Sleep(500);
            logStamper();
            SerialConsole.Text += devicePort.ReadExisting();
        }

        private void BarButton8_Click(object sender, EventArgs e)
        {
            at_cmd_staging.Text = BarButton8.Text;
        }

        private void BarButton1_Click(object sender, EventArgs e)
        {
            at_cmd_staging.Text = BarButton1.Text;
        }

        private void BarButton2_Click(object sender, EventArgs e)
        {
            at_cmd_staging.Text = BarButton2.Text;
        }

        private void BarButton3_Click(object sender, EventArgs e)
        {
            at_cmd_staging.Text = BarButton3.Text;
        }

        private void BarButton4_Click(object sender, EventArgs e)
        {
            at_cmd_staging.Text = BarButton4.Text;
        }

        private void BarButton5_Click(object sender, EventArgs e)
        {
            at_cmd_staging.Text = BarButton5.Text;
        }

        private void BarButton6_Click(object sender, EventArgs e)
        {
            at_cmd_staging.Text = BarButton6.Text;
        }

        private void BarButton7_Click(object sender, EventArgs e)
        {
            at_cmd_staging.Text = BarButton7.Text;
        }
        private void logStamper()
        {
            if (shouldLog)
            {
                dateTime = DateTime.Now;
                SerialConsole.Text += dateTime + " ";
            }
            // else bail out
        }

        private void log_time_date_toggle_ToggledChanged()
        {
            logTimeDateLogic();
        }
        private void logTimeDateLogic()
        {
            if (log_time_date_toggle.Toggled)
            {
                shouldLog = true;
                logStamper();
                SerialConsole.Text += "[i] Will log with Date and Time :-)\n";
            }
            else
            {
                shouldLog = false;
                SerialConsole.Text += "[i] Will no longer log with Date and Time :-)\n";
            }
        }
        private void detect3GModemDetails()
        {
            SerialConsole.Text += "[i] Detecting device, hold on...\n";
            var deviceModel = "";
            var deviceIMEI = "";
            var deviceManuf = "";
            var herpDerp = new[]{
                         new{This="AT+CGMM", Becomes=""},
                         new{This="AT+CGMI", Becomes=""},
                         new{This="AT+CGSN", Becomes=""},
                         new{This="OK", Becomes = Environment.NewLine},
                         new{This=" ", Becomes=""},

                     };
            devicePort.Write("AT+CGMI \r");
            Thread.Sleep(500);
            deviceManuf = devicePort.ReadExisting();
            foreach (var set in herpDerp)
            {
                deviceManuf = deviceManuf.Replace(set.This, set.Becomes);
            }
            deviceManuf = deviceManuf.Replace(Environment.NewLine + Environment.NewLine, "");
            // Model
            devicePort.Write("AT+CGMM \r");
            Thread.Sleep(500);
            deviceModel = devicePort.ReadExisting();
            foreach (var set in herpDerp)
            {
                deviceModel = deviceModel.Replace(set.This, set.Becomes);
            }
            deviceModel = deviceModel.Replace(Environment.NewLine + Environment.NewLine, "");

            // IMEI
            devicePort.Write("AT+CGSN \r");
            Thread.Sleep(500);
            deviceIMEI = devicePort.ReadExisting();
            foreach (var set in herpDerp)
            {
                deviceIMEI = deviceIMEI.Replace(set.This, set.Becomes);
            }
            deviceIMEI = deviceIMEI.Replace(Environment.NewLine + Environment.NewLine, "");
            SerialConsole.Text += "    Detected device:" + deviceManuf + "\n" + "    Device model:" + deviceModel + "\n" + "    Device IMEI:" + deviceIMEI + "\n";
            return;
        }
    }
}