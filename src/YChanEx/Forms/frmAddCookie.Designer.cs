namespace YChanEx;

partial class frmAddCookie {
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent() {
            this.lbHeader = new System.Windows.Forms.Label();
            this.lbName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.lbValue = new System.Windows.Forms.Label();
            this.txtDomain = new System.Windows.Forms.TextBox();
            this.lbDomain = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lbPath = new System.Windows.Forms.Label();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbHeader
            // 
            this.lbHeader.AutoSize = true;
            this.lbHeader.Location = new System.Drawing.Point(12, 9);
            this.lbHeader.Name = "lbHeader";
            this.lbHeader.Size = new System.Drawing.Size(318, 52);
            this.lbHeader.TabIndex = 0;
            this.lbHeader.Text = "Please enter the values here precisely.\r\n\r\nIf you have already downloaded somethi" +
    "ng,\r\nchanges will not take effect until you restart the application.";
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Location = new System.Drawing.Point(12, 67);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(44, 13);
            this.lbName.TabIndex = 1;
            this.lbName.Text = "Name *";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(15, 83);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(313, 22);
            this.txtName.TabIndex = 2;
            // 
            // txtValue
            // 
            this.txtValue.Location = new System.Drawing.Point(15, 128);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(313, 22);
            this.txtValue.TabIndex = 4;
            // 
            // lbValue
            // 
            this.lbValue.AutoSize = true;
            this.lbValue.Location = new System.Drawing.Point(12, 112);
            this.lbValue.Name = "lbValue";
            this.lbValue.Size = new System.Drawing.Size(43, 13);
            this.lbValue.TabIndex = 3;
            this.lbValue.Text = "Value *";
            // 
            // txtDomain
            // 
            this.txtDomain.Location = new System.Drawing.Point(15, 218);
            this.txtDomain.Name = "txtDomain";
            this.txtDomain.Size = new System.Drawing.Size(313, 22);
            this.txtDomain.TabIndex = 8;
            // 
            // lbDomain
            // 
            this.lbDomain.AutoSize = true;
            this.lbDomain.Location = new System.Drawing.Point(12, 202);
            this.lbDomain.Name = "lbDomain";
            this.lbDomain.Size = new System.Drawing.Size(55, 13);
            this.lbDomain.TabIndex = 7;
            this.lbDomain.Text = "Domain *";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(15, 173);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(313, 22);
            this.txtPath.TabIndex = 6;
            // 
            // lbPath
            // 
            this.lbPath.AutoSize = true;
            this.lbPath.Location = new System.Drawing.Point(12, 157);
            this.lbPath.Name = "lbPath";
            this.lbPath.Size = new System.Drawing.Size(30, 13);
            this.lbPath.TabIndex = 5;
            this.lbPath.Text = "Path";
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(172, 250);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 9;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(253, 250);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmAddCookie
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(344, 285);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtDomain);
            this.Controls.Add(this.lbDomain);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.lbPath);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.lbValue);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lbName);
            this.Controls.Add(this.lbHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::YChanEx.Properties.Resources.ProgramIcon;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(360, 320);
            this.MinimumSize = new System.Drawing.Size(360, 320);
            this.Name = "frmAddCookie";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add cookie...";
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lbHeader;
    private System.Windows.Forms.Label lbName;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.TextBox txtValue;
    private System.Windows.Forms.Label lbValue;
    private System.Windows.Forms.TextBox txtDomain;
    private System.Windows.Forms.Label lbDomain;
    private System.Windows.Forms.TextBox txtPath;
    private System.Windows.Forms.Label lbPath;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Button btnCancel;
}