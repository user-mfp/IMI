using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace IMI_Statistics
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region DECLARATIONS
        // Files
        List<string> filePaths;
        List<string> TMP_FILE;

        // Explicit data
        List<string> newTarget; // Count per session
        List<string> selectedTarget; // Count per Session
        List<int> id; // Active users per session
        List<int> visitor; // All visitors per session
        float firstBlood; // Time from start of the session to first event
        
        // Implicit data
        List<int> ids; // IDs over sessions -> max., min., avg.
        List<int> visitors; // Visitors over sessions -> max., min., avg.
        float activeQuota; // Over sessions: active users / present visitors
        List<float> firstBloods; // Over sessions -> max., min., avg.
        List<string> durations; // Over sessions -> max., min., avg.
        Dictionary<string, string> transitions; // Bidirectional: <newT1, newT2> and <newT2, newT1> will be combined in the end 
        // Tricky: Dictionary<string, float> selectionTimes; // Over sessions -> max., min., avg.
        Dictionary<string, int> newTargets; // Count over sessions
        Dictionary<string, int> selectedTargets; // Count over sessions
        Dictionary<string, float> targetQuotas; // Over sessions: <Target, (newT / selectedT)>
        #endregion

        #region READ FILES
        private void loadFiles()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == true)
            {
                this.filePaths = new List<string>();

                foreach (string path in ofd.FileNames)
                {
                    this.filePaths.Add(path);
                }
            }
            else
            {
                ofd = null;
            }
        }

        public void loadTxtToTmpFile(string filePath)
        {
            if (this.TMP_FILE == null)
                this.TMP_FILE = new List<string>();

            foreach (string line in System.IO.File.ReadAllLines(filePath, System.Text.Encoding.UTF8))
            { 
                this.TMP_FILE.Add(line);
            }
        }
        #endregion

        #region ANALYZE FILES
        private bool filesLoaded()
        {
            if (this.filePaths != null)
                return true;
            else
                return false;
        }

        private void analyzeFiles()
        {
            if (filesLoaded())
            {
                loadTxtToTmpFile(this.filePaths[0]);
                parseTmpFile();
            }
            else
                MessageBox.Show("Keine Dateien ausgewählt. Bitte laden sie Dateien.");
        }

        private void parseTmpFile()
        {
            // Parsing
            string line = this.TMP_FILE[2];
            string tmp_element = "";
            int tabs = 0;

            // Analysis
            List<DateTime> timestamps = new List<DateTime>();
            List<string> events = new List<string>();
            List<string> targets = new List<string>();
            List<int> userID = new List<int>();
            List<int> visitors = new List<int>();

            foreach (char i in line)
            {
                if (i != '\t')
                    tmp_element += i;
                else
                {
                    switch (tabs)
                    {
                        case 0:
                            timestamps.Add(parseTmpElementForDateTime(tmp_element));
                            break;
                        case 1:
                            events.Add(parseTmpElementForEvent(tmp_element));
                            break;
                        case 2:
                            targets.Add(parseTmpElementForTarget(tmp_element));
                            break;
                        case 3:
                            userID.Add(parseTmpElementForUserID(tmp_element));
                            break;
                        case 4:
                            visitors.Add(parseTmpElementForUsers(tmp_element));
                            break;
                        default:
                            break;                        
                    }
                    ++tabs;
                }

            }
        }

        private DateTime parseTmpElementForDateTime(string tmp_element)
        {
            string hhmmss = tmp_element.Remove(8).Replace('.', ':');
            string ms = tmp_element.Remove(0, 8);
            string time = hhmmss + ms;

            DateTime timestamp = new DateTime();

            try
            {
                timestamp = DateTime.Parse(time);
            }
            catch
            {
                timestamp = new DateTime(); 
            }

            return timestamp;
        }

        private void parseTmpElementForEvent(string tmp_element)
        {
            if (tmp_element == "New Target")
            { 
            
            }
            else if (tmp_element == "Select Target")
            { 
            
            }
            else if (tmp_element == "End Session")
            { 
            
            }
            else

        }

        private void parseTmpElementForTarget(string tmp_element)
        {

        }

        private void parseTmpElementForUserID(string tmp_element)
        {

        }

        private void parseTmpElementForUsers(string tmp_element)
        {

        }
        #endregion

        #region WRITE FILES
        private void writePaths()
        {
            string output = @"D:\MSC\StatDebug\" + DateTime.Now.ToString("HH.mm.ss") + "_filePaths.txt";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(output))
            {
                foreach (string filePath in this.filePaths)
                {
                    file.WriteLine(filePath);
                }
                file.Close();
            }
        }

        public void writeTxt(string path, string data)
        {
            System.IO.File.WriteAllText(path, data, System.Text.Encoding.UTF8);
        }
        #endregion

        #region EVENTS
        // LOAD LOGDATA-FILES
        private void button1_Click(object sender, RoutedEventArgs e) 
        {
            loadFiles();
            this.label1.Content = this.filePaths.Count.ToString() + " Files loaded.";
        }

        // ANALYZE LOGDATA-FILES
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            analyzeFiles();
        }

        // DEBUG-EVENT
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            writePaths();
        }
        #endregion
    }
}
