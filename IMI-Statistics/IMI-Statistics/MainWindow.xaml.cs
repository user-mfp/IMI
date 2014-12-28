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

        Dictionary<string, int> newTargets = new Dictionary<string, int>(); // Count over sessions  -> max., min., avg.
        Dictionary<KeyValuePair<string, string>, int> transitions = new Dictionary<KeyValuePair<string, string>, int>(); // Bidirectional: <newT1, newT2> and <newT2, newT1> will be combined in the end 

        Dictionary<string, int> selectedTargets = new Dictionary<string,int>(); // Count over sessions -> max., min., avg.
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
            // Cut upper lines
            timestamps.RemoveRange(0, 2);
            timestamps.RemoveRange((timestamps.Count - 1), 1);
            events.RemoveRange(0, 1);
            targets.RemoveRange(0, 1);
            userID.RemoveRange(0, 1);
            visitors.RemoveRange(0, 1);

            // Cut to last active event
            cutToLastActiveEvent(ref timestamps, ref events, ref targets, ref userID, ref visitors);

            this.timestamps.Add(timestamps);
            this.events.Add(events);
            this.targets.Add(targets);
            this.userID.Add(userID);
            this.visitors.Add(visitors);
        }

        private void cutToLastActiveEvent(ref List<DateTime> timestamps, ref List<string> events, ref List<string> targets, ref List<int> userID, ref List<int> visitors)
        {
            int cutIndex = getLastActiveEvent(events) + 1;
            int cutRange = timestamps.Count - cutIndex;

            // Cut to last active event
            timestamps.RemoveRange(cutIndex, cutRange);
            events.RemoveRange(cutIndex, cutRange);
            targets.RemoveRange(cutIndex, cutRange);
            userID.RemoveRange(cutIndex, cutRange);
            visitors.RemoveRange(cutIndex, cutRange);
        }

        private int getLastActiveEvent(List<string> events)
        {
            int cutIndex = -1;

            for (int i = events.Count - 1; i != -1; --i)
            {
                if (events[i] == "New Target" || events[i] == "Select Target")
                {
                    cutIndex = i;
                    break;
                }
            }

            return cutIndex;
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
            detSelectedTargets();
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

        private bool allEmpty()
        {
            if (this.emptySessions == this.filePaths.Count)
                return true;
            else
                return false;
        }

        private void detFistBloods()
        {
            foreach (List<DateTime> file in this.timestamps)
            {
                this.firstBloods.Add(file[1] - file[0]);
            }

            if (!allEmpty())
            {
                this.label2.Content += '\n' + "FIRST BLOOD" + '\n';
                this.label2.Content += "Longest time span until First Blood:" + '\t' + getMaxTimeSpan(this.firstBloods) + '\n';
                this.label2.Content += "Shortest time span until First Blood:" + '\t' + getMinTimeSpan(this.firstBloods) + '\n';
                this.label2.Content += "Average time span until First Blood:" + '\t' + getAvgTimeSpan(this.firstBloods) + '\n';
            }
        }

        private void detDurations()
        {
            foreach (List<DateTime> file in this.timestamps)
            {
                this.durations.Add(file[file.Count-2] - file[0]);
            }

            if (!allEmpty())
            {
                this.label2.Content += '\n' + "SESSION DURATION" + '\n';
                this.label2.Content += "Longest duration of a session:" + '\t' + getMaxTimeSpan(this.durations) + '\n';
                this.label2.Content += "Shortest duration of a session:" + '\t' + getMinTimeSpan(this.durations) + '\n';
                this.label2.Content += "Average duration of a session:" + '\t' + getAvgTimeSpan(this.durations) + '\n';
            }
        }

        private TimeSpan getMinTimeSpan(List<TimeSpan> timespans)
        {
            TimeSpan minTimeSpan = timespans[0];
            TimeSpan tmpTimeSpan = timespans[0];

            foreach(TimeSpan timespan in timespans)
            {
                if (timespan < minTimeSpan)
                    minTimeSpan = timespan;

                tmpTimeSpan = timespan;
            }
            
            return minTimeSpan;
        }

        private TimeSpan getMaxTimeSpan(List<TimeSpan> timespans)
        {
            TimeSpan maxTimeSpan = timespans[0];
            TimeSpan tmpTimeSpan = timespans[0];

            foreach (TimeSpan timespan in timespans)
            {
                if (timespan > maxTimeSpan)
                    maxTimeSpan = timespan;

                tmpTimeSpan = timespan;
            }

            return maxTimeSpan;
        }

        private TimeSpan getAvgTimeSpan(List<TimeSpan> timespans)
        {
            TimeSpan tmpTimeSpan = new TimeSpan();

            foreach (TimeSpan timespan in timespans)
            { 
                tmpTimeSpan += timespan;
            }

            tmpTimeSpan = TimeSpan.FromMilliseconds(tmpTimeSpan.TotalMilliseconds / timespans.Count);

            return tmpTimeSpan;
        }

        private void detNewTargets()
        {
            string tmp_target = "";
            KeyValuePair<string, string> tmp_pair;

            for (int file = 0; file != this.events.Count; ++file)//foreach (List<string> file in this.events)
            {
                for (int line = 0; line != this.events[file].Count; ++line)
                {
                    if (this.events[file][line] == "New Target")
                    {
                        // New Targets
                        if (this.newTargets.ContainsKey(this.targets[file][line]))
                            this.newTargets[this.targets[file][line]] += 1;
                        else
                            this.newTargets.Add(this.targets[file][line], 1);

                        // Transitions
                        if (tmp_target == "")
                        {
                            tmp_target = this.targets[file][line];
                        }
                        else
                        { 
                            tmp_pair = new KeyValuePair<string, string>(tmp_target, this.targets[file][line]);
                            
                            if (this.transitions.ContainsKey(tmp_pair))
                                this.transitions[tmp_pair] += 1;
                            else
                                this.transitions.Add(tmp_pair, 1);

                            tmp_target = this.targets[file][line];
                        }
                    }
                    else
                        tmp_target = "";
                }
            }

            if (!allEmpty())
            {
                //this.label2.Content += "NEW TARGETS" + '\n';
                //foreach (KeyValuePair<string, int> target in this.newTargets)
                //{
                //    this.label2.Content += target.Key + ": " + target.Value + '\n';
                //}

                sortTransitions();
                this.label2.Content += '\n' + "TRANSITIONS" + '\n';
                foreach (KeyValuePair<KeyValuePair<string, string>, int> transition in this.transitions)
                {
                    this.label2.Content += transition.Key.Key + " ; " + transition.Key.Value + ": " + transition.Value + '\n';
                }
            }
        }

        private void sortTransitions()
        {
            Dictionary<KeyValuePair<string, string>, int> transitions = new Dictionary<KeyValuePair<string,string>,int>();
            KeyValuePair<string, string> tmp_kvp;

            foreach (KeyValuePair<KeyValuePair<string, string>, int> transition in this.transitions)
            {
                tmp_kvp = new KeyValuePair<string,string>(transition.Key.Value, transition.Key.Key);

                if (transitions.ContainsKey(transition.Key))
                {
                    transitions[transition.Key] += transition.Value;
                }
                else if (transitions.ContainsKey(tmp_kvp))
                {
                    transitions[tmp_kvp] += transition.Value;
                }
                else
                {
                    transitions.Add(transition.Key, transition.Value);
                }
            }

            this.transitions = transitions;
        }

        private void detSelectedTargets()
        {
            for (int file = 0; file != this.events.Count; ++file)//foreach (List<string> file in this.events)
            {
                for (int line = 0; line != this.events[file].Count; ++line)
                {
                    if (this.events[file][line] == "Select Target")
                    {
                        // Target Selected
                        if (this.selectedTargets.ContainsKey(this.targets[file][line]))
                            this.selectedTargets[this.targets[file][line]] += 1;
                        else
                            this.selectedTargets.Add(this.targets[file][line], 1);
                    }
                }
            }

            if (!allEmpty())
            {
                this.label2.Content += '\n' + "SELECTED TARGETS" + '\n';
                foreach (KeyValuePair<string, int> target in this.selectedTargets)
                {
                    this.label2.Content += target.Key + ": " + target.Value + '\n';
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
