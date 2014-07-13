using AD_Splitter.xMicrosoft;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Google.API.Translate;

namespace AD_Splitter
{
    public partial class frmDataAD : Form
    {
        public frmDataAD()
        {
            InitializeComponent();
        }

        private void cmdStart_Click(object sender, EventArgs e)
        {
            if (txtOutFolder4Split.Text.Trim() == "")
            {
                MessageBox.Show("Please specify Destination Folder Path first, Thank you.");
                return;
            }

            SaveConfigSettings();

            if (chkHTML.Checked)
            {
                var processor = new LineProcessor();
                processor.ProcessLineByLine(txtFileName.Text, txtPrefix.Text, txtOutFolder4Split.Text, txtSplitChar.Text);
            }
            else
            {
                RunOldCode();
            }

            MessageBox.Show("Task Completed", "bismillah", MessageBoxButtons.OK);
        }

        private void aLLAHcomMuhammadcomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("iexplore.exe", "www.allah.com");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtFileName.Text = LoadFileDialogBox();            
        }

        private string LoadFileDialogBox()
        {
            of1.FileName = Environment.CurrentDirectory;
            DialogResult dr = of1.ShowDialog();
            if (dr == DialogResult.OK)       
                return of1.FileName;            

            return "";
        }

        string[] sections;

        /// <summary>
        /// Load Input File for reading
        /// </summary>
        private void LoadFileSplitLines(string FileName)
        {
            if (FileName == "") return;

            TextReader tr = new StreamReader(FileName, Encoding.Default);
            string wholeFile = tr.ReadToEnd();
            tr.Close();

            sections = wholeFile.Split(new string[] { txtSplitChar.Text }, StringSplitOptions.RemoveEmptyEntries);

        }        

        private void button2_Click(object sender, EventArgs e)
        {
            txtOutFolder4Split.Text = GetFolderPath();
        }

        private string GetFolderPath()
        {
            DialogResult dr = fb1.ShowDialog();
            if (dr == DialogResult.OK)
                return fb1.SelectedPath;

            return "";

        }

        string FileExtension = "txt";

        

        private void RunOldCode()
        {
            LoadFileSplitLines(txtFileName.Text);
            FileExtension = chkHTML.Checked ? "html" : "txt";
            if (sections == null || sections.Length == 0)
            {
                MessageBox.Show("File not read properly. File Name: " + txtFileName.Text);
                return;
            }

            TextWriter tr;
            string OutFileName = "";
            string TOCIndexFileName = "";


            for (int i = 0; i < sections.Length; i++)
            {

                // to be used in HTML pages
                //subSections[0] only will be used for naming file
                string[] subSections = sections[i].Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                string FileSectionName = CleanFileName(subSections[0]);

                // i+1 => MAKE NUMBERING IN AD_SPLITTER STARTS 0001 NOT 0000 WHICH IS CURRENTLY
                string PrefixPart = GetPrefixPart(txtPrefix.Text, i + 1);

                //add prefix
                FileSectionName = String.Format("{0}-{1}", PrefixPart, FileSectionName);

                if (FileSectionName.Length + txtOutFolder4Split.Text.Length > 250)
                {
                    int CutLength = 255 - txtOutFolder4Split.Text.Length;

                    if (CutLength > 0 && FileSectionName.Length > CutLength)
                        FileSectionName = FileSectionName.Substring(0, CutLength);
                }

                OutFileName = String.Format("{0}\\{1}.{2}", txtOutFolder4Split.Text, FileSectionName, FileExtension);
                tr = new StreamWriter(OutFileName, append: false, encoding: Encoding.UTF8);

                if (i == 0) // save TOC file name
                {
                    TOCIndexFileName = OutFileName;
                }
                else // insert topic file names and links in the TOC file
                {
                    StreamWriter trTOC = new StreamWriter(TOCIndexFileName, true, Encoding.Default);
                    string strLinkText = String.Format("{0}.{1}", FileSectionName, FileExtension);
                    trTOC.WriteLine(String.Format("<a href='{0}'>{0}</a> <br/>", strLinkText));
                    trTOC.Close();
                }


                //write data lines to HTML file or text file
                if (chkHTML.Checked)
                {
                    WriteLinesToHTMLFile(tr, TOCIndexFileName, i, subSections);
                }

                else
                { // only text files

                    tr.WriteLine(RemoveMultipleBlankLines(sections[i]));
                }

                tr.Close();
            }
        }

     
       

