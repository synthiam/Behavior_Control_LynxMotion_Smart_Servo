using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EZ_B;
using EZ_Builder;
using System.Linq;
using System.Text;

namespace LSS {

  public partial class FormMain : EZ_Builder.UCForms.FormPluginMaster {

    // WIKI https://www.robotshop.com/info/wiki/lynxmotion/view/lynxmotion-smart-servo/

    ServoCommands _servoCmds = new ServoCommands();

    // This is a duplicate of the _cf.CustomObjectv2 for performance
    ConfigServos _servoConfig = new ConfigServos();

    public FormMain() {

      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e) {

      // Bind to the events for moving a servo and changing connection state
      EZBManager.EZBs[0].OnConnectionChange += FormMain_OnConnectionChange;
      EZBManager.EZBs[0].Servo.OnServoMove += Servo_OnServoMove;
      EZBManager.EZBs[0].Servo.OnServoRelease += Servo_OnServoRelease;
      EZBManager.EZBs[0].Servo.OnServoSpeed += Servo_OnServoSpeed;
      EZBManager.EZBs[0].Servo.OnServoGetPosition += Servo_OnServoGetPosition;

      ExpressionEvaluation.FunctionEval.AdditionalFunctionEvent += FunctionEval_AdditionalFunctionEvent;

      Invokers.SetAppendText(tbLog, true, "Connected Events");

      if (EZBManager.EZBs[0].IsConnected) {

        initUART();
      }
    }

    void FormMain_OnConnectionChange(bool isConnected) {

      // If the connection is established, send an initialization to the ez-b for the uart which we will be using
      if (isConnected) {

        initUART();
      }
    }

    private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {

      EZBManager.EZBs[0].OnConnectionChange -= FormMain_OnConnectionChange;
      EZBManager.EZBs[0].Servo.OnServoMove -= Servo_OnServoMove;
      EZBManager.EZBs[0].Servo.OnServoRelease -= Servo_OnServoRelease;
      EZBManager.EZBs[0].Servo.OnServoSpeed -= Servo_OnServoSpeed;
      EZBManager.EZBs[0].Servo.OnServoGetPosition -= Servo_OnServoGetPosition;
      ExpressionEvaluation.FunctionEval.AdditionalFunctionEvent -= FunctionEval_AdditionalFunctionEvent;
    }

    int getUARTIndex() {

      if (((ConfigTitles.PortTypeEnum)_cf.STORAGE[ConfigTitles.PORT_TYPE]) == ConfigTitles.PortTypeEnum.UART0)
        return 0;
      else
        return 1;
    }

    private void initUART() {

      if (((ConfigTitles.PortTypeEnum)_cf.STORAGE[ConfigTitles.PORT_TYPE]) == ConfigTitles.PortTypeEnum.DigitalPort)
        return;

      if (EZBManager.EZBs[0].IsConnected) {

        UInt32 baud = Convert.ToUInt32(_cf.STORAGE[ConfigTitles.BAUD_RATE]);

        int uart = getUARTIndex();

        Invokers.SetAppendText(tbLog, true, "Init UART #{0} @ {1}bps", uart, baud);

        EZBManager.EZBs[0].Uart.UARTExpansionInit(uart, baud);
      }
    }

    public override void SetConfiguration(EZ_Builder.Config.Sub.PluginV1 cf) {

      cf.STORAGE.AddIfNotExist(ConfigTitles.BAUD_RATE, 115200);

      cf.STORAGE.AddIfNotExist(ConfigTitles.PORT_TYPE, ConfigTitles.PortTypeEnum.UART1);

      cf.STORAGE.AddIfNotExist(ConfigTitles.DIGITAL_PORT, Digital.DigitalPortEnum.D0);

      // If the servo config is empty, assign a blank one
      if (cf._customObjectEncodedV2.Length == 0)
        cf.SetCustomObjectV2(_servoConfig);

      _servoConfig = (ConfigServos)cf.GetCustomObjectV2(typeof(ConfigServos));

      base.SetConfiguration(cf);
    }

    public override EZ_Builder.Config.Sub.PluginV1 GetConfiguration() {

      _cf.SetCustomObjectV2(_servoConfig);

      return base.GetConfiguration();
    }

