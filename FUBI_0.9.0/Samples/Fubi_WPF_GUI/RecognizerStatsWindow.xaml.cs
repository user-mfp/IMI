using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Linq;
using System.Reflection;
using System.Drawing;

using FubiNET;

namespace Fubi_WPF_GUI
{
    /// <summary>
    /// Interaktionslogik für RecognizerStatsWindow.xaml
    /// </summary>
    public partial class RecognizerStatsWindow : Window
    {
        private Thread updateThread;
        private bool running = false;
        private delegate void NoArgDelegate();

        public RecognizerStatsWindow()
        {
            InitializeComponent();

            updateThread = new Thread(update);
            running = true;
            updateThread.Start();
        }

        private void update()
        {
            while (running)
            {
                DispatcherOperation currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(this.updateStats), null);
                Thread.Sleep(150); // Don't update more often
                while (currentOP.Status != DispatcherOperationStatus.Completed
                    && currentOP.Status != DispatcherOperationStatus.Aborted)
                {
                    currentOP.Wait(TimeSpan.FromMilliseconds(50)); // If the update unexpectedly takes longer
                }
            }
        }

        public Dictionary<uint, Dictionary<string, double>> recognitions = new Dictionary<uint, Dictionary<string, double>>();
        public Dictionary<uint, Dictionary<string, double>> handRecognitions = new Dictionary<uint, Dictionary<string, double>>();

        public Dictionary<uint, Dictionary<string, string>> hints = new Dictionary<uint, Dictionary<string, string>>();
        public Dictionary<uint, Dictionary<string, string>> handHints = new Dictionary<uint, Dictionary<string, string>>();

