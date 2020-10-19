﻿using System;
using System.Net;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmException : Form {
        public Exception ReportedException = null;
        public WebException ReportedWebException = null;
        public string WebAddress = string.Empty;
        public int WebErrorCode = -1;
        public bool SetCustomDescription = false;
        public string CustomDescription = null;
        //Language lang = Language.GetInstance();
        public bool FromLanguage = false;

        public frmException() {
            InitializeComponent();
            loadLanguage();
            this.Icon = Properties.Resources.YChanEx;
            DateTime TimeNow = DateTime.Now;
            lbDate.Text = string.Format("{0}/{1}/{2} {3}:{4}:{5}", TimeNow.Year, TimeNow.Month, TimeNow.Day, TimeNow.Hour, TimeNow.Minute, TimeNow.Second);
        }

        void loadLanguage() {
            //if (FromLanguage) {
            //    this.Text = Language.InternalEnglish.frmException;
            //    lbExceptionHeader.Text = Language.InternalEnglish.lbExceptionHeader;
            //    lbExceptionDescription.Text = Language.InternalEnglish.lbExceptionDescription;
            //    rtbExceptionDetails.Text = Language.InternalEnglish.rtbExceptionDetails;
            //    btnExceptionGithub.Text = Language.InternalEnglish.btnExceptionGithub;
            //    btnExceptionOk.Text = Language.InternalEnglish.btnExceptionOk;
            //}
            //else {
            //    this.Text = lang.frmException;
            //    lbExceptionHeader.Text = lang.lbExceptionHeader;
            //    lbExceptionDescription.Text = lang.lbExceptionDescription;
            //    lbExceptionDescription.Text = lang.lbExceptionDescription;
            //    btnExceptionGithub.Text = lang.btnExceptionGithub;
            //    btnExceptionOk.Text = lang.btnExceptionOk;
            //}
        }

        private void frmError_Load(object sender, EventArgs e) {
            string Exception = string.Empty;
            if (ReportedException != null) {
                Exception += "An exception occured" + "\n";
                Exception += "Message: " + ReportedException.Message + "\n";
                Exception += "Stacktrace: " + ReportedException.StackTrace + "\n";
                Exception += "Source: " + ReportedException.Source + "\n";
                Exception += "TargetSite: " + ReportedException.TargetSite + "\n";


                Exception += "Full report:\n" + ReportedException.ToString();
            }
            else if (ReportedWebException != null) {
                Exception += "A web exception occured" + "\n";
                Exception += "Message: " + ReportedWebException.Message + "\n";
                Exception += "Stacktrace: " + ReportedWebException.StackTrace + "\n";
                Exception += "Source: " + ReportedWebException.Source + "\n";
                Exception += "TargetSite: " + ReportedWebException.TargetSite + "\n";
                Exception += "InnerException: " + ReportedWebException.InnerException + "\n";
                Exception += "Response: " + ReportedWebException.Response + "\n";
                Exception += "WebAddress: " + WebAddress + "\n";


                Exception += "Full report:\n" + ReportedWebException.ToString();
            }
            else if (CustomDescription != null) {
                rtbExceptionDetails.Text = CustomDescription;
            }
            else {
                Exception = "An exception occured, but it didn't parse properly.\nCreate a new issue and tell me how you got here.";
            }

            string outputBuffer = "Feel free to copy + paste this entire text wall into a new issue on Github\n\nVersion: " + Properties.Settings.Default.AppVersion + "\nReported Exception: " + Exception;
            rtbExceptionDetails.Text = outputBuffer;
            lbVersion.Text = "v" + Properties.Settings.Default.AppVersion.ToString();
            System.Media.SystemSounds.Hand.Play();
        }

        private void btnOk_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Dispose();
        }

        private void btnGithub_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/murrty/youtube-dl-gui/issues");
        }

    }
}
