using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using EZ_B;
using EZ_Builder;
using EZ_Builder.Config.Sub;

namespace LSS {

  public partial class FormConfig : Form {

    PluginV1      _cf = new PluginV1();
    ServoCommands _servoCmds = new ServoCommands();

    public FormConfig() {

      InitializeComponent();

      cbChangeIDNew.Items.Clear();
      cbTestServoPort.Items.Clear();

      for (int x = (int)Servo.ServoPortEnum.V0; x < (int)Servo.ServoPortEnum.V99; x++) {

        Servo.ServoPortEnum item = (Servo.ServoPortEnum)x;

        cbChangeIDNew.Items.Add(item);
        cbTestServoPort.Items.Add(item);
      }

      cbBaudRates.Items.Clear();
      foreach (ServoCommands.BAUD_RATES baud in Enum.GetValues(typeof(ServoCommands.BAUD_RATES)))
        cbBaudRates.Items.Add(baud);

      cbBaudRates.SelectedItem = ServoCommands.BAUD_RATES.BAUD_115200;
      cbChangeIDNew.SelectedItem = Servo.ServoPortEnum.V1;
      cbTestServoPort.SelectedItem = Servo.ServoPortEnum.V0;

      ucTestServoNum.Value = Servo.SERVO_CENTER;
    }

    private void Form_FormClosing(object sender, FormClosingEventArgs e) {

    }

    public void SetConfiguration(PluginV1 cf) {

      _cf = cf;

      ConfigServos configServos = (ConfigServos)cf.GetCustomObjectV2(typeof(ConfigServos));

      flowLayoutPanel1.SuspendLayout();

      foreach (EZ_B.Servo.ServoPortEnum port in Enum.GetValues(typeof(EZ_B.Servo.ServoPortEnum)))
        if (port >= Servo.ServoPortEnum.V0 && port <= Servo.ServoPortEnum.V99) {

          UCServoPort ucServoPort = new UCServoPort();
          ucServoPort.Name = port.ToString();
          ucServoPort.SetPort = port;

          ConfigServosDetail configServoDetail = configServos.GetPort(port);

          if (configServoDetail != null) {

            ucServoPort.SetEnabled = true;
          }

          flowLayoutPanel1.Controls.Add(ucServoPort);
        }

      flowLayoutPanel1.ResumeLayout(true);

      tbBaudRate.Text = cf.STORAGE.GetKey(ConfigTitles.BAUD_RATE, (int)ServoCommands.BAUD_RATES.BAUD_115200).ToString();

      var portType = (ConfigTitles.PortTypeEnum)cf.STORAGE.GetKey(ConfigTitles.PORT_TYPE, ConfigTitles.PortTypeEnum.UART0);

      if (portType == ConfigTitles.PortTypeEnum.UART0)
        rbUARTPort0.Checked = true;
      else if (portType == ConfigTitles.PortTypeEnum.UART1)
        rbUARTPort1.Checked = true;
      else if (portType == ConfigTitles.PortTypeEnum.UART2)
        rbUARTPort2.Checked = true;
      else
        rbDigitalPort.Checked = true;

      ucPortButton1.SetConfig((Digital.DigitalPortEnum)cf.STORAGE.GetKey(ConfigTitles.DIGITAL_PORT, Digital.DigitalPortEnum.D0));
    }

    public PluginV1 GetConfiguration() {

      _cf.STORAGE.AddOrUpdate(ConfigTitles.BAUD_RATE, Convert.ToUInt32(tbBaudRate.Text));

      if (rbUARTPort0.Checked) {

        _cf.STORAGE.AddOrUpdate(ConfigTitles.PORT_TYPE, ConfigTitles.PortTypeEnum.UART0);
      } else if (rbUARTPort1.Checked) {

        _cf.STORAGE.AddOrUpdate(ConfigTitles.PORT_TYPE, ConfigTitles.PortTypeEnum.UART1);
      } else if (rbUARTPort2.Checked) {

        _cf.STORAGE.AddOrUpdate(ConfigTitles.PORT_TYPE, ConfigTitles.PortTypeEnum.UART2);
      } else {

        _cf.STORAGE.AddOrUpdate(ConfigTitles.PORT_TYPE, ConfigTitles.PortTypeEnum.DigitalPort);
        _cf.STORAGE.AddOrUpdate(ConfigTitles.DIGITAL_PORT, ucPortButton1.PortDigital);
      }

      List<ConfigServosDetail> configServosDetails = new List<ConfigServosDetail>();

      foreach (UCServoPort ucServoPort in flowLayoutPanel1.Controls)
        if (ucServoPort.SetEnabled)
          configServosDetails.Add(new ConfigServosDetail() {
            Port = ucServoPort.SetPort,
          });

      ConfigServos configServos = new ConfigServos();
      configServos.Servos = configServosDetails.ToArray();

      _cf.SetCustomObjectV2(configServos);

      return _cf;
    }

