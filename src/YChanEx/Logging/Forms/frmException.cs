#nullable enable
namespace murrty.logging;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using System.Windows.Forms;
using murrty.classes;
using murrty.controls;
public partial class frmException : Form {
    private ExceptionInfo ReportedException { get; }
    private DwmCompositionInfo? DwmInfo { get; }
    private bool UwU_ify { get; }

    internal frmException(ExceptionInfo ReportedException) {
        if (ReportedException is null) {
            MessageBox.Show("The reported exception is null and the exception cannot be displayed.", "YChanEx");
            this.Load += (_, _) => this.Dispose();
            throw new Exception();
        }

        this.ReportedException = ReportedException;
        InitializeComponent();

        this.MaximumSize = new(1280, 720);
        rtbExtraData.Clear();

        if (ReportedException.ExtraInfo is null) {
            rtbExtraData.Dispose();
            tabExceptionExtraInfo.Dispose();
        }

        // Roll for UwU-ification.
        RandomNumberGenerator RNG = new RNGCryptoServiceProvider();
        byte[] ByteData = new byte[sizeof(double)];
        RNG.GetBytes(ByteData);
        uint RandUint = BitConverter.ToUInt32(ByteData, 0);
        int GeneratedNumber = (int)Math.Floor(5000d * (RandUint / (uint.MaxValue + 1.0)));
        UwU_ify = GeneratedNumber == 621;
        Log.Debug($"The generated exception number was {GeneratedNumber}.");

        // Set the language.
        LoadLanguage();

        // Sets the location for the date label.
        Point GetDateLocation(Control NeighborControl) =>
            new(NeighborControl.Location.X - (lbDate.Size.Width + lbDate.Margin.Right + NeighborControl.Margin.Left), lbDate.Location.Y);

        // Check if allow retry.
        if (ReportedException.AllowRetry) {
            btnExceptionRetry.Enabled = btnExceptionRetry.Visible = true;
            if (ReportedException.AllowAbort) {
                btnExceptionAbort.Enabled = btnExceptionAbort.Visible = true;
                lbDate.Location = GetDateLocation(btnExceptionAbort);
            }
            else {
                btnExceptionAbort.Enabled = btnExceptionAbort.Visible = false;
                lbDate.Location = GetDateLocation(btnExceptionRetry);
            }
        }
        else {
            btnExceptionRetry.Enabled = btnExceptionRetry.Visible = false;
            if (ReportedException.AllowAbort) {
                btnExceptionAbort.Location = btnExceptionRetry.Location;
                btnExceptionAbort.Enabled = btnExceptionAbort.Visible = true;
                lbDate.Location = GetDateLocation(btnExceptionAbort);
            }
            else {
                btnExceptionAbort.Enabled = btnExceptionAbort.Visible = false;
                lbDate.Location = GetDateLocation(btnExceptionOk);
            }
        }

        // Add the date
        lbDate.Text = $"{ReportedException.ExceptionTime:yyyy/MM/dd HH:mm:ss}";

        if (DwmComposition.CompositionSupported && !ReportedException.SkipDwmComposition) {
            DwmInfo = new(
                hWnd: this.Handle,
                Margins: new() {
                    m_Top = pnDWM.Height,
                    m_Bottom = 0,
                    m_Left = 0,
                    m_Right = 0
                },
                DwmRectangle: new(
                    pnDWM.Location.X,
                    pnDWM.Location.Y,
                    pnDWM.Size.Width,
                    pnDWM.Size.Height),
                NewInfo: new(
                    text: lbExceptionHeader.Text,
                    font: lbExceptionHeader.Font,
                    color: Color.FromKnownColor(KnownColor.ActiveCaptionText),
                    glowsize: 10,
                    rectangle: new(
                        lbExceptionHeader.Location.X,
                        lbExceptionHeader.Location.Y,
                        lbExceptionHeader.Size.Width,
                        lbExceptionHeader.Size.Height))
            );

            pnDWM.Visible = false;
            lbExceptionHeader.Visible = false;
            //DwmComposition.ExtendFrame(DwmInfo);
            this.Paint += (_, _) => {
                DwmInfo.DwmRectangle.Width = pnDWM.Size.Width;
                DwmInfo.Rect.right = pnDWM.Size.Width;
                DwmInfo.dib.bmiHeader.biWidth = DwmInfo.Rect.right - DwmInfo.Rect.left;
                DwmComposition.ExtendFrame(DwmInfo);
                DwmComposition.FillBlackRegion(DwmInfo);
                if (DwmInfo.Text is not null) {
                    DwmComposition.DrawTextOnGlass(DwmInfo, DwmInfo.Text);
                }
            };
            this.MouseDown += (_, e) => {
                if (e.Button == MouseButtons.Left && DwmInfo.DwmRectangle.Contains(e.Location)) {
                    DwmComposition.MoveForm(DwmInfo);
                }
            };
        }
        else {
            lbExceptionHeader.Visible = true;
            pnDWM.BackColor = Color.FromKnownColor(KnownColor.Menu);
            pnDWM.Visible = true;
        }

        this.Shown += (_, _) => System.Media.SystemSounds.Hand.Play();
    }

