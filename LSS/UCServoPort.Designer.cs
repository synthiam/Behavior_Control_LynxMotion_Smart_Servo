namespace LSS {
  partial class UCServoPort {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbEnabled = new System.Windows.Forms.CheckBox();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbEnabled);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(443, 55);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "groupBox1";
      // 
      // cbEnabled
      // 
      this.cbEnabled.AutoSize = true;
      this.cbEnabled.Dock = System.Windows.Forms.DockStyle.Left;
      this.cbEnabled.Location = new System.Drawing.Point(3, 16);
      this.cbEnabled.Name = "cbEnabled";
      this.cbEnabled.Size = new System.Drawing.Size(65, 36);
      this.cbEnabled.TabIndex = 1;
      this.cbEnabled.Text = "Enabled";
      this.cbEnabled.UseVisualStyleBackColor = true;
      this.cbEnabled.CheckedChanged += new System.EventHandler(this.cbEnabled_CheckedChanged);
      // 
      // UCServoPort
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.groupBox1);
      this.Name = "UCServoPort";
      this.Size = new System.Drawing.Size(443, 55);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox cbEnabled;
  }
}
