namespace YChanEx {
    partial class FirstStart {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FirstStart));
            this.btnConf = new System.Windows.Forms.Button();
            this.lbAbout = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbSep = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnConf
            // 
            this.btnConf.Location = new System.Drawing.Point(127, 148);
            this.btnConf.Name = "btnConf";
            this.btnConf.Size = new System.Drawing.Size(78, 23);
            this.btnConf.TabIndex = 1;
            this.btnConf.Text = "Configure";
            this.btnConf.UseVisualStyleBackColor = true;
            this.btnConf.Click += new System.EventHandler(this.btnConf_Click);
            // 
            // lbAbout
            // 
            this.lbAbout.Location = new System.Drawing.Point(12, 9);
            this.lbAbout.Name = "lbAbout";
            this.lbAbout.Size = new System.Drawing.Size(276, 123);
            this.lbAbout.TabIndex = 0;
            this.lbAbout.Text = resources.GetString("lbAbout.Text");
            this.lbAbout.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(211, 148);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lbSep
            // 
            this.lbSep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbSep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbSep.Location = new System.Drawing.Point(16, 136);
            this.lbSep.Name = "lbSep";
            this.lbSep.Size = new System.Drawing.Size(276, 2);
            this.lbSep.TabIndex = 3;
            this.lbSep.Text = "Wooooooooooo";
            // 
            // FirstStart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(308, 180);
            this.Controls.Add(this.lbSep);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lbAbout);
            this.Controls.Add(this.btnConf);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(316, 210);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(316, 210);
            this.Name = "FirstStart";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " YChanEx - First Start";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnConf;
        private System.Windows.Forms.Label lbAbout;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lbSep;
    }
}