    int getUARTIndex() {

      if (rbUARTPort0.Checked)
        return 0;
      else if (rbUARTPort1.Checked)
        return 1;
      else
        return 2;
    }

    void serialWrite(byte[] data) {

      if (rbDigitalPort.Checked) {

        try {

          var baudRate = (Uart.BAUD_RATE_ENUM)Enum.Parse(typeof(Uart.BAUD_RATE_ENUM), "Baud_" + tbBaudRate.Text);

          EZBManager.EZBs[0].Uart.SendSerial(ucPortButton1.PortDigital, baudRate, data);
        } catch {

          MessageBox.Show("Invalid baud rate for low speed tx port. Valid baud rates are 4800, 9600, 19200, 38400, 57600, 115200");
        }
      } else {

        EZBManager.EZBs[0].Uart.UARTExpansionWrite(getUARTIndex(), data);
      }
    }

    private void ucTestServoNum_OnChange(int value) {

      EZBManager.EZBs[0].Servo.SetServoPosition((Servo.ServoPortEnum)cbTestServoPort.SelectedItem, value);
    }

    private void btnChangeID_Click(object sender, EventArgs e) {

      try {

        if (MessageBox.Show("Are you sure?", "Change ID", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
          return;

        var fromId = Utility.GetIdFromServo((Servo.ServoPortEnum)cbTestServoPort.SelectedItem);
        var toId = Utility.GetIdFromServo((Servo.ServoPortEnum)cbChangeIDNew.SelectedItem);

        serialWrite(_servoCmds.ChangeID(fromId, toId));

        MessageBox.Show("Done");
      } catch (Exception ex) {

        MessageBox.Show(ex.Message);
      }
    }

    private void btnLEDOn_Click(object sender, EventArgs e) {

      try {

        var id = Utility.GetIdFromServo((Servo.ServoPortEnum)cbTestServoPort.SelectedItem);

        serialWrite(_servoCmds.LED(id, ServoCommands.LED_COLORS.White));
      } catch (Exception ex) {

        MessageBox.Show(ex.Message);
      }
    }

    private void btnLEDOff_Click(object sender, EventArgs e) {

      try {

        var id = Utility.GetIdFromServo((Servo.ServoPortEnum)cbTestServoPort.SelectedItem);

        serialWrite(_servoCmds.LED(id, ServoCommands.LED_COLORS.Off));
      } catch (Exception ex) {

        MessageBox.Show(ex.Message);
      }
    }

    private void button1_Click(object sender, EventArgs e) {

      try {

        if (cbChangeIDNew.SelectedIndex < cbChangeIDNew.Items.Count - 1) {

          cbChangeIDNew.SelectedIndex++;

          cbTestServoPort.SelectedItem = cbChangeIDNew.SelectedItem;
        }
      } catch (Exception ex) {

        MessageBox.Show(ex.Message);
      }
    }

    private void btnSave_Click(object sender, EventArgs e) {

      if (!EZ_Builder.Common.IsInteger(tbBaudRate.Text)) {

        MessageBox.Show("Baud rate must be an integer number");

        return;
      }

      if (rbDigitalPort.Checked)
        try {

          Enum.Parse(typeof(Uart.BAUD_RATE_ENUM), "Baud_" + tbBaudRate.Text);

        } catch {

          MessageBox.Show("Valid baudrates for TX low speed serial mode are: 4800, 9600, 19200, 38400, 57600, 115200");

          return;
        }

      DialogResult = System.Windows.Forms.DialogResult.OK;
    }

    private void btnCancel_Click(object sender, EventArgs e) {

      DialogResult = System.Windows.Forms.DialogResult.Cancel;
    }

    private void button4_Click(object sender, EventArgs e) {

      if (!EZ_Builder.Common.IsInteger(tbBaudRate.Text)) {

        MessageBox.Show("Baud rate must be an integer number");

        return;
      }

      if (!EZBManager.EZBs[0].IsConnected) {

        MessageBox.Show("You must be connected to an EZ-B");

        return;
      }

      if (!rbDigitalPort.Checked)
        EZBManager.EZBs[0].Uart.UARTExpansionInit(getUARTIndex(), Convert.ToUInt32(tbBaudRate.Text));
    }

    private void btnChangeBaud_Click(object sender, EventArgs e) {

      try {

        if (MessageBox.Show(string.Format("Change baud rate on port {0}?", cbTestServoPort.SelectedItem), "Are you sure?", MessageBoxButtons.YesNo) != DialogResult.Yes)
          return;

        var baud = (ServoCommands.BAUD_RATES)cbBaudRates.SelectedItem;
        var id = Utility.GetIdFromServo((Servo.ServoPortEnum)cbTestServoPort.SelectedItem);

        serialWrite(_servoCmds.ChangeBaudRate(id, baud));

        MessageBox.Show("The baud rate has chnged. To communicate with this servo, you must cycle the power on the flashing servo and change the baud rate in the first page of this configuration menu", "Done");

      } catch (Exception ex) {

        MessageBox.Show(ex.Message);
      }
    }

    private void button6_Click(object sender, EventArgs e) {

      try {

        var id = Utility.GetIdFromServo((Servo.ServoPortEnum)cbTestServoPort.SelectedItem);

        serialWrite(_servoCmds.ReleaseServo(id));
      } catch (Exception ex) {

        MessageBox.Show(ex.Message);
      }
    }

    private void button8_Click(object sender, EventArgs e) {

      try {

        if (rbDigitalPort.Checked)
          throw new Exception("This feature is only available when using the hardware uart");

        EZBManager.EZBs[0].Uart.UARTExpansionInit(getUARTIndex(), Convert.ToUInt32(tbBaudRate.Text));

        var id = Utility.GetIdFromServo((Servo.ServoPortEnum)cbTestServoPort.SelectedItem);

        serialWrite(_servoCmds.SendPing(id));

        System.Threading.Thread.Sleep(500);

        var ret = EZBManager.EZBs[0].Uart.UARTExpansionReadAvailable(getUARTIndex());

        if (ret.Length <= 4)
          throw new Exception("Servo not responding");

        Invokers.SetAppendText(tbLog, true, "{0} Responded to ping", (Servo.ServoPortEnum)cbTestServoPort.SelectedItem);
      } catch (Exception ex) {

        Invokers.SetAppendText(tbLog, true, "{0} {1}", (Servo.ServoPortEnum)cbTestServoPort.SelectedItem, ex.Message);
      }
    }

    private void btnScanAndFind_Click(object sender, EventArgs e) {

      tbLog.Clear();

      tbLog.AppendText("Scanning all ports and servos...");
      tbLog.AppendText(Environment.NewLine);

      try {

        if (rbDigitalPort.Checked)
          throw new Exception("This feature is only available when using the hardware uart");

        EZBManager.EZBs[0].Uart.UARTExpansionInit(getUARTIndex(), Convert.ToUInt32(tbBaudRate.Text));

        {
          serialWrite(_servoCmds.SendPing(0xfe));

          System.Threading.Thread.Sleep(1000);

          var ret = EZBManager.EZBs[0].Uart.UARTExpansionReadAvailable(getUARTIndex());

          string mainStr = Encoding.ASCII.GetString(ret);

          var parsedList = mainStr.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

          foreach (var respStr in parsedList) {

            int parse1 = respStr.IndexOf("QID") + 3;

            var parsed = respStr.Substring(parse1);

            Invokers.SetAppendText(tbLog, true, "Found at ID {0}", parsed);
          }
        }

        Invokers.SetAppendText(tbLog, true, "Scan Completed");
      } catch (Exception ex) {

        Invokers.SetAppendText(tbLog, true, ex.ToString());
      }
    }
  }
}
