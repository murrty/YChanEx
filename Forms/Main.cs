using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace YChanEx {
    public partial class frmMain : Form {

        #region Variables
        public List<ImageBoard> clThreads = new List<ImageBoard>();                 // list of monitored threads
        public List<ImageBoard> clBoards  = new List<ImageBoard>();                 // list of monitored boards
        List<Thread> thrThreads    = new List<Thread>();                            // list of threads that download 
        Thread Scanner = null;                                                      // thread that addes stuff
        Thread checkUpdates;

        int tPos = -1;                                                              // Item position in lbThreads
        int bPos = -1;                                                              // Item position in lbBoards
        System.Windows.Forms.Timer scnTimer = new System.Windows.Forms.Timer();     // Timmer for scanning

        string settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\YChanEx"; // Get the AppData/Local folder
        Uri DeadThreadURL;                                                          // URL for 404'd thread
        bool is404;                                                                 // If a 404 has occured, used when clicking on the nfTray balloon text.
        bool hasUpdated = false;                                                    // If application was updated recently.
        string OldPath;                                                             // Old path var for opening the settings, for restarting timer
        #endregion

        #region frmMain (Load / Shown / SizeChanged / FormClosing) / edtURL (Enter / Leave) / btnAdd (Click)
        public frmMain() { InitializeComponent(); }

        private void frmMain_Load(object sender, EventArgs e) {
            Properties.Settings.Default.runningUpdate = false;
            tcApp.TabPages.RemoveAt(1);

            if (YCSettings.Default.updaterEnabled) {
                checkUpdates = new Thread(() => {
                    var cV = Updater.getCloudVersion();

                    if (Updater.isUpdateAvailable(cV)) {
                        if (MessageBox.Show("An update is available. \nNew verison: " + cV.ToString() + " | Your version: " + Properties.Settings.Default.currentVersion.ToString() + "\n\nWould you like to update?", "YChanEx", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                            Updater.createUpdaterStub(cV);
                            Updater.runUpdater();
                        }
                        this.Invoke((MethodInvoker)(() => mUpdateAvailable.Enabled = true));
                        this.Invoke((MethodInvoker)(() => mUpdateAvailable.Visible = true));
                    }
                    this.Invoke((MethodInvoker)(() => checkUpdates.Abort()));
                });
                checkUpdates.Start();
            }

            if (YCSettings.Default.firstStart) {
                FirstStart tFirstStart = new FirstStart();                        // if first start, show first start message
                if (tFirstStart.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    Settings tSettings = new Settings();
                    tSettings.ShowDialog();

                    tSettings.Close();
                    tSettings.Dispose();
                }
                else { Environment.Exit(0); }

                tFirstStart.Close();
                tFirstStart.Dispose();

                GC.Collect();
            }


            nfTray.ContextMenu = mTray;

            if (File.Exists(System.Windows.Forms.Application.StartupPath + @"\ycxu.bat")) {
                hasUpdated = true;
                File.Delete(System.Windows.Forms.Application.StartupPath + @"\ycxu.bat");
            }
            else {
                hasUpdated = false;
            }

            if (YCSettings.Default.trayIcon)
                nfTray.Visible = true;
            else
                nfTray.Visible = false;

            scnTimer.Enabled = false;                                              // disable timer                 
            scnTimer.Interval = YCSettings.Default.scannerTimer;           // set interval
            scnTimer.Tick += new EventHandler(this.scan);                           // when Timer ticks call scan()
            if (YCSettings.Default.saveOnClose) {                           // if enabled load URLs from file 
                string[] URLs;

                string boards = Controller.loadURLs(true);
                if (boards != "") {
                    URLs = boards.Split('\n');
                    for (int i = 0; i < URLs.Length - 1; i++) {
                        ImageBoard newImageboard = Controller.createNewIMB(URLs[i], true); // and add them 
                        lbBoards.Items.Add(URLs[i]);
                        clBoards.Add(newImageboard);
                    }
                    scnTimer.Enabled = true;                                       // activate the timer
                    scan(null, null);                                              // and start scanning
                }


                string threads = Controller.loadURLs(false);                         // load threads
                if (threads != "") {
                    URLs = threads.Split('\n');
                    for (int i = 0; i < URLs.Length - 1; i++) {
                        ImageBoard newImageboard = Controller.createNewIMB(URLs[i], false);
                        if (newImageboard == null) {
                            MessageBox.Show(URLs[i]);
                        }
                        else {
                            lbThreads.Items.Add(URLs[i]);
                            clThreads.Add(newImageboard);
                            Thread nIMB = new Thread(delegate() {
                                newImageboard.download();
                            });
                            thrThreads.Add(nIMB);
                            thrThreads.Last().Start();
                            if (!scnTimer.Enabled) {
                                scnTimer.Enabled = true;
                                scan(null, null);
                            }
                        }
                    }
                }
            }
        }
        private void frmMain_Shown(object sender, EventArgs e) {
            if (Properties.Settings.Default.debug)
                mDebug.Visible = true; mDebug.Enabled = true;

            // Download url if it is a website
            for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++) {
                string arg = Environment.GetCommandLineArgs()[i];
                if (i != 0) {
                    if (!arg.StartsWith("http://fchan.us") || arg.StartsWith("http://www.fchan.us") || arg.StartsWith("fchan.us/"))
                        arg = arg.Replace("http://", "https://"); // Always use a secure download if available.

                    Uri testURL;
                    if (Uri.TryCreate(arg, UriKind.Absolute, out testURL) && (testURL.Scheme == Uri.UriSchemeHttp || testURL.Scheme == Uri.UriSchemeHttps))
                        downloadURL(arg, true);
                }
            }

            // Check for update
            if (hasUpdated && nfTray.Visible) {
                nfTray.BalloonTipIcon = ToolTipIcon.Info;
                nfTray.BalloonTipTitle = "YChanEx Updated";
                nfTray.BalloonTipText = "YChanEx has been updated to v" + Properties.Settings.Default.currentVersion.ToString() + ".";
                nfTray.ShowBalloonTip(30000);
            }
            else if (hasUpdated) {
                MessageBox.Show("YChanEx has been updated to v" + Properties.Settings.Default.currentVersion.ToString() + ".");
            }

            // Set size & opacity
            this.Size = new Size(YCSettings.Default.formSizeW, YCSettings.Default.formSizeH);
            this.Opacity = 100;

            tcApp.Focus();
            lbBoards.SelectedIndex = -1;
            lbThreads.SelectedIndex = -1;
            bPos = -1;
            tPos = -1;
        }
        private void frmMain_SizeChanged(object sender, EventArgs e) {
            if (YCSettings.Default.minimizeToTray && this.WindowState == FormWindowState.Minimized) {
                this.WindowState = FormWindowState.Normal;
                this.Hide();                                                      // when minimized; hide from taskbar if trayicon enabled
                mTrayShow.Text = "Show";
                if (!nfTray.Visible)
                    nfTray.Visible = true;                                        // double check for icon
            }
            else {
                mTrayShow.Text = "Hide";
            }
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            scnTimer.Stop();
            YCSettings.Default.formSizeH = this.Height;
            YCSettings.Default.formSizeW = this.Width;

            if (YCSettings.Default.saveOnClose)
                Controller.saveURLs(clBoards, clThreads);

            if (Properties.Settings.Default.runningUpdate) {
                YCSettings.Default.Save();
                return;
            }

            if (YCSettings.Default.warnOnClose && clThreads.Count > 0) {
                e.Cancel = true;
                CloseWarn clw = new CloseWarn();
                if (clw.ShowDialog() == DialogResult.OK) {
                    foreach (string thrItem in lbThreads.Items)
                        thrThreads[lbThreads.Items.IndexOf(thrItem)].Abort();

                    YCSettings.Default.Save();
                    Environment.Exit(0);
                }
                else {
                    scnTimer.Start();
                }
                clw.Close();
                clw.Dispose();
            }
        }

        private void edtURL_Enter(object sender, EventArgs e) {
            if (ActiveForm.AcceptButton != btnAdd)
                ActiveForm.AcceptButton = btnAdd;
        }
        private void edtURL_Leave(object sender, EventArgs e) {
            if (ActiveForm.AcceptButton != null)
                ActiveForm.AcceptButton = null;
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            string dlURL;

            if (edtURL.Text.StartsWith("fchan.us/")) edtURL.Text = edtURL.Text.Replace("fchan.us/", "http://fchan.us/");

            if (!edtURL.Text.StartsWith("http://fchan.us") || edtURL.Text.StartsWith("http://www.fchan.us"))
                dlURL = edtURL.Text.Replace("http://", "https://").Replace("board/u18chan/", "");
            else
                dlURL = edtURL.Text;

            downloadURL(dlURL, false);

            edtURL.Clear();
        }
        #endregion

        #region Custom (downloadURL / addToHistory / getTitle / isUnique / getPlace / scan)
        private void downloadURL(string url, bool silentDownload) {
            if (!Controller.isSupported(url)) {
                MessageBox.Show("Please enter a supported site URL (Check the Github page for a list)");
                return;
            }

            bool board = (tcApp.SelectedIndex == 1);                             // Board Tab is open -> board=true; Thread tab -> board=false 
            ImageBoard newImageboard = Controller.createNewIMB(url.Trim(), board);
            if (url.StartsWith("fchan.us/"))
                url = url.Replace("fchan.us/", "http://fchan.us/");

            if (newImageboard != null) {
                if (isUnique(newImageboard.getURL(), clThreads)) {
                    if (board) {
                        lbBoards.Items.Add(url);
                        clBoards.Add(newImageboard);
                    }
                    else {
                        lbThreads.Items.Add(url);
                        clThreads.Add(newImageboard);
                        addToHistory(url);
                    }
                }
                else {
                    MessageBox.Show("URL is already in queue!");
                    return;
                }
            }
            else {
                MessageBox.Show("Corrupt URL, unsupported website or not a board/thread!");
                return;
            }

            if (!board) {
                Thread nIMB = new Thread(delegate() { newImageboard.download(); });
                nIMB.Name = newImageboard.getURL();
                thrThreads.Add(nIMB);
                thrThreads[thrThreads.Count - 1].Start();
            }

            if (!scnTimer.Enabled)
                scnTimer.Enabled = true;

            scan(null, null);

            if (YCSettings.Default.saveOnClose)
                Controller.saveURLs(clBoards, clThreads);
        }

        private void addToHistory(string URL) {
            if (YCSettings.Default.saveHistory) {
                if (URL.StartsWith("https://boards.4chan.org/")) {
                    //Controller.saveHistory(0, URL + " // " + getTitle(true, URL), URL);
                    Controller.saveHistory(0, URL + " // " + Controller.getHTMLTitle(0, URL), URL);
                }
                else if (URL.StartsWith("https://boards.420chan.org/")) {
                    //Controller.saveHistory(1, URL + " // " + getTitle(false, URL), URL);
                    Controller.saveHistory(1, URL + " // " + Controller.getHTMLTitle(1, URL), URL);
                }
                else if (URL.StartsWith("https://7chan.org/")) {
                    //Controller.saveHistory(2, URL + " // " + getTitle(false, URL), URL);
                    Controller.saveHistory(2, URL, URL);// + " // " + Controller.getHTMLTitle(2, URL), URL);
                }
                else if (URL.StartsWith("https://8ch.net/")) {
                    //Controller.saveHistory(3, URL + " // " + getTitle(false, URL), URL);
                    Controller.saveHistory(3, URL + " // " + Controller.getHTMLTitle(3, URL), URL);
                }
                else if (URL.StartsWith("http://fchan.us/")) {
                    //Controller.saveHistory(4, URL + " // " + getTitle(false, URL), URL);
                    Controller.saveHistory(4, URL + " // " + Controller.getHTMLTitle(4, URL), URL);
                }
                else if (URL.StartsWith("https://u18chan.com/")) {
                    //Controller.saveHistory(5, URL + " // " + getTitle(false, URL), URL);
                    Controller.saveHistory(5, URL, URL);// + " // " + Controller.getHTMLTitle(5, URL), URL);
                }
            }
        }

        private string getTitle(bool Is4Chan, string threadURL) {
            try {
                string threadTitle = "";
                string threadBoard = "";
                string boardTopic = "";
                Uri threadURI = new Uri(threadURL);
                threadBoard = "/" + threadURI.Segments[1] + " - ";

                if (Is4Chan)
                    boardTopic = fourChan.getTopic("/" + threadURI.Segments[1]);
                else if (threadURL.StartsWith("https://u18chan.com/"))
                    boardTopic = uEighteenChan.getTopic("/" + threadURL.Split('/')[3] + "/");

                HttpWebRequest getSource = (HttpWebRequest)WebRequest.Create(threadURI);
                getSource.UserAgent = Adv.Default.UserAgent;
                getSource.Method = "GET";
                getSource.Accept = "text/html";

                HttpWebResponse returnedSource = (HttpWebResponse)getSource.GetResponse();
                var stream = returnedSource.GetResponseStream();
                var reader = new StreamReader(stream);
                var html = reader.ReadToEnd();
                returnedSource.Close(); // Don't close yet.

                string source = html.ToString();

                Regex findTitle = new Regex("<title>(.*)</title>");
                MatchCollection matchLine = findTitle.Matches(source);

                if (matchLine.Count > 0) {
                    if (Is4Chan)
                        threadTitle = matchLine[0].Value.Replace("<title>", "").Replace("</title>", "").Replace(" - 4chan", "").Replace(threadBoard, "");
                    else
                        threadTitle = matchLine[0].Value.Replace("<title>", "").Replace("</title>", "").Replace(threadBoard, "");
                }

                if (Properties.Settings.Default.debug) {
                    MessageBox.Show(threadBoard);
                    MessageBox.Show(boardTopic);
                }

                if (Is4Chan)
                    return threadTitle.Replace(" - " + boardTopic, "").Replace("&gt;", ">");
                else
                    return threadTitle;
            }
            catch (Exception e) {
                Debug.Print(e.ToString());
                return "";
            }
        }

        private bool isUnique(string url, List<ImageBoard> List) {
            bool flag = true;
            for (int i = 0; i < List.Count; i++) {
                if (List[i].getURL() == url)
                    flag = false;
            }
            return flag;
        }
        private int getPlace(string url) {
            int plc = -1;
            for (int i = 0; i < clThreads.Count; i++) {
                if (clThreads[i].getURL() == url)
                    plc = i;
            }
            return plc;
        }
        private void scan(object sender, EventArgs e) {
            /*#if DEBUG
                        MessageBox.Show("Threads: (" + thrThreads.Count + ") (" + clThreads.Count +")");
                        MessageBox.Show("Boards: (" + thrBoards.Count + ") (" + clBoards.Count +")");
            #endif*/


            if (Scanner == null || !Scanner.IsAlive) {
                Scanner = new Thread(delegate() {
                    for (int k = 0; k < clThreads.Count; k++) {
                        if (clThreads[k].isGone()) {

                            DeadThreadURL = new UriBuilder(lbThreads.Items[k].ToString()).Uri;
                            string ReturnID = DeadThreadURL.Segments[3];
                            string ReturnBoard = DeadThreadURL.Segments[1].Replace("/", "");
                            // MessageBox.Show(ReturnID);

                            string deadAction = "";
                            if (YCSettings.Default.threadDeadAction == 1) {
                                deadAction = "\nClick here to copy archive link";
                            }
                            else if (YCSettings.Default.threadDeadAction == 2) {
                                deadAction = "\nClick here to open archive link";
                            }
                            else if (YCSettings.Default.threadDeadAction == 3) {
                                deadAction = "\nClick here to copy original link";
                            }
                            else if (YCSettings.Default.threadDeadAction == 4) {
                                deadAction = "\nClick here to open original link";
                            }
                            else if (YCSettings.Default.threadDeadAction == 5) {
                                deadAction = "\nClick here to copy thread ID";
                            }
                            else if (YCSettings.Default.threadDeadAction == 6) {
                                deadAction = "\nClick here to copy download folder path";
                            }
                            else if (YCSettings.Default.threadDeadAction == 7) {
                                deadAction = "\nClick here to open download folder path";
                            }

                            is404 = true;
                            nfTray.BalloonTipTitle = "Thread 404'd";
                            nfTray.BalloonTipText = "Thread " + ReturnID.Replace(".html", "") + " on /" + ReturnBoard + "/ has 404'd." + deadAction;
                            nfTray.BalloonTipIcon = ToolTipIcon.Error;
                            nfTray.Icon = Properties.Resources.YChanEx404;
                            nfTray.ShowBalloonTip(15000);

                            clThreads.RemoveAt(k);
                            thrThreads.RemoveAt(k);
                            lbThreads.Invoke((MethodInvoker)delegate { lbThreads.Items.RemoveAt(k); });
                        }
                    }

                    for (int k = 0; k < clBoards.Count; k++) {
                        string[] Threads = { };
                        try {
                            Threads = clBoards[k].getThreads().Split('\n');
                        }
                        catch (Exception exep) {
                            Debug.Print(exep.ToString());
                        }

                        for (int l = 0; l < Threads.Length; l++) {
                            ImageBoard newImageboard = Controller.createNewIMB(Threads[l], false);
                            if (newImageboard != null && isUnique(newImageboard.getURL(), clThreads)) {
                                lbThreads.Invoke((MethodInvoker)(() => { lbThreads.Items.Add(Threads[l]); }));
                                clThreads.Add(newImageboard);
                                Thread nIMB = new Thread(delegate() { newImageboard.download(); });
                                nIMB.Name = newImageboard.getURL();
                                thrThreads.Add(nIMB);
                                thrThreads[thrThreads.Count - 1].Start();
                            }
                        }
                    }

                    for (int k = 0; k < clThreads.Count; k++) {
                        if (!thrThreads[k].IsAlive) {
                            /*                        MessageBox.Show("Down: " + k);
                            */

                            thrThreads[k] = new Thread(delegate() {
                                int x = k;
                                try {
                                    clThreads[k - 1].download();   // why
                                }
                                catch (Exception exp) {
                                    Debug.Print(exp.ToString());
                                    //                                    MessageBox.Show(exp.Message + " k: " + x);
                                }
                            });
                            thrThreads[k].Name = clThreads[k].getURL();
                            thrThreads[k].Start();
                        }

                    }
                    GC.Collect();
                });
                Scanner.Start();
            }
        }
        #endregion

        #region lbThreads / lbBoards (MouseDown)
        private void lbThreads_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                tPos = -1;
                if (lbThreads.IndexFromPoint(e.Location) != -1) {
                    lbThreads.SelectedIndex = lbThreads.IndexFromPoint(e.X, e.Y);
                    tPos = lbThreads.IndexFromPoint(e.Location);
                    mThreads.Show(lbThreads, new Point(e.X, e.Y));
                }
            }
        }
        private void lbBoards_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                bPos = -1;
                if (lbBoards.IndexFromPoint(e.Location) != -1) {
                    lbBoards.SelectedIndex = lbBoards.IndexFromPoint(e.X, e.Y);
                    bPos = lbBoards.IndexFromPoint(e.Location);
                    mBoards.Show(lbBoards, new Point(e.X, e.Y));
                }
            }
        }
        #endregion
        #region nfTray (MouseDoubleClick / MouseMove / BalloonTipClicked / BalloonTipClosed)
        private void nfTray_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (this.Visible) {
                mTrayShow.Text = "Show";
                this.Hide();
            }
            else {
                mTrayShow.Text = "Hide";
                this.Show();
            }
        }
        private void nfTray_MouseMove(object sender, MouseEventArgs e) {
            nfTray.Text = "YChanEx\n\nBoard count: " + lbBoards.Items.Count.ToString() + "\nThread count: " + lbThreads.Items.Count.ToString();
        }
        private void nfTray_BalloonTipClicked(object sender, EventArgs e) {
            if (is404) {
                string ReturnID = DeadThreadURL.Segments[3];
                string ReturnBoard = DeadThreadURL.Segments[1].Replace("/", "");
                nfTray.Icon = Properties.Resources.YChanEx;

                if (YCSettings.Default.threadDeadAction == 0) {
                    is404 = false;
                    return;
                }
                else if (YCSettings.Default.threadDeadAction == 1) {
                    if (DeadThreadURL.ToString().StartsWith("https://boards.4chan.org/"))
                        Clipboard.SetText("https://archived.moe/" + ReturnBoard + "/threads/" + ReturnID);
                    // else if (DeadThreadURL.ToString().StartsWith("https://8ch.net/") || DeadThreadURL.ToString().StartsWith("https://www.8ch.net/") || DeadThreadURL.ToString().StartsWith("https://8chan.co/") || DeadThreadURL.ToString().StartsWith("https://www.8chan.co/"))
                    // 8CHAN does not have an archiving site yet, do they? idk.

                    is404 = false;
                }
                else if (YCSettings.Default.threadDeadAction == 2) {
                    if (DeadThreadURL.ToString().StartsWith("https://boards.4chan.org/"))
                        Process.Start("https://archived.moe/" + ReturnBoard + "/threads/" + ReturnID);
                    // else if (DeadThreadURL.ToString().StartsWith("https://8ch.net/") || DeadThreadURL.ToString().StartsWith("https://www.8ch.net/") || DeadThreadURL.ToString().StartsWith("https://8chan.co/") || DeadThreadURL.ToString().StartsWith("https://www.8chan.co/"))
                    // read above.

                    is404 = false;
                }
                else if (YCSettings.Default.threadDeadAction == 3) {
                    if (DeadThreadURL.ToString().StartsWith("https://boards.4chan.org/"))
                        Clipboard.SetText("https://4chan.org/" + ReturnBoard + "/threads/" + ReturnID);
                    else if (DeadThreadURL.ToString().StartsWith("https://8ch.net/") || DeadThreadURL.ToString().StartsWith("https://www.8ch.net/") || DeadThreadURL.ToString().StartsWith("https://8chan.co/") || DeadThreadURL.ToString().StartsWith("https://www.8chan.co/"))
                        Clipboard.SetText("https://8ch.net/" + ReturnBoard + "/res/" + ReturnID + ".html");

                    is404 = false;
                }
                else if (YCSettings.Default.threadDeadAction == 4) {
                    if (DeadThreadURL.ToString().StartsWith("https://boards.4chan.org/"))
                        Process.Start("https://4chan.org/" + ReturnBoard + "/threads/" + ReturnID);
                    else if (DeadThreadURL.ToString().StartsWith("https://8ch.net/") || DeadThreadURL.ToString().StartsWith("https://www.8ch.net/") || DeadThreadURL.ToString().StartsWith("https://8chan.co/") || DeadThreadURL.ToString().StartsWith("https://www.8chan.co/"))
                        Process.Start("https://8ch.net/" + ReturnBoard + "/res/" + ReturnID + ".html");

                    is404 = false;
                }
                else if (YCSettings.Default.threadDeadAction == 5) {
                    Clipboard.SetText(ReturnID);
                    is404 = false;
                }
                else if (YCSettings.Default.threadDeadAction == 6) {
                    if (DeadThreadURL.ToString().StartsWith("https://boards.4chan.org/"))
                        Clipboard.SetText(YCSettings.Default.downloadPath + @"\4chan\" + ReturnBoard + @"\" + ReturnID);
                    else if (DeadThreadURL.ToString().StartsWith("https://8ch.net/") || DeadThreadURL.ToString().StartsWith("https://www.8ch.net/") || DeadThreadURL.ToString().StartsWith("https://8chan.co/") || DeadThreadURL.ToString().StartsWith("https://www.8chan.co/"))
                        Clipboard.SetText(YCSettings.Default.downloadPath + @"\8ch\" + ReturnBoard + @"\" + ReturnID);

                    is404 = false;
                }
                else if (YCSettings.Default.threadDeadAction == 7) {
                    if (DeadThreadURL.ToString().StartsWith("https://boards.4chan.org/"))
                        Process.Start(YCSettings.Default.downloadPath + @"\4chan\" + ReturnBoard + @"\" + ReturnID);
                    else if (DeadThreadURL.ToString().StartsWith("https://8ch.net/") || DeadThreadURL.ToString().StartsWith("https://www.8ch.net/") || DeadThreadURL.ToString().StartsWith("https://8chan.co/") || DeadThreadURL.ToString().StartsWith("https://www.8chan.co/"))
                        Process.Start(YCSettings.Default.downloadPath + @"\8ch\" + ReturnBoard + @"\" + ReturnID);

                    is404 = false;
                }
            }
            else if (!is404) {
                nfTray.BalloonTipTitle = "Copied";
                nfTray.BalloonTipIcon = ToolTipIcon.Info;

                if (YCSettings.Default.threadDeadAction == 1) { nfTray.BalloonTipText = "Copied archive link."; }
                else if (YCSettings.Default.threadDeadAction == 3) { nfTray.BalloonTipText = "Copied original 4chan link"; }
                else if (YCSettings.Default.threadDeadAction == 5) { nfTray.BalloonTipText = "Copied thread ID"; }
                else if (YCSettings.Default.threadDeadAction == 6) { nfTray.BalloonTipText = "Copied download folder path"; }

                nfTray.ShowBalloonTip(30000);
            }
        }
        private void nfTray_BalloonTipClosed(object sender, EventArgs e) {
            if (is404)
                nfTray.Icon = Properties.Resources.YChanEx;
        }
        #endregion
        #region MenuBar (mSettings / mHistory / mLicenseAndSource / mAbout) Click
        private void mHistory_Click(object sender, EventArgs e) {
            History thHistory = new History();
            thHistory.Show();
        }

        private void mSettings_Click(object sender, EventArgs e) {
            if (Adv.Default.settingsDisableScan)
                scnTimer.Stop();

            OldPath = YCSettings.Default.downloadPath;

            Settings tSettings = new Settings();
            if (tSettings.ShowDialog() == DialogResult.OK) {
                if (tSettings.moveFolders)
                    if (YCSettings.Default.downloadPath != OldPath) {
                        if (scnTimer.Enabled)
                            scnTimer.Stop();

                        bool fcFolder = Directory.Exists(YCSettings.Default.downloadPath + @"\4chan");
                        bool ecFolder = Directory.Exists(YCSettings.Default.downloadPath + @"\8ch");

                        if (!Directory.Exists(YCSettings.Default.downloadPath))
                            Directory.CreateDirectory(YCSettings.Default.downloadPath);

                        if (fcFolder) {
                            Directory.Move(OldPath + @"\4chan", YCSettings.Default.downloadPath + @"\4chan");
                        }

                        if (ecFolder) {
                            Directory.Move(OldPath + @"\8ch", YCSettings.Default.downloadPath + @"\8ch");
                        }
                    }

                YCSettings.Default.Save();
            }

            tSettings.Close();
            tSettings.Dispose();

            if (YCSettings.Default.trayIcon)
                nfTray.Visible = true;
            else
                nfTray.Visible = false;

            OldPath = null;

            if (!scnTimer.Enabled)
                scnTimer.Start();

            GC.Collect();
        }

        private void mLicenseAndSource_Click(object sender, EventArgs e) {
            LicenseSource frmLicenseSrc = new LicenseSource();
            frmLicenseSrc.Show();
        }
        private void mAbout_Click(object sender, EventArgs e) {
            About tAbout = new About();
            tAbout.Show();
        }

        private void mUpdateAvailable_Click(object sender, EventArgs e) {
            if (Properties.Settings.Default.cloudVersion > Properties.Settings.Default.currentVersion)
                if (MessageBox.Show("Would you like to update YChanEx?", "YChanEx", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    Updater.createUpdaterStub(Convert.ToInt32(Properties.Settings.Default.cloudVersion)); Updater.runUpdater(); this.Close();
        }
        #endregion
        #region mTray (mTrayShow / mTrayOpen / mTrayClipboard  / mTrayExit) Click
        private void mTrayShow_Click(object sender, EventArgs e) {
            if (this.Visible) {
                mTrayShow.Text = "Show";
                this.Hide();
            }
            else {
                mTrayShow.Text = "Hide";
                this.Show();
            }
        }
        private void mTrayOpen_Click(object sender, EventArgs e) {
            string sPath = YCSettings.Default.downloadPath;
            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);
            Process.Start(sPath);
        }
        private void mTrayClipboard_Click(object sender, EventArgs e) {
            if (Clipboard.ContainsText())
                downloadURL(Clipboard.GetText(), false);
            else
                MessageBox.Show("Clipboard does not contain text.");
        }
        private void mTrayExit_Click(object sender, EventArgs e) {
            this.Close();
        }
        #endregion
        #region mThreads (mThreadsOpenF / mThreadsOpenB / mThreadsCopyL / mThreadsRemove) Click
        private void mThreadsOpenF_Click(object sender, EventArgs e) {
            if (tPos != -1) {
                string sPath = clThreads[tPos].getPath();
                if (!Directory.Exists(sPath))
                    Directory.CreateDirectory(sPath);
                Process.Start(sPath);
            }
        }
        private void mThreadsOpenB_Click(object sender, EventArgs e) {
            if (tPos != -1) {
                string sPath = lbThreads.Items[tPos].ToString();
                Process.Start(sPath);
            }
        }
        private void mThreadsCopyL_Click(object sender, EventArgs e) {
            if (tPos != -1) {
                string sLink = lbThreads.Items[tPos].ToString();
                Clipboard.SetText(sLink);
            }
        }
        private void mThreadsRemove_Click(object sender, EventArgs e) {
            if (tPos != -1) {
                clThreads.RemoveAt(tPos);
                thrThreads[tPos].Abort();
                thrThreads.RemoveAt(tPos);
                lbThreads.Items.RemoveAt(tPos);
            }
        }
        #endregion
        #region mBoards (mBoardsOpenF / mBoardsOpenB / mBoardsCopyL / mBoardsRemove) Click
        private void mBoardsOpenF_Click(object sender, EventArgs e) {
            if (bPos != -1) {
                string sPath = clBoards[bPos].getPath();
                if (!Directory.Exists(sPath))
                    Directory.CreateDirectory(sPath);
                Process.Start(sPath);
            }
        }
        private void mBoardsOpenB_Click(object sender, EventArgs e) {
            if (bPos != -1) {
                string sPath = lbBoards.Items[bPos].ToString();
                Process.Start(sPath);
            }
        }
        private void mBoardsCopyL_Click(object sender, EventArgs e) {
            if (bPos != -1) {
                string sLink = lbBoards.Items[bPos].ToString();
                Clipboard.SetText(sLink);
            }
        }
        private void mBoardsRemove_Click(object sender, EventArgs e) {
            if (bPos != -1) {
                clBoards.RemoveAt(bPos);
                lbBoards.Items.RemoveAt(bPos);
            }
        }
        #endregion

        #region Debug (mDebugTitle / mDebugID) Click
        private void mDebugTitle_Click(object sender, EventArgs e) {
            if (Clipboard.GetText().StartsWith("https://4chan.org/") || Clipboard.GetText().StartsWith("https://www.4chan.org/") || Clipboard.GetText().StartsWith("https://boards.4chan.org/"))
                MessageBox.Show(getTitle(true, Clipboard.GetText()));
            else if (Clipboard.GetText().StartsWith("https://8ch.net/") || Clipboard.GetText().StartsWith("https://www.8ch.net/") || Clipboard.GetText().StartsWith("https://8chan.co/") || Clipboard.GetText().StartsWith("https://www.8chan.co/"))
                MessageBox.Show(getTitle(false, Clipboard.GetText()));
        }
        private void mDebugID_Click(object sender, EventArgs e) { }
        #endregion

        private void mReload_Click(object sender, EventArgs e) {

        }
        private void mSave_Click(object sender, EventArgs e) {

        }
        private void mUpdates_Click(object sender, EventArgs e) {

        }

    }
}