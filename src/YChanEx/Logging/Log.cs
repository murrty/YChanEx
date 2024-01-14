#nullable enable
namespace murrty.classes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
using murrty.logging;
/// <summary>
/// This class will control the Errors that get reported in try statements.
/// </summary>
internal static class Log {
    #region Properties & Fields
    /// <summary>
    /// The default value that should appear in the message box titles.
    /// </summary>
    public const string MessageBoxTitle = "aphrodite";

    /// <summary>
    /// The log form that is used globally to log data.
    /// </summary>
    private static volatile frmLog? LogForm;

    /// <summary>
    /// Gets the computer versioning information, such as the running operating system, language, etc.
    /// </summary>
    public static string ComputerVersionInformation { get; private set; } = $"{nameof(ComputerVersionInformation)} not initialized.";

    /// <summary>
    /// Gets whether the log is enabled.
    /// </summary>
    [MemberNotNullWhen(true, nameof(LogForm))]
    public static bool LogEnabled { get; private set; }

    /// <summary>
    /// Gets or sets whether the log will write to file.
    /// </summary>
    public static bool AllowWritingToFile { get; set; }

    /// <summary>
    /// Gets whether logging is enabled and the log form is created and not disposed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(LogForm))]
    public static bool LogFormUsable => LogEnabled && LogForm?.IsDisposed == false;

    public static event EventHandler<EventArgs>? OnLogEnabled;
    public static event EventHandler<EventArgs>? OnLogDisabled;
    #endregion

    #region Log stuff
    public static void InitializeLogging() {
        // Catch any exceptions that are unhandled, so we can report it.
//#if !DEBUG
        try {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
//#endif

            EnableLogging();

//#if !DEBUG
            Info("Creating unhandled exception event.");
            AppDomain.CurrentDomain.UnhandledException += (sender, exception) => {
                ExceptionInfo NewException = new((Exception)exception.ExceptionObject) {
                    ExceptionTime = DateTime.Now,
                    ExceptionType = exception.IsTerminating ? ExceptionType.Unhandled : ExceptionType.Caught,
                    AllowRetry = false
                };
                AddExceptionToLog(NewException);
                using frmException ExceptionDisplay = new(NewException);
                ExceptionDisplay.ShowDialog();
            };

            Info("Creating unhandled thread exception event.");
            Application.ThreadException += (sender, exception) => {
                ExceptionInfo NewException = new(exception.Exception) {
                    ExceptionTime = DateTime.Now,
                    ExceptionType = ExceptionType.ThreadException,
                    AllowRetry = false
                };
                AddExceptionToLog(NewException);
                using frmException ExceptionDisplay = new(NewException);
                ExceptionDisplay.ShowDialog();
            };
        }
        catch (Exception ex) {
            ReportException(ex);
        }
//#endif

        Info("Creating ComputerVersionInformation for exceptions.");
        var MgtSearcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        var MgtInfo = MgtSearcher?.Get().Cast<System.Management.ManagementObject>().FirstOrDefault();

        if (MgtInfo is null) {
            ComputerVersionInformation = "coult not query computer information";
        }
        else {
            ComputerVersionInformation = $$"""
                Current version: {{YChanEx.Program.CurrentVersion}}
                Current culture: {{Thread.CurrentThread.CurrentCulture.EnglishName}}
                CLR: {{YChanEx.Program.CLR}}

                """ + (MgtInfo is null ? "The ManagementObject for Win32_OperatingSystem is null and cannot be used." : $$"""
                System caption: {{MgtInfo.Properties["Caption"].Value ?? "could not query"}}
                Version: {{MgtInfo.Properties["Version"].Value ?? "could not query"}}
                Service Pack major: {{MgtInfo.Properties["ServicePackMajorVersion"].Value ?? "could not query"}}
                Service Pack minor: {{MgtInfo.Properties["ServicePackMinorVersion"].Value ?? "could not query"}}
                """);
        }
    }

