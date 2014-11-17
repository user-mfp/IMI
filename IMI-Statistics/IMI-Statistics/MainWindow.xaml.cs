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

        // Raw data
        List<List<DateTime>> timestamps = new List<List<DateTime>>();
        List<List<string>> events = new List<List<string>>();
        List<List<string>> targets = new List<List<string>>();
        List<List<int>> userID = new List<List<int>>();
        List<List<int>> visitors = new List<List<int>>();
                
        // Implicit data
        int emptySessions;
        List<TimeSpan> firstBloods = new List<TimeSpan>(); // Over sessions -> max., min., avg.
        List<TimeSpan> durations = new List<TimeSpan>(); // Over sessions -> max., min., avg.

        Dictionary<string, int> newTargets = new Dictionary<string,int>(); // Count over sessions
        Dictionary<KeyValuePair<string, string>, int> transitions = new Dictionary<KeyValuePair<string, string>, int>(); // Bidirectional: <newT1, newT2> and <newT2, newT1> will be combined in the end 

        Dictionary<string, int> selectedTargets; // Count over sessions
        Dictionary<string, float> targetQuotas; // Over sessions: <Target, (newT / selectedT)>


        List<int> userIDsOA; // IDs over sessions -> max., min., avg.
        List<int> visitorsOA; // Visitors over sessions -> max., min., avg.
        float activeQuota; // Over sessions: active users / present visitors
        // Tricky: Dictionary<string, float> selectionTimes; // Over sessions -> max., min., avg.
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
            this.TMP_FILE = new List<string>();

            foreach (string line in System.IO.File.ReadAllLines(filePath, System.Text.Encoding.UTF8))
            { 
                this.TMP_FILE.Add(line);
            }
        }

        private void loadAndParseFiles()
        {
            foreach (string filePath in this.filePaths)
            {
                loadTxtToTmpFile(filePath);
                parseTmpFile();
            }
        }

        private void parseTmpFile()
        {
            // Analysis
            List<DateTime> timestamps = new List<DateTime>();
            List<string> events = new List<string>();
            List<string> targets = new List<string>();
            List<int> userID = new List<int>();
            List<int> visitors = new List<int>();

            foreach (string line in this.TMP_FILE)
            {
                string tmp_line = line + '\t';
                string tmp_element = "";
                int tabs = 0;

                foreach (char c in tmp_line)
                {
                    if (c != '\t')
                        tmp_element += c;
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
                        tmp_element = "";
                    }
                }
            }
            timestamps.RemoveRange(0, 2);
            timestamps.RemoveRange((timestamps.Count - 1), 1);
            events.RemoveRange(0, 1);
            targets.RemoveRange(0, 1);
            userID.RemoveRange(0, 1);
            visitors.RemoveRange(0, 1);

            this.timestamps.Add(timestamps);
            this.events.Add(events);
            this.targets.Add(targets);
            this.userID.Add(userID);
            this.visitors.Add(visitors);
        }

        private DateTime parseTmpElementForDateTime(string tmp_element)
        {
            string hhmmss = "";
            string ms = "";
            string time = "";

            if (tmp_element.Length > 8)
            {
                hhmmss = tmp_element.Remove(8).Replace('.', ':');
                ms = tmp_element.Remove(0, 8);
                time = hhmmss + ms;
            }

            DateTime timestamp;

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

        private string parseTmpElementForEvent(string tmp_element)
        {
            switch (tmp_element)
            {
                case "New Target":
                    return "New Target";
                case "Select Target":
                    return "Select Target";
                case "Start Session":
                    return "Start Session";
                case "Session paused":
                    return "Session Paused";
                case "Session resumed":
                    return "Session Resumed";
                case "End Session":
                    return "End Session";
                case "Duration":
                    return "Duration";
                default:
                    return null;
            }
        }

        private string parseTmpElementForTarget(string tmp_element)
        {
            return tmp_element;
        }

        private int parseTmpElementForUserID(string tmp_element)
        {
            int id;

            try
            {
                id = int.Parse(tmp_element);
            }
            catch
            {
                id = -1;
            }

            return id;
        }

        private int parseTmpElementForUsers(string tmp_element)
        {
            int users;

            try
            {
                users = int.Parse(tmp_element);
            }
            catch
            {
                users = -1;
            }

            return users;
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
            DateTime start = DateTime.Now;

            if (filesLoaded())
            {
                loadAndParseFiles();
                deleteEmptySessions();

                //TODO
                // 
                // - new Targets := Foreach targetName<-selectedTargetEvent() => Dictionary ++value
                // - selected Targets := Foreach targetName<-selectedTargetEvent() => Dictionary ++value
            }
            else
                MessageBox.Show("Keine Dateien ausgewählt. Bitte laden sie Dateien.");

            detFistBloods();
            detDurations();
            detNewTargets();

            this.label1.Content = this.filePaths.Count + " Files in " + (DateTime.Now - start).TotalSeconds.ToString() + "sec";
        }

        private void deleteEmptySessions()
        {
            int empty = 0;
            List<int> indexes = new List<int>();
            int deleted = 0;

            for (int file = 0; file != this.visitors.Count; ++file)
            {
                int check = -1;

                foreach (int line in this.visitors[file])
                {
                    if (line != -1)
                    {
                        check = line;   
                    }
                }

                if (check == -1)
                {
                    indexes.Add(file);
                    ++empty;
                }
            }

            foreach (int index in indexes)
            {
                int delete = index - deleted;
                this.timestamps.Remove(this.timestamps[delete]);
                this.events.Remove(this.events[delete]);
                this.targets.Remove(this.targets[delete]);
                this.userID.Remove(this.userID[delete]);
                this.visitors.Remove(this.visitors[delete]);
                ++deleted;
            }

            this.emptySessions = empty;
            this.label2.Content = empty + "/" + this.filePaths.Count + " Sessions leer"+ '\n';
        }

        private void detFistBloods()
        {
            foreach (List<DateTime> file in this.timestamps)
            {
                this.firstBloods.Add(file[1] - file[0]);
            }

            this.label2.Content += this.firstBloods[0].ToString() + " until First Blood" + '\n';
        }

        private void detDurations()
        {
            foreach (List<DateTime> file in this.timestamps)
            {
                this.durations.Add(file[file.Count-2] - file[0]);
            }

            this.label2.Content += this.durations[0].ToString() + " Duration" + '\n';
        }

        private void detNewTargets()
        {
            //Dictionary<string, int> newTargets
            foreach (List<string> file in this.events)
            {
                for (int evnt = 0; evnt != file.Count; ++evnt)
                {
                    if (file[evnt] == "New Target")
                    {

                    }
                }
            }
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

        private void writeDebug(List<DateTime> timestamps, List<string> events, List<string> targets, List<int> userID, List<int> visitors)
        {
            string path = @"D:\MSC\StatDebug\" + DateTime.Now.ToString("HH.mm.ss") + "_fileCopy.txt";
            string data = "";

            for (int i = 0; i != timestamps.Count; ++i)
            {
                data += timestamps[i].ToString("HH.mm.ss.fffffff") + '\t' + events[i] + '\t' + targets[i] + '\t' + userID[i] + '\t' + visitors[i] + '\n';
            }

            writeTxt(path, data);
        }

        private void writeTxt(string path, string data)
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
            writeDebug(this.timestamps[0], this.events[0], this.targets[0], this.userID[0], this.visitors[0]);
        }
        #endregion
    }
}
