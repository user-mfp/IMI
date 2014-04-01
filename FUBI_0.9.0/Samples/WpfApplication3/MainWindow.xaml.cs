using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using FubiNET;
using WpfApplication3.lib;

namespace WpfApplication3
{
    public partial class MainWindow : Window
    {
        #region DECLARATIONS
        /// <summary>
        /// DECLARATIONS
        /// 
        /// All neccessary Members are introduced.
        /// </summary>
        
        //--- TRACKING ---//
        // Declare and define type of sensor
        // Here: ASUS Xtion Pro (PrimeSense) with OpenNI 2.2 and NiTE 2.2 x86 installed
        private FubiUtils.SensorType sensorType = FubiUtils.SensorType.OPENNI2;
        // Declare tracked joints as list of points
        private List<Point3D> jointsToTrack = new List<Point3D>();
        //private List<Point3D> jointsToBuffer = new List<Point3D>(10);
        private double timestamp;
        private float confidence;

        //--- THREADING ---//
        // Declare tracking thread
        private Thread trackThread;
        private bool tracking = false;
        private delegate void NoArgDelegate();
        // Declare calibration thread
        private Thread calibrationThread;
        private bool calibrating = false;

        //--- CALIBRATING ---//
        private GeometryHandler geometryHandler = new GeometryHandler();
        private List<GeometryHandler.Vector> calibrationVectors = new List<GeometryHandler.Vector>();
        private List<GeometryHandler.Vector> mismatchVectors = new List<GeometryHandler.Vector>();
        private List<Point3D> calibrationPoints = new List<Point3D>();
        private List<Point3D> mismatchPoints = new List<Point3D>();
        private int calibrationSamples = 3; // Number of samples
        private int calibrationSampleVectors = 10; // Vectors per sample 
        private int calibrationSampleBreak = 4000; // Time between samples in ms
        private double xMax = 1100;
        private double xMin = -1300;
        private double yMax = -1000;
        private double yMin = -1200;
        private double zMax = 2200;
        private double zMin = -500;
        private List<Point3D> CHECK = new List<Point3D>(); 

        //--- EXHIBITION ---//
        //private Exhibition exhibition = new Exhibition();
        private ExhibitionPlane exhibitionPlane = new ExhibitionPlane();

        //--- DEBUGGING ---//
        private bool mode = false; // Debug = false, Life = true
        private DataLogger dataLogger;
        private int round = 0;
        private string vp;
        private Statistics statistic = new Statistics();
        //--- OUTPUTS ---//
        private string debug1 = "Debug...";
        private string debug2 = "Debug...";
        private string debug3 = "Debug...";
        private string debug4 = "Debug...";
        //--- DRAWING ---//
        private List<Shape> shapesToDraw = new List<Shape>();
        private int shapeSize = 25;
        #endregion

        #region INITIALIZATIONS
        /// <summary>
        /// INITIALIZATIONS
        /// 
        /// Hardware is started.
        /// </summary>

        public MainWindow()
        {
            InitializeComponent();

            initJoints();
            initShapes();

            // Starting the tracking thread
            this.trackThread = new Thread(track);
            this.tracking = true;
            this.trackThread.Start();
        }

        private void initJoints()
        {
            this.jointsToTrack.Add(new Point3D()); // RIGHT_ELBOW
            this.jointsToTrack.Add(new Point3D()); // RIGHT_HAND
            this.jointsToTrack.Add(new Point3D()); // LEFT_ELBOW
            this.jointsToTrack.Add(new Point3D()); // LEFT_HAND
            this.jointsToTrack.Add(new Point3D()); // HEAD
        }

        private void initShapes()
        {
            this.shapesToDraw.Add(new Rectangle()); // Right elbow
            this.shapesToDraw.Add(new Ellipse()); // Right hand
            this.shapesToDraw.Add(new Rectangle()); // Left elbow
            this.shapesToDraw.Add(new Ellipse()); // Left hand
            this.shapesToDraw.Add(new Ellipse()); // Head
            
            int count = 0;
            foreach (Shape joint in shapesToDraw)
            {
                joint.Width = shapeSize;
                joint.Height = shapeSize;
                if (count < 2)
                    joint.Fill = Brushes.Red;
                else if (count == 4)
                    joint.Fill = Brushes.Green;
                else
                    joint.Fill = Brushes.Blue;
                canvas1.Children.Add(joint);
                ++count;
            }
        }

