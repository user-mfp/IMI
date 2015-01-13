using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Windows.Threading;
using FubiNET;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Drawing;
using System;

namespace IMI_SummaeryDemo
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DECLARATIONS
        // JOINTS
        private Dictionary<FubiUtils.SkeletonJoint, Point3D> TRACKED_JOINTS;
        private FubiUtils.SkeletonJoint TRACKED_CENTER_JOINT;
        private Dictionary<FubiUtils.SkeletonJoint, Ellipse> SHOWN_JOINTS_1;
        private Dictionary<FubiUtils.SkeletonJoint, Ellipse> SHOWN_JOINTS_2;

        // Multi-user
        private List<uint> users;
        private List<List<Point3D>> usersJointsToTrack;
        private List<List<Ellipse>> userJointsToShow;
        // Layout
        private int centerJoint;
        private int relation;
        private int shapeSize = 25;
        // Threading
        private delegate void NoArgDelegate();
        private List<Point3D> jointsToTrack;
        private List<Ellipse> jointsToShow;
        private double timestamp; // DO NOT USE ! ! !
        private float confidence; // DO NOT USE ! ! !
        // Tracking-thread
        private bool tracking;
        private Thread trackThread;
        #endregion

        #region INITIALIZATION
        public MainWindow()
        {
            InitializeComponent();

            initFubi();
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

        private void initWindow()
        {
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
        }

        private void initJointsToTrack(FubiUtils.SkeletonJoint center_joint)
        {
            this.TRACKED_JOINTS = new Dictionary<FubiUtils.SkeletonJoint, Point3D>();

            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.HEAD, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.NECK, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.TORSO, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.WAIST, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_SHOULDER, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_ELBOW, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_WRIST, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_HAND, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_SHOULDER, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_ELBOW, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_WRIST, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_HAND, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_HIP, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_KNEE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_ANKLE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.LEFT_FOOT, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_HIP, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_KNEE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_ANKLE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.RIGHT_FOOT, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_NOSE, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_LEFT_EAR, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_RIGHT_EAR, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_FOREHEAD, new Point3D());
            this.TRACKED_JOINTS.Add(FubiUtils.SkeletonJoint.FACE_CHIN, new Point3D());

            this.TRACKED_CENTER_JOINT = center_joint;
        }

        private void initJointsToShowCanvas1()
        {
            this.SHOWN_JOINTS_1 = new Dictionary<FubiUtils.SkeletonJoint, Ellipse>();

            foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
            {
                Ellipse feedbackEllipse  = new Ellipse();
                feedbackEllipse.Width = this.shapeSize;
                feedbackEllipse.Height = this.shapeSize;
                feedbackEllipse.Fill = System.Windows.Media.Brushes.BurlyWood;

                Canvas.SetLeft(feedbackEllipse, (this.canvas1.Width / 2)); // Center of the canvas
                Canvas.SetTop(feedbackEllipse, (this.canvas1.Height / 2)); // Center of the canvas
                
                this.canvas1.Children.Add(feedbackEllipse);

                this.SHOWN_JOINTS_1.Add(joint.Key, feedbackEllipse);
            }
        }

        private void initJointsToShowCanvas2()
        {
            this.SHOWN_JOINTS_2 = new Dictionary<FubiUtils.SkeletonJoint, Ellipse>();

            foreach (KeyValuePair<FubiUtils.SkeletonJoint, Point3D> joint in this.TRACKED_JOINTS)
            {
                Ellipse feedbackEllipse = new Ellipse();
                feedbackEllipse.Width = this.shapeSize;
                feedbackEllipse.Height = this.shapeSize;
                feedbackEllipse.Fill = System.Windows.Media.Brushes.CornflowerBlue;

                Canvas.SetLeft(feedbackEllipse, (this.canvas2.Width / 2)); // Center of the canvas
                Canvas.SetTop(feedbackEllipse, (this.canvas2.Height / 2)); // Center of the canvas

                this.canvas2.Children.Add(feedbackEllipse);

                this.SHOWN_JOINTS_2.Add(joint.Key, feedbackEllipse);
            }
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

        private void closeAllThreads()
        {
            if (this.tracking)
            {
                stopTracking();
            }
            this.Close();
        }
        #endregion

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
                updateFeedback();
            }
        }

        private void updateJoints()
        {
            float x, y, z;
            // HEAD
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.HEAD, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.HEAD, x, y, z);
			// NECK
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.NECK, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.NECK, x, y, z);
			// TORSO
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.TORSO, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.TORSO, x, y, z);
			// WAIST
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.WAIST, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.WAIST, x, y, z);
			// LEFT_SHOULDER
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_SHOULDER, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.LEFT_SHOULDER, x, y, z);
			// LEFT_ELBOW
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.LEFT_ELBOW, x, y, z);
			// LEFT_WRIST
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_WRIST, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.LEFT_WRIST, x, y, z);
			// LEFT_HAND
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.LEFT_HAND, x, y, z);
			// RIGHT_SHOULDER
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_SHOULDER, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.RIGHT_SHOULDER, x, y, z);
			// RIGHT_ELBOW
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.RIGHT_ELBOW, x, y, z);
			// RIGHT_WRIST
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_WRIST, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.RIGHT_WRIST, x, y, z);
			// RIGHT_HAND
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.RIGHT_HAND, x, y, z);
			// LEFT_HIP
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_HIP, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.LEFT_HIP, x, y, z);
			// LEFT_KNEE
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_KNEE, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.LEFT_KNEE, x, y, z);
			// LEFT_ANKLE
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_ANKLE, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.LEFT_ANKLE, x, y, z);
			// LEFT_FOOT
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.LEFT_FOOT, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.LEFT_FOOT, x, y, z);
			// RIGHT_HIP
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_HIP, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.RIGHT_HIP, x, y, z);
			// RIGHT_KNEE
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_KNEE, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.RIGHT_KNEE, x, y, z);
			// RIGHT_ANKLE
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_ANKLE, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.RIGHT_ANKLE, x, y, z);
			// RIGHT_FOOT
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.RIGHT_FOOT, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.RIGHT_FOOT, x, y, z);
            // FACE_NOSE
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_NOSE, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.FACE_NOSE, x, y, z);
            // FACE_LEFT_EAR
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_LEFT_EAR, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.FACE_LEFT_EAR, x, y, z);
            // FACE_RIGHT_EAR
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_RIGHT_EAR, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.FACE_RIGHT_EAR, x, y, z);
            // FACE_FOREHEAD
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_FOREHEAD, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.FACE_FOREHEAD, x, y, z);
            // FACE_CHIN
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID() , FubiUtils.SkeletonJoint.FACE_CHIN, out x, out y, out z, out confidence, out timestamp);
            updateJoint((int)FubiUtils.SkeletonJoint.FACE_CHIN, x, y, z);
        }

        private void updateJoint(int joint, float x, float y, float z)
        {
            Point3D point = new Point3D(x, y, z);

            this.jointsToTrack[joint] = point;
        }

        private void updateFeedback()
        {
            int count = 0;
            foreach (Shape feedbackEllipse in this.jointsToShow)
            {
                Point3D canvasPosition = new Point3D();

                switch (this.relation)
                { 
                    default: // 0
                        canvasPosition = canvasPositionInRelationToZero(this.jointsToTrack[count]);
                        break;
                    case 1:
                        canvasPosition = canvasPositionInRelationToCenterJoint(this.jointsToTrack[count]);
                        break;

                }

                double feedbackEllipseSize = canvasSizeInRelationToCenterJoint(this.jointsToTrack[count]);
                feedbackEllipse.Width = feedbackEllipseSize;
                feedbackEllipse.Height = feedbackEllipseSize;

                Canvas.SetLeft(feedbackEllipse, canvasPosition.X - (feedbackEllipseSize / 2));
                Canvas.SetTop(feedbackEllipse, canvasPosition.Y - (feedbackEllipseSize / 2));

                ++count;
            }
        }

        private void releaseFubi()
        {
            Fubi.release();
        }
        #endregion

        #region LAYOUT
        private void toggleRelation()
        {
            switch (this.relation)
            { 
                default:
                    ++this.relation;
                    break;
                case 1:
                    this.relation = 0;
                    break;
            }
        }

        private Point3D canvasPositionInRelationToCenterJoint(Point3D jointPosition)
        {
            Point3D centerJoint = this.jointsToTrack[this.centerJoint];
            Point3D canvasPosition = new Point3D((this.canvas1.Width / 2), (this.canvas1.Height / 2), 0);

            if (jointPosition.X < centerJoint.X) // To the left
            {
                canvasPosition.X -= absoluteDifference(jointPosition.X, centerJoint.X);
            }
            else //(jointPosition.X > centerJoint.X || jointPosition.X == centerJoint.X) // To the right
            {
                canvasPosition.X += absoluteDifference(jointPosition.X, centerJoint.X);
            }

            if (jointPosition.Y < centerJoint.Y) // Below
            {
                canvasPosition.Y += absoluteDifference(jointPosition.Y, centerJoint.Y);
            }
            else //(jointPosition.Y > centerJoint.Y || jointPosition.Y == centerJoint.Y) // Above
            {
                canvasPosition.Y -= absoluteDifference(jointPosition.Y, centerJoint.Y);
            }

            return canvasPosition;
        }

        private Point3D canvasPositionInRelationToZero(Point3D jointPosition)
        {
            Point3D centerJoint = this.jointsToTrack[this.centerJoint];
            Point3D canvasPosition = new Point3D((this.canvas1.Width / 2), (this.canvas1.Height / 2), 0);

            if (jointPosition.X < 0) // To the left
            {
                canvasPosition.X -= absoluteDifference(jointPosition.X, 0);
            }
            else //(jointPosition.X > 0 || jointPosition.X == 0) // To the right
            {
                canvasPosition.X += absoluteDifference(jointPosition.X, 0);
            }

            if (jointPosition.Y < 0) // Below
            {
                canvasPosition.Y += absoluteDifference(jointPosition.Y, 0);
            }
            else //(jointPosition.Y > 0 || jointPosition.Y == 0) // Above
            {
                canvasPosition.Y -= absoluteDifference(jointPosition.Y, 0);
            }

            return canvasPosition;
        }

        private double canvasSizeInRelationToCenterJoint(Point3D jointPosition)
        {
            double centerDistance = this.jointsToTrack[this.centerJoint].Z;
            double canvasSize = this.shapeSize;

            if (jointPosition.Z != 0)
            {
                if (jointPosition.Z < centerDistance) // Closer
                {
                    canvasSize *= relationFactor(jointPosition.Z, centerDistance);
                }
                else //(jointPosition.X > centerDistance || jointPosition.X == centerDistance) // Further
                {
                    canvasSize *= relationFactor(jointPosition.Z, centerDistance);
                }
            }

            return canvasSize;
        }

        private double absoluteDifference(double lhs, double rhs)
        {
            double absDiff = Math.Abs(Math.Abs(lhs) - Math.Abs(rhs)) / 2.7;

            return absDiff;
        }

        private double relationFactor(double lhs, double rhs)
        {
            double relDiff = rhs / lhs;

            return relDiff;
        }
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
                case Key.R:
                    toggleRelation();
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
