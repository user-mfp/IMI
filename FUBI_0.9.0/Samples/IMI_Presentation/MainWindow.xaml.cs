using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using FubiNET;
using IMI;
using Microsoft.Win32;
using System.Windows.Input;
using System.ComponentModel;
using System;

namespace IMI_Presentation
{
    public partial class MainWindow : Window
    {
        #region ENUMS AND CONSTANTS
        // The mode determines the layout (visibility, labeling and functions)
        private enum Mode
        {
            Start = 0,
            Standby,
            Navigation,
            Presentation
        };
        // Internal path to exhibition
        private string IMI_EXHIBITION_PATH = @"..\Samples\IMI_Presentation\data\IMI_ExhibitionPath.txt";
        #endregion

        #region DECLARATIONS
        // Exhibition
        private Exhibition IMI_EXHIBITION = new Exhibition();
        private Exhibit TMP_EXHIBIT = new Exhibit();
        private string TMP_PATH;
        // Layout
        private Mode mode;
        private string contentLabel1;
        private string contentLabel2;
        // Dialog
        private OpenFileDialog loadConfigDialog;
        // Handler
        private GeometryHandler geometryHandler;
        private FileHandler fileHandler;
        private SessionHandler sessionHandler;
        // Tracking
        private delegate void NoArgDelegate();
        private List<Point3D> jointsToTrack;
        private double timestamp; // DO NOT USE ! ! !
        private float confidence; // DO NOT USE ! ! !
        private List<uint> ids;
        private Dictionary<uint, Point3D> users = new Dictionary<uint,Point3D>(); 
        private uint USER_ID;
        // Tracking-thread
        private bool tracking;
        private Thread trackThread;
        // Session-thread
        private bool paused;
        private bool sessioning;
        private Thread sessionThread;
        #endregion

        #region INITIALIZATIONS
        public MainWindow()
        {
            InitializeComponent();

            // Initialize tracking
            initJoints();
            this.tracking = false; // Turn off tracking
            this.sessioning = false; // Turn off session

            // Initialize dialogs
            initDialogs();

            // Initialize handlers
            initHandlers();

            // Initialize layout
            initExhibition();
        }

        private void initJoints()
        {
            this.jointsToTrack = new List<Point3D>();
            this.jointsToTrack.Add(new Point3D()); // [0] := RIGHT_ELBOW
            this.jointsToTrack.Add(new Point3D()); // [1] := RIGHT_HAND
            this.jointsToTrack.Add(new Point3D()); // [2] := LEFT_ELBOW
            this.jointsToTrack.Add(new Point3D()); // [3] := LEFT_HAND
            this.jointsToTrack.Add(new Point3D()); // [4] := HEAD
        }

        private void initFubi()
        {
            FubiUtils.SensorType sensorType = FubiUtils.SensorType.OPENNI2;
            FubiUtils.StreamOptions sOpt1 = new FubiUtils.StreamOptions(640, 480, 30);
            FubiUtils.StreamOptions sOpt2 = new FubiUtils.StreamOptions(640, 480);
            FubiUtils.StreamOptions sOpt3 = new FubiUtils.StreamOptions(-1, -1, -1);
            FubiUtils.StreamOptions sOpt4 = new FubiUtils.StreamOptions(-1, -1, -1);
            FubiUtils.SkeletonProfile sProf = FubiUtils.SkeletonProfile.ALL;
            FubiUtils.FilterOptions fOpt = new FubiUtils.FilterOptions();
            FubiUtils.SensorOptions sOpts = new FubiUtils.SensorOptions(sOpt1, sOpt2, sOpt3, sensorType, sProf);

            if (!Fubi.init(sOpts, fOpt))
            {
                Fubi.init(new FubiUtils.SensorOptions(sOpt1, sOpt2, sOpt4, sensorType, sProf), fOpt);
            }
        }

        private void initHandlers()
        {
            this.geometryHandler = new GeometryHandler();
            this.fileHandler = new FileHandler();
        }

        private void initDialogs()
        {
            this.loadConfigDialog = new OpenFileDialog();
            this.loadConfigDialog.Filter = "Config-Files|*.xml";
            this.loadConfigDialog.Title = "Konfigurationsdatei laden";
            this.loadConfigDialog.FileOk += new System.ComponentModel.CancelEventHandler(loadConfigDialog_FileOk);
        }