        private void initLogger()
        {
            this.dataLogger = new DataLogger(@"D:\Master\TestFolder\2014-3-4_defPlane\", this.vp, this.round);
            // Headline
            this.dataLogger.newPargraph("corners:" + '\t' + 1 + '\t' + "samples:" + '\t' + this.calibrationSamples.ToString() + '\t' + "vectors:" + '\t' + calibrationSampleVectors);
        }

        public void initFubi()
        {
            FubiUtils.StreamOptions sOpt1 = new FubiUtils.StreamOptions(640, 480, 30);
            FubiUtils.StreamOptions sOpt2 = new FubiUtils.StreamOptions(640, 480);
            FubiUtils.StreamOptions sOpt3 = new FubiUtils.StreamOptions(-1,-1, -1);
            FubiUtils.StreamOptions sOpt4 = new FubiUtils.StreamOptions(-1, -1, -1);
            FubiUtils.SkeletonProfile sProf = FubiUtils.SkeletonProfile.ALL;            
            FubiUtils.FilterOptions fOpt = new FubiUtils.FilterOptions();            
            FubiUtils.SensorOptions sOpts = new FubiUtils.SensorOptions(sOpt1, sOpt2, sOpt3, sensorType, sProf);

            if (!Fubi.init(sOpts, fOpt))
            {
                Fubi.init(new FubiUtils.SensorOptions(sOpt1, sOpt2, sOpt4, sensorType, sProf), fOpt);
            }            
        }
        #endregion

        #region RUNTIME
        /// <summary>
        /// RUNTIME
        /// 
        /// Update the FUBI-Framework.
        /// </summary>
        
        private void track()
        {
            // Initializing tracking
            DispatcherOperation currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(this.initFubi), null);
            while (currentOP.Status != DispatcherOperationStatus.Completed
                    && currentOP.Status != DispatcherOperationStatus.Aborted)
            {
                Thread.Sleep(100); // Wait for init to finish
            }

            // Tracking
            while (tracking)
            {
                Fubi.updateSensor();

                currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(this.updateFubi), null);
                //Thread.Sleep(29); // Time it should at least take to get new data
                while (currentOP.Status != DispatcherOperationStatus.Completed
                    && currentOP.Status != DispatcherOperationStatus.Aborted)
                {
                    Thread.Sleep(2); // If the update unexpectedly takes longer
                }
            }

