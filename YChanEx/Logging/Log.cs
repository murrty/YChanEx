﻿namespace murrty.classes;

using System.Diagnostics;
using System.Management;
using System.Windows.Forms;

using murrty.forms;

/// <summary>
/// This class will control the Errors that get reported in try statements.
/// </summary>
internal static class Log {

    #region Log stuff
    private static volatile frmLog LogForm = null;
    private static volatile bool LogEnabled = false;

    /// <summary>
    /// Gets a string that resolves information regarding the current computer.
    /// </summary>
    public static string ComputerVersionInformation {
        get; private set;
    } = "Not initialized";

    public static void InitializeLogging() {
        // Build up a string containing relevant information about the computer.
        Write("Creating DiagnosticInformation for caught exceptions.");
        ManagementObjectSearcher searcher = new("SELECT * FROM Win32_OperatingSystem");
        ManagementObject info = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
        ComputerVersionInformation =
            $"Current version: {YChanEx.Program.CurrentVersion}\n" +
            $"Curernt culture: {System.Threading.Thread.CurrentThread.CurrentCulture.EnglishName ?? "Unknown culture"}\n" +
            $"System Caption: {info.Properties["Caption"].Value ?? "couldn't retrieve"}\n" +
            $"Version: {info.Properties["Version"].Value ?? "couldn't retrieve"}\n" +
            $"Service Pack Major: {info.Properties["ServicePackMajorVersion"].Value ?? "couldn't retrieve"}\n" +
            $"Service Pack Minor: {info.Properties["ServicePackMinorVersion"].Value ?? "couldn't retrieve"}";

        // Catch any exceptions that are unhandled, so we can report it.
//#if !DEBUG
        try {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
//#endif

            EnableLogging();

//#if !DEBUG
            Write("Creating unhandled exception event.");
            AppDomain.CurrentDomain.UnhandledException += (sender, exception) => {
                using frmException UnrecoverableException = new(new(exception) {
                    Unrecoverable = true,
                    AllowRetry = false
                });
                UnrecoverableException.ShowDialog();
            };

            Write("Creating unhandled thread exception event.");
            Application.ThreadException += (sender, exception) => {
                using frmException UnrecoverableException = new(new(exception) {
                    Unrecoverable = true,
                    AllowRetry = false
                });
                UnrecoverableException.ShowDialog();
            };
        }
        catch (Exception ex) {
            ReportException(ex);
        }
//#endif
    }

    /// <summary>
    /// Enables the logging form.
    /// </summary>
    [DebuggerStepThrough]
    public static void EnableLogging() {
        if (LogEnabled && LogForm is not null) {
            Write("Logging is already enabled.");
        }
        else {
            LogForm = new frmLog();
            LogEnabled = true;
            Write("Logging has been enabled.", true);
        }
    }

    /// <summary>
    /// Disables the logging form, but debug logging will still occur.
    /// </summary>
    [DebuggerStepThrough]
    public static void DisableLogging() {
        if (LogEnabled) {
            Write("Disabling logging.");

            if (LogForm.WindowState == FormWindowState.Minimized || LogForm.WindowState == FormWindowState.Maximized) {
                LogForm.Opacity = 0;
                LogForm.WindowState = FormWindowState.Normal;
            }

            LogForm?.Dispose();
            LogEnabled = false;
        }
    }


    /// <summary>
    /// Shows the log form.
    /// </summary>
    [DebuggerStepThrough]
    public static void ShowLog() {
        if (LogEnabled && LogForm is not null) {
            if (LogForm.IsShown) {
                Write("The log form is already shown.");
                LogForm.Activate();
            }
            else {
                LogForm.Append("Showing log");
                LogForm.IsShown = true;
                LogForm.SetLanguage();
                LogForm.Show();
            }
        }
    }

    /// <summary>
    /// Writes a message to the log.
    /// </summary>
    /// <param name="message">The message to be sent to the log</param>
    /// <param name="initial">If the message is the initial one to be sent, does not add a new line break.</param>
    [DebuggerStepThrough]
    public static void Write(string message, bool initial = false) {
        Debug.Print(message);
        if (LogEnabled) {
            LogForm?.Append(message, initial);
        }
    }

    /// <summary>
    /// Writes a message to the log, not including date/time of the message.
    /// </summary>
    /// <param name="message">The message to be sent to the log</param>
    /// <param name="initial">If the message is the initial one to be sent, does not add a new line break.</param>
    [DebuggerStepThrough]
    public static void WriteNoDate(string message, bool initial = false) {
        Debug.Print(message);
        if (LogEnabled) {
            LogForm?.AppendNoDate(message, initial);
        }
    }
#endregion

    #region Exception handling
    /// <summary>
    /// Reports an exception to the program, writes to an error file (if enabled) and displays an exception form.
    /// </summary>
    /// <param name="ReceivedException">A runtime-resolved object that must be a typeof Exception, or it will not display any information.</param>
    /// <param name="ExtraInfo">Any extra information regarding the exception.</param>
    /// <returns>The <see cref="DialogResult"/> of the exception form.</returns>
    public static DialogResult ReportException(dynamic ReceivedException, object ExtraInfo = null) {
        Write($"A {ReceivedException.GetType().Name} occurred.");

        using frmException NewException = new(new(ReceivedException) {
            AllowRetry = false,
            ExtraInfo = ExtraInfo
        });
        return NewException.ShowDialog();
    }

    /// <summary>
    /// Reports an exception to the program that can be retried.
    /// </summary>
    /// <param name="ReceivedException">A runtime-resolved object that must be a typeof Exception, or it will not display any information.</param>
    /// <param name="ExtraInfo">Any extra information regarding the exception.</param>
    /// <returns>The <see cref="DialogResult"/> of the exception form.</returns>
    public static DialogResult ReportRetriableException(dynamic ReceivedException, object ExtraInfo = null) {
        Write($"A {ReceivedException.GetType().Name} occurred.");

        using frmException NewException = new(new(ReceivedException) {
            AllowRetry = true,
            ExtraInfo = ExtraInfo
        });
        return NewException.ShowDialog();
    }

    /// <summary>
    /// Reports an exception from a source that can be attempted again to the program, writes to an error file (if enabled) and displays an exception form.
    /// </summary>
    /// <param name="ReceivedException">A runtime-resolved object that must be a typeof Exception, or it will not display any information.</param>
    /// <param name="ExtraInfo">Any extra information regarding the exception.</param>
    /// <returns>The <see cref="DialogResult"/> of the exception form.</returns>
    public static DialogResult ReportRetriableLanguageException(dynamic ReceivedException, object ExtraInfo = null) {
        Write($"A {ReceivedException.GetType().Name} occurred.");

        using frmException NewException = new(new(ReceivedException) {
            AllowRetry = true,
            ExtraInfo = ExtraInfo,
            FromLanguage = true
        });
        return NewException.ShowDialog();
    }
    #endregion

}