        private void initExhibition()
        {
            string exhibitionPath = this.fileHandler.readTxt(this.IMI_EXHIBITION_PATH);

            if (exhibitionPath == "")
            {
                this.mode = Mode.Start;
                updateLayout();
            }
            else
            {
                this.IMI_EXHIBITION = this.fileHandler.loadExhibition(exhibitionPath);
                this.sessionHandler = new SessionHandler(Fubi.getClosestUserID(), this.IMI_EXHIBITION.getUserPosition(), 250.0);
                this.sessionHandler.initPlane(this.IMI_EXHIBITION.getExhibitionPlane());
                this.sessionHandler.makeLookupTable(this.IMI_EXHIBITION.getExhibits(), this.IMI_EXHIBITION.getExhibitionPlane());

                this.contentLabel1 = this.IMI_EXHIBITION.getName();
                this.mode = Mode.Standby;
                updateLayout();

                startTracking();
            }
        }
        #endregion

        #region RUNTIME
        private void track()
        {
            // Initializing tracking
            DispatcherOperation currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(initFubi), null);
            while (currentOP.Status != DispatcherOperationStatus.Completed && currentOP.Status != DispatcherOperationStatus.Aborted)
            {
                Thread.Sleep(100); // Wait for init to finish
            }

            // Tracking
            while (tracking)
            {
                Fubi.updateSensor();

                currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(updateFubi), null);
                //Thread.Sleep(29); // Time it should at least take to get new data
                while (currentOP.Status != DispatcherOperationStatus.Completed && currentOP.Status != DispatcherOperationStatus.Aborted)
                {
                    Thread.Sleep(2); // If the update unexpectedly takes longer
                }
            }