            // Handling tracking-data
            currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(this.releaseFubi), null);
            while (currentOP.Status != DispatcherOperationStatus.Completed
                    && currentOP.Status != DispatcherOperationStatus.Aborted)
            {
                Thread.Sleep(100); // Wait for release to finish
            }
        }
        #endregion

        #region UPDATING
        /// <summary>
        /// UPDATING
        /// 
        /// Joints are tracked and UI are redrawn. 
        /// </summary>
        
        private void updateFubi()
        {
            if (Fubi.getClosestUserID() != 0)
            {
                updateShapes(Fubi.getClosestUserID());
                this.canvas1.UpdateLayout();
            }
            this.label1.Content = this.debug1;
            this.label2.Content = this.debug2;
            this.label4.Content = this.debug4;
        }
        
        private void updateShapes(uint id) 
        {
            float x, y, z;
            // Right armoutn
            Fubi.getCurrentSkeletonJointPosition(id, FubiUtils.SkeletonJoint.HEAD, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.HEAD, x, y, z);
            //Fubi.getCurrentSkeletonJointPosition(id, FubiUtils.SkeletonJoint.RIGHT_SHOULDER, out x, out y, out z, out confidence, out timestamp);
            //updateJoint(FubiUtils.SkeletonJoint.RIGHT_SHOULDER, x, y, z);
            Fubi.getCurrentSkeletonJointPosition(id, FubiUtils.SkeletonJoint.RIGHT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_ELBOW, x, y, z);
            Fubi.getCurrentSkeletonJointPosition(id, FubiUtils.SkeletonJoint.RIGHT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_HAND, x, y, z);
            // Left arm
            Fubi.getCurrentSkeletonJointPosition(id, FubiUtils.SkeletonJoint.LEFT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_ELBOW, x, y, z);
            Fubi.getCurrentSkeletonJointPosition(id, FubiUtils.SkeletonJoint.LEFT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_HAND, x, y, z);
            setShapes();
        }

        private void updateJoint(FubiUtils.SkeletonJoint joint, float x, float y, float z)
        {
            int pointIndex = -1;
            Point3D point = new Point3D();
            switch (joint)
            {
                case FubiUtils.SkeletonJoint.RIGHT_ELBOW:
                    pointIndex = 0;
                    break;
                case FubiUtils.SkeletonJoint.RIGHT_HAND:
                    pointIndex = 1;
                    break;
                case FubiUtils.SkeletonJoint.LEFT_ELBOW:
                    pointIndex = 2;
                    break;
                case FubiUtils.SkeletonJoint.LEFT_HAND:
                    pointIndex = 3;
                    break;
                case FubiUtils.SkeletonJoint.HEAD:
                    pointIndex = 4;
                    break;
                default:
                    break;
            }

            if (pointIndex != -1)
            {
                point = this.jointsToTrack[pointIndex];
                point.X = x;
                point.Y = y;
                point.Z = z;
                this.jointsToTrack[pointIndex] = point;
                //jointsToBuff.Add(point);
            }
            /*
            int e = (int)this.jointsToTrack[0].X;
            int h = (int)this.jointsToTrack[1].X;
            int d = e - h;

            this.debug4 = "e: " + e;
            this.debug4 += '\n' + "h: " + h;
            this.debug4 += '\n' + "d: " + d;*/
        }

        private void updateDirection(GeometryHandler.Vector v)
        { 
            //Point3D e = this.jointsToTrack[0]; // Right elbow
            //Point3D h = this.jointsToTrack[1]; // Right hand
            //GeometryHandler.Vector v = new GeometryHandler.Vector(jointsToTrack[0], jointsToTrack[1]); // (RIGHT_ELBOW, RIGHT_HAND)
            
            if (v.Direction.X < 0)
                this.debug4 = "LEFT";
            else
                this.debug4 = "RIGHT";

            if (v.Direction.Y < 0)
                this.debug4 += '\n' + "DOWN";
            else
                this.debug4 += '\n' + "UP";

            if (v.Direction.Z < 0)
                this.debug4 += '\n' + "FRONT";
            else
                this.debug4 += '\n' + "BACK";
        }

        private void releaseFubi()
        {
            Fubi.release();
        }
        #endregion

        #region DRAWING
        /// <summary>
        /// DRAWING
        /// 
        /// Display user's tracked joints.
        /// </summary>
        
        private void setShapes()
        { 
            double normFak = 3000 / this.canvas1.Width; // Only Width, for canvas1 being a square
            /*for(int joint = 0; joint != 2; ++joint) // Only right arm
            {
                Canvas.SetTop(this.shapesToDraw[joint], getCanvasCoord(normFak, this.jointsToTrack[joint].Y));
                Canvas.SetLeft(this.shapesToDraw[joint], getCanvasCoord(normFak, this.jointsToTrack[joint].X));
            }*/

            int count = 0;
            foreach (Shape joint in this.shapesToDraw)
            {
                Canvas.SetTop(this.shapesToDraw[count], getCanvasCoord(normFak, this.jointsToTrack[count].Y));
                Canvas.SetLeft(this.shapesToDraw[count], getCanvasCoord(normFak, this.jointsToTrack[count].X));
                ++count;
            }
        }
        
        private int getCanvasCoord(double norm, double val)
        {
            return (int)((canvas1.Width / 2) - (val / norm));
        }
        #endregion

        #region CALIBRATION >>>KAPSELN! X-REFERENCE<<<
        /// <summary>
        /// CALIBRATION
        /// 
        /// The Exhibition's members' geometry are defined here.
        /// Later on, this should become a class (ExhibitionBuilder) on its own. 
        /// </summary>

        private void defineExhibition()
        {
            defineExhibitionPlane();
        }

        private void defineExhibit()
        {
            this.calibrationVectors.Clear();

            // DO SOMETHING USEFUL HERE
            // - Define Plane
            // - Define Exhibits

            this.calibrationThread.Abort();
            this.calibrating = false;        
        }

        Calibrator calibrator = new Calibrator(); // Initiate calibrator
        
        private void defineExhibitionPlane()
        {
            this.calibrationSampleBreak = 0;
            List<Point3D> corners1 = this.calibrator.definePlane(sampleVectors(3, 3, 10, 0)); // Calibration-points
            //List<Point3D> corners2 = this.calibrator.definePlane(sampleVectors(3, 3, 10, 0)); // Validation-points

            //if (this.calibrator.validatePlane(corners1, corners2))
            //{ 
                // DO STUFF
            //}
            this.debug4 = corners1.Count.ToString();
        }

        private List<GeometryHandler.Vector> sampleVectors(int points, int positions, int samples, int returnMode) // Amounts of points to define, positions to point from, samples per position and return mode: 0 = only pointing-samples, 1 = only aiming-samples, 2 = both samples
        {
            List<GeometryHandler.Vector> allVectors = new List<GeometryHandler.Vector>();
            List<GeometryHandler.Vector> pointingVectors = new List<GeometryHandler.Vector>();
            List<GeometryHandler.Vector> aimingVectors = new List<GeometryHandler.Vector>();
            
            for (int point = 0; point != points; ++point) // For each corner
            {
                for (int position = 0; position != positions; ++position) // For each position
                {
                    //--- INSTRUCTIONS ---//
                    // Position to go to
                    this.debug4 = "Position " + (position + 1); // Change to position #
                    System.Media.SystemSounds.Asterisk.Play(); // Notice user of changed instruction
                    countDown(this.calibrationSampleBreak); // Wait for 3s
                    // Corner to point at
                    this.debug4 = "Ecke " + (point + 1); // Point to corner #
                    System.Media.SystemSounds.Asterisk.Play(); // Notice user of changed instruction
                    countDown(this.calibrationSampleBreak / 2); // Wait for 3s

                    for (int sample = 0; sample != samples; ++sample)
                    {
                        //this.debug4 = sample.ToString();
                        allVectors = takeSample(position); // Take poining- and aiming sample simultaniously
                        pointingVectors.Add(allVectors[0]); // Add pointing-vector to pointing-vectors
                        aimingVectors.Add(allVectors[1]); // Add aiming-vector to aiming-vectors
                        Thread.Sleep(1000 / samples); // Sampling at [samples] per second
                    }
                }
            }
            allVectors.Clear(); // Clear allVectors-list

            switch (returnMode)
            { 
                case 0:
                    return pointingVectors;
                case 1:
                    return aimingVectors;
                case 2:
                    foreach (GeometryHandler.Vector pV in pointingVectors) // Add all pointing-vectors
                    {
                        allVectors.Add(pV);
                    }
                    foreach (GeometryHandler.Vector aV in aimingVectors) // Add all aiming-vectors
                    {
                        allVectors.Add(aV);
                    }
                    return allVectors;
                default:
                    return allVectors;
            }
        }
        
        /* OLD STUFF
        private void definePlane()
        {
            List<Point3D> corners = new List<Point3D>();
            
            // Initialize data logger
            initLogger();
            // Pointing-data's index = 1
            this.dataLogger.newPargraph("Corner" + '\t' + "Sample" + '\t' + "Vector" + '\t' + "V.S.X" + '\t' + "V.S.Y" + '\t' + "V.S.Z" + '\t' + "V.E.X" + '\t' + "V.E.Y" + '\t' + "V.E.Z" + '\t' + "V.D.X" + '\t' + "V.D.Y" + '\t' + "V.D.Z");
            // Pointing intersections calculations' index = 2
            this.dataLogger.newPargraph("A.S.X" + '\t' + "A.S.Y" + '\t' + "A.S.Z" + '\t' + "A.E.X" + '\t' + "A.E.Y" + '\t' + "A.E.Z" + '\t' + "A.D.X" + '\t' + "A.D.Y" + '\t' + "A.D.Z" + '\t' + "B.S.X" + '\t' + "B.S.Y" + '\t' + "B.S.Z" + '\t' + "B.E.X" + '\t' + "B.E.Y" + '\t' + "B.E.Z" + '\t' + "B.D.X" + '\t' + "B.D.Y" + '\t' + "B.D.Z" + '\t' + "I.X" + '\t' + "I.Y" + '\t' + "I.Z");
            // Looking-data's index = 3
            this.dataLogger.newPargraph("Corner" + '\t' + "Sample" + '\t' + "Vector" + '\t' + "V.S.X" + '\t' + "V.S.Y" + '\t' + "V.S.Z" + '\t' + "V.E.X" + '\t' + "V.E.Y" + '\t' + "V.E.Z" + '\t' + "V.D.X" + '\t' + "V.D.Y" + '\t' + "V.D.Z");
            // Looking intersections calculations' index = 4
            this.dataLogger.newPargraph("A.S.X" + '\t' + "A.S.Y" + '\t' + "A.S.Z" + '\t' + "A.E.X" + '\t' + "A.E.Y" + '\t' + "A.E.Z" + '\t' + "A.D.X" + '\t' + "A.D.Y" + '\t' + "A.D.Z" + '\t' + "B.S.X" + '\t' + "B.S.Y" + '\t' + "B.S.Z" + '\t' + "B.E.X" + '\t' + "B.E.Y" + '\t' + "B.E.Z" + '\t' + "B.D.X" + '\t' + "B.D.Y" + '\t' + "B.D.Z" + '\t' + "I.X" + '\t' + "I.Y" + '\t' + "I.Z");
            
            // For each of the THREE(:= corner != 3) corners defining the exhibition-plane
            for (int corner = 0; corner != 1; ++corner)
            {
                // Clear sampled vectors' list before new corner
                this.calibrationVectors.Clear();          

                // Set plane's corners
                corners.Add(definePointInSpace(corner));
            }

            this.debug4 = ((int)corners[0].X).ToString() + " ; " + ((int)corners[0].Y).ToString() + " ; " + ((int)corners[0].Z).ToString() + '\n';
            //this.debug4 += ((int)corners[1].X).ToString() + " ; " + ((int)corners[1].Y).ToString() + " ; " + ((int)corners[1].Z).ToString() + '\n';
            //this.debug4 += ((int)corners[2].X).ToString() + " ; " + ((int)corners[2].Y).ToString() + " ; " + ((int)corners[2].Z).ToString() + '\n';
            //this.debug4 += ((int)corners[3].X).ToString() + " ; " + ((int)corners[3].Y).ToString() + " ; " + ((int)corners[3].Z).ToString() + '\n';

            this.calibrating = false;
            this.calibrationThread.Abort();
        }

        private Point3D definePointInSpace(int corner)
        {
            GeometryHandler.Vector zero = new GeometryHandler.Vector();
            GeometryHandler.Vector tmpVec = new GeometryHandler.Vector();
            List<Point3D> intersectionsP = new List<Point3D>();
            List<Point3D> intersectionsL = new List<Point3D>();
            List<GeometryHandler.Vector> sampleVectors = new List<GeometryHandler.Vector>();
            List<GeometryHandler.Vector> watchVectors = new List<GeometryHandler.Vector>();

            // For each sample position
            for (int sample = 0; sample != this.calibrationSamples; ++sample)
            {
                //--- INSTRUCTIONS ---//
                // Change to position #
                this.debug4 = "Position " + (sample + 1);
                // Notice user of changed instruction
                System.Media.SystemSounds.Asterisk.Play();
                // Wait 3s
                countDown(this.calibrationSampleBreak);

                // Point to corner #
                this.debug4 = "Ecke " + (corner + 1);
                // Notice user of changed instruction
                System.Media.SystemSounds.Asterisk.Play();
                // Wait 3s
                countDown(this.calibrationSampleBreak / 2);

                // Clear list of previous samples' vectors
                sampleVectors.Clear();
                watchVectors.Clear();
                List<GeometryHandler.Vector> samples;
                this.debug4 = "Sampling...";
                // For each sample vector
                for (int vector = 0; vector != this.calibrationSampleVectors; ++vector)
                {
                    samples = takeSample(sample);
                    updateDirection(samples[0]);
                    // Only add valid Vectors
                    if (samples[0] != zero)
                        sampleVectors.Add(samples[0]);
                    if (samples[1] != zero)
                        watchVectors.Add(samples[1]);
                    // Wait .1s
                    Thread.Sleep(100);
                }

                // Only add average pointing- and looking vector as sample- and mismatch vector, which are not already in list
                if (this.calibrationVectors.Count != 0)
                {
                    tmpVec = this.geometryHandler.getAvgVector(sampleVectors);
                    if ((!this.mode) || tmpVec != this.calibrationVectors[this.calibrationVectors.Count - 1] || (tmpVec.Direction.X != 0 && tmpVec.Direction.Z != 0)) // Vector already in list OR flawly sampled
                    {
                        this.calibrationVectors.Add(tmpVec);
                        this.mismatchVectors.Add(this.geometryHandler.getAvgVector(watchVectors));
                    }
                    else
                    {
                        System.Media.SystemSounds.Question.Play();
                        debug4 = "Sample nicht gut!";
                        Thread.Sleep(1500);
                        --sample;
                    }
                }
                else
                { 
                    this.calibrationVectors.Add(this.geometryHandler.getAvgVector(sampleVectors));
                    this.mismatchVectors.Add(this.geometryHandler.getAvgVector(watchVectors));
                }

                // Position done
                System.Media.SystemSounds.Asterisk.Play();

                // Log the entries
                foreach (GeometryHandler.Vector vector in sampleVectors)
                {
                    this.dataLogger.addLineToParagraph(1, corner.ToString() + '\t' + sample.ToString() + '\t' + "point" +'\t' + geometryHandler.getString(vector));              
                }
                foreach (GeometryHandler.Vector vector in watchVectors)
                {
                    this.dataLogger.addLineToParagraph(3, corner.ToString() + '\t' + sample.ToString() + '\t' + "look" + '\t' + geometryHandler.getString(vector));
                }
                this.dataLogger.addLineToParagraph(1, corner.ToString() + '\t' + sample.ToString() + '\t' + "avgVec" + '\t' + geometryHandler.getString(this.calibrationVectors[this.calibrationVectors.Count - 1]));
                this.dataLogger.addLineToParagraph(3, corner.ToString() + '\t' + sample.ToString() + '\t' + "avgVec" + '\t' + geometryHandler.getString(this.mismatchVectors[this.mismatchVectors.Count - 1]));                
            }
            // this.calibrationVectors.Count should be == this.calibrationSamples                
            
            // Intersect each sampled vector with every other sampled vector (pointing)               
            foreach (GeometryHandler.Vector vectorA in this.calibrationVectors)
            {
                foreach (GeometryHandler.Vector vectorB in this.calibrationVectors)
                {
                    if (vectorA != vectorB) //(vectorA.Start != vectorB.Start && vectorA.End != vectorB.End)
                    {
                        foreach (Point3D intersection in this.geometryHandler.vectorsIntersectTest(vectorA, vectorB))
                        {
                            //this.CHECK.Add(this.geometryHandler.vectorsIntersectTest(vectorA, vectorB));//(new GeometryHandler.Vector(new Point3D(12, 11, 13), new Point3D(1, 1, 1)), new GeometryHandler.Vector(new Point3D(0, -1, 0), new Point3D(12, 11, 13))));
                            intersectionsP.Add(intersection); 
                            this.dataLogger.addLineToParagraph(2, geometryHandler.getString(vectorA) + '\t' + geometryHandler.getString(vectorB) + '\t' + ((int)intersection.X).ToString() + '\t' + ((int)intersection.Y).ToString() + '\t' + (int)intersection.Z);
                        }
                    }
                }
            }
            Point3D intersectionP = this.geometryHandler.getCenter(intersectionsP);
            this.calibrationPoints.Add(intersectionP);

            // Intersect each sampled vector with every other sampled vector (looking)               
            foreach (GeometryHandler.Vector vectorA in this.mismatchVectors)
            {
                foreach (GeometryHandler.Vector vectorB in this.mismatchVectors)
                {
                    if (vectorA != vectorB) //(vectorA.Start != vectorB.Start && vectorA.End != vectorB.End)
                    {
                        foreach (Point3D intersection in this.geometryHandler.vectorsIntersect(vectorA, vectorB))
                        {
                            intersectionsL.Add(intersection);
                            this.dataLogger.addLineToParagraph(4, geometryHandler.getString(vectorA) + '\t' + geometryHandler.getString(vectorB) + '\t' + ((int)intersection.X).ToString() + '\t' + ((int)intersection.Y).ToString() + '\t' + (int)intersection.Z);
                        }
                    }
                }
            }
            Point3D intersectionL = this.geometryHandler.getCenter(intersectionsL);
            this.mismatchPoints.Add(intersectionL);

            this.dataLogger.writeFile();

            //Point3D checkPoint = cutPoint(this.geometryHandler.getCenter(this.CHECK));
            //this.debug4 = (int)checkPoint.X + " ; " + (int)checkPoint.Y + " ; " + (int)checkPoint.Z;

            // Clear all member-lists for next turn
            this.calibrationVectors.Clear();
            this.mismatchVectors.Clear();
            
            return intersectionP;
        }

        private Point3D cutPoint(Point3D point)
        {
            // Check x-value
            if (point.X > this.xMax)
                point.X = this.xMax;
            else if (point.X < this.xMin)
                point.X = this.xMin;
            // Check y-value
            if (point.Y > this.yMax)
                point.Y = this.yMax;
            else if (point.Y < this.yMin)
                point.Y = this.yMin;
            // Check z-value
            if (point.Z > this.zMax)
                point.Z = this.zMax;
            else if (point.Z < this.zMin)
                point.Z = this.zMin;
            
            return point;
        }
        */
        
        private List<GeometryHandler.Vector> takeSample(int position)
        {
            Random random = new Random();
            double r1 = random.Next(-5, 5);
            double r2 = random.Next(-5, 5);
            List<GeometryHandler.Vector> vectors = new List<GeometryHandler.Vector>();

            vectors.Add(takePointingSample(position, r1, r2));
            vectors.Add(takeLookingSample(position, r1, r2));

            return vectors;
        }

        private GeometryHandler.Vector takePointingSample(int p, double r1, double r2)
        {
            if (this.mode) // System is life
                return new GeometryHandler.Vector(jointsToTrack[0], jointsToTrack[1]); // (RIGHT_ELBOW, RIGHT_HAND)
            else // Debug-mode
            {
                GeometryHandler.Vector v;
                switch (p)
                {
                    case 0:
                        v = new GeometryHandler.Vector(new Point3D(802 + r1, -92 + r1, 1821 + r1), new Point3D(619 + r2, -189 + r2, 1624 + r2));
                        break;
                    case 1:
                        v = new GeometryHandler.Vector(new Point3D(99 + r1, -87 + r1, 1864 + r1), new Point3D(-41 + r2, -221 + r2, 1615 + r2));
                        break;
                    case 2:
                        v = new GeometryHandler.Vector(new Point3D(-502 + r1, -93 + r1, 1904 + r1), new Point3D(-453 + r2, -211 + r2, 1649 + r2));
                        break;
                    default:
                        v = new GeometryHandler.Vector();
                        break;
                }
                return v;
            }
        }

        private GeometryHandler.Vector takeLookingSample(int s, double r1, double r2)
        {
            if (this.mode) // System is life
                return new GeometryHandler.Vector(jointsToTrack[4], jointsToTrack[1]); // (RIGHT_ELBOW, RIGHT_HAND)
            else // Debug-mode
            {
                GeometryHandler.Vector v;
                switch (s)
                {
                    case 0:
                        v = new GeometryHandler.Vector(new Point3D(794 + r1, 294 + r1, 2063 + r1), new Point3D(619 + r2, -189 + r2, 1624 + r2));
                        break;
                    case 1:
                        v = new GeometryHandler.Vector(new Point3D(-11 + r1, 308 + r1, 2108 + r1), new Point3D(-41 + r2, -221 + r2, 1615 + r2));
                        break;
                    case 2:
                        v = new GeometryHandler.Vector(new Point3D(-709 + r1, 290 + r1, 2067 + r1), new Point3D(-453 + r2, -211 + r2, 1649 + r2));
                        break;
                    default:
                        v = new GeometryHandler.Vector();
                        break;
                }
                return v;
            }
        }
        
        private void countDown(int ms)
        {
            int sleep = ms / 1000;
            for (int time = sleep; time != 0; --time)
            {
                Thread.Sleep(1000);
            }
        }
        #endregion

        #region EVENTS
        /// <summary>
        /// EVENTS
        /// 
        /// Calls for calibration and debugging methods.
        /// </summary>

        //--- "KILL" - PROPERLY CLOSE THE APPLICATION ---//
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (tracking)
            {
                // close tracking thread
                this.trackThread.Abort();
            }
            if (calibrating) 
            {
                this.calibrationThread.Abort();
            }
            // cloese the application
            this.Close();
        }

        //--- "CALIBRATE" ---//
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.debug2 = this.round.ToString();

            if (!this.calibrating)
            {
                // Starting the calibrating thread
                this.calibrationVectors.Clear(); // Sicher ist sicher! ;)
                this.calibrationThread = new Thread(defineExhibitionPlane);
                this.calibrating = true;
                this.calibrationThread.Start();
            }
            else
            {
                this.calibrationVectors.Clear();
                this.calibrating = false;
                this.calibrationThread.Abort();
                this.debug4 = "Debug...";
            }

            ++this.round;
        }

        //--- "FINISH" WRITE LOG-DATA TO FILE ---//
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            int round = this.round;
            this.round = 42;
            List<Point3D> aim = new List<Point3D>();

            initLogger();
            this.dataLogger.newPargraph("P.X" + '\t' + "P.Y" + '\t' + "P.Z" + '\t' + "L.X" + '\t' + "L.Y" + '\t' + "L.Z" + '\t' + "EHMM.X" + '\t' + "EHMM.Y" + '\t' + "EHMM.Z" + '\t' + "EHMW.X" + '\t' + "EHMW.Y" + '\t' + "EHMW.Z");
            
            // All the points pointed and looked at
            for (int i = 0; i != this.mismatchPoints.Count; ++i)
            {
                aim.Add(this.geometryHandler.getCenter(this.mismatchPoints[i], this.calibrationPoints[i]));
                this.dataLogger.addLineToParagraph(1, this.calibrationPoints[i].X.ToString() + '\t' + this.calibrationPoints[i].Y.ToString() + '\t' + this.calibrationPoints[i].Z.ToString() + '\t' + this.mismatchPoints[i].X.ToString() + '\t' + this.mismatchPoints[i].Y.ToString() + '\t' + this.mismatchPoints[i].Z.ToString() + '\t' + (this.calibrationPoints[i].X - this.mismatchPoints[i].X).ToString() + '\t' + (this.calibrationPoints[i].Y - this.mismatchPoints[i].Y).ToString() + '\t' + (this.calibrationPoints[i].Z - this.mismatchPoints[i].Z).ToString() + '\t' + aim[i].X.ToString() + '\t' + aim[i].Y.ToString() + '\t' + aim[i].Z.ToString());
            }
            
            this.dataLogger.addLineToParagraph(1, "");
            // Average points pointed and looked at
            Point3D pointing = this.statistic.getAvg(this.calibrationPoints);
            Point3D mismatch = this.statistic.getAvg(this.mismatchPoints);
            Point3D aiming = this.statistic.getAvg(aim);
            this.dataLogger.addLineToParagraph(1, "MW");
            this.dataLogger.addLineToParagraph(1, pointing.X.ToString() + '\t' + pointing.Y.ToString() + '\t' + pointing.Z.ToString() + '\t' + mismatch.X.ToString() + '\t' + mismatch.Y.ToString() + '\t' + mismatch.Z.ToString() + '\t' + (pointing.X - mismatch.X).ToString() + '\t' + (pointing.Y - mismatch.Y).ToString() + '\t' + (pointing.Z - mismatch.Z).ToString() + '\t' + aiming.X.ToString() + '\t' + aiming.Y.ToString() + '\t' + aiming.Z.ToString());

            this.dataLogger.addLineToParagraph(1, "");
            // Standard deviation points pointed and looked at
            pointing = this.statistic.getStdAbw(this.calibrationPoints);
            mismatch = this.statistic.getStdAbw(this.mismatchPoints);
            aiming = this.statistic.getStdAbw(aim);
            this.dataLogger.addLineToParagraph(1, "StdAbw");
            this.dataLogger.addLineToParagraph(1, pointing.X.ToString() + '\t' + pointing.Y.ToString() + '\t' + pointing.Z.ToString() + '\t' + mismatch.X.ToString() + '\t' + mismatch.Y.ToString() + '\t' + mismatch.Z.ToString() + '\t' + (pointing.X - mismatch.X).ToString() + '\t' + (pointing.Y - mismatch.Y).ToString() + '\t' + (pointing.Z - mismatch.Z).ToString() + '\t' + aiming.X.ToString() + '\t' + aiming.Y.ToString() + '\t' + aiming.Z.ToString());

            this.dataLogger.addLineToParagraph(1, "");
            // Empiric standard deviation points pointed and looked at
            pointing = this.statistic.getEmpStdAbw(this.calibrationPoints);
            mismatch = this.statistic.getEmpStdAbw(this.mismatchPoints);
            aiming = this.statistic.getEmpStdAbw(aim);
            this.dataLogger.addLineToParagraph(1, "Emp. StdAbw");
            this.dataLogger.addLineToParagraph(1, pointing.X.ToString() + '\t' + pointing.Y.ToString() + '\t' + pointing.Z.ToString() + '\t' + mismatch.X.ToString() + '\t' + mismatch.Y.ToString() + '\t' + mismatch.Z.ToString() + '\t' + (pointing.X - mismatch.X).ToString() + '\t' + (pointing.Y - mismatch.Y).ToString() + '\t' + (pointing.Z - mismatch.Z).ToString() + '\t' + aiming.X.ToString() + '\t' + aiming.Y.ToString() + '\t' + aiming.Z.ToString());

            this.dataLogger.addLineToParagraph(1, "");
            // Standard error points pointed and looked at
            pointing = this.statistic.getStdErr(this.calibrationPoints);
            mismatch = this.statistic.getStdErr(this.mismatchPoints);
            aiming = this.statistic.getStdErr(aim);
            this.dataLogger.addLineToParagraph(1, "StdFehler");
            this.dataLogger.addLineToParagraph(1, pointing.X.ToString() + '\t' + pointing.Y.ToString() + '\t' + pointing.Z.ToString() + '\t' + mismatch.X.ToString() + '\t' + mismatch.Y.ToString() + '\t' + mismatch.Z.ToString() + '\t' + (pointing.X - mismatch.X).ToString() + '\t' + (pointing.Y - mismatch.Y).ToString() + '\t' + (pointing.Z - mismatch.Z).ToString() + '\t' + aiming.X.ToString() + '\t' + aiming.Y.ToString() + '\t' + aiming.Z.ToString());
            
            this.dataLogger.writeFile();
            this.round = round;
        }

        //--- UPDATE VPs NAME ---//
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.vp = this.textBox1.Text;
            this.debug1 = this.textBox1.Text;
            this.round = 0;
            this.debug2 = this.round.ToString();
        }

        //--- DETERMINE PROGRAM'S OPERATING MODE ---//
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.comboBox1.SelectedIndex == 0)
            {
                this.mode = false;
                this.textBox1.Text = "DEBUG";
            }
            else
            {
                this.mode = true; ;
                this.textBox1.Text = "LIFE";
            }
        }

        //--- DEBUG STATISTICS ---//
        private void label3_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            List<double> check = new List<double>();
            check.Add(1);
            check.Add(2);
            check.Add(4);
            check.Add(8);
            check.Add(16);
            check.Add(32);
            this.debug4 = "Average:" + '\t' + this.statistic.getAvg(check).ToString() + '\n';
            this.debug4 += "Varianz:" + '\t' + this.statistic.getVar(check).ToString() + '\n';
            this.debug4 += "StdAbw:" + '\t' + this.statistic.getStdAbw(check).ToString() + '\n';
            this.debug4 += "empVar:" + '\t' + this.statistic.getEmpVar(check).ToString() + '\n';
            this.debug4 += "empStdAbw:" + '\t' + this.statistic.getEmpStdAbw(check).ToString() + '\n';
            this.debug4 += "StdErr:" + '\t' + this.statistic.getStdErr(check).ToString();
        }
        #endregion
    }
}