    /// <summary>
    /// Enables the logging form.
    /// </summary>
    [MemberNotNull(nameof(LogForm))]
    internal static void EnableLogging() {
        if (LogFormUsable) {
            Info("Logging is already enabled.");
        }
        else {
            LogForm = new();
            LogEnabled = true;
            Info("Logging has been enabled.");
            OnLogEnabled?.Invoke(null, EventArgs.Empty);

            //TestMessages();
            //if (aphrodite.Program.DebugMode) {
            //    ShowLog();
            //}
        }
    }

    /// <summary>
    /// Disables the logging form, but debug logging will still occur.
    /// </summary>
    internal static void DisableLogging() {
        if (!LogEnabled) {
            return;
        }

        if (LogFormUsable) {
            if (LogForm.WindowState == FormWindowState.Minimized || LogForm.WindowState == FormWindowState.Maximized) {
                LogForm.Opacity = 0;
                LogForm.WindowState = FormWindowState.Normal;
            }
            LogForm.Close(true);
            LogForm.Dispose();
            LogForm = null;
        }

        LogEnabled = false;
        OnLogDisabled?.Invoke(null, EventArgs.Empty);
    }

    [Conditional("DEBUG")]
    internal static void ShowLogMainForm() {
        if (!LogFormUsable) {
            EnableLogging();
        }
        if (!LogForm.IsShown) {
            LogForm.ShowLog();
        }
    }

    /// <summary>
    /// Shows the log form.
    /// </summary>
    internal static void ShowLog() {
        if (LogFormUsable) {
            LogForm.ShowLog();
        }
    }

    /// <summary>
    /// Hides the log form.
    /// </summary>
    internal static void HideLog() {
        if (LogFormUsable) {
            LogForm.HideLog();
        }
    }

