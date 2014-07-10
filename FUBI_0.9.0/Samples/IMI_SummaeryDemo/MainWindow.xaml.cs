﻿using System.Windows;
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
        // Layout
        private int centerJoint;
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
            initCanvas();
            initJoints();

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

        private void initJoints()
        {
            this.jointsToTrack = new List<Point3D>();
            this.jointsToTrack.Add(new Point3D()); //  0 := HEAD
			this.jointsToTrack.Add(new Point3D()); //  1 := NECK
			this.jointsToTrack.Add(new Point3D()); //  2 := TORSO
            this.centerJoint = 2; // := TORSO IS CENTER OF THE SCREEN
			this.jointsToTrack.Add(new Point3D()); //  3 := WAIST
			this.jointsToTrack.Add(new Point3D()); //  4 := LEFT_SHOULDER
			this.jointsToTrack.Add(new Point3D()); //  5 := LEFT_ELBOW
			this.jointsToTrack.Add(new Point3D()); //  6 := LEFT_WRIST
			this.jointsToTrack.Add(new Point3D()); //  7 := LEFT_HAND
			this.jointsToTrack.Add(new Point3D()); //  8 := RIGHT_SHOULDER
			this.jointsToTrack.Add(new Point3D()); //  9 := RIGHT_ELBOW
			this.jointsToTrack.Add(new Point3D()); // 10 := RIGHT_WRIST
			this.jointsToTrack.Add(new Point3D()); // 11 := RIGHT_HAND
			this.jointsToTrack.Add(new Point3D()); // 12 := LEFT_HIP
			this.jointsToTrack.Add(new Point3D()); // 13 := LEFT_KNEE
			this.jointsToTrack.Add(new Point3D()); // 14 := LEFT_ANKLE
			this.jointsToTrack.Add(new Point3D()); // 15 := LEFT_FOOT
			this.jointsToTrack.Add(new Point3D()); // 16 := RIGHT_HIP
			this.jointsToTrack.Add(new Point3D()); // 17 := RIGHT_KNEE
			this.jointsToTrack.Add(new Point3D()); // 18 := RIGHT_ANKLE
			this.jointsToTrack.Add(new Point3D()); // 19 := RIGHT_FOOT
            this.jointsToTrack.Add(new Point3D()); // 20 := FACE_NOSE
            this.jointsToTrack.Add(new Point3D()); // 21 := FACE_LEFT_EAR
            this.jointsToTrack.Add(new Point3D()); // 22 := FACE_RIGHT_EAR
            this.jointsToTrack.Add(new Point3D()); // 23 := FACE_FOREHEAD
            this.jointsToTrack.Add(new Point3D()); // 24 := FACE_CHIN

            this.jointsToShow = new List<Ellipse>();
            foreach (Point3D point in this.jointsToTrack)
            {
                Ellipse feedbackEllipse  = new Ellipse();
                feedbackEllipse.Width = 50;
                feedbackEllipse.Height = 50;
                feedbackEllipse.Fill = System.Windows.Media.Brushes.Ivory;

                Canvas.SetLeft(feedbackEllipse, (this.canvas1.Width / 2)); // Center of the canvas
                Canvas.SetTop(feedbackEllipse, (this.canvas1.Height / 2)); // Center of the canvas
                
                this.canvas1.Children.Add(feedbackEllipse);

                this.jointsToShow.Add(feedbackEllipse);
            }
        }

        private void initCanvas()
        {
            this.canvas1.Width = (double)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            this.canvas1.Height = (double)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
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
                Point3D canvasPosition = canvasPositionInRelationToCenterJoint(this.jointsToTrack[count]);

                Canvas.SetLeft(feedbackEllipse, canvasPosition.X); // Center of the canvas
                Canvas.SetTop(feedbackEllipse, canvasPosition.Y); // Center of the canvas

                ++count;
            }
        }

        private void releaseFubi()
        {
            Fubi.release();
        }
        #endregion

        #region LAYOUT
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

        private double absoluteDifference(double lhs, double rhs)
        {
            double absDiff = Math.Abs(Math.Abs(lhs) - Math.Abs(rhs)) / 2.5;

            return absDiff;
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
                default:
                    break;
            }
        }
        #endregion
    }
}
