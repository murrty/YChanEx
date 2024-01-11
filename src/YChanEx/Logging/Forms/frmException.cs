﻿namespace murrty.forms;

using System.Drawing;
using System.Net;

using System.Threading;
using System.Security.Cryptography;
using System.Windows.Forms;
using murrty.classes;

public partial class frmException : Form {
    /// <summary>
    /// The private ExceptionInfo holding information about the exception.
    /// </summary>
    private ExceptionInfo ReportedException { get; }

    private readonly DwmComposition DwmCompositor;
    private readonly DwmCompositionInfo DwmInfo;

    public frmException(ExceptionInfo ReportedException) {
        this.ReportedException = ReportedException;
        InitializeComponent();
        this.Icon = global::YChanEx.Properties.Resources.ProgramIcon;
        this.ShowIcon = true;
        LoadLanguage();
        rtbExtraData.Clear();

        // Check if there is a valid github link
        btnExceptionGithub.Enabled = true;
        btnExceptionGithub.Visible = true;

        // Check if allow retry.
        if (!ReportedException.AllowRetry) {
            btnExceptionRetry.Enabled = false;
            btnExceptionRetry.Visible = false;
            btnExceptionGithub.Location = btnExceptionRetry.Location;
            lbDate.Location = new(btnExceptionGithub.Location.X - 119, lbDate.Location.Y);
        }
        else {
            btnExceptionRetry.Enabled = true;
            btnExceptionRetry.Visible = true;
        }

        // Add the date
        lbDate.Text = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}";