    void serialWrite(byte[] data) {

      if (((ConfigTitles.PortTypeEnum)_cf.STORAGE[ConfigTitles.PORT_TYPE]) == ConfigTitles.PortTypeEnum.DigitalPort) {

        var baudRate = (Uart.BAUD_RATE_ENUM)Enum.Parse(typeof(Uart.BAUD_RATE_ENUM), "Baud_" + _cf.STORAGE[ConfigTitles.BAUD_RATE].ToString());

        EZBManager.EZBs[0].Uart.SendSerial((Digital.DigitalPortEnum)_cf.STORAGE[ConfigTitles.DIGITAL_PORT], baudRate, data);
      } else {

        EZBManager.EZBs[0].Uart.UARTExpansionWrite(getUARTIndex(), data);
      }
    }

    public override void SendCommand(string windowCommand, params string[] values) {

      if (windowCommand.Equals("setled", StringComparison.InvariantCultureIgnoreCase)) {

        if (values.Length != 2)
          throw new Exception(
            string.Format(
              "Expecting 2 parameters, you passed {0}. The possible parameters are ({1})",
              values.Length,
              Common.GetListFromArray(Enum.GetNames(typeof(ServoCommands.LED_COLORS)))));

        Servo.ServoPortEnum servoPort = new EZ_Builder.Scripting.HelperPortParser(values[0]).ServoPort;

        if (servoPort < EZ_B.Servo.ServoPortEnum.V0 || servoPort > EZ_B.Servo.ServoPortEnum.V99)
          throw new Exception("Only virtual servos are supported for LSS. Virtual servos start with a 'v', such as v0, v1, v2..");

        var status = (ServoCommands.LED_COLORS)Enum.Parse(typeof(ServoCommands.LED_COLORS), values[1], true);

        var servoConfig = _servoConfig.GetPort(servoPort);

        if (servoConfig == null)
          throw new Exception(string.Format("Virtual Servo {0} is not configured for LSS usage", servoPort));

        serialWrite(_servoCmds.LED(Utility.GetIdFromServo(servoPort), status));
      } else if (windowCommand.Equals("TorqueEnable", StringComparison.InvariantCultureIgnoreCase)) {

        if (values.Length != 2)
          throw new Exception(string.Format("Expecting 2 parameters, you passed {0}", values.Length));

        Servo.ServoPortEnum servoPort = new EZ_Builder.Scripting.HelperPortParser(values[0]).ServoPort;

        bool status = EZ_Builder.Scripting.Helper.GetTrueOrFalse(values[1]) == EZ_Builder.Scripting.Helper.TrueFalseEnum.True;

        if (servoPort < EZ_B.Servo.ServoPortEnum.V0 || servoPort > EZ_B.Servo.ServoPortEnum.V99)
          throw new Exception("Only virtual servos are supported for LSS. Virtual servos start with a 'v', such as v0, v1, v2..");

        var servoConfig = _servoConfig.GetPort(servoPort);

        if (servoConfig == null)
          throw new Exception(string.Format("Virtual Servo {0} is not configured for LSS usage", servoPort));

      } else {

        base.SendCommand(windowCommand, values);
      }
    }

    public override object[] GetSupportedControlCommands() {

      List<string> cmds = new List<string>();

      cmds.Add("SetLED, Port, true|false");
      cmds.Add("TorqueEnable, Port, true|false");

      return cmds.ToArray();
    }

    private void Servo_OnServoGetPosition(Servo.ServoPortEnum servoPort, EZ_B.Classes.GetServoValueResponse getServoResponse) {

      if (getServoResponse.Success)
        return;

      if (!EZBManager.EZBs[0].IsConnected) {

        getServoResponse.Success = false;
        getServoResponse.ErrorStr = "Not connected to EZ-B";

        return;
      }

      var resp = getServoPosition(servoPort);

      getServoResponse.Success = resp.Success;
      getServoResponse.Position = resp.Position;
      getServoResponse.ErrorStr = resp.ErrorStr;
    }