        private void WriteLinesToHTMLFile(TextWriter tr, string TOCIndexFileName, int i, string[] subSections)
        {
            tr.WriteLine("<html> <body>");
            // Link to Home- TOC file at top
            tr.WriteLine(String.Format("<a href='{0}'>Home</a> <br/>", Path.GetFileName(TOCIndexFileName))); 
            
            tr.WriteLine(String.Format("<h1>{0}</h1>", subSections[0]));

            for (int j = 0; j < subSections.Length; j++)
            {
                tr.WriteLine("<br/>");
                tr.WriteLine(subSections[j]);
            }

            if (i == 0)
                tr.WriteLine("<br/>");

            else // Link to Home Page 0000
                tr.WriteLine(String.Format("<br/> <a href='{0}'>Home</a> <br/>", Path.GetFileName(TOCIndexFileName))); // Link to Home- TOC file at bottom

            tr.WriteLine("</body></html>");
        }

        private string RemoveMultipleBlankLines(string input)
        {
            while (input.Contains("\r\n\r\n"))
            {
                input = input.Replace("\r\n\r\n", Environment.NewLine);
            }            
            return input;
        }

        private string GetPrefixPart(string prefix, int Count)
        {
            if (Count < 10)
                return String.Format("{0}000{1}", prefix, Count);
            else if (Count < 100)
                return String.Format("{0}00{1}", prefix, Count);
            else if (Count < 1000)
                return String.Format("{0}0{1}", prefix, Count);
            else
                return prefix + Count;
            
        }