    /// <summary>
    /// Writes a message to the log.
    /// </summary>
    /// <param name="time">
    /// The time of the log message.
    /// </param>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    internal static void Write(DateTime time, string message) {
        System.Diagnostics.Debug.Print(message);
        if (LogFormUsable) {
            LogForm.Append(time, $"{message}");
        }
    }

    /// <summary>
    /// Writes a message to the log for debugging. This will not occur on non 'DEBUG' conditional builds.
    /// </summary>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    [Conditional("DEBUG")]
    public static void Debug(string message) => Debug(DateTime.Now, message);
    /// <summary>
    /// Writes a message to the log for debugging. This will not occur on non 'DEBUG' conditional builds.
    /// </summary>
    /// <param name="time">
    /// The time of the log message.
    /// </param>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    [Conditional("DEBUG")]
    internal static void Debug(DateTime time, string message) {
        System.Diagnostics.Debug.Print(message);
        if (LogFormUsable) {
            LogForm.AppendDebug(time, message);
        }
    }

    /// <summary>
    /// Writes a message to the log with the 'INFO' level for general log messages.
    /// </summary>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    public static void Info(string message) => Info(DateTime.Now, message);
    /// <summary>
    /// Writes a message to the log with the 'INFO' level for general log messages.
    /// </summary>
    /// <param name="time">
    /// The time of the log message.
    /// </param>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    internal static void Info(DateTime time, string message) => Write(time, "[INFO] " + message);

    /// <summary>
    /// Writes a message to the log with the 'WARN' level for warning log messages.
    /// </summary>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    public static void Warn(string message) => Warn(DateTime.Now, message);
    /// <summary>
    /// Writes a message to the log with the 'WARN' level for warning log messages.
    /// </summary>
    /// <param name="time">
    /// The time of the log message.
    /// </param>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    internal static void Warn(DateTime time, string message) => Write(time, "[WARN] " + message);

    /// <summary>
    /// Writes a message to the log with the 'ERRO' level for error log messages.
    /// </summary>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    public static void Error(string message) => Error(message);
    /// <summary>
    /// Writes a message to the log with the 'ERRO' level for error log messages.
    /// </summary>
    /// <param name="time">
    /// The time of the log message.
    /// </param>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    internal static void Error(DateTime time, string message) => Write(time, "[ERRO] " + message);

    /// <summary>
    /// Writes a message to the log with the 'NTWK' level for network log messages.
    /// </summary>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    public static void Network(string message) => Network(DateTime.Now, message);
    /// <summary>
    /// Writes a message to the log with the 'NTWK' level for network log messages.
    /// </summary>
    /// <param name="time">
    /// The time of the log message.
    /// </param>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    internal static void Network(DateTime time, string message) => Write(time, "[NTWK] " + message);

    /// <summary>
    /// Writes a message to the log with the 'ARGS' level for argument log messages.
    /// Custom providers should use this when parsing arguments.
    /// </summary>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    public static void Arguments(string message) => Arguments(DateTime.Now, message);
    /// <summary>
    /// Writes a message to the log with the 'ARGS' level for argument log messages.
    /// Custom providers should use this when parsing arguments.
    /// </summary>
    /// <param name="time">
    /// The time of the log message.
    /// </param>
    /// <param name="message">
    /// The message to be sent to the log.
    /// </param>
    internal static void Arguments(DateTime time, string message) => Write(time, "[ARGS] " + message);
    #endregion

    #region Exception handling
    // Oh god, oh fuck \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException) =>
        ReportException(ReceivedException, null, null, false, false, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, bool Retriable) =>
        ReportException(ReceivedException, null, null, Retriable, false, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Abortable">Whether the exception is abortable.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, bool Retriable, bool Abortable) =>
        ReportException(ReceivedException, null, null, Retriable, Abortable, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo) =>
        ReportException(ReceivedException, ExtraInfo, null, false, false, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, bool Retriable) =>
        ReportException(ReceivedException, ExtraInfo, null, Retriable, false, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Abortable">Whether the exception is abortable.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, bool Retriable, bool Abortable) =>
        ReportException(ReceivedException, ExtraInfo, null, Retriable, Abortable, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, string? ExtraMessage) =>
        ReportException(ReceivedException, null, ExtraMessage, false, false, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, string? ExtraMessage, bool Retriable) =>
        ReportException(ReceivedException, null, ExtraMessage, Retriable, false, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Abortable">Whether the exception is abortable.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, string? ExtraMessage, bool Retriable, bool Abortable) =>
        ReportException(ReceivedException, null, ExtraMessage, Retriable, Abortable, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, string? ExtraMessage) =>
        ReportException(ReceivedException, ExtraInfo, ExtraMessage, false, false, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, string? ExtraMessage, bool Retriable) =>
        ReportException(ReceivedException, ExtraInfo, ExtraMessage, Retriable, false, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Abortable">Whether the exception is abortable.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, string? ExtraMessage, bool Retriable, bool Abortable) =>
        ReportException(ReceivedException, ExtraInfo, ExtraMessage, Retriable, Abortable, null);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, IWin32Window? Owner) =>
        ReportException(ReceivedException, null, null, false, false, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, bool Retriable, IWin32Window? Owner) =>
        ReportException(ReceivedException, null, null, Retriable, false, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Abortable">Whether the exception is abortable.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, bool Retriable, bool Abortable, IWin32Window? Owner) =>
        ReportException(ReceivedException, null, null, Retriable, Abortable, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, IWin32Window? Owner) =>
        ReportException(ReceivedException, ExtraInfo, null, false, false, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, bool Retriable, IWin32Window? Owner) =>
        ReportException(ReceivedException, ExtraInfo, null, Retriable, false, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Abortable">Whether the exception is abortable.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, bool Retriable, bool Abortable, IWin32Window? Owner) =>
        ReportException(ReceivedException, ExtraInfo, null, Retriable, Abortable, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, string? ExtraMessage, IWin32Window? Owner) =>
        ReportException(ReceivedException, null, ExtraMessage, false, false, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, string? ExtraMessage, bool Retriable, IWin32Window? Owner) =>
        ReportException(ReceivedException, null, ExtraMessage, Retriable, false, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Abortable">Whether the exception is abortable.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, string? ExtraMessage, bool Retriable, bool Abortable, IWin32Window? Owner) =>
        ReportException(ReceivedException, null, ExtraMessage, Retriable, Abortable, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, string? ExtraMessage, IWin32Window? Owner) =>
        ReportException(ReceivedException, ExtraInfo, ExtraMessage, false, false, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, string? ExtraMessage, bool Retriable, IWin32Window? Owner) =>
        ReportException(ReceivedException, ExtraInfo, ExtraMessage, Retriable, false, Owner);

    /// <summary>
    /// Reports an exception to the user and logs it to the application.
    /// </summary>
    /// <param name="ReceivedException">The receieved exception that will be reported.</param>
    /// <param name="ExtraInfo">Optional extra information regarding the error.</param>
    /// <param name="ExtraMessage">The extra message to display on the exception form.</param>
    /// <param name="Retriable">Whether the exception is retriable.</param>
    /// <param name="Abortable">Whether the exception is abortable.</param>
    /// <param name="Owner">The Win32 window that sent the exception so the form will block its input.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    public static DialogResult ReportException(Exception ReceivedException, object? ExtraInfo, string? ExtraMessage, bool Retriable, bool Abortable, IWin32Window? Owner) {
        // Gets the time this gets called for reporting the exception time.
        DateTime ExceptionTime = DateTime.Now;

        // Create the exception info class with data relating to the exception and next actions.
        ExceptionInfo ExceptionData = new(ReceivedException, ExtraInfo, ExtraMessage, Owner) {
            AllowRetry = Retriable,
            AllowAbort = Abortable,
            CustomDescription = null,
            ExceptionTime = ExceptionTime,
            FromLanguage = false,
            SkipDwmComposition = false,
            ExceptionType = ExceptionType.Caught,
        };

        // Returns the exception forms dialog result.
        return DisplayException(ExceptionData, AllowWritingToFile);
    }

    // Functionality \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    /// <summary>
    /// Displays an exception dialog.
    /// </summary>
    /// <param name="ReceivedException">The <see cref="ExceptionInfo"/> instance.</param>
    /// <param name="CanWriteToFile">Whether to write the exception to a file.</param>
    /// <returns>The <see cref="DialogResult"/> of the displayed exception form.</returns>
    private static DialogResult DisplayException(ExceptionInfo ReceivedException, bool CanWriteToFile) {
        // Write it to the log, excluding the date.
        Error($"An {ReceivedException.Exception.GetType().Name} occurred.");

        // Adds the exception to the log form.
        AddExceptionToLog(ReceivedException);

        // Creates the exception form and displays the dialog.
        using frmException NewException = new(ReceivedException);
        DialogResult ExceptionResult = NewException.ShowDialog(ReceivedException.WindowOwner);

        // Writes the exception to a file, if enabled.
        if (CanWriteToFile) {
            WriteToFile(ReceivedException);
        }

        // Returns the dialog result.
        return ExceptionResult;
    }
    /// <summary>
    /// Adds an exception to the log form.
    /// </summary>
    /// <param name="ReceivedException">The <see cref="Exception"/> receieved.</param>
    internal static void AddExceptionToLog(ExceptionInfo ReceivedException) {
        if (LogEnabled) {
            LogForm?.AddException(ReceivedException);
        }
    }
    /// <summary>
    /// Writes an exception to a file.
    /// </summary>
    /// <param name="ReceivedException">The <see cref="ExceptionInfo"/> instance.</param>
    private static void WriteToFile(ExceptionInfo ReceivedException) {
        if (AllowWritingToFile) {
            bool RetrySaving = true;
            do {
                try {
                    System.IO.File.WriteAllText(
                        $"\\ex_{ReceivedException.ExceptionTime:yyyy-MM-dd_HH-mm-ss.fff}.log", ReceivedException.Exception.ToString());
                }
                catch (Exception SaveException) {
                    ExceptionInfo FileException = new(SaveException) {
                        AllowRetry = true,
                        WindowOwner = ReceivedException.WindowOwner
                    };
                    RetrySaving = DisplayException(FileException, false) == DialogResult.Retry;
                }
            } while (RetrySaving);
        }
    }
    #endregion
}