using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Drawing;
using System;

using FubiNET;
using IMI;

namespace IMI_SummaeryDemo
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DECLARATIONS
        // IMI-INSTANCES
        private GeometryHandler GEOMETRY_HANDLER = new GeometryHandler();

        // JOINTS
        private Dictionary<FubiUtils.SkeletonJoint, Point3D> TRACKED_JOINTS;
        private Dictionary<FubiUtils.SkeletonJoint, Ellipse> SHOWN_JOINTS_1;
        private Dictionary<FubiUtils.SkeletonJoint, Ellipse> SHOWN_JOINTS_2;
        private FubiUtils.SkeletonJoint TRACKED_CENTER_JOINT;

        // LAYOUT
        private Point3D ZERO_POINT = new Point3D();
        private int ELLIPSE_SIZE = 25;
        private double CANVAS_WIDTH;
        private double CANVAS_HEIGHT;
        private int CANVAS_VIEW_MODE_1; // default = 0; 0 := frontal, 1 := side, 2 := top  
        private int CANVAS_VIEW_MODE_2; // default = 0; 0 := frontal, 1 := side, 2 := top
        private bool VIEW;

        // PRESENTATION
        private string IMAGE_PATH;
        private int CURRENT_IMAGE;
        private List<System.Windows.Media.Imaging.BitmapImage> IMAGES;
        System.Windows.Media.Imaging.BitmapImage IMAGE;
        private List<string> NOTES;

        // THREADING
        private delegate void NoArgDelegate();
        private double timestamp; // DO NOT USE ! ! !
        private float confidence; // DO NOT USE ! ! !
        // Tracking-thread
        private bool tracking;
        private Thread trackThread;
        // Gesture-thread
        private bool gesturing;
        private Thread gestureThread;
        private double THRESHOLD = 222.0;
        #endregion

        #region INITIALIZATION
        public MainWindow()
        {
            InitializeComponent();
            
            initFubi();
            initImages();
            initWindow();
            initJointsToTrack(FubiUtils.SkeletonJoint.WAIST);
            initJointsToShowCanvas1();
            initJointsToShowCanvas2();

            startTracking();
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

        private void initImages()
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.ShowDialog();
            this.IMAGE_PATH = folderDialog.SelectedPath;

            this.IMAGES = new List<BitmapImage>();

            int tmp_image_counter = 0;
            string tmp_path = this.IMAGE_PATH;
            bool slides = true; // THERE ARE STILL SLIDES IN THIS PATH

            while (slides)
            {
                try
                {
                    tmp_path = this.IMAGE_PATH + '\\' + "Slide" + tmp_image_counter.ToString() + ".jpg";
                    this.IMAGES.Add(new BitmapImage(new Uri(tmp_path)));
                    // INCLUDE LOADING OF NOTES HERE
                    ++tmp_image_counter;
                }
                catch
                {
                    slides = false;
                }
            }
        }

        private void initWindow()
        {
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            this.CANVAS_WIDTH = (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 30) / 2;
            this.CANVAS_HEIGHT = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 20;
                        
            this.VIEW = true; // PRESENTATION
            this.canvas1.Visibility = Visibility.Hidden;
            this.canvas2.Visibility = Visibility.Hidden;

            this.CURRENT_IMAGE = -1;
            this.image1.Source = new BitmapImage(new Uri(this.IMAGE_PATH + '\\' + "black.jpg")); //this.IMAGES[this.CURRENT_IMAGE];
            this.image1.Visibility = Visibility.Visible;
        }

        private void initJointsToTrack(FubiUtils.SkeletonJoint center_joint)
        {
            this.TRACKED_JOINTS = new Dictionary<FubiUtils.SkeletonJoint, Point3D>();
            
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_CHIN, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_FOREHEAD, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_RIGHT_EAR, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_LEFT_EAR, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_NOSE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_FOOT, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_ANKLE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_KNEE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_HIP, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_FOOT, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_ANKLE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_KNEE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_HIP, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_HAND, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_WRIST, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_ELBOW, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_SHOULDER, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_HAND, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_WRIST, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_ELBOW, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_SHOULDER, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.WAIST, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.TORSO, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.NECK, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.HEAD, new Point3D());

            this.TRACKED_CENTER_JOINT = center_joint;
        }

        private void initJointsToShowCanvas1()
        {
            this.CANVAS_VIEW_MODE_1 = 0;

            this.SHOWN_JOINTS_1 = new Dictionary<FubiUtils.SkeletonJoint, Ellipse>();

            foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
            {
                Ellipse feedbackEllipse  = new Ellipse();
                feedbackEllipse.Width = this.ELLIPSE_SIZE;
                feedbackEllipse.Height = this.ELLIPSE_SIZE;
                feedbackEllipse.Fill = System.Windows.Media.Brushes.White;

                Canvas.SetLeft(feedbackEllipse, (this.CANVAS_WIDTH / 2)); // Center of the canvas
                Canvas.SetTop(feedbackEllipse, (this.CANVAS_HEIGHT / 2)); // Center of the canvas
                
                this.canvas1.Children.Add(feedbackEllipse);

                this.SHOWN_JOINTS_1.Add(joint.Key, feedbackEllipse);
            }

            this.SHOWN_JOINTS_1[FubiUtils.SkeletonJoint.HEAD].Fill = System.Windows.Media.Brushes.Red;
            this.SHOWN_JOINTS_1[FubiUtils.SkeletonJoint.RIGHT_ELBOW].Fill = System.Windows.Media.Brushes.Green;
            this.SHOWN_JOINTS_1[FubiUtils.SkeletonJoint.RIGHT_HAND].Fill = System.Windows.Media.Brushes.Blue;
        }

        private void initJointsToShowCanvas2()
        {
            this.CANVAS_VIEW_MODE_2 = 2;

            this.SHOWN_JOINTS_2 = new Dictionary<FubiUtils.SkeletonJoint, Ellipse>();

            foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
            {
                Ellipse feedbackEllipse = new Ellipse();
                feedbackEllipse.Width = this.ELLIPSE_SIZE;
                feedbackEllipse.Height = this.ELLIPSE_SIZE;
                feedbackEllipse.Fill = System.Windows.Media.Brushes.White;

                Canvas.SetLeft(feedbackEllipse, (this.CANVAS_WIDTH / 2)); // Center of the canvas
                Canvas.SetTop(feedbackEllipse, (this.CANVAS_HEIGHT / 2)); // Center of the canvas

                this.canvas2.Children.Add(feedbackEllipse);

                this.SHOWN_JOINTS_2.Add(joint.Key, feedbackEllipse);
            }

            this.SHOWN_JOINTS_2[FubiUtils.SkeletonJoint.HEAD].Fill = System.Windows.Media.Brushes.Red;
            this.SHOWN_JOINTS_2[FubiUtils.SkeletonJoint.RIGHT_ELBOW].Fill = System.Windows.Media.Brushes.Green;
            this.SHOWN_JOINTS_2[FubiUtils.SkeletonJoint.RIGHT_HAND].Fill = System.Windows.Media.Brushes.Blue;
        }
        #endregion

        #region TRACKING
        private void startTracking()
        {
            this.trackThread = new Thread(track);
            this.tracking = true;
            this.trackThread.Start();
        }

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

        private void toggleTracking()
        {
            if (this.tracking)
            {
                if (this.gesturing)
                {
                    stopGesturing();
                }
                stopTracking();
            }
            else
            {
                startTracking();
            }
        }

        private void stopTracking()
        {
            this.tracking = false;
            this.trackThread.Abort();
        }
        #endregion

        #region GESTURING
        private void startGesturing()
        {
            System.Media.SystemSounds.Asterisk.Play();
            this.gestureThread = new Thread(gesture);
            this.gesturing = true;
            //this.gestureThread.Start();
        }

        private void gesture()
        {
            while (this.gesturing)
            {
                if (this.VIEW) // PRESENTATION ACTIVE
                {
                    if (detectHandToShoulderRight())
                    {
                        nextImage();
                        Thread.Sleep(1000);
                    }
                    if (detectHandToShoulderLeft())
                    {
                        prevImage();
                        Thread.Sleep(1000);
                    }
                }
                else // JOINTS ACTIVE
                {
                    if (detectHandToShoulderRight())
                    {
                        toggleCanavasMode1();
                        Thread.Sleep(1000);
                    }
                    if (detectHandToShoulderLeft())
                    {
                        toggleCanavasMode2();
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private bool detectHandToShoulderRight()
        {
            double distance = this.GEOMETRY_HANDLER.getDistance(this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.RIGHT_HAND], this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.RIGHT_SHOULDER]);

            if (distance < this.THRESHOLD)
                return true;
            else
                return false;
        }

        private bool detectHandToShoulderLeft()
        {
            double distance = this.GEOMETRY_HANDLER.getDistance(this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.LEFT_HAND], this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.LEFT_SHOULDER]);

            if (distance < this.THRESHOLD)
                return true; 
            else
                return false;
        }

        private bool detectHandToNeckRight()
        {
            double distance = this.GEOMETRY_HANDLER.getDistance(this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.RIGHT_HAND], this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.NECK]);

            if (distance < this.THRESHOLD)
                return true;
            else
                return false;
        }

        private bool detectHandToNeckLeft()
        {
            double distance = this.GEOMETRY_HANDLER.getDistance(this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.LEFT_HAND], this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.NECK]);

            if (distance < this.THRESHOLD)
                return true;
            else
                return false;
        }
        
        private bool detectHandToHeadRight()
        {
            double distance = this.GEOMETRY_HANDLER.getDistance(this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.RIGHT_HAND], this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.HEAD]);

            if (distance < this.THRESHOLD)
                return true;
            else
                return false;
        }

        private bool detectHandToHeadLeft()
        {
            double distance = this.GEOMETRY_HANDLER.getDistance(this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.LEFT_HAND], this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.HEAD]);

            if (distance < this.THRESHOLD)
                return true;
            else
                return false;
        }
        
        private bool detectHandToEarRight()
        {
            double distance = this.GEOMETRY_HANDLER.getDistance(this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.RIGHT_HAND], this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.FACE_RIGHT_EAR]);

            if (distance < this.THRESHOLD)
                return true;
            else
                return false;
        }

        private bool detectHandToEarLeft()
        {
            double distance = this.GEOMETRY_HANDLER.getDistance(this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.LEFT_HAND], this.TRACKED_JOINTS[FubiUtils.SkeletonJoint.FACE_LEFT_EAR]);

            if (distance < this.THRESHOLD)
                return true;
            else
                return false;
        }

        private void toggleGesturing()
        {
            if (this.gesturing)
            {
                stopGesturing();
            }
            else
            {
                startGesturing();
            }
        }

        private void stopGesturing()
        {
            System.Media.SystemSounds.Hand.Play();
            this.gesturing = false;
            this.gestureThread.Abort();
        }
        #endregion

        private void closeAllThreads()
        {
            if (this.gesturing)
            {
                stopGesturing();
            }
            if (this.tracking)
            {
                stopTracking();
            }
            this.Close();
        }

        #region RUNTIME
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
            if (Fubi.getNumUsers() != 0)
            {
                updateJoints();

                if (!this.VIEW) // JOINTS
                {
                    updateCanvas1();
                    updateCanvas2();
                }
                else // PRESENTATION
                {
                    updateImage();
                }

                if (!this.gesturing && trackableUser(Fubi.getUserID(Fubi.getClosestUserID())))
                {
                    startGesturing();
                }
            }
        }

        private void updateJoints()
        {
            float x, y, z;

            // HEAD
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.HEAD, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.HEAD, x, y, z);
			// NECK
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.NECK, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.NECK, x, y, z);
			// TORSO
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.TORSO, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.TORSO, x, y, z);
			// WAIST
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.WAIST, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.WAIST, x, y, z);
			// LEFT_SHOULDER
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_SHOULDER, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_SHOULDER, x, y, z);
			// LEFT_ELBOW
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_ELBOW, x, y, z);
			// LEFT_WRIST
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_WRIST, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.LEFT_WRIST, x, y, z);
			// LEFT_HAND
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_HAND, x, y, z);
			// RIGHT_SHOULDER
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_SHOULDER, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_SHOULDER, x, y, z);
			// RIGHT_ELBOW
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_ELBOW, x, y, z);
			// RIGHT_WRIST
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_WRIST, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.RIGHT_WRIST, x, y, z);
			// RIGHT_HAND
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_HAND, x, y, z);
			// LEFT_HIP
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_HIP, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_HIP, x, y, z);
			// LEFT_KNEE
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_KNEE, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.LEFT_KNEE, x, y, z);
			// LEFT_ANKLE
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_ANKLE, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.LEFT_ANKLE, x, y, z);
			// LEFT_FOOT
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_FOOT, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.LEFT_FOOT, x, y, z);
			// RIGHT_HIP
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_HIP, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_HIP, x, y, z);
			// RIGHT_KNEE
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_KNEE, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.RIGHT_KNEE, x, y, z);
			// RIGHT_ANKLE
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_ANKLE, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.RIGHT_ANKLE, x, y, z);
			// RIGHT_FOOT
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_FOOT, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.RIGHT_FOOT, x, y, z);
            // FACE_NOSE
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_NOSE, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.FACE_NOSE, x, y, z);
            // FACE_LEFT_EAR
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_LEFT_EAR, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.FACE_LEFT_EAR, x, y, z);
            // FACE_RIGHT_EAR
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_RIGHT_EAR, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.FACE_RIGHT_EAR, x, y, z);
            // FACE_FOREHEAD
            //Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_FOREHEAD, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.FACE_FOREHEAD, x, y, z);
            // FACE_CHIN
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_CHIN, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.FACE_CHIN, x, y, z);
        }

        private void updateJoint(FubiUtils.SkeletonJoint joint, float x, float y, float z)
        {
            Point3D point = new Point3D(x, y, z);

            this.TRACKED_JOINTS[joint] = point;
        }

        private void releaseFubi()
        {
            Fubi.release();
        }
        #endregion

        #region LAYOUT
        private void toggleView()
        {
            if (!this.VIEW)
            {
                this.VIEW = true; // PRESENTATION
                this.canvas1.Visibility = Visibility.Hidden;
                this.canvas2.Visibility = Visibility.Hidden;
                this.image1.Visibility = Visibility.Visible;
            }
            else
            {
                this.VIEW = false; // JOINTS
                this.canvas1.Visibility = Visibility.Visible;
                this.canvas2.Visibility = Visibility.Visible;
                this.image1.Visibility = Visibility.Hidden;
            }
        }

        private void toggleCanavasMode1()
        {
            switch (this.CANVAS_VIEW_MODE_1)
            { 
                default:
                    this.CANVAS_VIEW_MODE_1 = 0;
                    break;
                case 0:
                    this.CANVAS_VIEW_MODE_1 = 1;
                    break;
                case 1:
                    this.CANVAS_VIEW_MODE_1 = 2;
                    break;
                case 2:
                    this.CANVAS_VIEW_MODE_1 = 0;
                    break;
            }
        }

        private void toggleCanavasMode2()
        {
            switch (this.CANVAS_VIEW_MODE_2)
            {
                default:
                    this.CANVAS_VIEW_MODE_2 = 0;
                    break;
                case 0:
                    this.CANVAS_VIEW_MODE_2 = 1;
                    break;
                case 1:
                    this.CANVAS_VIEW_MODE_2 = 2;
                    break;
                case 2:
                    this.CANVAS_VIEW_MODE_2 = 0;
                    break;
            }
        }

        private void updateCanvas1()
        {
            switch (this.CANVAS_VIEW_MODE_1)
            {
                default: //0 := frontal
                    foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
                    {
                        if (joint.Value != this.ZERO_POINT)
                        {
                            Point3D canvasPosition = canvasPositionInRelationToCenterJointFront(joint.Value);
                            Ellipse canvasEllipse = this.SHOWN_JOINTS_1[joint.Key];

                            Canvas.SetLeft(canvasEllipse, canvasPosition.X - (canvasEllipse.Width / 2));
                            Canvas.SetTop(canvasEllipse, canvasPosition.Y - (canvasEllipse.Height / 2));

                            this.SHOWN_JOINTS_1[joint.Key].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.SHOWN_JOINTS_1[joint.Key].Visibility = Visibility.Hidden;
                        }
                    }
                    break;
                case 1: //1 := side
                    foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
                    {
                        if (joint.Value != this.ZERO_POINT)
                        {
                            Point3D canvasPosition = canvasPositionInRelationToCenterJointSide(joint.Value);
                            Ellipse canvasEllipse = this.SHOWN_JOINTS_1[joint.Key];

                            Canvas.SetLeft(canvasEllipse, canvasPosition.X - (canvasEllipse.Width / 2));
                            Canvas.SetTop(canvasEllipse, canvasPosition.Y - (canvasEllipse.Height / 2));

                            this.SHOWN_JOINTS_1[joint.Key].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.SHOWN_JOINTS_1[joint.Key].Visibility = Visibility.Hidden;
                        }
                    }
                    break;
                case 2: //2 := top
                    foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
                    {
                        if (joint.Value != this.ZERO_POINT)
                        {
                            Point3D canvasPosition = canvasPositionInRelationToCenterJointTop(joint.Value);
                            Ellipse canvasEllipse = this.SHOWN_JOINTS_1[joint.Key];

                            Canvas.SetLeft(canvasEllipse, canvasPosition.X - (canvasEllipse.Width / 2));
                            Canvas.SetTop(canvasEllipse, canvasPosition.Y - (canvasEllipse.Height / 2));

                            this.SHOWN_JOINTS_1[joint.Key].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.SHOWN_JOINTS_1[joint.Key].Visibility = Visibility.Hidden;
                        }
                    }
                    break;
            }
        }

        private void updateCanvas2()
        {
            switch (this.CANVAS_VIEW_MODE_2)
            {
                default: //0 := frontal
                    foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
                    {
                        if (joint.Value != this.ZERO_POINT)
                        {
                            Point3D canvasPosition = canvasPositionInRelationToCenterJointFront(joint.Value);
                            Ellipse canvasEllipse = this.SHOWN_JOINTS_2[joint.Key];

                            Canvas.SetLeft(canvasEllipse, canvasPosition.X - (canvasEllipse.Width / 2));
                            Canvas.SetTop(canvasEllipse, canvasPosition.Y - (canvasEllipse.Height / 2));

                            this.SHOWN_JOINTS_2[joint.Key].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.SHOWN_JOINTS_2[joint.Key].Visibility = Visibility.Hidden;
                        }
                    }
                    break;
                case 1: //1 := side
                    foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
                    {
                        if (joint.Value != this.ZERO_POINT)
                        {
                            Point3D canvasPosition = canvasPositionInRelationToCenterJointSide(joint.Value);
                            Ellipse canvasEllipse = this.SHOWN_JOINTS_2[joint.Key];

                            Canvas.SetLeft(canvasEllipse, canvasPosition.X - (canvasEllipse.Width / 2));
                            Canvas.SetTop(canvasEllipse, canvasPosition.Y - (canvasEllipse.Height / 2));

                            this.SHOWN_JOINTS_2[joint.Key].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.SHOWN_JOINTS_2[joint.Key].Visibility = Visibility.Hidden;
                        }
                    }
                    break;
                case 2: //2 := top
                    foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
                    {
                        if (joint.Value != this.ZERO_POINT)
                        {
                            Point3D canvasPosition = canvasPositionInRelationToCenterJointTop(joint.Value);
                            Ellipse canvasEllipse = this.SHOWN_JOINTS_2[joint.Key];

                            Canvas.SetLeft(canvasEllipse, canvasPosition.X - (canvasEllipse.Width / 2));
                            Canvas.SetTop(canvasEllipse, canvasPosition.Y - (canvasEllipse.Height / 2));

                            this.SHOWN_JOINTS_2[joint.Key].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.SHOWN_JOINTS_2[joint.Key].Visibility = Visibility.Hidden;
                        }
                    }
                    break;
            }
        }

        private Point3D canvasPositionInRelationToCenterJointFront(Point3D jointPosition)
        {
            Point3D centerJoint = this.TRACKED_JOINTS[this.TRACKED_CENTER_JOINT];
            Point3D canvasPosition = new Point3D((this.CANVAS_WIDTH / 2), (this.CANVAS_HEIGHT / 2), 0);
            Vector3D jointVector= (jointPosition - centerJoint) / 3;

            canvasPosition.X += jointVector.X;
            canvasPosition.Y -= jointVector.Y;

            return canvasPosition;
        }

        private Point3D canvasPositionInRelationToCenterJointSide(Point3D jointPosition)
        {
            Point3D centerJoint = this.TRACKED_JOINTS[this.TRACKED_CENTER_JOINT];
            Point3D canvasPosition = new Point3D((this.CANVAS_WIDTH / 2), (this.CANVAS_HEIGHT / 2), 0);
            Vector3D jointVector = (jointPosition - centerJoint) / 3;

            canvasPosition.X += jointVector.Z;
            canvasPosition.Y -= jointVector.Y;

            //canvasPosition.X = canvasPosition.Z;

            return canvasPosition;
        }

        private Point3D canvasPositionInRelationToCenterJointTop(Point3D jointPosition)
        {
            Point3D centerJoint = this.TRACKED_JOINTS[this.TRACKED_CENTER_JOINT];
            Point3D canvasPosition = new Point3D((this.CANVAS_WIDTH / 2), (this.CANVAS_HEIGHT / 2), 0);
            Vector3D jointVector = (jointPosition - centerJoint) / 3;

            canvasPosition.X += jointVector.X;
            canvasPosition.Y -= jointVector.Z;

            return canvasPosition;
        }

        private void updateImage()
        {
            this.image1.Source = this.IMAGE;
        }
        #endregion

        #region INPUTS
        private void nextImage()
        {
            if (this.VIEW) // PRESENTATION ACTIVE
            {
                if (this.CURRENT_IMAGE != (this.IMAGES.Count - 1)) // FINAL IMAGE NOT REACHED, YET
                {
                    ++this.CURRENT_IMAGE;
                }

                this.IMAGE = this.IMAGES[this.CURRENT_IMAGE];
            }
        }

        private void prevImage()
        {
            if (this.VIEW) // PRESENTATION ACTIVE
            {
                if (this.CURRENT_IMAGE != 0) // FINAL IMAGE NOT REACHED, YET
                {
                    --this.CURRENT_IMAGE;
                }

                this.IMAGE = this.IMAGES[this.CURRENT_IMAGE];
            }
        }

        /*private void nextImage()
        { 
            
        }

        private void prevImage()
        { 
            
        }*/
        #endregion

        #region EVENTS
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    closeAllThreads();
                    break;
                case Key.Space:
                    toggleTracking();
                    break;
                case Key.V:
                    toggleView();
                    break;
                case Key.G:
                    toggleGesturing();
                    break;
                case Key.NumPad1:
                    toggleCanavasMode1();
                    break;
                case Key.NumPad2:
                    toggleCanavasMode2();
                    break;
                case Key.Right:
                    nextImage();
                    break;
                case Key.Left:
                    prevImage();
                    break;
                default:
                    break;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            nextImage();
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            prevImage();
        }
        #endregion
    }
}