            // Handling tracking-data
            currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(releaseFubi), null);
            while (currentOP.Status != DispatcherOperationStatus.Completed && currentOP.Status != DispatcherOperationStatus.Aborted)
            {
                Thread.Sleep(100); // Wait for release to finish
            }
        }

        private List<uint> trackableUserIds()
        {
            ushort users = Fubi.getNumUsers();
            List<uint> ids = new List<uint>();

            for (uint id = 0; id != users; ++id)
            {
                if (trackableUser(id))
                {
                    ids.Add(id);
                }
            }

            return ids;
        }

        private bool trackableUser(uint id)
        {
            return (Fubi.isUserInScene(Fubi.getUserID(id)) && Fubi.isUserTracked(Fubi.getUserID(id)));
        }

        private void updateFubi()
        {
            this.ids = trackableUserIds(); // Get all trackable users

            if (this.ids.Count != 0) // There are visitors
            {
                if (!this.sessioning) // Start session
                {
                    startSession();
                }

                this.users.Clear(); // Remove all ids                
                foreach (uint id in this.ids) // For each trackable user
                {
                    this.users.Add(id, takeHipSample(id)); // Add user and user's position
                }
                this.USER_ID = this.sessionHandler.getCurrentUserID(this.users); // Update current user id

                if (this.USER_ID != 99) // There is a user in the interaction zone
                {
                    updateJoints();
                }
            }
            else // There are no (more) visitors
            {
                if (this.sessioning)
                {
                    stopSession();
                    this.contentLabel2 = "No Visitors: Start Stanby-Countdown";
                }
            }
            updateLabels();
        }

        private void updateSession()
        {
            while (this.sessioning)
            {
                if (this.USER_ID != 99 && !this.paused) // There is no user in the interaction zone
                {
                    Point3D pos = this.sessionHandler.getPosition(takeAimingSample());
                    int target = this.sessionHandler.getTarget(takeAimingSample());

                    if (target != 99) // There is a valid target
                    {
                        this.contentLabel2 = "ID:" + '\t' + this.USER_ID
                            + '\n' + "Target:" + '\t' + this.IMI_EXHIBITION.getExhibits()[target].getName();
                    }
                    else
                    {
                        this.contentLabel2 = "ID:" + '\t' + this.USER_ID
                            + '\n' + "Pos:" + '\t' + (int)pos.X + ";" + (int)pos.Y + ";" + (int)pos.Z;
                    }
                }
                else if (this.USER_ID == 99 && !this.paused)
                {
                    this.contentLabel2 = "Empty Zone: Waiting...";
                }
                else if ((this.USER_ID == 99 || this.USER_ID != 99) && this.paused)
                {
                    this.contentLabel2 = "Pausiert: No Sampling...";
                }
                else
                {
                    this.contentLabel2 = "ID: " + this.USER_ID + '\n' + "" + "Paused: " + this.paused;
                }
            }
        }

        private void updateJoints()
        {
            float x, y, z;

            // Right arm
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.USER_ID), FubiUtils.SkeletonJoint.RIGHT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_ELBOW, x, y, z);
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.USER_ID), FubiUtils.SkeletonJoint.RIGHT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_HAND, x, y, z);
            // Left arm
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.USER_ID), FubiUtils.SkeletonJoint.LEFT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_ELBOW, x, y, z);
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.USER_ID), FubiUtils.SkeletonJoint.LEFT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_HAND, x, y, z);
            // Nose (head)
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.USER_ID), FubiUtils.SkeletonJoint.FACE_NOSE, out x, out y, out z, out this.confidence, out this.timestamp); // this.USER_ID, FubiUtils.SkeletonJoint.HEAD, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.FACE_NOSE, x, y, z);
        }

        private void updateJoint(FubiUtils.SkeletonJoint joint, float x, float y, float z)
        {
            Point3D point = new Point3D(x, y, z);

            switch (joint)
            {
                case FubiUtils.SkeletonJoint.RIGHT_ELBOW:
                    this.jointsToTrack[0] = point;
                    break;
                case FubiUtils.SkeletonJoint.RIGHT_HAND:
                    this.jointsToTrack[1] = point;
                    break;
                case FubiUtils.SkeletonJoint.LEFT_ELBOW:
                    this.jointsToTrack[2] = point;
                    break;
                case FubiUtils.SkeletonJoint.LEFT_HAND:
                    this.jointsToTrack[3] = point;
                    break;
                case FubiUtils.SkeletonJoint.FACE_NOSE:
                    this.jointsToTrack[4] = point;
                    break;
                default:
                    break;
            }
        }
        
        private void releaseFubi()
        {
            Fubi.release();
        }
        #endregion

        #region SAMPLING
        private Point3D takeHipSample(uint id)
        {
            float lx, ly, lz;
            float rx, ry, rz;

            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(id), FubiUtils.SkeletonJoint.LEFT_HIP, out lx, out ly, out lz, out this.confidence, out this.timestamp);
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(id), FubiUtils.SkeletonJoint.RIGHT_HIP, out rx, out ry, out rz, out this.confidence, out this.timestamp);

            return this.geometryHandler.getCenter(new Point3D((double)lx, (double)ly, (double)lz), new Point3D((double)rx, (double)ry, (double)rz));
        }

        private List<GeometryHandler.Vector> takeSamples()
        {
            List<GeometryHandler.Vector> samples = new List<GeometryHandler.Vector>();
            
            samples.Add(takePointingSample());
            samples.Add(takeAimingSample());

            return samples;
        }

        private GeometryHandler.Vector takePointingSample()
        {
            GeometryHandler.Vector vector = new GeometryHandler.Vector();

            while (!this.geometryHandler.vectorOK(vector))
            {
                vector.reset(jointsToTrack[0], jointsToTrack[1]); // (RIGHT_ELBOW, RIGHT_HAND)
            }

            return vector;
        }

        private GeometryHandler.Vector takeAimingSample()
        {
            GeometryHandler.Vector vector = new GeometryHandler.Vector();

            while (!this.geometryHandler.vectorOK(vector))
            {
                vector.reset(jointsToTrack[4], jointsToTrack[1]); // (HEAD, RIGHT_HAND)
            }

            return vector;
        }
        #endregion

        #region THREADS
        private void startTracking()
        {
            // Starting the tracking-thread properly
            this.trackThread = new Thread(track);
            this.tracking = true;
            this.trackThread.Start();
        }

        private void startSession()
        {
            // Starting the session-thread properly
            this.sessionThread = new Thread(updateSession);
            this.sessioning = true;
            this.paused = false;
            this.sessionThread.Start();
        }

        private void stopTracking()
        {
            // Stopping the tracking-thread properly
            this.tracking = false;
            this.trackThread.Abort();
        }

        private void stopSession()
        {
            // Stopping the tracking-thread properly
            this.sessioning = false;
            this.paused = true;
            this.sessionThread.Abort();
        }

        private void pauseSession()
        {
            if (this.tracking && this.sessioning) // Session in progress
            {
                if (!this.paused)
                {
                    this.paused = true;
                }
                else
                {
                    this.paused = false;
                }
            }
        }

        private void pauseSession(int ms)
        {
            if (this.tracking && this.sessioning) // Session in progress
            {
                if (!this.paused)
                {
                    this.paused = true;
                    Thread.Sleep(ms);
                    this.paused = false;
                }
            }
        }

        private void closeAllThreads()
        {
            if (this.sessioning)
            {
                stopSession();
            }
            if (this.tracking)
            {
                stopTracking();
            }
            this.Close();
        }
        #endregion
        
        #region LAYOUT
        private void updateLayout()
        {
            switch (this.mode)
            { 
                case Mode.Start:
                    showStart();
                    break;
                case Mode.Standby:
                    showStandby();
                    break;
                case Mode.Navigation:
                    showNavigation();
                    break;
                case Mode.Presentation:
                    showPresentation();
                    break;
                default:
                    break;
            }
        }

        private void showStart()
        { 
            // Images
            this.image1.Visibility = Visibility.Hidden;
            this.image2.Visibility = Visibility.Hidden;

            // Labels
            this.label1.Visibility = Visibility.Hidden;
            this.label2.Visibility = Visibility.Hidden;

            // Button
            this.button1.Content = "Ausstelleung laden"; // Load Exhibition
            this.button1.Visibility = Visibility.Visible;
        }

        private void showStandby()
        {
            // Images
            this.image1.Visibility = Visibility.Hidden;
            this.image2.Visibility = Visibility.Hidden;

            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;
            this.textBlock2.Text = this.contentLabel2;
            this.label2.Visibility = Visibility.Visible;

            // Button
            this.button1.Visibility = Visibility.Hidden;
        
        }

        private void showNavigation()
        {
            // Images
            this.image1.Visibility = Visibility.Hidden;
            this.image2.Visibility = Visibility.Hidden;

            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;
            this.textBlock2.Text = this.contentLabel2;
            this.label2.Visibility = Visibility.Visible;

            // Button
            this.button1.Visibility = Visibility.Hidden;
        
        }

        private void showPresentation()
        {
            // Images
            this.image1.Visibility = Visibility.Hidden;
            this.image2.Visibility = Visibility.Hidden;

            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;
            this.textBlock2.Text = this.contentLabel2;
            this.label2.Visibility = Visibility.Visible;

            // Button
            this.button1.Visibility = Visibility.Hidden;
        
        }

        private void loadImage1(string path)
        { 
            
        }

        private void loadImage2(string path)
        { 
        
        }

        private void updateLabels()
        {
            this.textBlock1.Text = this.contentLabel1;
            this.textBlock2.Text = this.contentLabel2;
        }
        #endregion

        #region EVENTS
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch (this.mode)
            { 
                case Mode.Start:
                    if (!this.tracking)
                    {
                        if (this.loadConfigDialog.ShowDialog() == true)
                        {
                            this.IMI_EXHIBITION = this.fileHandler.loadExhibition(this.TMP_PATH);
                            this.fileHandler.writeTxt(this.IMI_EXHIBITION_PATH, this.TMP_PATH);
                            this.TMP_PATH = null;
                            this.sessionHandler = new SessionHandler(42, this.IMI_EXHIBITION.getUserPosition());

                            this.contentLabel1 = this.IMI_EXHIBITION.getName();
                            this.mode = Mode.Standby;
                            updateLayout();

                            startTracking();
                        }
                    }
                    else
                    {
                        closeAllThreads();
                    }
                    break;
                case Mode.Standby:
                    if (!this.tracking)
                    {
                        startTracking();
                    }
                    else
                    {
                        closeAllThreads();
                    }
                    break;
                case Mode.Navigation:
                    if (!this.tracking)
                    {
                        startTracking();
                    }
                    else
                    {
                        closeAllThreads();
                    }
                    break;
                case Mode.Presentation:
                    if (!this.tracking)
                    {
                        startTracking();
                    }
                    else
                    {
                        closeAllThreads();
                    }
                    break;
                default:
                    break;
            }            
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            { 
                case Key.Escape:
                    closeAllThreads();
                    break;
                case Key.Space:
                    pauseSession(3000);
                    break;
                default:
                    break;
            }
        }

        void loadConfigDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.TMP_PATH = this.loadConfigDialog.FileName;
        }
        #endregion
    }
}
