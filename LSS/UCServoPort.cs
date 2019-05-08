using System;
using System.Windows.Forms;

namespace LSS {

  public partial class UCServoPort : UserControl {

    EZ_B.Servo.ServoPortEnum _port;

    public UCServoPort() {

      InitializeComponent();

      cbEnabled_CheckedChanged(null, null);
    }

    public EZ_B.Servo.ServoPortEnum SetPort {
      get {
        return _port;
      }
      set {
        groupBox1.Text = value.ToString();
        _port = value;
      }
    }

    public bool SetEnabled {
      get {
        return cbEnabled.Checked;
      }
      set {
        cbEnabled.Checked = value;

        cbEnabled_CheckedChanged(null, null);
      }
    }

    private void cbEnabled_CheckedChanged(object sender, EventArgs e) {

    }
  }
}
