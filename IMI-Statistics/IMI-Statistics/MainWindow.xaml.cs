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
        // FILES
        List<string> filePaths;
        List<string> TMP_FILE;

        // RAW DATA
        List<List<DateTime>> timestamps;
        List<List<string>> events;
        List<List<string>> targets;
        List<List<int>> userID;
        List<List<int>> visitors;
                
        // IMPLICIT DATA
        int emptySessions;
        List<int> emptyIndexes;
        List<int> activeIndexes;
        List<TimeSpan> emptyDurations;
        List<TimeSpan> firstBloods; 
        List<TimeSpan> durations; 

        Dictionary<string, int> newTargets;
        List<string> newTargetsRaw;
        Dictionary<KeyValuePair<string, string>, int> transitionsOfTargets; // Bidirectional: <newT1, newT2> and <newT2, newT1> will be combined in the end 
        
        Dictionary<string, int> selectedTargets;
        List<string> selectedTargetsRaw;
        Dictionary<KeyValuePair<string, string>, int> transitionsOfSelections; // Bidirectional: <newT1, newT2> and <newT2, newT1> will be combined in the end 

        //Dictionary<string, float> targetQuotas; // Over sessions: <Target, (newT / selectedT)>
        //List<int> userIDsOA; // IDs over sessions -> max., min., avg.
        //List<int> visitorsOA; // Visitors over sessions -> max., min., avg.
        //float activeQuota; // Over sessions: active users / present visitors
        // Tricky: Dictionary<string, float> selectionTimes; // Over sessions -> max., min., avg.
        #endregion

        #region INITIALIZATION
        private void InitContainers()
        {
            this.timestamps = new List<List<DateTime>>();
            this.events = new List<List<string>>();
            this.targets = new List<List<string>>();
            this.userID = new List<List<int>>();
            this.visitors = new List<List<int>>();
            this.firstBloods = new List<TimeSpan>();
            this.durations = new List<TimeSpan>();
            this.emptyDurations = new List<TimeSpan>();
            this.newTargets = new Dictionary<string, int>();
            this.newTargetsRaw = new List<string>();
            this.transitionsOfTargets = new Dictionary<KeyValuePair<string, string>, int>();
            this.selectedTargets = new Dictionary<string, int>();
            this.selectedTargetsRaw = new List<string>();
            this.transitionsOfSelections = new Dictionary<KeyValuePair<string, string>, int>();
        }
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
            cutToLastEvent(ref timestamps, ref events, ref targets, ref userID, ref visitors);

            this.timestamps.Add(timestamps);
            this.events.Add(events);
            this.targets.Add(targets);
            this.userID.Add(userID);
            this.visitors.Add(visitors);
        }

        private void cutToLastEvent(ref List<DateTime> timestamps, ref List<string> events, ref List<string> targets, ref List<int> userID, ref List<int> visitors)
        {
            int cutIndex = getLastEvent(events) + 2;
            int cutRange = timestamps.Count - cutIndex;

            // Cut to last active event
            timestamps.RemoveRange(cutIndex, cutRange);
            events.RemoveRange(cutIndex, cutRange);
            targets.RemoveRange(cutIndex, cutRange);
            userID.RemoveRange(cutIndex, cutRange);
            visitors.RemoveRange(cutIndex, cutRange);
        }

        private int getLastEvent(List<string> events)
        {
            int cutIndex = -1;

            for (int i = events.Count - 1; i != -1; --i)
            {
                if (events[i] == "New Target" || events[i] == "Select Target" || events[i] == "Start Session")
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

        /*private void analyzeFiles()
        {
            DateTime start = DateTime.Now;

            if (filesLoaded())
            {
                InitContainers();
                loadAndParseFiles();
                classifySessions();

                //TODO
                // 
                // - new Targets := Foreach targetName<-selectedTargetEvent() => Dictionary ++value
                // - selected Targets := Foreach targetName<-selectedTargetEvent() => Dictionary ++value
            }
            else
                MessageBox.Show("Keine Dateien ausgewählt. Bitte laden sie Dateien.");

            //detFistBloods();
            //detDurations();
            //detEmptyDurations();
            //detSelectedTargets();
            //detNewTargets();

            this.label1.Content = this.filePaths.Count + " Files in " + (DateTime.Now - start).TotalSeconds.ToString() + "sec";
        }*/

        private void makeAnalysis()
        {
            DateTime start = DateTime.Now;

            InitContainers();
            loadAndParseFiles();
            classifySessions();

            if (this.checkFirstBlood.IsChecked == true)
                detFistBloods();

            if (this.checkActiveDurations.IsChecked == true)
                detDurations();

            if (this.checkEmptyDurations.IsChecked == true)
                detEmptyDurations();

            if (this.checkMarkedTargets.IsChecked == true)
                detNewTargets();

            if (this.checkSelectedTargets.IsChecked == true)
                detSelectedTargets();

            this.label1.Content = this.filePaths.Count + " Files in " + (DateTime.Now - start).TotalSeconds.ToString() + "sec";
        }

        private void classifySessions()
        {
            this.emptyIndexes = detEmptySessions();
            this.activeIndexes = detActiveSessions();

            this.label2.Content += '\n' + "SESSIONS" + '\n';
            this.label2.Content += "- AKTIVE:" + '\t' + this.activeIndexes.Count.ToString() + '\n';
            this.label2.Content += "- PASSIVE:" + '\t' + this.emptyIndexes.Count.ToString() + '\n';
        }

        private List<int> detEmptySessions()
        {
            int empty = 0;
            List<int> indexes = new List<int>();

            for (int file = 0; file != this.visitors.Count; ++file)
            {
                int check = -1;

                foreach (int line in this.visitors[file])
                {
                    if (line != -1)
                    {
                        check = line;
                        break;
                    }
                }

                if (check == -1)
                {
                    indexes.Add(file);
                    ++empty;
                }
            }

            this.emptySessions = empty;

            return indexes;
        }

        private List<int> detActiveSessions()
        {
            List<int> indexes = new List<int>();

            for (int index = 0; index != this.timestamps.Count; ++index)
            {
                if (!this.emptyIndexes.Contains(index))
                {
                    indexes.Add(index);
                }
            }

            return indexes;            
        }

        /*private void deleteEmptySessions(List<int> indexes)
        {
            int deleted = 0;

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
        }
        */

        private bool allEmpty()
        {
            if (this.emptySessions == this.filePaths.Count)
                return true;
            else
                return false;
        }

        private bool allActive()
        {
            if (this.emptySessions == 0)
                return true;
            else
                return false;
        }

        private void detFistBloods() // TODO: First Blood nur für aktive Sessions!
        {
            foreach (List<DateTime> file in this.timestamps)
            {
                this.firstBloods.Add(file[1] - file[0]);
            }

            //OUTPUT ON GUI
            this.label2.Content += '\n' + "VORBEREITUNGSDAUERN" + '\n';

            if (allEmpty()) // ALL SESSIONS ARE PASSIVE
            {
                this.label2.Content += "- KEINE AKTIVEN SESSIONS" + '\n';
            }
            else // AT LEAST ONE ACTIVE SESSION
            {
                this.label2.Content += "- LÄNGSTE VORBEREITUNG:" + '\t' + getMaxTimeSpan(this.firstBloods) + '\n';
                this.label2.Content += "- KÜRZESTE VORBEREITUNG:" + '\t' + getMinTimeSpan(this.firstBloods) + '\n';
                this.label2.Content += "- MITTLERE VORBEREITUNG:" + '\t' + getAvgTimeSpan(this.firstBloods) + '\n';
            }
        }

        private void detDurations()
        {
            List<DateTime> file = new List<DateTime>();

            foreach (int index in this.activeIndexes)
            { 
                file = this.timestamps[index];

                if (file.Count != 2)
                    this.durations.Add(file[file.Count - 2] - file[0]);
                else
                    this.durations.Add(file[file.Count - 1] - file[0]);                
            }

            // OUTPUT ON GUI
            this.label2.Content += '\n' + "DAUER AKTIVER SESSIONS" + '\n';

            if (allEmpty()) // ALL SESSIONS ARE PASSIVE
            {
                this.label2.Content += "- KEINE AKTIVEN SESSIONS" + '\n';
            }
            else // AT LEAST ONE ACTIVE SESSION
            {
                this.label2.Content += "- LÄNGSTE AKTIVE SESSION:" + '\t' + getMaxTimeSpan(this.durations) + '\n';
                this.label2.Content += "- KÜRZESTE AKTIVE SESSION:" + '\t' + getMinTimeSpan(this.durations) + '\n';
                this.label2.Content += "- MITTLERE AKTIVE SESSION:" + '\t' + getAvgTimeSpan(this.durations) + '\n';
            }
        }

        private void detEmptyDurations()
        {
            List<DateTime> file = new List<DateTime>();

            foreach (int index in this.emptyIndexes)
            {
                file = this.timestamps[index];

                if (file.Count != 2)
                    this.emptyDurations.Add(file[file.Count - 2] - file[0]);
                else
                    this.emptyDurations.Add(file[file.Count - 1] - file[0]);
            }

            // OUTPUT ON GUI
            this.label2.Content += '\n' + "DAUER PASSIVER SESSIONS" + '\n';

            if (allActive()) // ALL SESSIONS ARE ACTIVE
            {
                this.label2.Content += "- KEINE PASSIVEN SESSIONS" + '\n';
            }
            else // AT LEAST ONE PASSIVE SESSION
            {
                this.label2.Content += "- LÄNGSTE PASSIVE SESSION:" + '\t' + getMaxTimeSpan(this.emptyDurations) + '\n';
                this.label2.Content += "- KÜRZESTE PASSIVE SESSION:" + '\t' + getMinTimeSpan(this.emptyDurations) + '\n';
                this.label2.Content += "- MITTLERE PASSIVE SESSION:" + '\t' + getAvgTimeSpan(this.emptyDurations) + '\n';
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
            string tmp_file = "";
            string tmp_target = "";
            KeyValuePair<string, string> tmp_pair;

            for (int file = 0; file != this.events.Count; ++file)
            {
                for (int line = 0; line != this.events[file].Count; ++line)
                {
                    if (this.events[file][line] == "New Target")
                    {
                        tmp_file += this.targets[file][line] + '\t';

                        if (this.newTargets.ContainsKey(this.targets[file][line]))
                            this.newTargets[this.targets[file][line]] += 1;
                        else
                            this.newTargets.Add(this.targets[file][line], 1);

                        // TRANSITION
                        if (tmp_target == "")
                        {
                            tmp_target = this.targets[file][line];
                        }
                        else
                        { 
                            tmp_pair = new KeyValuePair<string, string>(tmp_target, this.targets[file][line]);
                            
                            if (this.transitionsOfTargets.ContainsKey(tmp_pair))
                                this.transitionsOfTargets[tmp_pair] += 1;
                            else
                                this.transitionsOfTargets.Add(tmp_pair, 1);

                            tmp_target = this.targets[file][line];
                        }
                    }
                    else
                        tmp_target = "";
                }

                this.newTargetsRaw.Add(tmp_file);
                tmp_file = "";
            }

            if (!allEmpty())
            {
                sortTransitions(ref this.transitionsOfTargets);
            }
        }

        private void detSelectedTargets() // TODO: this.selectedTargetsRaw
        {
            string tmp_file = "";
            string tmp_target = "";
            KeyValuePair<string, string> tmp_pair;

            for (int file = 0; file != this.events.Count; ++file)
            {
                for (int line = 0; line != this.events[file].Count; ++line)
                {
                    if (this.events[file][line] == "Select Target")
                    {
                        tmp_file += this.targets[file][line] + '\t';

                        if (this.selectedTargets.ContainsKey(this.targets[file][line]))
                            this.selectedTargets[this.targets[file][line]] += 1;
                        else
                            this.selectedTargets.Add(this.targets[file][line], 1);

                        // TRANSITION
                        if (tmp_target == "")
                        {
                            tmp_target = this.targets[file][line];
                        }
                        else
                        {
                            tmp_pair = new KeyValuePair<string, string>(tmp_target, this.targets[file][line]);

                            if (this.transitionsOfSelections.ContainsKey(tmp_pair))
                                this.transitionsOfSelections[tmp_pair] += 1;
                            else
                                this.transitionsOfSelections.Add(tmp_pair, 1);

                            tmp_target = this.targets[file][line];
                        }
                    }
                }

                this.selectedTargetsRaw.Add(tmp_file);
                tmp_file = "";                
            }

            if (!allEmpty())
            {
                //sortTransitions(ref this.transitionsOfSelections);
            }

            // OUTPUT ON GUI
            this.label2.Content += '\n' + "SELEKTIERTE EXPONATE" + '\n';

            if (allEmpty()) // ALL SESSIONS ARE PASSIVE
            {
                this.label2.Content += "KEINE AKTIVEN SESSIONS" + '\n';
            }
            else // AT LEAST ONE ACTIVE SESSION
            {
                foreach (KeyValuePair<string, int> target in this.selectedTargets)
                {
                    this.label2.Content += "- " + target.Key + ": " + target.Value + '\n';
                }
            }
        }


        private void sortTransitions(ref Dictionary<KeyValuePair<string, string>, int> transitions)
        {
            Dictionary<KeyValuePair<string, string>, int> tmp_transitions = new Dictionary<KeyValuePair<string, string>, int>();
            KeyValuePair<string, string> tmp_kvp;

            foreach (KeyValuePair<KeyValuePair<string, string>, int> transition in transitions)
            {
                tmp_kvp = new KeyValuePair<string, string>(transition.Key.Value, transition.Key.Key);

                if (tmp_transitions.ContainsKey(transition.Key))
                {
                    tmp_transitions[transition.Key] += transition.Value;
                }
                else if (tmp_transitions.ContainsKey(tmp_kvp))
                {
                    tmp_transitions[tmp_kvp] += transition.Value;
                }
                else
                {
                    tmp_transitions.Add(transition.Key, transition.Value);
                }
            }

            transitions = tmp_transitions;
        }

        #endregion

        #region WRITE FILES
        private string getCurrentFolder()
        {
            return this.filePaths[0].Remove(this.filePaths[0].LastIndexOf('\\') + 1);
        }

        private void writeActions()
        {
            if (this.checkFirstBlood.IsChecked == true && this.firstBloods != null)
                writeFirstBloods();

            if (this.checkActiveDurations.IsChecked == true && this.durations != null)
                writeActiveDurations();

            if (this.checkEmptyDurations.IsChecked == true && this.emptyDurations != null)
                writeEmptyDurations();

            if (this.checkMarkedTargets.IsChecked == true && this.newTargets != null)
                writeMarkedTargets();

            if (this.checkSelectedTargets.IsChecked == true && this.selectedTargets != null)
                writeSelectedTargets();
        }

        private void writeFirstBloods()
        {
            List<string> tmp_lines = new List<string>();
            string tmp_line = "";

            // WRITE FIRST BLOODS
            foreach (TimeSpan duration in this.firstBloods)
            {
                tmp_line = duration.ToString();
                tmp_lines.Add(tmp_line);
            }
            writeTxt("VORBEREITUNG", tmp_lines);
        }

        private void writeActiveDurations()
        {
            List<string> tmp_lines = new List<string>();
            string tmp_line = "";

            // WRITE ACTIVE DURATIONS
            foreach (TimeSpan duration in this.durations)
            {
                tmp_line = duration.ToString();
                tmp_lines.Add(tmp_line);
            }
            writeTxt("DAUER_AKTIV", tmp_lines);
        }

        private void writeEmptyDurations()
        {
            List<string> tmp_lines = new List<string>();
            string tmp_line = "";

            // WRITE PASSIVE DURATIONS
            foreach (TimeSpan duration in this.emptyDurations)
            {
                tmp_line = duration.ToString();
                tmp_lines.Add(tmp_line);
            }
            writeTxt("DAUER_PASSIV", tmp_lines);
        }

        private void writeMarkedTargets()
        {
            List<string> tmp_lines = new List<string>();
            string tmp_line = "";

            // WRITE MARKED TARGETS-STATS
            foreach (KeyValuePair<string, int> target in this.newTargets)
            {
                tmp_line = target.Key + '\t' + target.Value.ToString();
                tmp_lines.Add(tmp_line);
            }
            writeTxt("EXP_MARKIERT", tmp_lines);
            tmp_lines.Clear(); // Delete all lines after writing process

            // WRITE MARKED TARGETS-RAW
            foreach (string targets in this.newTargetsRaw)
            {
                tmp_lines.Add(targets);
            }
            writeTxt("EXP_MARK_ROH", tmp_lines);
            tmp_lines.Clear(); // Delete all lines after writing process

            // WRITE MARKED TARGETS-TRANSITIONS
            foreach (KeyValuePair<KeyValuePair<string, string>, int> transition in this.transitionsOfTargets)
            {
                tmp_line = transition.Key.Key + '\t' + transition.Key.Value + '\t' + transition.Value;
                tmp_lines.Add(tmp_line);
            }
            writeTxt("EXP_MARK_TRANS", tmp_lines);
        }

        private void writeSelectedTargets()
        {
            List<string> tmp_lines = new List<string>();
            string tmp_line = "";

            // WRITE SELECTED TARGETS-STATS
            foreach (KeyValuePair<string, int> target in this.selectedTargets)
            {
                tmp_line = target.Key + '\t' + target.Value.ToString();
                tmp_lines.Add(tmp_line);
            }
            writeTxt("EXP_SELEKTIERT", tmp_lines);
            tmp_lines.Clear();

            // WRITE SELECTED TARGETS-RAW
            foreach (string selections in this.selectedTargetsRaw)
            {
                tmp_line = selections;
                tmp_lines.Add(tmp_line);
            }
            writeTxt("EXP_SELEKT_ROH", tmp_lines);
            tmp_lines.Clear();

            // WRITE SELECTED TARGETS-TRANSITIONS
            foreach (KeyValuePair<KeyValuePair<string, string>, int> transition in this.transitionsOfSelections)
            {
                tmp_line = transition.Key.Key + '\t' + transition.Key.Value + '\t' + transition.Value;
                tmp_lines.Add(tmp_line);
            }
            writeTxt("EXP_SELEKT_TRANS", tmp_lines);
        }

        private void writeTxt(string name, List<string> lines)
        {
            // CREATE FILEPATH
            string path = getCurrentFolder();
            string filePath = path + DateTime.Now.ToString().Replace(':', '.') + "_" + name + ".txt";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
            {
                foreach (string line in lines)
                {
                    file.WriteLine(line);
                }
                file.Close();
            }
        }
        #endregion

        #region EVENTS
        // LOAD LOGDATA-FILES
        private void button1_Click(object sender, RoutedEventArgs e) 
        {
            loadFiles();
            this.label1.Content = this.filePaths.Count.ToString() + " DATEIEN GELADEN.";
        }

        // ANALYZE LOGDATA-FILES
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (filesLoaded()) // ONLY IF THERE ARE FILES TO ANALYZE
            {
                this.label1.Content = this.filePaths.Count.ToString() + " DATEIEN ANALYSIEREN...";
                makeAnalysis();
            }
            else
                MessageBox.Show("Keine Dateien ausgewählt. Bitte laden sie Dateien.");
        }

        // WRITE ANALYZED FILE(S)
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (filesLoaded())
            {
                this.label1.Content = this.filePaths.Count.ToString() + " DATEIEN SCHREIBEN...";
                writeActions();
            }
            else
                MessageBox.Show("Keine Analyse-Daten vorhanden. Bitte laden und/oder analysieren sie Dateien.");
        }
        #endregion
    }
}