        private void updateStats()
        {
            //Check all users
            ushort numUsers = Fubi.getNumUsers();
            ushort numHands = Fubi.getNumHands();

            if (numUsers == 0 && numHands == 0)
            {
                warnLabel.Visibility = System.Windows.Visibility.Visible;
            }
            else
                warnLabel.Visibility = System.Windows.Visibility.Hidden;

            
            for (uint i = 0; i < numUsers; i++)
            {
                // Not existent yet
                if (i >= statsTree.Items.Count)
                {
                    statsTree.Items.Add(new TvUser());
                    ((TvUser)statsTree.Items[(int)i]).IsExpanded = true;
                }
                // Wrong type, i.e. TvHand instead of TvUser
                if (statsTree.Items[(int)i].GetType() != typeof(TvUser))
                {
                    statsTree.Items[(int)i] = new TvUser();
                    ((TvUser)statsTree.Items[(int)i]).IsExpanded = true;
                }

                uint id = Fubi.getUserID(i);
                TvUser tUser = (TvUser)statsTree.Items[(int)i];
                tUser.id = id;

                if (id > 0)
                {
                    // Update user defined combinations
                    uint numRecs = Fubi.getNumUserDefinedCombinationRecognizers();
                    uint actualRecs = 0;
                    for (uint pc = 0; pc < numRecs; ++pc)
                    {
                        string name = Fubi.getUserDefinedCombinationRecognizerName(pc);
                        FubiUtils.RecognizerTarget target = Fubi.getCombinationRecognitizerTargetSensor(name);
                        if (target == FubiUtils.RecognizerTarget.ALL_SENSORS || target == FubiUtils.RecognizerTarget.BODY_SENSOR)
                        {
                            while (actualRecs >= tUser.Recs.Count)
                                tUser.Recs.Add(new TvRec());

                            TvRec rec = tUser.Recs[(int)actualRecs];
                            rec.id = pc;
                            rec.name = name;
                            uint numStates;
                            bool isInterrupted, isInTransition;
                            rec.currState = Fubi.getCurrentCombinationRecognitionState(name, id, out numStates, out isInterrupted, out isInTransition) + 1;
                            rec.numStates = numStates;
                            rec.isInterrupted = isInterrupted;
                            rec.isInTransition = isInTransition;
                            if (recognitions.ContainsKey(id) && recognitions[id].ContainsKey(name) && Fubi.getCurrentTime() - recognitions[id][name] < 2.0)
                                rec.bgColor = "Yellow";
                            else
                                rec.bgColor = Color.Transparent.Name;
                            if (hints.ContainsKey(id) && hints[id].ContainsKey(name))
                                rec.hint = hints[id][name];
                            actualRecs++;
                        }
                    }

                    while (tUser.Recs.Count > actualRecs)
                    {
                        tUser.Recs.RemoveAt(tUser.Recs.Count - 1);
                    }
                }
            }

            for (uint i = 0; i < numHands; i++)
            {
                uint index = i + numUsers;
                if (index >= statsTree.Items.Count)
                {
                    statsTree.Items.Add(new TvHand());
                    ((TvHand)statsTree.Items[(int)index]).IsExpanded = true;
                }
                // Wrong type, i.e. TvUser instead of TvUHand
                if (statsTree.Items[(int)index].GetType() != typeof(TvHand))
                {
                    statsTree.Items[(int)index] = new TvHand();
                    ((TvHand)statsTree.Items[(int)index]).IsExpanded = true;
                }

                uint id = Fubi.getHandID(i);
                TvHand tHand = (TvHand)statsTree.Items[(int)index];
                tHand.id = id;

                if (id > 0)
                {
                    // Update combinations
                    uint numRecs = Fubi.getNumUserDefinedCombinationRecognizers();
                    uint actualRecs = 0;
                    for (uint pc = 0; pc < numRecs; ++pc)
                    {
                        string name = Fubi.getUserDefinedCombinationRecognizerName(pc);
                        FubiUtils.RecognizerTarget target = Fubi.getCombinationRecognitizerTargetSensor(name);
                        if (target == FubiUtils.RecognizerTarget.ALL_SENSORS || target == FubiUtils.RecognizerTarget.FINGER_SENSOR)
                        {
                            while (actualRecs >= tHand.Recs.Count)
                                tHand.Recs.Add(new TvRec());

                            TvRec rec = tHand.Recs[(int)actualRecs];
                            rec.id = pc;
                            rec.name = name;
                            uint numStates;
                            bool isInterrupted, isInTransition;
                            rec.currState = Fubi.getCurrentCombinationRecognitionStateForHand(name, id, out numStates, out isInterrupted, out isInTransition) + 1;
                            rec.numStates = numStates;
                            rec.isInterrupted = isInterrupted;
                            rec.isInTransition = isInTransition;
                            if (handRecognitions.ContainsKey(id) && handRecognitions[id].ContainsKey(name) && Fubi.getCurrentTime() - handRecognitions[id][name] < 2.0)
                                rec.bgColor = "Yellow";
                            else
                                rec.bgColor = Color.Transparent.Name;
                            if (handHints.ContainsKey(id) && handHints[id].ContainsKey(name))
                                rec.hint = handHints[id][name];

                            actualRecs++;
                        }
                    }

                    while (tHand.Recs.Count > actualRecs)
                    {
                        tHand.Recs.RemoveAt(tHand.Recs.Count - 1);
                    }
                }
            }

            while (statsTree.Items.Count > numUsers+numHands)
            {
                statsTree.Items.RemoveAt(statsTree.Items.Count - 1);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            running = false;
            updateThread.Join(1000);
        }
    }

    // Data classes for the treeview items
    public class TvUser : TreeViewItemBase
    {
        public TvUser()
        {
        }

        private uint _id;
        public uint id
        {
            get 
            {
                return _id;
            } 
            set
            { 
                if (value != _id)
                {
                    _id = value;
                    float r, g, b;
                    Fubi.getColorForUserID(_id, out r, out g, out b);
                    var colorLookup = typeof(Color)
                       .GetProperties(BindingFlags.Public | BindingFlags.Static)
                       .Select(f => (Color)f.GetValue(null, null))
                       .Where(c => c.IsNamedColor)
                       .ToLookup(c => c.ToArgb());
                    Color col = Color.FromArgb((int)(r * 255.0f), (int)(g * 255.0f), (int)(b * 255.0f));
                    _color = colorLookup[col.ToArgb()].First().Name;
                    NotifyPropertyChanged("id");
                    NotifyPropertyChanged("color");
                } 
            }
        }

        private string _color;
        public string color
        {
            get
            {
                return _color;
            }
        }