    void Servo_OnServoRelease(Servo.ServoPortEnum[] servos) {

      List<byte> cmdData = new List<byte>();

      foreach (var servoPort in servos) {

        if (servoPort < EZ_B.Servo.ServoPortEnum.V0 || servoPort > EZ_B.Servo.ServoPortEnum.V99)
          continue;

        var servoConfig = _servoConfig.GetPort(servoPort);

        if (servoConfig == null)
          continue;

        cmdData.AddRange(_servoCmds.ReleaseServo(Utility.GetIdFromServo(servoConfig.Port)));
      }

      if (cmdData.Count != 0)
        serialWrite(cmdData.ToArray());
    }

    void Servo_OnServoSpeed(EZ_B.Classes.ServoSpeedItem[] servos) {

      List<byte> cmdData = new List<byte>();

      foreach (var servo in servos) {

        if (servo.Port < EZ_B.Servo.ServoPortEnum.V0 || servo.Port > EZ_B.Servo.ServoPortEnum.V99)
          continue;

        var servoConfig = _servoConfig.GetPort(servo.Port);

        if (servoConfig == null)
          continue;

        int speed = servo.Speed * 51;

        cmdData.AddRange(_servoCmds.ServoSpeed(Utility.GetIdFromServo(servo.Port), speed));
      }

      if (cmdData.Count != 0)
        serialWrite(cmdData.ToArray());
    }

    void Servo_OnServoMove(EZ_B.Classes.ServoPositionItem[] servos) {

      List<byte> cmdData = new List<byte>();

      foreach (var servo in servos) {

        if (servo.Port < EZ_B.Servo.ServoPortEnum.V0 || servo.Port > EZ_B.Servo.ServoPortEnum.V99)
          continue;

        var servoConfig = _servoConfig.GetPort(servo.Port);

        if (servoConfig == null)
          continue;

        int position = (int)EZ_B.Functions.RemapScalar(servo.Position, Servo.SERVO_MIN, Servo.SERVO_MAX, -90, 90);

        cmdData.AddRange(_servoCmds.MoveServoCmd(Utility.GetIdFromServo(servo.Port), position));
      }

      if (cmdData.Count != 0)
        serialWrite(cmdData.ToArray());
    }

    Servo.ServoPortEnum getCmdParameterPort(object[] parameters) {

      if (parameters.Length != 1)
        throw new Exception("Requires only 1 parameter, which is the virtual servo (i.e. V0)");

      var port = (Servo.ServoPortEnum)Enum.Parse(typeof(EZ_B.Servo.ServoPortEnum), parameters[0].ToString(), true);

      if (port < Servo.ServoPortEnum.V0 || port > Servo.ServoPortEnum.V99)
        throw new Exception("Servo must be a Vxx servo between V0 and V99");

      return port;
    }

    private void FunctionEval_AdditionalFunctionEvent(object sender, ExpressionEvaluation.AdditionalFunctionEventArgs e) {

      if (e.Name.Equals("GetLSSTemp", StringComparison.InvariantCultureIgnoreCase)) {

        var port = getCmdParameterPort(e.Parameters);

        e.ReturnValue = getServoTemp(port);
      } else if (e.Name.Equals("GetLSSPing", StringComparison.InvariantCultureIgnoreCase)) {

        var port = getCmdParameterPort(e.Parameters);

        e.ReturnValue = getServoPing(port);
      } else if (e.Name.Equals("GetLSSPosition", StringComparison.InvariantCultureIgnoreCase)) {

        var port = getCmdParameterPort(e.Parameters);

        var resp = getServoPosition(port);

        if (!resp.Success)
          throw new Exception("Servo did not respond");

        e.ReturnValue = resp.Position;
      }
    }

    private void ucConfigurationButton1_Click(object sender, EventArgs e) {

      using (FormConfig form = new FormConfig()) {

        form.SetConfiguration(_cf);

        if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK) {

          _cf = form.GetConfiguration();

          initUART();

          _servoConfig = (ConfigServos)_cf.GetCustomObjectV2(typeof(ConfigServos));
        }
      }
    }

    private void btnForceInit_Click(object sender, EventArgs e) {

      if (!EZBManager.EZBs[0].IsConnected) {

        MessageBox.Show("You must be connected to an EZ-B to send the initialization.");

        return;
      }

      try {

        initUART();
      } catch (Exception ex) {

        MessageBox.Show("LSS Error: " + ex.Message);
      }
    }

