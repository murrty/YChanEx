// ExceptionForm Base 1.0.3

using System.Net;
using System.Windows.Forms;
using murrty.classes;

namespace murrty.forms {

    public partial class frmException : Form {

        #region Fields & Properties

        /// <summary>
        /// The constant string of the URL to the github issues page.
        /// </summary>
        private const string GithubPage = null;

        /// <summary>
        /// The private ExceptionInfo holding information about the exception.
        /// </summary>
        private ExceptionInfo ReportedException { get; }

        #endregion

        #region Form Events

        public frmException(ExceptionInfo ReportedException) {
            this.ReportedException = ReportedException;

            InitializeComponent();
            LoadLanguage();

            if (string.IsNullOrWhiteSpace(GithubPage)) {
                btnExceptionGithub.Enabled = false;
                btnExceptionGithub.Visible = false;
                lbDate.Location = new(btnExceptionRetry.Location.X - 119, lbDate.Location.Y);
            }
            if (!ReportedException.AllowRetry) {
                btnExceptionRetry.Visible = false;
                btnExceptionRetry.Enabled = false;
                if (!string.IsNullOrWhiteSpace(GithubPage)) {
                    btnExceptionGithub.Location = btnExceptionRetry.Location;
                    lbDate.Location = new(btnExceptionGithub.Location.X - 119, lbDate.Location.Y);
                }
                else {
                    lbDate.Location = new(btnExceptionOk.Location.X - 119, lbDate.Location.Y);
                }
            }

            lbDate.Text = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";
            btnExceptionRetry.Enabled = ReportedException.AllowRetry;
        }

        private void frmError_Load(object sender, EventArgs e) {
            if (ReportedException.CustomDescription is null) {
                switch (ReportedException.ReceivedException) {
                    case UnhandledExceptionEventArgs UnEx: {
                        rtbExceptionDetails.Text = "An unhandled exception occurred.\n" +
                                                  $"The application will exit after closing this dialog.\n\n" +
                                                  $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                                                  $"{UnEx.ExceptionObject}\n";
                    } break;

                    case System.Threading.ThreadExceptionEventArgs ThrEx: {
                        rtbExceptionDetails.Text = "An unhandled thread exception occurred.\n" +
                                                  $"The application will exit after closing this dialog.\n\n" +
                                                  $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                                                  $"{ThrEx.Exception}\n";
                    } break;

                    case WebException WebEx: {
                        rtbExceptionDetails.Text = (ReportedException.Unrecoverable ? "An unrecoverable web exception occurred, and the application will exit." : "A web exception occured.") + "\n\n" +
                                                  $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                                                  $"{(ReportedException.ExtraInfo is not null ? "Web Address: " + $"{ReportedException.ExtraInfo}\n" : "")}" +
                                                  $"Message: {WebEx.Message}\n" +
                                                  $"Stacktrace: {WebEx.StackTrace}\n" +
                                                  $"Source: {WebEx.Source}\n" +
                                                  $"Target Site: {WebEx.TargetSite}\n" +
                                                  $"Inner Exception: {WebEx.InnerException}\n" +
                                                  $"Response: {WebEx.Response}\n";
                    } break;

                    case System.Threading.ThreadAbortException ThrAbrEx: {
                        rtbExceptionDetails.Text = (ReportedException.Unrecoverable ? "An unrecoverable thread abort exception occurred, and the application will exit." : "A thread abort exception occurred.") + "\n\n" +
                                                  $"This exception may have been thrown on accident.\n" +
                                                  $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                                                  $"Message: {ThrAbrEx.Message}\n" +
                                                  $"Stacktrace: {ThrAbrEx.StackTrace}\n" +
                                                  $"Source: {ThrAbrEx.Source}\n" +
                                                  $"Target Site: {ThrAbrEx.TargetSite}\n" +
                                                  $"Inner Exception: {ThrAbrEx.InnerException}\n";
                    } break;

                    case Exception Ex: {
                        rtbExceptionDetails.Text = (ReportedException.Unrecoverable ? "An unrecoverable exception occurred, and the application will exit." : "An exception occured.") + "\n\n" +
                                                  $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                                                  $"Message: {Ex.Message}\n" +
                                                  $"Stacktrace: {Ex.StackTrace}\n" +
                                                  $"Source: {Ex.Source}\n" +
                                                  $"Target Site: {Ex.TargetSite}\n" +
                                                  $"Inner Exception: {Ex.InnerException}\n";
                    } break;

                    default: {
                        rtbExceptionDetails.Text = "An uncast exception occurred. The updater may exit after this dialog closes." +
                                                  $"{(ReportedException.ExtraMessage is not null ? $"\n\n{ReportedException.ExtraMessage}" : "")}\n";
                    } break;
                }

                rtbExceptionDetails.Text += "\n========== FULL REPORT ==========\n" +
                                            ReportedException.ReceivedException.ToString() +
                                            "\n========== END  REPORT ==========\n";

                rtbExceptionDetails.Text += "\n========== OS  INFO ==========\n" +
                                            "(Please don't omit this info, it may be important)\n" +
                                            GetRelevantInformation() +
                                            "\n========== END INFO ==========";
            }
            else rtbExceptionDetails.Text = ReportedException.CustomDescription;

            lbVersion.Text = "v" + YChanEx.Program.CurrentVersion.ToString();
        }

        private void frmException_Shown(object sender, EventArgs e) {
            System.Media.SystemSounds.Hand.Play();
        }

        private void btnExceptionGithub_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(GithubPage);
        }

        private void btnExceptionRetry_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Retry;
            this.Dispose();
        }

        private void btnExceptionOk_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Dispose();
        }

        #endregion

        #region Supporting Methods

        public static string GetRelevantInformation() {
            return $"Current version: {YChanEx.Program.CurrentVersion}\nCurrent culture: {System.Threading.Thread.CurrentThread.CurrentCulture.EnglishName}\nOS: {Log.ComputerVersionInformation}";
        }

        void LoadLanguage() {
            this.Text = "An exception occured";
            lbExceptionHeader.Text = "An exception has occured";
            lbExceptionDescription.Text = "Below is the error that occured. Feel free to open a new issue and report it.";
            rtbExceptionDetails.Text = "Feel free to copy + paste this entire text wall into a new issue on Github.";
            btnExceptionGithub.Text = "Github";
            btnExceptionOk.Text = "OK";
            btnExceptionRetry.Text = "Retry";
        }

        #endregion

    }

    /// <summary>
    /// The base exception detail class containing information about the exception, and modifiers about the actions.
    /// </summary>
    public class ExceptionInfo {

        #region Variables / Fields / Whatever
        /// <summary>
        /// The dynamic exception that is received.
        /// </summary>
        public dynamic ReceivedException = null;
        /// <summary>
        /// Any extra info regarding the exception.
        /// </summary>
        public object ExtraInfo = null;
        /// <summary>
        /// An extra message that's printed before the main exception text.
        /// </summary>
        public string ExtraMessage = null;
        /// <summary>
        /// The description that is posted to the exception form instead of one that gets parsed.
        /// </summary>
        public string CustomDescription = null;
        /// <summary>
        /// If the exception was caused from loading the language file.
        /// </summary>
        public bool FromLanguage = false;
        /// <summary>
        /// If the cause of exception can be retried.
        /// </summary>
        public bool AllowRetry = false;
        /// <summary>
        /// If the exception is not recoverable, and the progarm must be terminated.
        /// </summary>
        public bool Unrecoverable = false;
        #endregion

        #region Constructor
        public ExceptionInfo(dynamic ReceivedException) {
            this.ReceivedException = ReceivedException;
        }
        #endregion

    }

}
