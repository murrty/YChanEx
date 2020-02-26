namespace YChanEx {
    partial class frmSettings {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabDownloads = new System.Windows.Forms.TabPage();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.edtTimer = new System.Windows.Forms.NumericUpDown();
            this.lbTimer = new System.Windows.Forms.Label();
            this.lbSavePath = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.chkMove = new System.Windows.Forms.CheckBox();
            this.tabApplication = new System.Windows.Forms.TabPage();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.tabReset = new System.Windows.Forms.TabPage();
            this.btnUserScript = new System.Windows.Forms.Button();
            this.btnProtocol = new System.Windows.Forms.Button();
            this.btnSCan = new System.Windows.Forms.Button();
            this.btnSSave = new System.Windows.Forms.Button();
            this.hintTextBox1 = new YChanEx.HintTextBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.checkBox10 = new System.Windows.Forms.CheckBox();
            this.ttSettings = new System.Windows.Forms.ToolTip(this.components);
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabDownloads.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.edtTimer)).BeginInit();
            this.tabApplication.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabDownloads);
            this.tabControl1.Controls.Add(this.tabApplication);
            this.tabControl1.Controls.Add(this.tabAdvanced);
            this.tabControl1.Controls.Add(this.tabReset);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(402, 170);
            this.tabControl1.TabIndex = 0;
            // 
            // tabDownloads
            // 
            this.tabDownloads.Controls.Add(this.checkBox3);
            this.tabDownloads.Controls.Add(this.hintTextBox1);
            this.tabDownloads.Controls.Add(this.checkBox5);
            this.tabDownloads.Controls.Add(this.checkBox4);
            this.tabDownloads.Controls.Add(this.checkBox2);
            this.tabDownloads.Controls.Add(this.checkBox1);
            this.tabDownloads.Controls.Add(this.edtTimer);
            this.tabDownloads.Controls.Add(this.lbTimer);
            this.tabDownloads.Controls.Add(this.lbSavePath);
            this.tabDownloads.Controls.Add(this.btnBrowse);
            this.tabDownloads.Controls.Add(this.chkMove);
            this.tabDownloads.Location = new System.Drawing.Point(4, 22);
            this.tabDownloads.Name = "tabDownloads";
            this.tabDownloads.Padding = new System.Windows.Forms.Padding(3);
            this.tabDownloads.Size = new System.Drawing.Size(394, 144);
            this.tabDownloads.TabIndex = 0;
            this.tabDownloads.Text = "Downloads";
            this.tabDownloads.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(147, 89);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(113, 17);
            this.checkBox5.TabIndex = 27;
            this.checkBox5.Text = "Prevent duplicates";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(147, 66);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(136, 17);
            this.checkBox4.TabIndex = 26;
            this.checkBox4.Text = "Save original file names";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(11, 89);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(130, 17);
            this.checkBox2.TabIndex = 24;
            this.checkBox2.Text = "Download Thumbnails";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(11, 66);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(106, 17);
            this.checkBox1.TabIndex = 23;
            this.checkBox1.Text = "Download HTML";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // edtTimer
            // 
            this.edtTimer.Location = new System.Drawing.Point(73, 37);
            this.edtTimer.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.edtTimer.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.edtTimer.Name = "edtTimer";
            this.edtTimer.Size = new System.Drawing.Size(56, 20);
            this.edtTimer.TabIndex = 21;
            this.edtTimer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.edtTimer.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // lbTimer
            // 
            this.lbTimer.AutoSize = true;
            this.lbTimer.Location = new System.Drawing.Point(8, 39);
            this.lbTimer.Name = "lbTimer";
            this.lbTimer.Size = new System.Drawing.Size(59, 13);
            this.lbTimer.TabIndex = 22;
            this.lbTimer.Text = "Timer (sec)";
            // 
            // lbSavePath
            // 
            this.lbSavePath.AutoSize = true;
            this.lbSavePath.Location = new System.Drawing.Point(5, 14);
            this.lbSavePath.Name = "lbSavePath";
            this.lbSavePath.Size = new System.Drawing.Size(57, 13);
            this.lbSavePath.TabIndex = 20;
            this.lbSavePath.Text = "Save &Path\r\n";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(268, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(24, 20);
            this.btnBrowse.TabIndex = 18;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            // 
            // chkMove
            // 
            this.chkMove.AutoSize = true;
            this.chkMove.Checked = true;
            this.chkMove.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMove.Location = new System.Drawing.Point(296, 6);
            this.chkMove.Name = "chkMove";
            this.chkMove.Size = new System.Drawing.Size(93, 30);
            this.chkMove.TabIndex = 19;
            this.chkMove.Text = "Move existing \r\ndownloads";
            this.chkMove.UseVisualStyleBackColor = true;
            // 
            // tabApplication
            // 
            this.tabApplication.Controls.Add(this.checkBox10);
            this.tabApplication.Controls.Add(this.checkBox9);
            this.tabApplication.Controls.Add(this.checkBox8);
            this.tabApplication.Controls.Add(this.checkBox7);
            this.tabApplication.Controls.Add(this.checkBox6);
            this.tabApplication.Location = new System.Drawing.Point(4, 22);
            this.tabApplication.Name = "tabApplication";
            this.tabApplication.Padding = new System.Windows.Forms.Padding(3);
            this.tabApplication.Size = new System.Drawing.Size(394, 144);
            this.tabApplication.TabIndex = 1;
            this.tabApplication.Text = "Application";
            this.tabApplication.UseVisualStyleBackColor = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdvanced.Size = new System.Drawing.Size(394, 144);
            this.tabAdvanced.TabIndex = 2;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // tabReset
            // 
            this.tabReset.Location = new System.Drawing.Point(4, 22);
            this.tabReset.Name = "tabReset";
            this.tabReset.Padding = new System.Windows.Forms.Padding(3);
            this.tabReset.Size = new System.Drawing.Size(394, 144);
            this.tabReset.TabIndex = 3;
            this.tabReset.Text = "Reset";
            this.tabReset.UseVisualStyleBackColor = true;
            // 
            // btnUserScript
            // 
            this.btnUserScript.Location = new System.Drawing.Point(12, 176);
            this.btnUserScript.Name = "btnUserScript";
            this.btnUserScript.Size = new System.Drawing.Size(75, 24);
            this.btnUserScript.TabIndex = 25;
            this.btnUserScript.Text = "Userscript";
            this.btnUserScript.UseVisualStyleBackColor = true;
            // 
            // btnProtocol
            // 
            this.btnProtocol.Enabled = false;
            this.btnProtocol.Location = new System.Drawing.Point(93, 176);
            this.btnProtocol.Name = "btnProtocol";
            this.btnProtocol.Size = new System.Drawing.Size(116, 24);
            this.btnProtocol.TabIndex = 24;
            this.btnProtocol.Text = "Install protocol";
            this.btnProtocol.UseVisualStyleBackColor = true;
            this.btnProtocol.Visible = false;
            // 
            // btnSCan
            // 
            this.btnSCan.Location = new System.Drawing.Point(315, 176);
            this.btnSCan.Name = "btnSCan";
            this.btnSCan.Size = new System.Drawing.Size(75, 24);
            this.btnSCan.TabIndex = 23;
            this.btnSCan.Text = "Cancel";
            this.btnSCan.UseVisualStyleBackColor = true;
            // 
            // btnSSave
            // 
            this.btnSSave.Location = new System.Drawing.Point(234, 176);
            this.btnSSave.Name = "btnSSave";
            this.btnSSave.Size = new System.Drawing.Size(75, 24);
            this.btnSSave.TabIndex = 22;
            this.btnSSave.Text = "Save";
            this.btnSSave.UseVisualStyleBackColor = true;
            // 
            // hintTextBox1
            // 
            this.hintTextBox1.Location = new System.Drawing.Point(68, 11);
            this.hintTextBox1.Name = "hintTextBox1";
            this.hintTextBox1.ReadOnly = true;
            this.hintTextBox1.Size = new System.Drawing.Size(194, 20);
            this.hintTextBox1.TabIndex = 28;
            this.hintTextBox1.Text = "C:\\";
            this.hintTextBox1.TextHint = "C:\\";
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Location = new System.Drawing.Point(28, 29);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(79, 17);
            this.checkBox6.TabIndex = 27;
            this.checkBox6.Text = "checkBox6";
            this.checkBox6.UseVisualStyleBackColor = true;
            // 
            // checkBox7
            // 
            this.checkBox7.AutoSize = true;
            this.checkBox7.Location = new System.Drawing.Point(126, 29);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(79, 17);
            this.checkBox7.TabIndex = 28;
            this.checkBox7.Text = "checkBox7";
            this.checkBox7.UseVisualStyleBackColor = true;
            // 
            // checkBox8
            // 
            this.checkBox8.AutoSize = true;
            this.checkBox8.Location = new System.Drawing.Point(247, 29);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(79, 17);
            this.checkBox8.TabIndex = 29;
            this.checkBox8.Text = "checkBox8";
            this.checkBox8.UseVisualStyleBackColor = true;
            // 
            // checkBox9
            // 
            this.checkBox9.AutoSize = true;
            this.checkBox9.Location = new System.Drawing.Point(126, 52);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(79, 17);
            this.checkBox9.TabIndex = 30;
            this.checkBox9.Text = "checkBox9";
            this.checkBox9.UseVisualStyleBackColor = true;
            // 
            // checkBox10
            // 
            this.checkBox10.AutoSize = true;
            this.checkBox10.Location = new System.Drawing.Point(247, 52);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(85, 17);
            this.checkBox10.TabIndex = 31;
            this.checkBox10.Text = "checkBox10";
            this.checkBox10.UseVisualStyleBackColor = true;
            // 
            // ttSettings
            // 
            this.ttSettings.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(11, 112);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(166, 17);
            this.checkBox3.TabIndex = 29;
            this.checkBox3.Text = "Save download queue on exit";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(402, 212);
            this.Controls.Add(this.btnUserScript);
            this.Controls.Add(this.btnProtocol);
            this.Controls.Add(this.btnSCan);
            this.Controls.Add(this.btnSSave);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(410, 242);
            this.MinimumSize = new System.Drawing.Size(410, 242);
            this.Name = "frmSettings";
            this.Text = "ychanex settings";
            this.tabControl1.ResumeLayout(false);
            this.tabDownloads.ResumeLayout(false);
            this.tabDownloads.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.edtTimer)).EndInit();
            this.tabApplication.ResumeLayout(false);
            this.tabApplication.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabDownloads;
        private System.Windows.Forms.NumericUpDown edtTimer;
        private System.Windows.Forms.Label lbTimer;
        private System.Windows.Forms.Label lbSavePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.CheckBox chkMove;
        private System.Windows.Forms.TabPage tabApplication;
        private System.Windows.Forms.TabPage tabAdvanced;
        private System.Windows.Forms.TabPage tabReset;
        private System.Windows.Forms.Button btnUserScript;
        private System.Windows.Forms.Button btnProtocol;
        private System.Windows.Forms.Button btnSCan;
        private System.Windows.Forms.Button btnSSave;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private HintTextBox hintTextBox1;
        private System.Windows.Forms.CheckBox checkBox10;
        private System.Windows.Forms.CheckBox checkBox9;
        private System.Windows.Forms.CheckBox checkBox8;
        private System.Windows.Forms.CheckBox checkBox7;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.ToolTip ttSettings;
        private System.Windows.Forms.CheckBox checkBox3;
    }
}