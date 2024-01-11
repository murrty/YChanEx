namespace murrty.forms;

using System.Windows.Forms;
using YChanEx;

/// <summary>
/// Represents the Log form.
/// </summary>
public partial class frmLog : Form {
    /// <summary>
    /// Gets or sets whether the log form is shown.
    /// </summary>
    public bool IsShown {
        get; set;
    }

    /// <summary>
    /// Initializes a new Log form.
    /// </summary>
    public frmLog() {
        InitializeComponent();
    }

    private void frmLog_Load(object sender, EventArgs e) {
        if (Config.ValidPoint(Saved.LogFormLocation)) {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = Saved.LogFormLocation;
        }

        if (Config.ValidSize(Saved.LogFormSize)) {
            this.Size = Saved.LogFormSize;
        }
    }

    private void frmLog_FormClosing(object sender, FormClosingEventArgs e) {
        e.Cancel = true;
        Saved.LogFormLocation = this.Location;
        Saved.LogFormSize = this.Size;
        this.Hide();
        IsShown = false;
    }

    private void btnClear_Click(object sender, EventArgs e) {
        rtbLog.Clear();
        Append("Log has been cleared", true);
    }

    private void btnClose_Click(object sender, EventArgs e) {
        this.Hide();
        IsShown = false;
    }

    /// <summary>
    /// Appends text to the log.
    /// </summary>
    /// <param name="message">The message to append.</param>
    /// <param name="initial">Whether the message is the first message of the log.</param>
    [System.Diagnostics.DebuggerStepThrough]
    public void Append(string message, bool initial = false) {
        if (rtbLog.InvokeRequired) {
            rtbLog.Invoke(() => Append(message, initial));
        }
        else {
            rtbLog.AppendText(
                $"{(initial ? "" : "\n")}[{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}] {message}"
            );
        }
    }

    /// <summary>
    /// Appends text to the log, not including date/time of the message.
    /// </summary>
    /// <param name="message">The message to append.</param>
    /// <param name="initial">Whether the message is the first message of the log.</param>
    [System.Diagnostics.DebuggerStepThrough]
    public void AppendNoDate(string message, bool initial = false) {
        if (rtbLog.InvokeRequired) {
            rtbLog.Invoke(() => Append(message, initial));
        }
        else {
            rtbLog.AppendText(
                $"{(initial ? "" : "\n")}{message}"
            );
        }
    }

    public void SetLanguage() {
        this.Text = "YChanEx log";
        btnClear.Text = "Clear";
        btnClose.Text = "Close";
    }
}