    private void LoadLanguage() {
        if (UwU_ify) {
            this.Text = "Exception occowwed unu";
            lbExceptionHeader.Text = "An exception occowwed qwq";
            lbExceptionDescription.Text = "The pwogwam accidentawy made a fucky wucky";
            rtbExceptionDetails.Text = "Sowwy for fucky wucky, u can powst dis as a new issue on githuwb :3";
            btnExceptionRetry.Text = "Retwy";
            btnExceptionOk.Text = "Okie uwu";
            btnExceptionAbort.Text = "Abowot";
            tabExceptionDetails.Text = "Detaiws";
            tabExceptionExtraInfo.Text = "Extwa info";
        }
        else {
            this.Text = "YChanEx exception report";
            lbExceptionHeader.Text = "An exception occurred";
            lbExceptionDescription.Text = $"The program {(ReportedException.ExceptionType == ExceptionType.Unhandled ? "ran into an unhandled exception and must exit." : "caught an exception.")}";
            rtbExceptionDetails.Text = "Details will be posted below.";
            btnExceptionRetry.Text = "Retry";
            btnExceptionOk.Text = "OK";
            btnExceptionAbort.Text = "Abort";
            tabExceptionDetails.Text = "Exception details";
            tabExceptionExtraInfo.Text = "Extra exception info";
        }
    }

