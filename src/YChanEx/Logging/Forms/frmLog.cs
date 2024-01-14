#nullable enable

namespace murrty.logging;
using System.Windows.Forms;
using YChanEx;

/// <summary>
/// Represents the Log form.
/// </summary>
internal partial class frmLog : Form {
    /// <summary>
    /// The amount of lines allowed on the log form.
    /// </summary>
    private const int EntryLimit = 200;

    /// <summary>
    /// Gets whether the log form is shown.
    /// </summary>
    public bool IsShown { get; private set; }
    /// <summary>
    /// Gets the amount of entires in the log.
    /// </summary>
    public int TotalEntries { get => rtbLog?.Lines.Length ?? 0; }
    /// <summary>
    /// Gets the amount of exceptions cached for the user.
    /// </summary>
    public int TotalExceptions { get => tcExceptions?.TabCount ?? 0; }
    /// <summary>
    /// Whether the logging system is being disabled.
    /// </summary>
    public bool DisablingLogging { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="frmLog"/> class.
    /// </summary>
    public frmLog() {
        InitializeComponent();
        SetLanguage();
        lbClrVersion.Text = "CLR: " + Program.CLR;

        if (!Program.DebugMode) {
            btnTestLine.Enabled = btnTestLine.Visible = false;
            btnTestLine.Dispose();
        }
        rtbLog.ReadOnly = true;

        this.Shown += frmLog_Shown;
    }

    /// <summary>
    /// Closes the log form.
    /// </summary>
    /// <param name="DisablingLogging">
    /// Whether logging is being disabled, so the form will be disposed property.
    /// </param>
    public void Close(bool DisablingLogging) {
        this.DisablingLogging = DisablingLogging;
        Close();
    }

    /// <summary>
    /// Loads the log form.
    /// </summary>
    private void frmLog_Load(object? sender, EventArgs e) {
        //if (FormSettings.frmLog_Location.ValidPoint()) {
        //    this.StartPosition = FormStartPosition.Manual;
        //    this.Location = FormSettings.frmLog_Location;
        //}

        //if (FormSettings.frmLog_Size.ValidSize()) {
        //    this.Size = FormSettings.frmLog_Size;
        //}

        tcMain.SelectedTab = tpExceptions;
        tcMain.SelectedTab = tpMainLog;
    }
    /// <summary>
    /// When the log form is shown, it will move the pos lower.
    /// </summary>
    private void frmLog_Shown(object? sender, EventArgs e) {
        this.Shown -= frmLog_Shown;
        int CaretPos = rtbLog.SelectionStart;
        int Length = rtbLog.SelectionLength;
        rtbLog.SelectionLength = 0;
        rtbLog.SelectionStart = rtbLog.Text.Length;
        rtbLog.SelectionLength = CaretPos;
        rtbLog.SelectionLength = Length;
        rtbLog.Focus();
        lbLines.Focus();
    }
    /// <summary>
    /// Hides the log instead of closing and disposing the log.
    /// </summary>
    private void frmLog_FormClosing(object? sender, FormClosingEventArgs e) {
        //FormSettings.frmLog_Location = this.Location;
        //FormSettings.frmLog_Size = this.Size;
        if (!DisablingLogging) {
            e.Cancel = true;
            IsShown = false;
            this.Hide();
        }
    }

    /// <summary>
    /// Clears the log entries.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnClear_Click(object? sender, EventArgs e) {
        rtbLog.Clear();
        Append(DateTime.Now, "Log has been cleared.");
    }
    /// <summary>
    /// Removes an exception from the cached exception details.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnRemoveException_Click(object? sender, EventArgs e) {
        if (tcExceptions.SelectedTab is not null) {
            Control[] ex = tcExceptions.SelectedTab.Controls.Find("TextBox", false);
            if (ex.Length > 0 && ex[0] is TextBox txt)
                txt.Dispose();

            int Index = tcExceptions.SelectedIndex;
            if (tcExceptions.TabCount > 1) {
                tcExceptions.SelectTab(
                    tcExceptions.TabCount > 0 ? Index + 1 < tcExceptions.TabCount ?
                    Index + 1 : Index - 1 : 0);
            }

            tcExceptions.TabPages[Index].Dispose();

            if (tcExceptions.TabCount > 0) {
                btnRemoveException.Enabled = true;
                lbNoExceptionHistory.Visible = false;
            }
            else {
                btnRemoveException.Enabled = false;
                lbNoExceptionHistory.Visible = true;
            }

            UpdateCounts();
        }
    }
    /// <summary>
    /// Hides the log instead of closing, which allows it to continue working in the background.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnClose_Click(object? sender, EventArgs e) {
        this.Hide();
        IsShown = false;
    }

    /// <summary>
    /// Displays the log.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public void ShowLog() {
        if (!IsShown) {
            this.Show();
            IsShown = true;
        }
        else {
            this.Activate();
        }
    }

    /// <summary>
    /// Hides the log.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public void HideLog() {
        if (IsShown) {
            this.Hide();
            IsShown = false;
        }
    }

    /// <summary>
    /// Checks the length of the log, preventing it from going over the 'EntryLimit'.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    private void CheckLogLength() {
        if (rtbLog.Lines.Length >= EntryLimit) {
            rtbLog.Clear();
            Append(DateTime.Now, "Log has been auto-cleared (200 lines maximum).");
        }
    }

    /// <summary>
    /// Updates the counts of log entries and exceptions received.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    private void UpdateCounts() {
        lbLines.Text = $$"""
            log entries: {{rtbLog.Lines.Length}} / 200
            exceptions received: {{tcExceptions.TabCount}}
            """;
    }

    /// <summary>
    /// Appends text to the log, not including date/time of the message.
    /// </summary>
    /// <param name="time">The time of the message.</param>
    /// <param name="message">The message to append.</param>
    [System.Diagnostics.DebuggerStepThrough]
    public void Append(DateTime time, string message) {
        if (rtbLog.InvokeRequired) {
            rtbLog.Invoke(() => Append(time, message));
            return;
        }
        CheckLogLength();
        rtbLog.AppendText($"[{time:HH:mm:ss.fff}] {message}\n");
    }

    /// <summary>
    /// Appends a debug message to the log.
    /// </summary>
    /// <param name="time">The time of the message.</param>
    /// <param name="message">The message to append.</param>
    [System.Diagnostics.Conditional("DEBUG")]
    public void AppendDebug(DateTime time, string message) {
        if (rtbLog.InvokeRequired) {
            rtbLog.Invoke(() => AppendDebug(time, message));
            return;
        }
        CheckLogLength();
        rtbLog.AppendText($"[{time:HH:mm:ss.fff}] {message}\n");
    }

    /// <summary>
    /// Sets the language of the form.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public void SetLanguage() {
        this.Text = "aphrodite log";
        tpMainLog.Text = "log entries";
        tpExceptions.Text = "previous exceptions";

        lbNoExceptionHistory.Text = "no exception history is available to review. either no exceptions occurred or you cleared them all.";
        btnRemoveException.Text = "remove";

        UpdateCounts();
        btnClear.Text = "clear";
        btnClose.Text = "close";
    }

    /// <summary>
    /// Adds a new exception to the log.
    /// </summary>
    /// <param name="Exception">The exception received</param>
    [System.Diagnostics.DebuggerStepThrough]
    public void AddException(ExceptionInfo Exception) {
        if (this.InvokeRequired) {
            this.Invoke(() => AddException(Exception));
            return;
        }

        TabPage ExceptionPage = new($"{Exception.Exception.GetType().Name} @ {Exception.ExceptionTime:HH:mm:ss}");
        RichTextBox ExceptionDetails = new() {
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            Name = "ExceptionTextBox"
        };
        ExceptionPage.Controls.Add(ExceptionDetails);
        ExceptionDetails.Dock = DockStyle.Fill;
        ExceptionDetails.Text = $$"""
            A {{Exception.ExceptionType switch {
            ExceptionType.Caught => "caught ",
            ExceptionType.Unhandled => "unhandled ",
            ExceptionType.ThreadException => "thread-exception ",
            _ => ""
        }}}{{Exception.Exception.GetType().Name}} occurred.

            {{Exception.Exception.GetType().FullName}} -> {{Exception.Exception.Source}}
            {{Exception.Exception.StackTrace}}
            """;
        ExceptionDetails.Font = rtbLog.Font;
        tcExceptions.TabPages.Insert(0, ExceptionPage);
        tcExceptions.SelectedTab = ExceptionPage;
        if (tcExceptions.TabCount > 0) {
            btnRemoveException.Enabled = true;
            lbNoExceptionHistory.Visible = false;
        }
        else {
            btnRemoveException.Enabled = false;
            lbNoExceptionHistory.Visible = true;
        }

        UpdateCounts();
    }

    /// <summary>
    /// Debugs the log by appending "Hello" to the log.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [System.Diagnostics.DebuggerStepThrough]
    private void btnTestLine_Click(object? sender, EventArgs e) => rtbLog.AppendText("Hello\n");
}