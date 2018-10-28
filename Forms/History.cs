using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YChanEx {
    public partial class History : Form {

        /* Chan int guide (used to determine what board to work with):
         * 0 = 4chan
         * 1 = 420chan
         * 2 = 7chan
         * 3 = 8chan
         * 4 = fchan
         * 5 = u18chan
        */

        #region Variables
        string settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\YChanEx";

        ListBox fourChHistory = new ListBox();
        ListBox ftChHistory = new ListBox();
        ListBox sevenChHistory = new ListBox();
        ListBox eightChHistory = new ListBox();
        ListBox fChHistory = new ListBox();
        ListBox u18ChHistory = new ListBox();
        #endregion

        #region Form (History / History_Shown / History_FormClosing) / lbHistory / cbSite / cbBoard / chkSorted
        public History() { InitializeComponent(); }

        private void History_Shown(object sender, EventArgs e) {
            loadHistoryMem();

            cbSite.SelectedIndex = YCSettings.Default.historyIndex;
            cbBoard.SelectedIndex = 0;
            enumerateBoardsInHistory(0);
            if (YCSettings.Default.sortHistory)
                chkSorted.Checked = true;
        }
        private void History_FormClosing(object sender, FormClosingEventArgs e) {
            fourChHistory.Items.Clear();
            fourChHistory.Dispose();

            ftChHistory.Items.Clear();
            ftChHistory.Dispose();

            sevenChHistory.Items.Clear();
            sevenChHistory.Dispose();

            eightChHistory.Items.Clear();
            eightChHistory.Dispose();

            fChHistory.Items.Clear();
            fChHistory.Dispose();

            u18ChHistory.Items.Clear();
            u18ChHistory.Dispose();

            if (YCSettings.Default.sortHistory != chkSorted.Checked) { YCSettings.Default.sortHistory = chkSorted.Checked; YCSettings.Default.Save(); }

            this.Dispose();
        }

        private void lbHistory_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                lbHistory.SelectedIndex = lbHistory.IndexFromPoint(e.X, e.Y);
                mHistory.Show(lbHistory, new Point(e.X, e.Y));
            }
        }

        private void cbSite_SelectedIndexChanged(object sender, EventArgs e) {
            if (Properties.Settings.Default.debug)
                MessageBox.Show(cbSite.SelectedIndex.ToString());

            YCSettings.Default.historyIndex = cbSite.SelectedIndex;

            enumerateBoardsInHistory(cbSite.SelectedIndex);
            loadHistory(cbSite.SelectedIndex);

            if (cbSite.SelectedIndex == 0) {
                if (!Directory.Exists(YCSettings.Default.downloadPath + @"\4chan"))
                    btnOpenDownloads.Enabled = false;
                else
                    btnOpenDownloads.Enabled = true;

                mOpenArchive.Enabled = true;
            }
            else if (cbSite.SelectedIndex == 1){
                if (!Directory.Exists(YCSettings.Default.downloadPath + @"\420chan"))
                    btnOpenDownloads.Enabled = false;
                else
                    btnOpenDownloads.Enabled = true;

                mOpenArchive.Enabled = false;
            }
            else if (cbSite.SelectedIndex == 2) {
                if (!Directory.Exists(YCSettings.Default.downloadPath + @"\7chan"))
                    btnOpenDownloads.Enabled = false;
                else
                    btnOpenDownloads.Enabled = true;

                mOpenArchive.Enabled = false;
            }
            else if (cbSite.SelectedIndex == 3) {
                if (!Directory.Exists(YCSettings.Default.downloadPath + @"\8ch"))
                    btnOpenDownloads.Enabled = false;
                else
                    btnOpenDownloads.Enabled = true;

                mOpenArchive.Enabled = false;
            }
            else if (cbSite.SelectedIndex == 4) {
                if (!Directory.Exists(YCSettings.Default.downloadPath + @"\fchan"))
                    btnOpenDownloads.Enabled = false;
                else
                    btnOpenDownloads.Enabled = true;

                mOpenArchive.Enabled = false;
            }
            else if (cbSite.SelectedIndex == 5) {
                if (!Directory.Exists(YCSettings.Default.downloadPath + @"\u18chan"))
                    btnOpenDownloads.Enabled = false;
                else
                    btnOpenDownloads.Enabled = true;

                mOpenArchive.Enabled = false;
            }
        }
        private void cbBoard_SelectedIndexChanged(object sender, EventArgs e) {
            if (cbBoard.SelectedIndex != 0) {
                lbHistory.Items.Clear();
                if (cbSite.SelectedIndex == 0) {
                    if (fourChHistory.Items.Count != 0) {
                        foreach (var lsItem in fourChHistory.Items) {
                            Uri threadURL = new UriBuilder(getURL(lsItem.ToString())).Uri;
                            string board = "/" + threadURL.Segments[1];
                            if (board == cbBoard.SelectedItem.ToString())
                                lbHistory.Items.Add(lsItem);
                        }
                    }
                }
                else if (cbSite.SelectedIndex == 1) {
                    if (ftChHistory.Items.Count != 0) {
                        foreach (var lsItem in ftChHistory.Items) {
                            Uri threadURL = new UriBuilder(getURL(lsItem.ToString())).Uri;
                            string board = "/" + threadURL.Segments[1];
                            if (board == cbBoard.SelectedItem.ToString())
                                lbHistory.Items.Add(lsItem);
                        }
                    }
                }
                else if (cbSite.SelectedIndex == 2) {
                    if (sevenChHistory.Items.Count != 0) {
                        foreach (var lsItem in sevenChHistory.Items) {
                            Uri threadURL = new UriBuilder(getURL(lsItem.ToString())).Uri;
                            string board = "/" + threadURL.Segments[1];
                            if (board == cbBoard.SelectedItem.ToString())
                                lbHistory.Items.Add(lsItem);
                        }
                    }
                }
                else if (cbSite.SelectedIndex == 3) {
                    if (eightChHistory.Items.Count != 0) {
                        foreach (var lsItem in eightChHistory.Items) {
                            Uri threadURL = new UriBuilder(getURL(lsItem.ToString())).Uri;
                            string board = "/" + threadURL.Segments[1];
                            if (board == cbBoard.SelectedItem.ToString())
                                lbHistory.Items.Add(lsItem);
                        }
                    }
                }
                else if (cbSite.SelectedIndex == 4) {
                    if (fChHistory.Items.Count != 0) {
                        foreach (var lsItem in fChHistory.Items) {
                            Uri threadURL = new UriBuilder(getURL(lsItem.ToString())).Uri;
                            string board = "/" + threadURL.Segments[1];
                            if (board == cbBoard.SelectedItem.ToString())
                                lbHistory.Items.Add(lsItem);
                        }
                    }
                }
                else if (cbSite.SelectedIndex == 5) {
                    if (u18ChHistory.Items.Count != 0) {
                        foreach (var lsItem in u18ChHistory.Items) {
                            Uri threadURL = new UriBuilder(getURL(lsItem.ToString())).Uri;
                            string board = "/" + threadURL.Segments[1];
                            if (board == cbBoard.SelectedItem.ToString())
                                lbHistory.Items.Add(lsItem);
                        }
                    }
                }
            }
            else {
                loadHistory(cbSite.SelectedIndex);
            }
        }

        private void chkSorted_CheckedChanged(object sender, EventArgs e) { lbHistory.Sorted = chkSorted.Checked; }
        #endregion

        #region Custom (loadHistoryMem / loadHistory / enumerateBoardsInHistory / getURL)
        public void loadHistoryMem() {
            if (File.Exists(settingsDir + @"\4chanhistory.dat")) {
                using (StreamReader readFile = new StreamReader(settingsDir + @"\4chanhistory.dat")) {
                    string readLine;
                    while ((readLine = readFile.ReadLine()) != null) { fourChHistory.Items.Add(readLine); }
                }
            }

            if (File.Exists(settingsDir + @"\420chanhistory.dat")) {
                using (StreamReader readFile = new StreamReader(settingsDir + @"\420chanhistory.dat")) {
                    string readLine;
                    while ((readLine = readFile.ReadLine()) != null) { ftChHistory.Items.Add(readLine); }
                }
            }

            if (File.Exists(settingsDir + @"\7chanhistory.dat")) {
                using (StreamReader readFile = new StreamReader(settingsDir + @"\7chanhistory.dat")) {
                    string readLine;
                    while ((readLine = readFile.ReadLine()) != null) { sevenChHistory.Items.Add(readLine); }
                }
            }

            if (File.Exists(settingsDir + @"\8chanhistory.dat")) {
                using (StreamReader readFile = new StreamReader(settingsDir + @"\8chanhistory.dat")) {
                    string readLine;
                    while ((readLine = readFile.ReadLine()) != null) { eightChHistory.Items.Add(readLine); }
                }
            }

            if (File.Exists(settingsDir + @"\fchanhistory.dat")) {
                using (StreamReader readFile = new StreamReader(settingsDir + @"\fchanhistory.dat")) {
                    string readLine;
                    while ((readLine = readFile.ReadLine()) != null) { fChHistory.Items.Add(readLine); }
                }
            }

            if (File.Exists(settingsDir + @"\u18chanhistory.dat")) {
                using (StreamReader readFile = new StreamReader(settingsDir + @"\u18chanhistory.dat")) {
                    string readLine;
                    while ((readLine = readFile.ReadLine()) != null) { u18ChHistory.Items.Add(readLine); }
                }
            }
        }

        public void loadHistory(int chan) {
            lbHistory.Items.Clear();

            if (chan == 0) {
                if (fourChHistory.Items.Count != 0)
                    foreach (var lsItem in fourChHistory.Items) { lbHistory.Items.Add(lsItem); }
            }
            else if (chan == 1) {
                if (ftChHistory.Items.Count != 0)
                    foreach (var lsItem in ftChHistory.Items) { lbHistory.Items.Add(lsItem); }
            }
            else if (chan == 2) {
                if (sevenChHistory.Items.Count != 0)
                    foreach (var lsItem in sevenChHistory.Items) { lbHistory.Items.Add(lsItem); }
            }
            else if (chan == 3) {
                if (eightChHistory.Items.Count != 0)
                    foreach (var lsItem in eightChHistory.Items) { lbHistory.Items.Add(lsItem); }
            }
            else if (chan == 4) {
                if (fChHistory.Items.Count != 0)
                    foreach (var lsItem in fChHistory.Items) { lbHistory.Items.Add(lsItem); }
            }
            else if (chan == 5) {
                if (u18ChHistory.Items.Count != 0)
                    foreach (var lsItem in u18ChHistory.Items) { lbHistory.Items.Add(lsItem); }
            }
        }

        public void enumerateBoardsInHistory(int chan) {
            cbBoard.Items.Clear();
            cbBoard.Items.Add("All boards");

            if (chan == 0) {
                if (File.Exists(settingsDir + @"\4chanhistory.dat")) {
                    using (StreamReader readFile = new StreamReader(settingsDir + @"\4chanhistory.dat")) {
                        string readLine;
                        while ((readLine = readFile.ReadLine()) != null) {
                            Uri threadURL = new UriBuilder(getURL(readLine)).Uri;
                            string board = "/" + threadURL.Segments[1];

                            if (!cbBoard.Items.Contains(board))
                                cbBoard.Items.Add(board);
                        }
                    }
                }
            }
            else if (chan == 1) {
                if (File.Exists(settingsDir + @"\420chanhistory.dat")) {
                    using (StreamReader readFile = new StreamReader(settingsDir + @"\420chanhistory.dat")) {
                        string readLine;
                        while ((readLine = readFile.ReadLine()) != null) {
                            Uri threadURL = new UriBuilder(getURL(readLine)).Uri;
                            string board = "/" + threadURL.Segments[1];

                            if (!cbBoard.Items.Contains(board))
                                cbBoard.Items.Add(board);
                        }
                    }
                }
            }
            else if (chan == 2) {
                if (File.Exists(settingsDir + @"\7chanhistory.dat")) {
                    using (StreamReader readFile = new StreamReader(settingsDir + @"\7chanhistory.dat")) {
                        string readLine;
                        while ((readLine = readFile.ReadLine()) != null) {
                            Uri threadURL = new UriBuilder(getURL(readLine)).Uri;
                            string board = "/" + threadURL.Segments[1];

                            if (!cbBoard.Items.Contains(board))
                                cbBoard.Items.Add(board);
                        }
                    }
                }
            }
            else if (chan == 3) {
                if (File.Exists(settingsDir + @"\8chanhistory.dat")) {
                    using (StreamReader readFile = new StreamReader(settingsDir + @"\8chanhistory.dat")) {
                        string readLine;
                        while ((readLine = readFile.ReadLine()) != null) {
                            Uri threadURL = new UriBuilder(getURL(readLine)).Uri;
                            string board = "/" + threadURL.Segments[1];

                            if (!cbBoard.Items.Contains(board))
                                cbBoard.Items.Add(board);
                        }
                    }
                }
            }
            else if (chan == 4) {
                if (File.Exists(settingsDir + @"\fchanhistory.dat")) {
                    using (StreamReader readFile = new StreamReader(settingsDir + @"\fchanhistory.dat")) {
                        string readLine;
                        while ((readLine = readFile.ReadLine()) != null) {
                            Uri threadURL = new UriBuilder(getURL(readLine)).Uri;
                            string board = "/" + threadURL.Segments[1];

                            if (!cbBoard.Items.Contains(board))
                                cbBoard.Items.Add(board);
                        }
                    }
                }
            }
            else if (chan == 5) {
                if (File.Exists(settingsDir + @"\u18chanhistory.dat")) {
                    using (StreamReader readFile = new StreamReader(settingsDir + @"\u18chanhistory.dat")) {
                        string readLine;
                        while ((readLine = readFile.ReadLine()) != null) {
                            Uri threadURL = new UriBuilder(getURL(readLine)).Uri;
                            string board = "/" + threadURL.Segments[1];

                            if (!cbBoard.Items.Contains(board))
                                cbBoard.Items.Add(board);
                        }
                    }
                }
            }
            cbBoard.SelectedIndex = 0;
        }

        private string getURL(string inputString) {
            // Very choppy URL parsing.
            var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string retURL = "";
            int spacer = 0;

            if (spacer != 1)
                foreach (Match m in linkParser.Matches(inputString)) {
                    retURL = m.Value;
                    spacer += 1;
                    break;
                }

            return retURL;
        }
        #endregion

        #region btnOpen / btnClear / btnClose
        private void btnOpenDownloads_Click(object sender, EventArgs e) {
            if (cbSite.SelectedIndex == 0) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\4chan"))
                    Process.Start(YCSettings.Default.downloadPath + @"\4chan");
            }
            else if (cbSite.SelectedIndex == 1){
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\420chan"))
                    Process.Start(YCSettings.Default.downloadPath + @"\420chan");
            }
            else if (cbSite.SelectedIndex == 2) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\7chan"))
                    Process.Start(YCSettings.Default.downloadPath + @"\7chan");
            }
            else if (cbSite.SelectedIndex == 3) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\8ch"))
                    Process.Start(YCSettings.Default.downloadPath + @"\8ch");
            }
            else if (cbSite.SelectedIndex == 4) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\fchan"))
                    Process.Start(YCSettings.Default.downloadPath + @"\fchan");
            }
            else if (cbSite.SelectedIndex == 5) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\u18chan"))
                    Process.Start(YCSettings.Default.downloadPath + @"\u18chan");
            }
        }
        private void btnClear_Click(object sender, EventArgs e) {
            switch (MessageBox.Show("Are you sure you want to clear your history?", "YChanEx", MessageBoxButtons.YesNo)) {
                case System.Windows.Forms.DialogResult.Yes:
                    if (File.Exists(settingsDir + @"\4chanhistory.dat"))
                        File.Delete(settingsDir + @"\4chanhistory.dat");

                    if (File.Exists(settingsDir + @"\420chanhistory.dat"))
                        File.Delete(settingsDir + @"\420chanhistory.dat");

                    if (File.Exists(settingsDir + @"\7chanhistory.dat"))
                        File.Delete(settingsDir + @"\7chanhistory.dat");

                    if (File.Exists(settingsDir + @"\8chanhistory.dat"))
                        File.Delete(settingsDir + @"\8chanhistory.dat");

                    if (File.Exists(settingsDir + @"\fchanhistory.dat"))
                        File.Delete(settingsDir + @"\fchanhistory.dat");

                    if (File.Exists(settingsDir + @"\u18chanhistory.dat"))
                        File.Delete(settingsDir + @"\u18chanhistory.dat");

                    lbHistory.Items.Clear();
                    break;
            }
        }
        private void btnClose_Click(object sender, EventArgs e) {
            lbHistory.Items.Clear();
            this.Close();
        }
        #endregion

        #region mHistory (mCopyLink / mCopyID / mShowInDownloads / mRemove)
        private void mOpenLink_Click(object sender, EventArgs e) {
            Uri threadURL = new UriBuilder(getURL(lbHistory.SelectedItem.ToString())).Uri;
            Process.Start(threadURL.ToString());
        }
        private void mOpenArchive_Click(object sender, EventArgs e) {
            Uri threadURL = new UriBuilder(getURL(lbHistory.SelectedItem.ToString())).Uri;
            string thrID = threadURL.Segments[3];
            string thrBoard = threadURL.Segments[1];

            string fourChArchive = "https://archived.moe/" + thrBoard + "thread/" + thrID;
            string fourtwentyChArchive = "bruh...";
            string sevenChArchive = "Lol?";
            string infChArchive = "Lol, infinitechan archives";
            string fChArchive = "...?";
            string u18ChArchive = "Lol, u18chan archives";

            if (cbSite.SelectedIndex == 0)
                Process.Start(fourChArchive);
            else {
                if (MessageBox.Show(cbSite.SelectedItem + " does not have a archive available or does not need an archive.\nWould you like to try to open the URL?", "YChanEx", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    Process.Start(lbHistory.SelectedItem.ToString());
            }
        }
        private void mShowInDownloads_Click(object sender, EventArgs e) {
            Uri threadURL = new UriBuilder(getURL(lbHistory.SelectedItem.ToString())).Uri;
            string thrID = threadURL.Segments[3];
            string thrBoard = threadURL.Segments[1];
            threadURL = null;

            if (cbSite.SelectedIndex == 0) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\4chan\" + thrBoard + @"\" + thrID))
                    Process.Start(YCSettings.Default.downloadPath + @"\4chan\" + thrBoard + @"\" + thrID);
            }
            else if (cbSite.SelectedIndex == 1) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\420chan\" + thrBoard + @"\" + thrID))
                    Process.Start(YCSettings.Default.downloadPath + @"\420chan\" + thrBoard + @"\" + thrID);
            }
            else if (cbSite.SelectedIndex == 2) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\7chan\" + thrBoard + @"\" + thrID))
                    Process.Start(YCSettings.Default.downloadPath + @"\7chan\" + thrBoard + @"\" + thrID);
            }
            else if (cbSite.SelectedIndex == 3) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\8ch\" + thrBoard + @"\" + thrID))
                    Process.Start(YCSettings.Default.downloadPath + @"\8ch\" + thrBoard + @"\" + thrID);
            }
            else if (cbSite.SelectedIndex == 4) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\fchan\" + thrBoard + @"\" + thrID))
                    Process.Start(YCSettings.Default.downloadPath + @"\fchan\" + thrBoard + @"\" + thrID);
            }
            else if (cbSite.SelectedIndex == 5) {
                if (Directory.Exists(YCSettings.Default.downloadPath + @"\u18chan\" + thrBoard + @"\" + thrID))
                    Process.Start(YCSettings.Default.downloadPath + @"\u18chan\" + thrBoard + @"\" + thrID);
            }
        }
        private void mCopyLink_Click(object sender, EventArgs e) {
            if (lbHistory.SelectedIndex != -1) {
                Uri threadURL = new UriBuilder(getURL(lbHistory.SelectedItem.ToString())).Uri;
                Clipboard.SetText(threadURL.ToString());
            }
        }
        private void mCopyID_Click(object sender, EventArgs e) {
            Uri threadURL = new UriBuilder(getURL(lbHistory.SelectedItem.ToString())).Uri;
            Clipboard.SetText(threadURL.Segments[3].Replace(".html", ""));

            if (Properties.Settings.Default.debug)
                MessageBox.Show(threadURL.Segments[3].Replace(".html", "") + "\nCopied to clipboard");
            else
                MessageBox.Show("Copied ID \"" + threadURL.Segments[3].Replace(".html", "") + "\" to clipboard.");
        }
        private void mRemove_Click(object sender, EventArgs e) {
            Uri threadURL = new UriBuilder(getURL(lbHistory.SelectedItem.ToString())).Uri;
            if (MessageBox.Show("Do you want to remove thread " + threadURL.Segments[3].Replace(".html", "") + " from the history?", "YChanEx", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                string itemToDelete = lbHistory.SelectedItem.ToString();
                if (Properties.Settings.Default.debug)
                    MessageBox.Show(itemToDelete);

                if (cbSite.SelectedIndex == 0) {
                    fourChHistory.Items.RemoveAt(fourChHistory.Items.IndexOf(itemToDelete));
                    File.Delete(settingsDir + @"\4chanhistory.dat");
                    using (FileStream fileStream = File.Open(settingsDir + @"\4chanhistory.dat", FileMode.Create))
                    using (StreamWriter writeFile = new StreamWriter(fileStream)) {
                        foreach (var historyItem in fourChHistory.Items)
                            writeFile.WriteLine(historyItem.ToString());
                    }
                    lbHistory.Items.RemoveAt(lbHistory.SelectedIndex);
                }
                if (cbSite.SelectedIndex == 1) {
                    ftChHistory.Items.RemoveAt(ftChHistory.Items.IndexOf(itemToDelete));
                    File.Delete(settingsDir + @"\420chanhistory.dat");
                    using (FileStream fileStream = File.Open(settingsDir + @"\420chanhistory.dat", FileMode.Create))
                    using (StreamWriter writeFile = new StreamWriter(fileStream)) {
                        foreach (var historyItem in ftChHistory.Items)
                            writeFile.WriteLine(historyItem.ToString());
                    }
                    lbHistory.Items.RemoveAt(lbHistory.SelectedIndex);
                }
                else if (cbSite.SelectedIndex == 2) {
                    sevenChHistory.Items.RemoveAt(sevenChHistory.Items.IndexOf(itemToDelete));
                    File.Delete(settingsDir + @"\7chanhistory.dat");
                    using (FileStream fileStream = File.Open(settingsDir + @"\7chanhistory.dat", FileMode.Create))
                    using (StreamWriter writeFile = new StreamWriter(fileStream)) {
                        foreach (var historyItem in sevenChHistory.Items)
                            writeFile.WriteLine(historyItem.ToString());
                    }
                    lbHistory.Items.RemoveAt(lbHistory.SelectedIndex);
                }
                else if (cbSite.SelectedIndex == 3) {
                    eightChHistory.Items.RemoveAt(eightChHistory.Items.IndexOf(itemToDelete));
                    File.Delete(settingsDir + @"\8chanhistory.dat");
                    using (FileStream fileStream = File.Open(settingsDir + @"\8chanhistory.dat", FileMode.Create))
                    using (StreamWriter writeFile = new StreamWriter(fileStream)) {
                        foreach (var historyItem in eightChHistory.Items)
                            writeFile.WriteLine(historyItem.ToString());
                    }
                    lbHistory.Items.RemoveAt(lbHistory.SelectedIndex);
                }
                else if (cbSite.SelectedIndex == 4) {
                    fChHistory.Items.RemoveAt(fChHistory.Items.IndexOf(itemToDelete));
                    File.Delete(settingsDir + @"\fchanhistory.dat");
                    using (FileStream fileStream = File.Open(settingsDir + @"\fchanhistory.dat", FileMode.Create))
                    using (StreamWriter writeFile = new StreamWriter(fileStream)) {
                        foreach (var historyItem in fChHistory.Items)
                            writeFile.WriteLine(historyItem.ToString());
                    }
                    lbHistory.Items.RemoveAt(lbHistory.SelectedIndex);
                }
                else if (cbSite.SelectedIndex == 5) {
                    u18ChHistory.Items.RemoveAt(u18ChHistory.Items.IndexOf(itemToDelete));
                    File.Delete(settingsDir + @"\u18chanhistory.dat");
                    using (FileStream fileStream = File.Open(settingsDir + @"\u18chanhistory.dat", FileMode.Create))
                    using (StreamWriter writeFile = new StreamWriter(fileStream)) {
                        foreach (var historyItem in u18ChHistory.Items)
                            writeFile.WriteLine(historyItem.ToString());
                    }
                }
            }
        }
        #endregion

    }
}