    private void frmError_Load(object? sender, EventArgs e) {
        // A custom description was set, so we aren't going to write anything except for
        // what was written in the custom descrption.
        if (ReportedException.CustomDescription is not null) {
            rtbExceptionDetails.Text = $"{ReportedException.CustomDescription}\n";
        }

        // We need to figure out what exception occurred.
        // If the custom description is null, we can generate one.
        else if (ReportedException.Exception is not null) {
            rtbExceptionDetails.Text = ReportedException.ExceptionType switch {
                ExceptionType.Caught => $"A caught {ReportedException.Exception.GetType().Name} occurred.",
                ExceptionType.Unhandled => $"An unrecoverable {ReportedException.Exception.GetType().Name} occurred and the application will exit.",
                ExceptionType.ThreadException => $"An uncaught {ReportedException.Exception.GetType().Name} occurred and the application may resume.",
                _ => $"A caught {ReportedException.Exception.GetType().Name} occurred and the state of the application is undeterminable.",
            } + "\n\n";

            switch (ReportedException.Exception) {
                // Abort/Cancellation related exceptions.
                case ThreadAbortException ThrAbrEx: {
                    rtbExceptionDetails.AppendText("This exception may have been pushed here on accident.\n");

                    if (ReportedException.ExtraMessage is not null)
                        rtbExceptionDetails.AppendText(ReportedException.ExtraMessage + "\n");

                    rtbExceptionDetails.AppendText($$"""
                        Message: {{ThrAbrEx.Message}}
                        Source: {{ThrAbrEx.Source}}
                        Target Site: {{ThrAbrEx.TargetSite}}
                        Stacktrace:
                        {{ThrAbrEx.StackTrace}}
                        """ + (ThrAbrEx.InnerException is not null ? "\nInner Exception: " + ThrAbrEx.InnerException : "") + "\n");
                } break;
                case TaskCanceledException TkCdEx: {
                    rtbExceptionDetails.AppendText("This exception may have been pushed here on accident.\n");

                    if (ReportedException.ExtraMessage is not null)
                        rtbExceptionDetails.AppendText(ReportedException.ExtraMessage + "\n");

                    rtbExceptionDetails.AppendText($$"""
                        Message: {{TkCdEx.Message}}
                        Source: {{TkCdEx.Source}}
                        Target Site: {{TkCdEx.TargetSite}}
                        Stacktrace:
                        {{TkCdEx.StackTrace}}
                        """ + (TkCdEx.InnerException is not null ? "\nInner Exception: " + TkCdEx.InnerException : "") + "\n");
                } break;
                case OperationCanceledException OpCdEx: {
                    rtbExceptionDetails.AppendText("This exception may have been pushed here on accident.\n");

                    if (ReportedException.ExtraMessage is not null)
                        rtbExceptionDetails.AppendText(ReportedException.ExtraMessage + "\n");

                    rtbExceptionDetails.AppendText($$"""
                        Message: {{OpCdEx.Message}}
                        Source: {{OpCdEx.Source}}
                        Target Site: {{OpCdEx.TargetSite}}
                        Stacktrace:
                        {{OpCdEx.StackTrace}}
                        """ + (OpCdEx.InnerException is not null ? "\nInner Exception: " + OpCdEx.InnerException : "") + "\n");
                } break;

                // Possible exceptions.
                case WebException WebEx: {
                    if (ReportedException.ExtraMessage is not null)
                        rtbExceptionDetails.AppendText(ReportedException.ExtraMessage + "\n");

                    rtbExceptionDetails.AppendText($$"""
                        Message: {{WebEx.Message}}
                        Response: {{WebEx.Response}}
                        Source: {{WebEx.Source}}
                        Target Site: {{WebEx.TargetSite}}
                        Stacktrace:
                        {{WebEx.StackTrace}}
                        """ + (WebEx.InnerException is not null ? "\nInner Exception: " + WebEx.InnerException : "") + "\n");
                } break;
                case HttpException HttpEx: {
                    if (ReportedException.ExtraMessage is not null)
                        rtbExceptionDetails.AppendText(ReportedException.ExtraMessage + "\n");

                    rtbExceptionDetails.AppendText($$"""
                        StatusCode: {{(int)HttpEx.StatusCode}} - {{HttpEx.StatusDescription}}
                        Source: {{HttpEx.Source}}
                        Target Site: {{HttpEx.TargetSite}}
                        Stacktrace:
                        {{HttpEx.StackTrace}}
                        """ + (HttpEx.InnerException is not null ? "\nInner Exception: " + HttpEx.InnerException : "") + "\n");
                } break;

                case Exception Ex: {
                    if (ReportedException.ExtraMessage is not null)
                        rtbExceptionDetails.AppendText(ReportedException.ExtraMessage + "\n");

                    rtbExceptionDetails.AppendText($$"""
                        Message: {{Ex.Message}}
                        Type: {{Ex.GetType().FullName}}
                        Source: {{Ex.Source}}
                        Target Site: {{Ex.TargetSite}}
                        Stacktrace:
                        {{Ex.StackTrace}}
                        """ + (Ex.InnerException is not null ? "\nInner Exception: " + Ex.InnerException : "") + "\n");
                } break;

                default: {
                    rtbExceptionDetails.Text = ReportedException.ExceptionType switch {
                        ExceptionType.Caught => "A caught unknown-typed exception occured.",
                        ExceptionType.Unhandled => "An unrecoverable unknown-typed exception occurred, and the application may exit.",
                        ExceptionType.ThreadException => "An uncaught unknown-typed exception occurred and the application may resume.",
                        _ => "A caught unknown-typed exception occurred and the state of the application is undeterminable.",
                    } + "\n\n" + $"{(ReportedException.ExtraMessage is not null ? $"\n\n{ReportedException.ExtraMessage}" : "")}\n";
                } break;
            }
        }

        // The exception itself is null, but the reported data is not.
        else {
            rtbExceptionDetails.Text = "An exception occurred, but the received exception is null.";
        }

        // Add the OS info to the end of the main exception display.
        rtbExceptionDetails.AppendText("\n" + $$"""
                ========== OS  INFO ==========
                {{Log.ComputerVersionInformation}}
                ========== END INFO ==========
                """);

        // Display the extra info, as a ToString value.
        if (ReportedException.ExtraInfo is not null) {
            rtbExtraData.Text = ReportedException.ExtraInfo.ToString() ?? "Extra info was provided, but it doesn't contain string data.";
        }

        // Sets the version of the program to the exception form.
        lbVersion.Text = "v" + YChanEx.Program.CurrentVersion.ToString();
    }

    private void btnExceptionGithub_Click(object sender, EventArgs e) =>
        System.Diagnostics.Process.Start("https://github.com/murrty/ychanex/issues");
}
