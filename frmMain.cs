using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmMain : Form, IFormInterface {

        #region Variables
        private readonly List<frmDownloader> Threads = new List<frmDownloader>();   // The list of Thread download forms
        private readonly List<string> ThreadURLs = new List<string>();              // The list of Thread URLs

        private Thread ThreadLoader;            // The Thread for reloading saved threads

        private bool Icon404WasShown = false;   // Determines if the 404 icon has been shown on the tray.
        private bool ThreadsModified = false;   // Determines if the threads lists were modified to resave them.
        #endregion

        #region Usability methods
        /// <summary>
        /// Reloads the saved threads in the last program queue.
        /// </summary>
        public void ReloadSavedThreads() {
            ThreadLoader = new Thread(() => {
                try {
                    if (General.Default.SaveQueueOnExit && !Program.IsDebug) {
                        if (File.Exists(Program.ApplicationFilesLocation + "\\threads.xml")) {
                            List<SavedThreadInfo> Threads = ProgramSettings.LoadThreads();
                            for (int i = 0; i < Threads.Count; i++) {
                                this.Invoke((Action)delegate {
                                    AddNewThread(Threads[i]);
                                });
                            }
                        }
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
            }) {
                Name = "Saved threads reloader"
            };
            ThreadLoader.Start();
        }

        /// <summary>
        /// Loads threads from the arguments.
        /// </summary>
        public void CheckArgumentsForThreads() {
            foreach (string argument in Environment.GetCommandLineArgs()) {
                if (argument.StartsWith("ychanex:")) {
                    this.Invoke((Action)delegate {
                        AddNewThread(argument.Substring(8));
                    });
                }
            }
        }
        
        /// <summary>
        /// Sets the thread status from another thread handle to change the status on the main form.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread to find the index in the listview.</param>
        /// <param name="NewStatus">The new custom status to be set onto it.</param>
        public void SetItemStatus(string ThreadURL, ThreadStatus Status) {
            int ThreadIndex = ThreadURLs.IndexOf(ThreadURL);
            switch (Status) {
                case ThreadStatus.ThreadRetrying:
                    Threads[ThreadIndex].CurrentThread.OverallStatus = ThreadStatus.ThreadIsAlive;
                    ThreadsModified = true;
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Retrying";
                    break;
                case ThreadStatus.Waiting:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Waiting";
                    break;
                case ThreadStatus.ThreadNotModified:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "No new posts";
                    break;
                case ThreadStatus.ThreadScanning:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Scanning";
                    break;
                case ThreadStatus.ThreadDownloading:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Downloading";
                    break;
                case ThreadStatus.ThreadIs404:
                    Threads[ThreadIndex].CurrentThread.OverallStatus = Status;
                    niTray.BalloonTipText = Threads[ThreadIndex].CurrentThread.ThreadID + " on /" + Threads[ThreadIndex].CurrentThread.ThreadBoard + "/ has 404'd";
                    switch (Threads[ThreadIndex].CurrentThread.Chan) {
                        case ChanType.FourChan:
                            niTray.BalloonTipTitle = "4chan";
                            break;
                        case ChanType.FourTwentyChan:
                            niTray.BalloonTipTitle = "420chan";
                            break;
                        case ChanType.SevenChan:
                            niTray.BalloonTipTitle = "7chan";
                            break;
                        case ChanType.EightChan:
                            niTray.BalloonTipTitle = "8chan";
                            break;
                        case ChanType.EightKun:
                            niTray.BalloonTipTitle = "8kun";
                            break;
                        case ChanType.fchan:
                            niTray.BalloonTipTitle = "fchan";
                            break;
                        case ChanType.u18chan:
                            niTray.BalloonTipTitle = "u18chan";
                            break;
                        default:
                            niTray.BalloonTipTitle = "Thread 404";
                            break;
                    }
                    if (changeTray.Enabled) {
                        changeTray.Stop();
                    }
                    niTray.Icon = Properties.Resources.YChanEx404;
                    Icon404WasShown = true;
                    changeTray.Start();
                    niTray.ShowBalloonTip(5000);
                    ThreadsModified = true;
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "404'd";
                    break;
                case ThreadStatus.ThreadIsAborted:
                    Threads[ThreadIndex].CurrentThread.OverallStatus = Status;
                    ThreadsModified = true;
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Aborted";
                    break;
                case ThreadStatus.ThreadIsArchived:
                    Threads[ThreadIndex].CurrentThread.OverallStatus = Status;
                    ThreadsModified = true;
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Archived";
                    break;
                case ThreadStatus.ThreadReloaded:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Reloaded";
                    break;
                case ThreadStatus.ThreadUpdateName:
                    if (Threads[ThreadIndex].CurrentThread.RetrievedThreadName) {
                        lvThreads.Items[ThreadIndex].SubItems[clName.Index].Text = Threads[ThreadIndex].CurrentThread.ThreadName;
                    }
                    else {
                        lvThreads.Items[ThreadIndex].SubItems[clName.Index].Text = ThreadURL;
                    }
                    break;
                default:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Unknown?";
                    break;
            }
            GC.Collect();
        }

        /// <summary>
        /// Adds a new thread to the queue via URL.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread that will be added to the queue.</param>
        /// <returns>Boolean based on it's success</returns>
        private bool AddNewThread(string ThreadURL, bool FromTray = false) {
            if (ThreadURL.StartsWith("view-source:")) { ThreadURL = ThreadURL.Substring(12); }

            if (Chans.SupportedChan(ThreadURL)) {
                if (ThreadURLs.Contains(ThreadURL)) {
                    int ThreadURLIndex = ThreadURLs.IndexOf(ThreadURL);
                    if (Threads[ThreadURLIndex].CurrentThread.OverallStatus != ThreadStatus.ThreadIsAlive) {
                        Threads[ThreadURLIndex].ManageThread(ThreadEvent.RetryDownload);
                    }
                    return true;
                }
                else {
                    frmDownloader newThread = new frmDownloader(this);
                    ThreadInfo NewInfo = new ThreadInfo();
                    NewInfo.Chan = Chans.GetChanType(ThreadURL);
                    switch (NewInfo.Chan) {
                        case ChanType.fchan:
                            if (!Downloads.Default.fchanWarning) {
                                MessageBox.Show(
                                    "fchan works, but isn't supported. I'm keeping this in for people, but here's your only warning: I will not help with any issues regarding fchan, and they will not be acknowledged.\n\n" +
                                    "The reason I'm not going to continue working on fchan is because of all the logic shenanigans I have to do to get files, and even then it's still not perfect for some files.\n\n\n" +
                                    "I might fix it and update it later, but I'm not going to touch it anymore. You're on your own with it.\n\n" +
                                    "This is the only time this warning will appear.");
                                Downloads.Default.fchanWarning = true;
                                Downloads.Default.Save();
                            }
                            break;
                        case ChanType.None:
                            newThread.Dispose();
                            return false;
                    }
                    newThread.Name = ThreadURL;
                    NewInfo.ThreadURL = ThreadURL;
                    NewInfo.OverallStatus = ThreadStatus.ThreadIsAlive;
                    newThread.CurrentThread = NewInfo;
                    newThread.ManageThread(ThreadEvent.ParseForInfo);
                    newThread.Show();

                    ListViewItem lvi = new ListViewItem();
                    lvi.Name = ThreadURL;
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.ImageIndex = (int)NewInfo.Chan;
                    lvi.SubItems[clStatus.Index].Text = "Creating";
                    lvi.SubItems[clThread.Index].Text = "/" + NewInfo.ThreadBoard + "/ - " + NewInfo.ThreadID;
                    lvi.SubItems[clName.Index].Text = ThreadURL;

                    lvThreads.Items.Add(lvi);

                    ThreadURLs.Add(ThreadURL);
                    Threads.Add(newThread);

                    if (chkCreateThreadInTheBackground.Checked) {
                        newThread.Hide();
                    }

                    if (FromTray) {
                        newThread.Hide();
                        niTray.BalloonTipText = "Added /" + NewInfo.ThreadBoard + "/" + NewInfo.ThreadID + "/ to the queue";
                        niTray.ShowBalloonTip(5000);
                    }

                    newThread.Opacity = 100;
                    newThread.ShowInTaskbar = true;

                    newThread.ManageThread(ThreadEvent.StartDownload);
                    ThreadsModified = true;

                    niTray.Text = "YChanEx - " + lvThreads.Items.Count + " threads";
                    return true;
                }
            }
            else {
                return false;
            }
        }
        /// <summary>
        /// Adds a new thread to the queue via saved threads.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        private bool AddNewThread(SavedThreadInfo Info) {
            if (Chans.SupportedChan(Info.ThreadURL) && !ThreadURLs.Contains(Info.ThreadURL)) {
                frmDownloader newThread = new frmDownloader(this);
                ThreadInfo NewInfo = new ThreadInfo() {
                    Chan = Chans.GetChanType(Info.ThreadURL)
                };
                switch (NewInfo.Chan) {
                    case ChanType.None:
                        newThread.Dispose();
                        return false;
                }
                newThread.Name = Info.ThreadURL;
                NewInfo.ThreadURL = Info.ThreadURL;
                NewInfo.RetrievedThreadName = Info.RetrievedThreadName;
                NewInfo.ThreadName = Info.ThreadName;
                NewInfo.SetCustomName = Info.SetCustomName;
                NewInfo.CustomName = Info.CustomName;
                newThread.CurrentThread = NewInfo;
                newThread.ManageThread(ThreadEvent.ParseForInfo);
                newThread.Show();
                newThread.Hide();

                ListViewItem lvi = new ListViewItem {
                    Name = Info.ThreadURL
                };
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                lvi.ImageIndex = (int)NewInfo.Chan;
                lvi.SubItems[clStatus.Index].Text = "Creating";
                lvi.SubItems[clThread.Index].Text = "/" + NewInfo.ThreadBoard + "/ - " + NewInfo.ThreadID;

                lvThreads.Items.Add(lvi);

                newThread.Opacity = 100;
                newThread.ShowInTaskbar = true;

                ThreadURLs.Add(Info.ThreadURL);
                NewInfo.OverallStatus = ThreadStatus.ThreadIsAlive;
                Threads.Add(newThread);
                if (Info.SetCustomName) {
                    lvi.SubItems[clName.Index].Text = Info.CustomName;
                    newThread.CurrentThread.SetCustomName = true;
                }
                else {
                    if (Info.RetrievedThreadName) {
                        lvi.SubItems[clName.Index].Text = Info.ThreadName;
                    }
                    else {
                        lvi.SubItems[clName.Index].Text = Info.ThreadURL;
                    }
                }

                newThread.ManageThread(ThreadEvent.StartDownload);

                niTray.Text = "YChanEx - " + lvThreads.Items.Count + " threads";
                return true;
            }

            return false;
        }


        /// <summary>
        /// The method to determine if the application should exit, while also performing last-second saving for threads and form location/size.
        /// </summary>
        /// <returns>True, if the application is allowed to exit. False, if the application isn't allowed to exit.</returns>
        private bool ApplicationShouldExit() {
            if (this.Visible) {
                if (General.Default.MinimizeInsteadOfExiting) {
                    this.WindowState = FormWindowState.Normal;
                    this.Hide();
                    niTray.Visible = true;
                    return false;
                }
                else {
                    if (General.Default.ShowExitWarning && lvThreads.Items.Count > 0) {
                        switch (MessageBox.Show("You have threads currently in the queue. Would you like to minimize instead of exit?", "YChanEx", MessageBoxButtons.YesNoCancel)) {
                            case DialogResult.Yes:
                                if (General.Default.MinimizeToTray) {
                                    this.Hide();
                                    if (!niTray.Visible) {
                                        niTray.Visible = true;
                                    }
                                }
                                else {
                                    this.WindowState = FormWindowState.Minimized;
                                }
                                return false;
                            case DialogResult.Cancel:
                                return false;
                        }
                    }
                }
            }

            if (General.Default.SaveQueueOnExit && !Program.IsDebug) {
                if (ThreadsModified) {
                    if (ThreadURLs.Count > 0) {
                        List<ThreadInfo> Infos = new List<ThreadInfo>();
                        for (int i = 0; i < ThreadURLs.Count; i++) {
                            Infos.Add(Threads[i].CurrentThread);
                        }
                        ProgramSettings.SaveThreads(Infos);
                    }
                }
            }

            if (ThreadLoader != null && ThreadLoader.IsAlive) {
                ThreadLoader.Abort();
            }

            if (this.Location.X != -32000 && this.Location.Y != -32000) {
                Saved.Default.MainFormLocation = this.Location;
            }
            Saved.Default.MainFormSize = this.Size;
            Saved.Default.CreateThreadInTheBackground = chkCreateThreadInTheBackground.Checked;
            Saved.Default.MainFormColumnSizes = ProgramSettings.GetColumnSizes(clIcon.Width, clStatus.Width, clThread.Width, clName.Width);
            Saved.Default.Save();

            for (int i = 0; i < Threads.Count; i++) {
                Threads[i].ManageThread(ThreadEvent.AbortForClosing);
                Threads[i].Dispose();
            }
            ThreadURLs.Clear();
            Threads.Clear();
            niTray.Visible = false;

            return true;
        }
        #endregion

        #region Form Controls
        protected override void WndProc(ref Message m) {
            switch (m.Msg) {
                case Win32.WM_COPYDATA:
                    Win32.CopyDataStruct RecievedData = (Win32.CopyDataStruct)Marshal.PtrToStructure(m.LParam, typeof(Win32.CopyDataStruct));
                    string[] Threads = Marshal.PtrToStringUni(RecievedData.lpData).Split('|');
                    int ThreadCount = 0;
                    foreach (string Thread in Threads) {
                        AddNewThread(Thread);
                        ThreadCount++;
                    }
                    if (niTray.Visible) {
                        niTray.BalloonTipText = "Added " + ThreadCount + " thread(s) to the queue.";
                        niTray.ShowBalloonTip(5000);
                    }
                    break;

                case Win32.WM_SHOWFORM:
                    this.WindowState = FormWindowState.Normal;
                    this.Show();
                    this.Focus();
                    if (!General.Default.ShowTrayIcon) {
                        niTray.Visible = false;
                    }
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        public frmMain() {
            InitializeComponent();
            ilIcons.Images.Add(Properties.Resources._4chan);
            ilIcons.Images.Add(Properties.Resources._420chan);
            ilIcons.Images.Add(Properties.Resources._7chan);
            ilIcons.Images.Add(Properties.Resources._8chan);
            ilIcons.Images.Add(Properties.Resources._8kun);
            ilIcons.Images.Add(Properties.Resources._fchan);
            ilIcons.Images.Add(Properties.Resources._u18chan);
            lvThreads.SmallImageList = ilIcons;
            lvThreads.ContextMenu = cmThreads;
        }
        private void frmMain_Load(object sender, EventArgs e) {
            if (General.Default.ShowTrayIcon) {
                niTray.Visible = true;
            }
            niTray.ContextMenu = cmTray;

            if (Saved.Default.MainFormLocation.X != -32000 && Saved.Default.MainFormLocation.Y != -32000) {
                this.Location = Saved.Default.MainFormLocation;
            }
            if (Saved.Default.MainFormSize.Width != -32000 && Saved.Default.MainFormSize.Height != -32000) {
                this.Size = Saved.Default.MainFormSize;
            }
            if (!string.IsNullOrEmpty(Saved.Default.MainFormColumnSizes)) {
                List<int> Sizes = ProgramSettings.GetColumnSizes(Saved.Default.MainFormColumnSizes.Split('|'));
                clIcon.Width = Sizes[0];
                clStatus.Width = Sizes[1];
                clThread.Width = Sizes[2];
                clName.Width = Sizes[3];
            }

            chkCreateThreadInTheBackground.Checked = Saved.Default.CreateThreadInTheBackground;

            UpdateChecker.CheckForUpdate();
        }
        private void frmMain_Shown(object sender, EventArgs e) {
            ReloadSavedThreads();
            CheckArgumentsForThreads();
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            if (General.Default.MinimizeInsteadOfExiting) {
                this.Hide();
                niTray.Visible = true;
                e.Cancel = true;
            }

            if (!ApplicationShouldExit()) {
                e.Cancel = true;
            }
        }
        private void frmMain_SizeChanged(object sender, EventArgs e) {
            if (this.WindowState == FormWindowState.Minimized && General.Default.MinimizeToTray) {
                this.WindowState = FormWindowState.Normal;
                this.Hide();
                niTray.Visible = true;
            }
        }
        private void mSettings_Click(object sender, EventArgs e) {
            frmSettings Settings = new frmSettings();
            Settings.ShowDialog();
            Settings.Dispose();
            Program.SettingsOpen = false;

            if (General.Default.ShowTrayIcon) {
                niTray.Visible = true;
            }
            else {
                niTray.Visible = false;
            }

            if (Threads.Count > 0) {
                for (int Thread = 0; Thread < Threads.Count; Thread++) {
                    Threads[Thread].UpdateThreadName();
                }
            }
        }
        private void mAbout_Click(object sender, EventArgs e) {
            frmAbout About = new frmAbout();
            About.ShowDialog();
        }
        private void txtThreadURL_KeyPress(object sender, KeyPressEventArgs e) {
            switch (e.KeyChar) {
                case (char)Keys.Enter:
                    if (AddNewThread(txtThreadURL.Text)) {
                        txtThreadURL.Clear();
                    }
                    e.Handled = true;
                    break;
            }
        }
        private void btnAdd_Click(object sender, EventArgs e) {
            if (AddNewThread(txtThreadURL.Text)) {
                txtThreadURL.Clear();
            }
        }
        private void lvThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                Threads[lvThreads.SelectedIndices[0]].Show();
                Threads[lvThreads.SelectedIndices[0]].Activate();
            }
        }
        private void cmThreads_Popup(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                mStatus.Enabled = true;

                if (lvThreads.SelectedIndices.Count == 1) {
                    mSetCustomName.Enabled = true;
                    if (Threads[lvThreads.SelectedIndices[0]].CurrentThread.OverallStatus != ThreadStatus.ThreadIsAlive) {
                        mRetryDownload.Enabled = true;
                    }
                    else {
                        mRetryDownload.Enabled = false;
                    }
                }
                else {
                    mRetryDownload.Enabled = false;
                    mSetCustomName.Enabled = false;
                }

                mOpenDownloadFolder.Enabled = true;
                mOpenThreadInBrowser.Enabled = true;
                mCopyThreadURL.Enabled = true;
                mCopyThreadID.Enabled = true;

                mRemove.Enabled = true;
            }
            else {
                mStatus.Enabled = false;
                mRetryDownload.Enabled = false;
                mSetCustomName.Enabled = false;

                mOpenDownloadFolder.Enabled = false;
                mOpenThreadInBrowser.Enabled = false;
                mCopyThreadURL.Enabled = false;
                mCopyThreadID.Enabled = false;

                mRemove.Enabled = false;
            }
        }
        private void changeTray_Tick(object sender, EventArgs e) {
            if (Icon404WasShown) {
                Icon404WasShown = false;
            }
            else {
                niTray.Icon = Properties.Resources.YChanEx;
                changeTray.Stop();
            }
        }
        private void niTray_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (!this.Visible) {
                this.WindowState = FormWindowState.Normal;
                this.Show();
                this.Activate();
                if (!General.Default.ShowTrayIcon) {
                    niTray.Visible = false;
                }
            }
        }
        #endregion

        #region cmThreads Controls
        private void mStatus_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    Threads[lvThreads.SelectedIndices[CurrentThread]].Show();
                }
            }
        }
        private void mRetryDownload_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedItems.Count == 1) {
                if (Threads[lvThreads.SelectedIndices[0]].CurrentThread.OverallStatus != ThreadStatus.ThreadIsAlive) {
                    Threads[lvThreads.SelectedIndices[0]].ManageThread(ThreadEvent.RetryDownload);
                    ThreadsModified = true;
                    mRetryDownload.Enabled = false;
                }
            }
        }
        private void mSetCustomName_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                frmNewName newName = new frmNewName();
                newName.txtNewName.Text = lvThreads.Items[lvThreads.SelectedIndices[0]].SubItems[clName.Index].Text;
                switch (newName.ShowDialog()) {
                    case System.Windows.Forms.DialogResult.OK:
                        lvThreads.Items[lvThreads.SelectedIndices[0]].SubItems[clName.Index].Text = newName.txtNewName.Text;
                        Threads[lvThreads.SelectedIndices[0]].CurrentThread.SetCustomName = true;
                        Threads[lvThreads.SelectedIndices[0]].CurrentThread.CustomName = newName.txtNewName.Text;
                        break;
                    case System.Windows.Forms.DialogResult.No:
                        lvThreads.Items[lvThreads.SelectedIndices[0]].SubItems[clName.Index].Text = Threads[lvThreads.SelectedIndices[0]].CurrentThread.ThreadName;
                        Threads[lvThreads.SelectedIndices[0]].CurrentThread.SetCustomName = false;
                        Threads[lvThreads.SelectedIndices[0]].CurrentThread.CustomName = null;
                        break;
                }
                newName.Dispose();
            }
            //using (Form newName = new Form())
            //using (TextBox threadName = new TextBox())
            //using (Button cancelName = new Button())
            //using (Button setName = new Button()){
            //    newName.FormBorderStyle = FormBorderStyle.FixedSingle;
            //    newName.MaximizeBox = false;
            //    newName.MinimizeBox = false;
            //    newName.Name = "NewName-" + ThreadDownloadForms[lvThreads.SelectedIndices[0]].ThreadID;
            //    newName.Size = new System.Drawing.Size(300, 130);
            //    newName.StartPosition = FormStartPosition.CenterScreen;
            //    newName.Text = "Set thread name";

            //    threadName.Location = new System.Drawing.Point(12, 20);
            //    threadName.Name = "txtThreadName";
            //    threadName.Size = new System.Drawing.Size(258, 20);
            //    newName.Controls.Add(threadName);

            //    cancelName.Click += (s, arg) => {
            //        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //    };
            //    cancelName.Location = new System.Drawing.Point(114, 58);
            //    cancelName.Name = "btnCancel";
            //    cancelName.Size = new System.Drawing.Size(75, 23);
            //    cancelName.Text = "Cancel";
            //    newName.Controls.Add(cancelName);

            //    setName.Click += (s, arg) => {
            //        this.DialogResult = System.Windows.Forms.DialogResult.OK;
            //    };
            //    setName.Location = new System.Drawing.Point(195, 58);
            //    setName.Name = "btnSetName";
            //    setName.Size = new System.Drawing.Size(75, 23);
            //    setName.Text = "Set name";
            //    newName.Controls.Add(setName);

            //    switch (newName.ShowDialog()) {
            //        case System.Windows.Forms.DialogResult.OK:
            //            lvThreads.Items[lvThreads.SelectedIndices[0]].SubItems[clName.Index].Text = threadName.Text;
            //            break;
            //    }
            //}
        }
        private void mOpenDownloadFolder_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    string FoundThreadDownloadPath = Threads[lvThreads.SelectedIndices[CurrentThread]].CurrentThread.DownloadPath;
                    if (!string.IsNullOrEmpty(FoundThreadDownloadPath)) {
                        if (System.IO.Directory.Exists(FoundThreadDownloadPath)) {
                            System.Diagnostics.Process.Start(FoundThreadDownloadPath);
                        }
                    }
                }
            }
        }
        private void mOpenThreadInBrowser_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    System.Diagnostics.Process.Start(ThreadURLs[lvThreads.SelectedIndices[CurrentThread]]);
                }
            }
        }
        private void mCopyThreadURL_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                string ClipboardBuffer = string.Empty;
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    ClipboardBuffer += ThreadURLs[lvThreads.SelectedIndices[CurrentThread]] + "\r\n";
                }
                ClipboardBuffer = ClipboardBuffer.Trim('\n').Trim('\r');
                Clipboard.SetText(ClipboardBuffer);
            }
        }
        private void mCopyThreadID_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                string ThreadIDBuffer = string.Empty;
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    ThreadIDBuffer += Threads[lvThreads.SelectedIndices[CurrentThread]].CurrentThread.ThreadID;
                }
                ThreadIDBuffer = ThreadIDBuffer.Trim('\n').Trim('\r');
                if (!string.IsNullOrEmpty(ThreadIDBuffer)) {
                    Clipboard.SetText(ThreadIDBuffer);
                }
            }
        }
        private void mRemove_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                if (Saved.Default.DownloadFormSize != Threads[0].Size) {
                    Saved.Default.DownloadFormSize = Threads[0].Size;
                }

                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    int CurrentIndex = lvThreads.SelectedIndices[CurrentThread];
                    if (Threads[CurrentThread].CurrentThread.OverallStatus != ThreadStatus.ThreadIsAlive) {
                        Threads[CurrentIndex].ManageThread(ThreadEvent.AbortDownload);
                    }


                    Threads[CurrentIndex].Dispose();
                    Threads.RemoveAt(CurrentIndex);
                    ThreadURLs.RemoveAt(CurrentIndex);
                    lvThreads.Items.RemoveAt(CurrentIndex);
                    ThreadsModified = true;
                }
            }
        }
        #endregion

        #region cmTray Controls
        private void mTrayShowYChanEx_Click(object sender, EventArgs e) {
            if (this.Visible == false) {
                this.WindowState = FormWindowState.Normal;
                this.Show();
                this.Activate();
                if (!General.Default.ShowTrayIcon) {
                    niTray.Visible = false;
                }
            }
        }
        private void mTrayExit_Click(object sender, EventArgs e) {
            if (ApplicationShouldExit()) {
                Environment.Exit(0);
            }
        }
        private void mAddThread_Click(object sender, EventArgs e) {
            if (Clipboard.ContainsText() && AddNewThread(Clipboard.GetText(), true)) {

            }
        }
        #endregion

    }

    public interface IFormInterface {
        void SetItemStatus(string ThreadURL, ThreadStatus Status);
    }
}