        private string CleanFileName(string fileName)
        {
            if (fileName == null || fileName.Length == 0) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in fileName)
                if (Char.IsLetter(c) || char.IsWhiteSpace(c))     
                    sb.Append(c);                           
            return sb.ToString().Trim();
        }

        private void SaveConfigSettings()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration("AD_Splitter.exe");

                if (config != null)
                {
                    config.AppSettings.Settings["DestinationPath"].Value = txtOutFolder4Split.Text;
                    config.AppSettings.Settings["GoogleInPath"].Value = txtGoogleInFolder.Text;
                    config.AppSettings.Settings["GoogleOutPath"].Value = txtGoogleOutFolder.Text;
                    config.AppSettings.Settings["LangFrom"].Value = txtLangFrom.Text;
                    config.AppSettings.Settings["LangTo"].Value = txtLangTo.Text;
                    config.Save();
                }
            }
            catch (Exception)
            {                
                //ignore
            }
            
        }

        /// <summary>
        /// HTML based or text based
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkHTML_CheckedChanged(object sender, EventArgs e)
        {
            if (chkHTML.Checked)
            {
                FileExtension = "html";
            }
            else
            {
                FileExtension = "txt";
            }
        }

        private void frmDataAD_Load(object sender, EventArgs e)
        {           
            txtOutFolder4Split.Text = ConfigurationManager.AppSettings["DestinationPath"];
            txtGoogleInFolder.Text = ConfigurationManager.AppSettings["GoogleInPath"];
            txtGoogleOutFolder.Text = ConfigurationManager.AppSettings["GoogleOutPath"];
            txtLangFrom.Text = ConfigurationManager.AppSettings["LangFrom"];
            txtLangTo.Text = ConfigurationManager.AppSettings["LangTo"];

            timerPause.Enabled = false;
        }

        int MAX_LENGTH = 300;
        TranslateClient GoogleClient = new TranslateClient("www.allah.com/" + DateTime.Now.Ticks);
        xMicrosoft.SoapService MicrosoftClient = new SoapService(); 

        /// <summary>
        /// split line into words 
        /// create lines of maximum 300 words in a list
        /// get their translation back from google
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetTranslationBack(string line)
        {
            try
            {
                string[] words = line.Split(new char[] { });
                List<string> sendWordList = new List<string>();

                string sendWord = "";

                MAX_LENGTH = radGoogle.Checked ? 300 : 500; // 1000 for microsoft

                for (int i = 0; i < words.Length; i++)
                {
                    sendWord = String.Format("{0} {1}", sendWord, words[i]);

                    if (sendWord.Length >= MAX_LENGTH)
                    {
                        sendWordList.Add(sendWord);
                        sendWord = "";
                    }
                }

                if (sendWord != "") sendWordList.Add(sendWord); //insert the last portion

                //translate here
                string translated = "";

                if (radGoogle.Checked)
                {
                    for (int i = 0; i < sendWordList.Count; i++)
                    {
                        while (MicrosoftClient == null)
                        {
                            Thread.Sleep(2000);
                        }
                        translated += GoogleClient.Translate(sendWordList[i], txtLangFrom.Text, txtLangTo.Text);
                    }
                }
                
                //Microsoft translator
                else if (radMS.Checked)
                {
                    for (int i = 0; i < sendWordList.Count; i++)
                    {
                        while (MicrosoftClient == null)
                        {
                            Thread.Sleep(2000);
                        }
                        translated = MicrosoftClient.Translate("6CA9AD9D9A700D2FD286468D8706AD8886C04A9C", sendWordList[i], txtLangFrom.Text, txtLangTo.Text);
                    }
                }                

                return translated;
            }
            catch (Exception ex)
            {
                bgWorker.ReportProgress(0, "GetTranslationBack:" + ex.Message.ToString());
                return "-1";
            }
            
        }

        private void cmdGoogleTranslate_Click(object sender, EventArgs e)
        {
            try
            {
                lblStartTime.Text += DateTime.Now.ToShortTimeString();
                SaveConfigSettings();

                timerPause.Interval = Convert.ToInt32(txtMins.Text) * 60 * 1000;
                timerPause.Enabled = true;
                cmdPause.Enabled = true;
                cmdGoogleTranslate.Enabled = false;

                if (txtGoogleInFolder.Text == "")
                {
                    MessageBox.Show("Please select Google Input folder. Files will be read from this folder. Thank you!");
                    return;
                }

                if (txtGoogleOutFolder.Text == "")
                {
                    MessageBox.Show("Please select Google destination folder. Translated Files will be stored there. shukran!");
                    return;
                }               
                
                WriteGoogleTranslatedFile(); 
                
            }
            catch (Exception ex)
            {
                timerPause.Enabled = false;
                MessageBox.Show("cmdGoogleTranslate_Click:" + ex);
            }
            
        }

       
        StreamReader sr = null;            StreamWriter sw = null;
        
        private void WriteGoogleTranslatedFile()
        {
            MAX_LENGTH = Convert.ToInt32(txtChunk.Text);
            Cursor.Current = Cursors.WaitCursor;
            
            try
            {
                this.Text = "bismillah! Translation has started now in Output Folder...";

                bgWorker.WorkerReportsProgress = true;
                bgWorker.RunWorkerAsync();                                
            }
            catch (Exception ex)
            {
                this.Text = "alhamdulillah! completed with some errors.";
                timerPause.Enabled = false;
                MessageBox.Show("WriteGoogleTranslatedFile:" + ex.ToString());
            }
            
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            
            try
            {
                string[] files = Directory.GetFiles(txtGoogleInFolder.Text);

                for (int i = 0; i < files.Length; i++)
                {
                    sr = new StreamReader(files[i], Encoding.Default);
                    CreateOutputFileWithNewName(files[i], ref sw);
                    
                    bgWorker.ReportProgress(i, files[i]);

                    string line = ""; 
                    string result = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line != "")
                        {
                            //show first 10 characters from line in windows title
                            //this.Text = "Getting translation for " + line.Substring(0, (line.Length >= 10) ? 10 : line.Length);
                            bgWorker.ReportProgress(i, files[i] + " : " + line.Substring(0, (line.Length >= 20) ? 20 : line.Length) + " ........");
                            result = GetTranslationBack(line);
                            int G = 1;

                            while (result=="-1")
                            {
                                bgWorker.ReportProgress(i, String.Format("***Service complaining!..pausing for {0} seconds and trying again...", 
                                    G * 10));
                                GoogleClient = null;
                                MicrosoftClient = null;
                                
                                timerPause.Enabled = false;
                                Thread.Sleep(10000 * G++);

                                if (radGoogle.Checked)
                                    GoogleClient = new TranslateClient("www.allah.com/" + DateTime.Now.Ticks);
                                else
                                    MicrosoftClient = new SoapService();

                                timerPause.Enabled = true;

                                result = GetTranslationBack(line);
                            }

                            sw.Write(String.Format(" {0}{1}", result, Environment.NewLine));
                            sw.Flush();
                            Thread.Sleep(50);
                        }
                    }
                    
                    sr.Close(); sw.Close(); Cursor.Current = Cursors.Arrow;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }    
        private void CreateOutputFileWithNewName(string OutFileName, ref StreamWriter sw)
        {
            //this is default name
            string outGoogleName = txtGoogleOutFolder.Text + "\\" + 
                                   txtLangFrom.Text + "-" + txtLangTo.Text + "-" + 
                                   Path.GetFileName(OutFileName);

            try
            {
                outGoogleName = txtGoogleOutFolder.Text + "\\" +
                                GetTranslationBack(Path.GetFileName(OutFileName));
            }
            catch (Exception) {  /*ignore, use default name*/ }

            sw = new StreamWriter(outGoogleName.Trim());
        }       

        private void cmdGoogleFile_Click(object sender, EventArgs e)
        {
            txtGoogleInFolder.Text = GetFolderPath();
            txtGoogleOutFolder.Text = txtGoogleInFolder.Text + "\\TranslatedOutput";
            Directory.CreateDirectory(txtGoogleOutFolder.Text);
        }

        private void cmdGoogleFolder_Click(object sender, EventArgs e)
        {
            txtGoogleOutFolder.Text = GetFolderPath();
        }

        private void timerPause_Tick(object sender, EventArgs e)
        {
            PauseWorkHandler();
        }        
        
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblStartTime.Text += " - End Time:" + DateTime.Now.ToShortTimeString();
            cmdPause.Enabled = false;
            cmdGoogleTranslate.Enabled = true;
            this.Text = "alhamdulillah! completed with success";
            timerPause.Enabled = false;
            MessageBox.Show("alhamdulillah! completed with success");
        }

        private void cmdPause_Click(object sender, EventArgs e)
        {
            PauseWorkHandler();
        }

        private void PauseWorkHandler()
        {
            if (bgWorker2.IsBusy)
                return;
            bgWorker2.RunWorkerAsync();
            bgWorker2.WorkerReportsProgress = true;
            cmdPause.Enabled = false;
        }
        private void bgWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            PauseWork();
            cmdPause.Enabled = true;
        }

        private void PauseWork()
        {
            MicrosoftClient = null;
            this.Text = String.Format("Google Taking breath - Pausing for {0} seconds...", txtSeconds.Text);
            txtLog.Text += String.Format("{0}Google Taking breath - Pausing for {1} seconds...", Environment.NewLine, txtSeconds.Text);
            Thread.Sleep(Convert.ToInt32(txtSeconds.Text) * 1000);

            //client = new TranslateClient("www.allah.com");
            MicrosoftClient = new SoapService();

            this.Text = "Working again now...";
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string s = e.UserState.ToString();
            lblMessage.Text = String.Format("{0}. {1}", DateTime.Now.ToShortTimeString(), s);
            
            if (!s.Contains("...."))
                txtLog.Text += Environment.NewLine + s;
        }

        private void linkGoogleInput_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer", txtGoogleInFolder.Text);
        }

        private void linkGoogleOut_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer", txtGoogleOutFolder.Text);
        }

        private void txtMins_TextChanged(object sender, EventArgs e)
        {
            timerPause.Interval = Convert.ToInt32(txtMins.Text) * 60 * 1000;
        }

        private void lnkSplitOutFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer", txtOutFolder4Split.Text);
        }
        
    }
}























