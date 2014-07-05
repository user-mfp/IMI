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
using System.Collections;
using System.Windows.Media;
using System.Windows.Shapes;

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
        private string IMI_EXHIBITION_PATH = @"C:\IMI-DATA\Daten\IMI_ExhibitionPath.txt";
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
        private BitmapImage contentImage2;
        // Feedback
        private List<Ellipse> exhibitFeedbackPositions;
        private Ellipse feedbackEllipse;
        private Point3D feedbackPosition;
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
        private uint IMI_ID;
        private int TMP_TARGET = 99;
        private int IMI_TARGET = 99;
        // Tracking-thread
        private bool tracking;
        private Thread trackThread;
        // Session-thread
        private bool paused = false;
        private bool sessioning;
        private Thread sessionThread;
        // Timed threads
        private bool selecting;
        private Thread selectionThread;
        private bool presenting;
        private Thread presentationThread;
        private bool waiting;
        private Thread endWaitThread;
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
            initFeedbackPositions();
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

        private void initFeedbackPositions()
        {
            this.exhibitFeedbackPositions = new List<Ellipse>();
            double planeCanvasRatio = this.sessionHandler.getPlaneCanvasRatio();
            Point3D canvasSize = this.sessionHandler.getCanvasSize();

            // Define canvas size and set planeScreenRatio
            this.canvas1.Width = canvasSize.X;
            this.canvas1.Height = canvasSize.Y;

            // Define Shapes for each exhibit
            foreach (Exhibit exhibit in this.IMI_EXHIBITION.getExhibits())
            {
                Ellipse position = new Ellipse();
                    
                position.Width = exhibit.getKernelSize() * planeCanvasRatio;
                position.Height = exhibit.getKernelSize() * planeCanvasRatio;                
                position.Fill = Brushes.CornflowerBlue;
                position.Opacity = 0.5;

                this.canvas1.Children.Add(position);                
                this.exhibitFeedbackPositions.Add(position);
            }

            // Define screen position for each exhibit according to its position on exhibition plane
            int count = 0;
            foreach (Shape joint in this.exhibitFeedbackPositions)
            {
                Point3D position = this.sessionHandler.getCanvasPosition(this.IMI_EXHIBITION.getExhibit(count).getPosition());

                // Offset, since origin (0;0) is in top left corner of shape
                position.X -= joint.Width / 2; // Move shape half its width to the left 
                position.Y -= joint.Height / 2; // Move shape half its height up

                Canvas.SetLeft(joint, position.X);
                Canvas.SetTop(joint, position.Y);
                ++count;
            }

            // Define feedback shape
            this.feedbackEllipse = new Ellipse();
            this.feedbackEllipse.Width = 50;
            this.feedbackEllipse.Height = 50;
            this.feedbackEllipse.Fill = Brushes.Coral;
            // Define feedback position
            this.feedbackPosition = new Point3D();
            Canvas.SetLeft(this.feedbackEllipse, (this.canvas1.Width / 2)); // Center of the canvas
            Canvas.SetTop(this.feedbackEllipse, (this.canvas1.Height / 2)); // Center of the canvas
            this.canvas1.Children.Add(this.feedbackEllipse);
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

            if (exhibitionPath == "") // There is no exhibition
            {
                this.mode = Mode.Start;
                updateLayout();
            }
            else // There is an exhibition
            {
                this.IMI_EXHIBITION = this.fileHandler.loadExhibition(exhibitionPath);
                this.sessionHandler = new SessionHandler(Fubi.getClosestUserID(), this.IMI_EXHIBITION.getUserPosition(), 250.0, this.IMI_EXHIBITION.getExhibitionPlane(), new Point3D(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, 0));
                this.sessionHandler.makeLookupTable(this.IMI_EXHIBITION.getExhibits(), this.IMI_EXHIBITION.getExhibitionPlane());
                
                loadBackground(this.IMI_EXHIBITION.getBackgroundImage().Value);
                this.contentLabel1 = "Standby - " + this.IMI_EXHIBITION.getName();
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
                if (!this.sessioning) // No session in progress
                {
                    startSession(); // Start session
                    this.contentLabel1 = "Navigation - " + this.IMI_EXHIBITION.getName();
                    this.mode = Mode.Navigation;
                    this.TMP_EXHIBIT = null;
                }

                this.users.Clear(); // Remove all ids                
                foreach (uint id in this.ids) // For each trackable user
                {
                    this.users.Add(id, takeHipSample(id)); // Add user and user's position
                }
                this.IMI_ID = this.sessionHandler.getCurrentUserID(this.users); // Update current user id

                if (this.IMI_ID != 99) // There is a user in the interaction zone
                {
                    updateJoints();
                    updateFeedbackPosition();
                }
                else // (this.IMI_ID == 99) // There is no user in the interaction zone
                { 
                    // Keep going
                }
            }
            else // (this.ids.Count == 0) // There are no visitors
            {
                if (this.sessioning)
                {
                    stopSession(); // Start a session
                    this.contentLabel1 = "Standby - " + this.IMI_EXHIBITION.getName();
                    this.mode = Mode.Standby;
                }
            }
            //this.contentLabel2 = "User: " + this.IMI_ID + '\n' + "Exp.: " + this.IMI_TARGET + '\n' + "track: " + this.tracking + '\n' + "sess: " + this.sessioning + '\n' + "paus: " + this.paused + '\n';
            updateLayout();
        }

        private void updateSession()
        {
            while (this.sessioning)
            {
                if (!this.paused) // Session in progress
                {
                    if (this.IMI_ID != 99) // User in interaction zone
                    {
                        this.feedbackPosition = this.sessionHandler.getPosition(takeAimingSample());
                        this.TMP_TARGET = this.sessionHandler.getTarget(takeAimingSample());
                        updateTarget();
                    }
                    else // (this.IMI_ID == 99) // No user in interaction zone 
                    {
                        //this.contentLabel2 = "Mode." + this.mode.ToString() + ": No updateTarget()." + '\n' + "IMI_ID: " + this.IMI_ID; //"Mode.Navigation: No updateTarget()"
                    }
                }
                else // (this.paused) // Session paused
                {
                    if (this.IMI_ID != 99) // User in interaction zone
                    {
                        //this.contentLabel2 = "Paused: Not sampling, but tracking and user in zone.";
                    }
                    else // (this.IMI_ID == 99) // No user in interaction zone
                    {
                        //this.contentLabel2 = "Paused: Not sampling, but tracking and no user in zone.";
                    }
                }
            }
        }

        private void updateJoints()
        {
            float x, y, z;

            // Right arm
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.IMI_ID), FubiUtils.SkeletonJoint.RIGHT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_ELBOW, x, y, z);
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.IMI_ID), FubiUtils.SkeletonJoint.RIGHT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_HAND, x, y, z);
            // Left arm
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.IMI_ID), FubiUtils.SkeletonJoint.LEFT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_ELBOW, x, y, z);
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.IMI_ID), FubiUtils.SkeletonJoint.LEFT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_HAND, x, y, z);
            // Nose (head)
            Fubi.getCurrentSkeletonJointPosition(Fubi.getUserID(this.IMI_ID), FubiUtils.SkeletonJoint.FACE_NOSE, out x, out y, out z, out this.confidence, out this.timestamp); // this.IMI_ID, FubiUtils.SkeletonJoint.HEAD, out x, out y, out z, out confidence, out timestamp);
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

        private void updateTarget()
        {
            if (this.IMI_TARGET != this.TMP_TARGET && this.TMP_TARGET != 99) // New target assigned AND it is valid
            {
                this.IMI_TARGET = this.TMP_TARGET; // Assign new, valid target
                if (this.selecting) // Timer already running or no valid target (exhibit) selected := deselect
                {
                    stopSelectionTimer();
                }
                startSelectionTimer();
            }
        }

        private void updateFeedbackPosition()
        {
            Point3D screenPosition = this.sessionHandler.getCanvasPosition(this.feedbackPosition);
            screenPosition.X -= (this.feedbackEllipse.Width / 2);
            screenPosition.Y -= (this.feedbackEllipse.Height / 2);
            if (screenPosition.X > 0 && screenPosition.Y > 0)
            {
                Canvas.SetLeft(this.feedbackEllipse, screenPosition.X);
                Canvas.SetTop(this.feedbackEllipse, screenPosition.Y);
            }
            else
            {
                Canvas.SetLeft(this.feedbackEllipse, (this.canvas1.Width / 2));
                Canvas.SetTop(this.feedbackEllipse, (this.canvas1.Height / 2));
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
            this.sessionThread = new Thread(updateSession);
            this.sessioning = true;
            this.sessionThread.Start();
        }

        private void startSelectionTimer()
        {
            this.selectionThread = new Thread(selection);
            this.selecting = true;
            this.selectionThread.Start();
        }

        private void startPresentation()
        {
            this.presentationThread = new Thread(presentation);
            this.presenting = true;
            this.presentationThread.Start();
        }

        private void stopTracking()
        {
            this.tracking = false;
            this.trackThread.Abort();
        }

        private void stopSession()
        {
            this.sessioning = false;
            this.sessionThread.Abort();
        }

        private void stopSelectionTimer()
        {
            this.selecting = false;
            this.selectionThread.Abort();
        }

        private void stopPresentation()
        {
            // Reload navigtion's properties
            this.contentLabel1 = "Navigation - " + this.IMI_EXHIBITION.getName();
            this.contentLabel2 = "";
            this.mode = Mode.Navigation;
            this.TMP_EXHIBIT = null;

            this.presenting = false;
            this.presentationThread.Abort();
        }

        private void pauseSession()
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

        private void pauseSession(int ms)
        {
            if (!this.paused) // Session in progress
            {
                this.paused = true;
                Thread.Sleep(ms);
                this.paused = false;
            }
            else
            {
                this.paused = false;
            }
        }

        private void closeAllThreads()
        {
            if (this.tracking)
            {
                stopTracking();
            }
            if (this.sessioning)
            {
                stopSession();
            }
            if (this.presenting)
            {
                stopPresentation();
            }
            this.Close();
        }
        #endregion

        #region TIMERS
        private void selection()
        {
            Thread.Sleep(this.IMI_EXHIBITION.getSelectionTime()); // Wait for confirmation time to elapse
            if (this.presenting) // PResentation allready running
            {
                stopPresentation(); // Abort running presentation
            }

            this.TMP_EXHIBIT = this.IMI_EXHIBITION.getExhibit(this.IMI_TARGET); // Set current exhibit

            this.contentLabel1 = this.TMP_EXHIBIT.getName(); // Set the current exhibit's name as headline
            this.contentLabel2 = this.TMP_EXHIBIT.getDescription(); // Set the current exhibit's description
            this.mode = Mode.Presentation; // Go to presentation mode

            startPresentation();
            pauseSession(this.IMI_EXHIBITION.getLockTime());
            stopSelectionTimer(); // Selection done := close this thread
        }

        private void presentation()
        {
            foreach (KeyValuePair<string, BitmapImage> image in this.TMP_EXHIBIT.getImages())
            {
                this.contentLabel1 = this.TMP_EXHIBIT.getName();
                this.contentLabel2 = this.TMP_EXHIBIT.getDescription();
                this.contentImage2 = image.Value; // Load next image
                Thread.Sleep(this.IMI_EXHIBITION.getSlideTime()); // Wait for the slideTime
            }
            Thread.Sleep(this.IMI_EXHIBITION.getEndWait()); // Wait a little more
            
            stopPresentation();
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
            // Canvas
            this.canvas1.Visibility = Visibility.Visible;

            // Images
            this.Background = Brushes.BlanchedAlmond;
            this.image2.Visibility = Visibility.Hidden;

            // Labels
            this.label1.Visibility = Visibility.Hidden;
            this.label2.Visibility = Visibility.Hidden;

            // Button
            this.button1.Content = "Ausstelleung laden"; // "Load exhibition"
            this.button1.Visibility = Visibility.Visible;
        }

        private void showStandby()
        {
            // Canvas
            this.canvas1.Visibility = Visibility.Hidden;

            // Images
            loadBackground(this.IMI_EXHIBITION.getBackgroundImage().Value);
            this.image2.Visibility = Visibility.Hidden;

            // Labels
            this.label1.Visibility = Visibility.Hidden;
            this.label2.Visibility = Visibility.Hidden;

            // Button
            this.button1.Visibility = Visibility.Hidden;        
        }

        private void showNavigation()
        {
            // Canvas
            this.canvas1.Visibility = Visibility.Visible;

            // Images
            loadBackground(this.IMI_EXHIBITION.getOverview().Value);
            this.image2.Visibility = Visibility.Hidden;

            // Labels
            this.label1.Visibility = Visibility.Hidden;
            this.label2.Visibility = Visibility.Hidden;

            // Button
            this.button1.Visibility = Visibility.Hidden;        
        }

        private void showPresentation()
        {
            // Canvas
            this.canvas1.Visibility = Visibility.Hidden;

            // Images
            this.Background = Brushes.BlanchedAlmond;
            this.image2.Source = this.contentImage2;
            this.image2.Visibility = Visibility.Visible;

            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;
            this.textBlock2.Text = this.contentLabel2;
            this.label2.Visibility = Visibility.Visible;

            // Button
            this.button1.Visibility = Visibility.Hidden;        
        }

        private void loadBackground(BitmapImage image)
        {
            this.Background = new ImageBrush(image);
        }

        private void loadImage2(BitmapImage image)
        {
            this.image2.Source = image;
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
                            this.sessionHandler = new SessionHandler(99, this.IMI_EXHIBITION.getUserPosition(), 250.0, this.IMI_EXHIBITION.getExhibitionPlane(), new Point3D(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, 0));
                
                            this.contentLabel1 = "Standby - " + this.IMI_EXHIBITION.getName();
                            this.mode = Mode.Standby;
                            updateLayout();

                            startTracking();
                        }
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
                    pauseSession();
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