        private readonly ObservableCollection<TvRec> _recs = new ObservableCollection<TvRec>();
        public ObservableCollection<TvRec> Recs { get { return _recs; } }
    }
    public class TvHand : TreeViewItemBase
    {
        public TvHand()
        {
        }

        private uint _id;
        public uint id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    float r, g, b;
                    Fubi.getColorForUserID(_id, out r, out g, out b);
                    var colorLookup = typeof(Color)
                       .GetProperties(BindingFlags.Public | BindingFlags.Static)
                       .Select(f => (Color)f.GetValue(null, null))
                       .Where(c => c.IsNamedColor)
                       .ToLookup(c => c.ToArgb());
                    Color col = Color.FromArgb((int)(r * 255.0f), (int)(g * 255.0f), (int)(b * 255.0f));
                    var namedC = colorLookup[col.ToArgb()];
                    if (namedC.Count() > 0)
                    {
                        _color = namedC.First().Name;
                    }
                    else
                        _color = Color.LightGreen.Name;
                    NotifyPropertyChanged("id");
                    NotifyPropertyChanged("color");
                }
            }
        }

        private string _color;
        public string color
        {
            get
            {
                return _color;
            }
        }

        private readonly ObservableCollection<TvRec> _recs = new ObservableCollection<TvRec>();
        public ObservableCollection<TvRec> Recs { get { return _recs; } }
    }
    public class TvRec : TreeViewItemBase
    {
        public TvRec()
        {
        }

        private uint _id;
        public uint id { get { return _id; } set { if (value != _id) { _id = value; NotifyPropertyChanged("id"); } } }
        private string _name;
        public string name { get { return _name; } set { if (value != _name) { _name = value; NotifyPropertyChanged("name"); } } }
        private int _currState;
        public int currState { get { return _currState; } set { if (value != _currState) { _currState = value; NotifyPropertyChanged("currState"); NotifyPropertyChanged("statColor"); NotifyPropertyChanged("progress"); } } }

        private string _bgColor;
        public string bgColor
        {
            set
            {
                if (value != _bgColor)
                {
                    _bgColor = value;
                    NotifyPropertyChanged("bgColor");
                }
            }
            get
            {
                return _bgColor;
            }
        }

        private string _hint;
        public string hint
        {
            set
            {
                if (value != _hint)
                {
                    _hint = value;
                    NotifyPropertyChanged("hint");
                }
            }
            get
            {
                return _hint;
            }
        }

        private uint _numStates;
        public uint numStates { get { return _numStates; } set { if (value != _numStates) { _numStates = value; NotifyPropertyChanged("numStates"); NotifyPropertyChanged("progress"); } } }

        private double _progress;
        public double progress
        {
            get
            {
                if (_currState > 0 && _numStates > 0)
                    _progress = (double)(_currState) / (double)_numStates;
                else
                    _progress = 0;
                return _progress;
            }
            set
            {
                _progress = value; // only a dummy setter as the progress bar needs it...
            }
        }

        private bool _isInterrupted, _isInTransition;
        public bool isInterrupted
        {
            set
            {
                if (value != _isInterrupted)
                {
                    _isInterrupted = value;
                    NotifyPropertyChanged("statusText");
                    NotifyPropertyChanged("statColor");
                }
            }
        }
        public bool isInTransition
        {
            set
            {
                if (value != _isInTransition)
                {
                    _isInTransition = value;
                    NotifyPropertyChanged("statusText");
                    NotifyPropertyChanged("statColor");
                }
            }
        }
        public string statusText
        {
            get
            {
                string text = "";
                if (_isInterrupted)
                    text += "Interrupted ";
                if (_isInTransition)
                    text += "InTransition ";
                return text;
            } 
        }

        public string statColor
        {
            get
            {
                if (_currState > 0)
                {
                    if (_isInterrupted && _isInTransition)
                        return "Purple";
                    if (_isInterrupted)
                        return "Orange";
                    if (_isInTransition)
                        return "Green";
                    return "Blue";
                }
                else
                    return "Red";
            }
        }
    }
    public class TreeViewItemBase : INotifyPropertyChanged
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }
        private bool isExpanded;
        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