        if (DwmComposition.CompositionSupported && !ReportedException.SkipDwmComposition) {
            DwmCompositor = new();
            DwmInfo = new(
                this.Handle,
                new() {
                    m_Top = pnDWM.Height,
                    m_Bottom = 0,
                    m_Left = 0,
                    m_Right = 0
                },
                new(
                    pnDWM.Location.X,
                    pnDWM.Location.Y,
                    this.MaximumSize.Width,
                    pnDWM.Size.Height
                ),
                new(
                    lbExceptionHeader.Text,
                    lbExceptionHeader.Font,
                    Color.FromKnownColor(KnownColor.ActiveCaptionText),
                    10,
                    new(
                        lbExceptionHeader.Location.X,
                        lbExceptionHeader.Location.Y,
                        lbExceptionHeader.Size.Width,
                        lbExceptionHeader.Size.Height
                    )
                )
            );

            pnDWM.Visible = false;
            lbExceptionHeader.Visible = false;
            DwmCompositor.ExtendFrame(DwmInfo);
            this.Paint += (s, e) => {
                DwmCompositor.FillBlackRegion(DwmInfo);
                DwmCompositor.DrawTextOnGlass(DwmInfo, DwmInfo.Text);
            };
            this.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left && DwmInfo.DwmRectangle.Contains(e.Location)) {
                    DwmCompositor.MoveForm(DwmInfo);
                }
            };
        }
        else {
            lbExceptionHeader.Visible = true;
            pnDWM.BackColor = Color.FromKnownColor(KnownColor.Menu);
            pnDWM.Visible = true;
        }

        this.Shown += (s, e) => System.Media.SystemSounds.Hand.Play();
    }

    private void LoadLanguage() {
        // Just fuck my shit up
        RandomNumberGenerator RNG = new RNGCryptoServiceProvider();
        byte[] ByteData = new byte[sizeof(double)];
        RNG.GetBytes(ByteData);
        uint RandUint = BitConverter.ToUInt32(ByteData, 0);
        switch ((int)Math.Floor(0 + (((double)2500 - 0) * (RandUint / (uint.MaxValue + 1.0))))) {
            case 621: {
                this.Text = "wychawnex exception wepowt unu";
                lbExceptionHeader.Text = $"An {(ReportedException.Unrecoverable ? "unwecovewabwe exception" : "exception")} occowwed qwq";
                lbExceptionDescription.Text = $"The pwogwam accidentawy made a fucky wucky{(ReportedException.Unrecoverable ? " n must exiwt" : "")}";
                rtbExceptionDetails.Text = "Sowwy for fucky wucky, u can powst dis as a new issue on githuwb :3";
                btnExceptionGithub.Text = "Githuwb >w<";
                btnExceptionRetry.Text = "Retwy";
                btnExceptionOk.Text = "Okie uwu";
                tabExceptionDetails.Text = "Detaiws";
                tabExceptionExtraInfo.Text = "Extwa info";
            } break;

            default: {
                this.Text = "YChanEx exception report";
                lbExceptionHeader.Text = $"An {(ReportedException.Unrecoverable ? "unrecoverable exception" : "exception")} occurred";
                lbExceptionDescription.Text = $"The program {(ReportedException.Unrecoverable ? "ran into an unhandled exception and must exit." : "caught an exception.")}";
                rtbExceptionDetails.Text = "";
                btnExceptionGithub.Text = "Github";
                btnExceptionRetry.Text = "Retry";
                btnExceptionOk.Text = "OK";
                tabExceptionDetails.Text = "Exception details";
                tabExceptionExtraInfo.Text = "Extra exception info";
            } break;
        }
    }

    private void frmError_Load(object sender, EventArgs e) {
        // We need to figure out what exception occurred.
        // If the custom description is null, we can generate one.
        if (ReportedException.CustomDescription is null) {
            switch (ReportedException.ReceivedException) {
                //case UnhandledExceptionEventArgs UnEx: {
                //    rtbExceptionDetails.Text = "An unhandled exception occurred.\n" +
                //                              $"The application will exit after closing this dialog.\n\n" +
                //                              $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                //                              $"{UnEx.ExceptionObject}\n";
                //} break;

                //case ThreadExceptionEventArgs ThrEx: {
                //    rtbExceptionDetails.Text = "An unhandled thread exception occurred.\n" +
                //                              $"The application will exit after closing this dialog.\n\n" +
                //                              $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                //                              $"{ThrEx.Exception}\n";
                //} break;

                case ThreadAbortException ThrAbrEx: {
                    rtbExceptionDetails.Text = (ReportedException.Unrecoverable ? "An unrecoverable ThreadAbortException occurred, and the application will exit." : "A caught ThreadAbortException occurred.") + "\n\n" +
                                              "This exception may have been pushed here on accident.\n" +
                                              $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                                              $"Message: {ThrAbrEx.Message}\n" +
                                              $"Stacktrace: {ThrAbrEx.StackTrace}\n" +
                                              $"Source: {ThrAbrEx.Source}\n" +
                                              $"Target Site: {ThrAbrEx.TargetSite}\n" +
                                              $"Inner Exception: {ThrAbrEx.InnerException}\n";
                }
                break;

                case WebException WebEx: {
                    rtbExceptionDetails.Text = (ReportedException.Unrecoverable ? "An unrecoverable WebException occurred, and the application will exit." : "A caught WebException occured.") + "\n\n" +
                                              $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                                              $"Message: {WebEx.Message}\n" +
                                              $"Stacktrace: {WebEx.StackTrace}\n" +
                                              $"Source: {WebEx.Source}\n" +
                                              $"Target Site: {WebEx.TargetSite}\n" +
                                              $"Inner Exception: {WebEx.InnerException}\n" +
                                              $"Response: {WebEx.Response}\n";
                }
                break;

                case Exception Ex: {
                    rtbExceptionDetails.Text = (ReportedException.Unrecoverable ? $"An unrecoverable {ReportedException.ReceivedException.GetType().Name} occurred, and the application will exit." : $"A caught {ReportedException.ReceivedException.GetType().Name} occured.") + "\n\n" +
                                              $"{(ReportedException.ExtraMessage is not null ? $"{ReportedException.ExtraMessage}\n\n" : "")}" +
                                              $"Message: {Ex.Message}\n" +
                                              $"Stacktrace: {Ex.StackTrace}\n" +
                                              $"Source: {Ex.Source}\n" +
                                              $"Target Site: {Ex.TargetSite}\n" +
                                              $"Inner Exception: {Ex.InnerException}\n";
                }
                break;

                default: {
                    rtbExceptionDetails.Text = "An uncast exception occurred. The updater may exit after this dialog closes." +
                                              $"{(ReportedException.ExtraMessage is not null ? $"\n\n{ReportedException.ExtraMessage}" : "")}\n";
                }
                break;
            }

            rtbExceptionDetails.Text += "\n========== FULL REPORT ==========\n" +
                                        ReportedException.ReceivedException +
                                        "\n========== END  REPORT ==========\n";
        }
        // A custom description was set, so we aren't going to write anything except for
        // what was written in the custom descrption.
        else {
            rtbExceptionDetails.Text = $"{ReportedException.CustomDescription}\n";
        }

        if (ReportedException.ExtraInfo is not null) {
            string ExtraInfo = ReportedException.ExtraInfo.ToString();
            rtbExtraData.Text = ExtraInfo.Length > 0 ? ExtraInfo : "Extra info was provided, but it doesn't contain data.";
        }
        else {
            rtbExtraData.Text = "No extra info was provided.";
        }

        rtbExceptionDetails.Text += "\n========== OS  INFO ==========\n" +
                                    "(Please don't omit this info, it may be important)\n" +
                                    Log.ComputerVersionInformation +
                                    "\n========== END INFO ==========";

        lbVersion.Text = "v" + YChanEx.Program.CurrentVersion.ToString();
    }

    private void btnExceptionGithub_Click(object sender, EventArgs e) =>
        System.Diagnostics.Process.Start("https://github.com/murrty/ychanex/issues");
}