    EZ_B.Classes.GetServoValueResponse getServoPosition(Servo.ServoPortEnum servo) {

      var resp = new EZ_B.Classes.GetServoValueResponse();

      if (((ConfigTitles.PortTypeEnum)_cf.STORAGE[ConfigTitles.PORT_TYPE]) == ConfigTitles.PortTypeEnum.DigitalPort) {

        resp.ErrorStr = "This feature is only available when using the hardware uart";
        resp.Success = false;

        return resp;
      }

      var servoConfig = _servoConfig.GetPort(servo);

      System.Diagnostics.Debug.WriteLine("requesting for " + servo);

      if (servoConfig == null) {

        System.Diagnostics.Debug.WriteLine("doesnt exist " + servo);

        resp.ErrorStr = "Not the correct servo";
        resp.Success = false;

        return resp;
      }

      var id = Utility.GetIdFromServo(servo);

      Invokers.SetAppendText(tbLog, true, "Reading position from {0}", servo);

      initUART();

      serialWrite(_servoCmds.GetCurrentPositionCmd(id));

      System.Threading.Thread.Sleep(100);

      var ret = EZBManager.EZBs[0].Uart.UARTExpansionReadAvailable(getUARTIndex());

      if (ret.Length <= 4) {

        resp.ErrorStr = "Servo did not respond";
        resp.Success = false;

        return resp;
      }

      string respStr = Encoding.ASCII.GetString(ret);

      // this never happens because the servos talk over each other and the data is corrupt
      if (respStr.Count(x => x == '*') > 1)
        throw new Exception("More than one servo responded");

      int parse1 = respStr.IndexOf("QD") + 2;

      respStr = respStr.Substring(parse1).Replace("\r", "");

      var position = Convert.ToDouble(respStr) / 10;

      resp.Position = (int)EZ_B.Functions.RemapScalar(position, -90, 90, Servo.SERVO_MIN, Servo.SERVO_MAX);
      resp.Success = true;

      return resp;
    }

    bool getServoPing(Servo.ServoPortEnum servo) {

      if (((ConfigTitles.PortTypeEnum)_cf.STORAGE[ConfigTitles.PORT_TYPE]) == ConfigTitles.PortTypeEnum.DigitalPort)
        throw new Exception("This feature is only available when using the hardware uart");

      var servoConfig = _servoConfig.GetPort(servo);

      if (servoConfig == null)
        throw new Exception("No servo configured for this ID");

      var id = Utility.GetIdFromServo(servo);

      Invokers.SetAppendText(tbLog, true, "Reading load from {0}", servo);

      initUART();

      serialWrite(_servoCmds.SendPing(id));

      System.Threading.Thread.Sleep(500);

      var ret = EZBManager.EZBs[0].Uart.UARTExpansionReadAvailable(getUARTIndex());

      if (ret.Length <= 4)
        return false;

      string respStr = Encoding.ASCII.GetString(ret);

      // this never happens because the servos talk over each other and the data is corrupt
      if (respStr.Count(x => x == '*') > 1)
        throw new Exception("More than one servo responded");

      return true;
    }

    decimal getServoTemp(Servo.ServoPortEnum servo) {

      if (((ConfigTitles.PortTypeEnum)_cf.STORAGE[ConfigTitles.PORT_TYPE]) == ConfigTitles.PortTypeEnum.DigitalPort)
        throw new Exception("This feature is only available when using the hardware uart");

      var servoConfig = _servoConfig.GetPort(servo);

      if (servoConfig == null)
        throw new Exception("No servo configured for this ID");

      var id = Utility.GetIdFromServo(servo);

      Invokers.SetAppendText(tbLog, true, "Reading temp from {0}", servo);

      initUART();

      serialWrite(_servoCmds.GetTemp(id));

      System.Threading.Thread.Sleep(500);

      var ret = EZBManager.EZBs[0].Uart.UARTExpansionReadAvailable(getUARTIndex());

      if (ret.Length <= 6)
        throw new Exception("Servo did not respond");

      string respStr = Encoding.ASCII.GetString(ret);

      // this never happens because the servos talk over each other and the data is corrupt
      if (respStr.Count(x => x == '*') > 1)
        throw new Exception("More than one servo responded");

      int parse1 = respStr.IndexOf("QT") + 2;

      respStr = respStr.Substring(parse1).Replace("\r", "");

      return Convert.ToDecimal(respStr) / 10;
    }
  }
}
