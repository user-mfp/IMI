using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;
using FubiNET;

namespace IMI_Administration
{
    public partial class MainWindow : Window
    {
        #region ENUMS AND CONSTANTS
        // Headlines determine the layout (visibility, labeling and functions)
        private enum Headline
        {
            Start = 0,
            Exhibition, //1
            LoadExhibit, //2
            NewExhibit, //3
            EditExhibit, //4
            ExhibitionPlane, //5
            ExhibitionPlaneDef, //6
            ExhibitionPlaneVal, //7
            ExhibitionPlaneDone, //8
            NewName, //9
            ExhibitDef, //10
            ExhibitVal, //11
            ExhibitDone, //12
            ExhibitionSettings, //13
            ExhibitSettings //14
        };

        // Settings determine the propoerty to be edited (exhibition or exhibit)
        private enum Setting
        {
            // Exhibition
            None = 0,
            UserPosition, //1
            BackgroundImage, //2
            Overview, //3
            Threshold, //4
            SelectionTime, //5
            LockTime, //6
            SlideTime, //7
            EndWait, //8
            // Exhibit
            KernelSize, //9
            KernelWeight, //10
            Position //11
        };

        // Canstants for... 
        // ... setting attributes
        private double HIGH = 1.5; // := 150% default
        private double LOW = 0.5; // := 50% default
        // ... defining and validating
        private int SAMPLING_BREAK = 4000; // := 4s waiting between points and positions
        private int SAMPLING_POINTS = 3; // Number of points
        private int SAMPLING_VECTORS = 10; // Vectors per sample
        private int SAMPLING_POSITIONS = 3; // Number of positions
        private string INSTRUCTIONS_EXHIBITION_PLANE = "Zeigen Sie von drei Positionen aus, welche ca. 1m auseinander liegen, auf drei Eckpunkte der Ausstellungsebene." + '\n' + '\n' + "Befolgen Sie dazu bitte die, an dieser Stelle erscheinenden, Anweisungen.";
        private string INSTRUCTIONS_EXHIBIT = "Zeigen Sie von drei Positionen aus, welche ca. 1m auseinander liegen, auf das Exponat." + '\n' + '\n' + "Befolgen Sie dazu bitte die, an dieser Stelle erscheinenden, Anweisungen.";
        #endregion

        #region DECLARATIONS
        // Exhibition
        private Exhibition exhibition;
        private Headline headline;
        private Setting setting;
        private string TMP_NAME; // FOR TEMPORARY USE ONLY ! ! !
        private string TMP_PATH; // FOR TEMPORARY USE ONLY ! ! !
        private Exhibit TMP_EXHIBIT; // FOR TEMPORARY USE ONLY ! ! !
        private int TMP_EXHIBIT_INDEX; // FOR TEMPORARY USE ONLY ! ! !
        private GeometryHandler.Plane TMP_EXHIBITION_PLANE; // FOR TEMPORARY USE ONLY ! ! !
        private GeometryHandler.Plane TMP_EXHIBITION_PLANE_2; // FOR TEMPORARY USE ONLY ! ! !
        private Point3D TMP_POSITION; // FOR TEMPORARY USE ONLY ! ! !
        private Point3D TMP_POSITION_2; // FOR TEMPORARY USE ONLY ! ! !
        // Layout
        private string contentLabel1;
        private string contentLabel2;
        private string contentButton1;
        private string contentButton2;
        private string contentButton3;
        private string contentButton4;
        private string contentButton5;
        private string contentTextBox1;
        private string contentTextBox2;
        // Dialogs
        private OpenFileDialog loadConfigDialog;
        private OpenFileDialog loadTextDialog;
        private OpenFileDialog loadImageDialog;
        private SaveFileDialog saveConfigDialog;
        private SaveFileDialog saveTextDialog;
        // Handler
        private GeometryHandler geometryHandler;
        private FileHandler fileHandler;
        // Tracking
        private delegate void NoArgDelegate();
        private List<Point3D> jointsToTrack;
        private double timestamp; // DO NOT USE ! ! !
        private float confidence; // DO NOT USE ! ! !
        private uint USER_ID = 1; // DO NOT TOUCH ! ! !
        // Tracking-thread
        private bool tracking;
        private Thread trackThread;
        // Calibration
        //Calibrator calibrator; // Declare calibrator
        private CalibrationHandler calibrationHandler;
        private List<Point3D> calibrationPoints = new List<Point3D>();
        private List<Point3D> mismatchPoints = new List<Point3D>();
        // Calibration-thread
        private bool calibrating;
        private Thread calibrationThread;
        #endregion

        #region INITIALIZATIONS
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize tracking
            initJoints();
            this.tracking = false; // Turn off tracking
            this.calibrating = false; // Turn off any calibration
            
            // Initialize dialogs
            initDialogs();

            // Initialize handlers
            this.geometryHandler = new GeometryHandler();
            this.fileHandler = new FileHandler();
            
            // Initialize layout
            this.headline = Headline.Start;
            this.setting = Setting.None;
            updateLayout();
        }
        
        private void initJoints()
        {
            this.jointsToTrack = new List<Point3D>();
            this.jointsToTrack.Add(new Point3D()); // RIGHT_ELBOW
            this.jointsToTrack.Add(new Point3D()); // RIGHT_HAND
            this.jointsToTrack.Add(new Point3D()); // LEFT_ELBOW
            this.jointsToTrack.Add(new Point3D()); // LEFT_HAND
            this.jointsToTrack.Add(new Point3D()); // HEAD
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

        private void initDialogs()
        {
            this.loadConfigDialog = new OpenFileDialog();
            this.loadConfigDialog.Filter = "Config-Files|*.xml";
            this.loadConfigDialog.Title = "Konfigurationsdatei laden";
            this.loadConfigDialog.FileOk += new System.ComponentModel.CancelEventHandler(loadConfigDialog_FileOk);
            this.loadTextDialog = new OpenFileDialog();
            this.loadTextDialog.Filter = "Text-Files|*.txt";
            this.loadTextDialog.Title = "Textdatei laden";
            this.loadTextDialog.FileOk += new System.ComponentModel.CancelEventHandler(loadTextDialog_FileOk);
            this.loadImageDialog = new OpenFileDialog();
            this.loadImageDialog.Filter = "Alle unterstützen Grafiken|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png";
            this.loadImageDialog.Title = "Bilddatei laden";
            this.loadImageDialog.FileOk += new System.ComponentModel.CancelEventHandler(loadImageDialog_FileOk);
            this.saveConfigDialog = new SaveFileDialog();
            this.saveConfigDialog.Filter = "Config-Files|*.xml";
            this.saveConfigDialog.Title = "Konfigurationsdatei speichern";
            this.saveConfigDialog.FileOk += new System.ComponentModel.CancelEventHandler(saveConfigDialog_FileOk);
            this.saveTextDialog = new SaveFileDialog(); ;
            this.saveTextDialog.Filter = "Text-Files|*.txt";
            this.saveTextDialog.Title = "Textdatei speichern";
            this.saveTextDialog.FileOk += new System.ComponentModel.CancelEventHandler(saveTextDialog_FileOk);
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
        
        private bool trackableUser(uint userID)
        {
            if (Fubi.isUserInScene(Fubi.getClosestUserID()) && Fubi.isUserTracked(Fubi.getClosestUserID()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void updateFubi()
        {
            if (trackableUser(Fubi.getClosestUserID())) // There is a trackable user
            {
                updateJoints();
            }
            else
            {
                //this.contentLabel2 = System.DateTime.Now.ToString("HH.mm.ss");
            }
            
            // Updating the Layout
            switch (this.headline)
            {
                case Headline.ExhibitionPlaneDef:
                    if (trackableUser(Fubi.getClosestUserID())) // There is a trackable user
                    {
                        if (this.calibrating)
                        {
                            this.button5.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            this.button5.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        this.button5.Visibility = Visibility.Hidden;
                    }
                    break;
                case Headline.ExhibitionPlaneVal:
                    if (trackableUser(Fubi.getClosestUserID())) // There is a trackable user
                    {
                        if (this.calibrating)
                        {
                            this.button5.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            this.button5.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        this.button5.Visibility = Visibility.Hidden;
                    }
                    break;
                case Headline.ExhibitDef:
                    if (trackableUser(Fubi.getClosestUserID())) // There is a trackable user
                    {
                        if (this.calibrating)
                        {
                            this.button5.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            this.button5.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        this.button5.Visibility = Visibility.Hidden;
                    }
                    break;
                case Headline.ExhibitVal:
                    if (trackableUser(Fubi.getClosestUserID())) // There is a trackable user
                    {
                        if (this.calibrating)
                        {
                            this.button5.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            this.button5.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        this.button5.Visibility = Visibility.Hidden;
                    }
                    break;
                case Headline.ExhibitDone:
                    if (!this.calibrating)
                    {
                        this.button5.Visibility = Visibility.Visible;
                    }
                    break;
                default:
                    {
                        this.button5.Visibility = Visibility.Visible;
                    }
                    break;
            }
            updateButtons();
            updateLabels();
        }

        private void updateJoints()
        {
            float x, y, z;
                        
            // Right arm
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID(), FubiUtils.SkeletonJoint.RIGHT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_ELBOW, x, y, z);
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID(), FubiUtils.SkeletonJoint.RIGHT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.RIGHT_HAND, x, y, z);
            
            // Left arm
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID(), FubiUtils.SkeletonJoint.LEFT_ELBOW, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_ELBOW, x, y, z);
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID(), FubiUtils.SkeletonJoint.LEFT_HAND, out x, out y, out z, out confidence, out timestamp);
            updateJoint(FubiUtils.SkeletonJoint.LEFT_HAND, x, y, z);

            // Nose (head)
            Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID(), FubiUtils.SkeletonJoint.FACE_NOSE, out x, out y, out z, out this.confidence, out this.timestamp); // this.USER_ID, FubiUtils.SkeletonJoint.HEAD, out x, out y, out z, out confidence, out timestamp);
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

        #region THREADS
        private void startTracking()
        {
            // Starting the tracking-thread properly
            this.trackThread = new Thread(track);
            this.tracking = true;
            this.trackThread.Start();
        }

        private void calibrationTest()
        {
            for (int i = 0; i != 10; ++i)
            {
                this.contentLabel2 = (10 - i).ToString();
                Thread.Sleep(1000);
            }

            stopCalibration();
        }

        private void startPlaneDefinition()
        {
            // Starting a calibration-thread porperly
            this.calibrationThread = new Thread(definePlane);
            this.calibrating = true;
            this.calibrationThread.Start();
        }

        private void startPositionDefinition()
        {
            // Starting a calibration-thread porperly
            this.calibrationThread = new Thread(definePosition);
            this.calibrating = true;
            this.calibrationThread.Start();
        }

        private void startUserPositionDefinition()
        {
            // Starting the calibrationthread properly
            this.calibrationThread = new Thread(defineUserPosition);
            this.calibrating = true;
            this.calibrationThread.Start();
        }

        private void stopTracking()
        {
            // Stopping the tracking-thread properly
            this.tracking = false;
            this.trackThread.Abort();
        }

        private void stopCalibration()
        {
            // Stopping any calibration-thread porperly
            this.calibrating = false;
            this.calibrationThread.Abort();
            updateLayout();
        }
        #endregion
        
        #region LAYOUT
        // Update layout to...
        private void updateLayout()
        {
            switch (this.headline)
            { 
                case Headline.Start:
                    this.contentLabel1 = "START";
                    this.contentButton1 = "Ausstellung laden";
                    this.contentButton2 = "Ausstellung erstellen";
                    this.contentButton5 = "beenden";
                    showStart();
                    break;
                case Headline.Exhibition:
                    showExhibition();
                    break;
                case Headline.LoadExhibit:
                    showLoadExhibit();
                    break;
                case Headline.NewExhibit:
                    showNewExhibit();
                    break;
                case Headline.EditExhibit:
                    showEditExhibit();
                    break;
                case Headline.ExhibitionPlane:
                    showExhibitionPlane();
                    break;
                case Headline.ExhibitionPlaneDef:
                    showExhibitionPlaneDef();
                    break;
                case Headline.ExhibitionPlaneVal:
                    showExhibitionPlaneVal();
                    break;
                case Headline.ExhibitionPlaneDone:
                    showExhibitionPlaneDone();
                    break;
                case Headline.NewName:
                    showNewName();
                    break;
                case Headline.ExhibitDef:
                    showExhibitDef();
                    break;
                case Headline.ExhibitVal:
                    showExhibitVal();
                    break;
                case Headline.ExhibitDone:
                    showExhibitDone();
                    break;
                case Headline.ExhibitionSettings:
                    showExhibitionSettings();
                    break;
                case Headline.ExhibitSettings:
                    showExhibitSettings();
                    break;
                default:
                    break;
            }
        }
        
        // ... show the exhibition's settings
        private void showExhibitionSettings()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Visible;

            // ComboBoxes
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("Benutzerposition"); //UserPosition
            this.comboBox1.Items.Add("Hintergrundbild"); //BackgroundImage
            this.comboBox1.Items.Add("Übersicht"); //Overview
            this.comboBox1.Items.Add("Genauigkeit"); // Threshold
            this.comboBox1.Items.Add("Auswahlzeit"); // SelectionTime
            this.comboBox1.Items.Add("Sperrdauer"); // LockTime
            this.comboBox1.Items.Add("Dauer pro Bild"); // SlideTime
            this.comboBox1.Items.Add("Abklingzeit"); // EndWait
            this.comboBox1.Visibility = Visibility.Visible;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show an exhibit's settings
        private void showExhibitSettings()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Visible;

            // Boxes
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("Position"); // Position
            this.comboBox1.Items.Add("Radius"); // KernelSize
            this.comboBox1.Items.Add("Gewichtung"); // KernelWeight
            this.comboBox1.Visibility = Visibility.Visible;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }
        
        // ... show the outcome of definition and validation of an exhibit's position
        private void showExhibitDone()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1; //this.TMP_NAME.ToUpper() + " - POSITIONSBESTIMMUNG";
            this.label1.Visibility = Visibility.Visible;

            this.textBlock2.Text = "- Position erfolgreich bestimmt." + '\n' + "oder" + '\n' + "- Position nicht erfolgreich bestimmt.";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;
          
            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = this.contentButton5;//"OK";
            this.button5.Visibility = Visibility.Visible;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }
        
        // ... show dialogue for an exhibit's position's validation
        private void showExhibitVal()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.textBlock2.Text = this.contentLabel2;
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = this.contentButton4;
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Visible;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }
        
        // ... show dialogue for an exhibit's position's validation
        private void showExhibitDef()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.textBlock2.Text = this.contentLabel2;
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = this.contentButton4;
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Visible;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show dialogue for an new name of an exhibition oder exhibit
        private void showNewName()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.textBlock2.Text = this.contentLabel2;
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Hidden;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Text = this.contentTextBox2;
            this.textBox2.Visibility = Visibility.Visible;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show the outcome of definition and validation of an exhibition plane
        private void showExhibitionPlaneDone()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;// "AUSSTELLUNGSEBENE - BESTIMMUNG";
            this.label1.Visibility = Visibility.Visible;

            this.textBlock2.Text = this.contentLabel2;// "- Ebene erfolgreich bestimmt." + '\n' + "oder" + '\n' + "- Ebene nicht erfolgreich bestimmt.";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = this.contentButton5;// "OK";
            this.button5.Visibility = Visibility.Visible;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show dialogue for an exhibition plane's validation
        private void showExhibitionPlaneVal()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.textBlock2.Text = this.contentLabel2;
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = this.contentButton4;
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Visible;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show dialogue for an exhibition plane's definition
        private void showExhibitionPlaneDef()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.textBlock2.Text = this.contentLabel2;
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = this.contentButton4;
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Hidden;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show the exhibition plane
        private void showExhibitionPlane()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = this.contentButton1;
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = this.contentButton2;
            this.button2.Visibility = Visibility.Visible;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = this.contentButton4;
            this.button4.Visibility = Visibility.Visible;

            this.button5.Visibility = Visibility.Hidden;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show the editing of an exhibit
        private void showEditExhibit()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;// +" - BEARBEITEN";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Content = this.contentButton2;
            this.button2.Visibility = Visibility.Visible;

            this.button3.Content = this.contentButton3;
            this.button3.Visibility = Visibility.Visible;

            this.button4.Content = this.contentButton4;
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Visible;

            // ComboBoxes
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("neues Bild");
            if (this.TMP_EXHIBIT.getImages() != null) // The exhibit has images
            {
                foreach (KeyValuePair<string, BitmapImage> image in this.TMP_EXHIBIT.getImages())
                {
                    int start = image.Key.LastIndexOf('\\') + 1;
                    int length = image.Key.LastIndexOf('.') - start;
                    string imageName = image.Key.Substring(start, length);
                    this.comboBox1.Items.Add(imageName);
                }
            }
            this.comboBox1.Visibility = Visibility.Visible;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Text = this.contentTextBox1;
            this.textBox1.Visibility = Visibility.Visible;

            this.textBox2.Visibility = Visibility.Hidden;
        }

        // ... show a new exhibit
        private void showNewExhibit()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = this.contentButton1;
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = this.contentButton2;
            this.button2.Visibility = Visibility.Visible;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show an existing exhibit
        private void showLoadExhibit()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = this.contentButton1;// "bearbeiten";
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = this.contentButton2;// "löschen";
            this.button2.Visibility = Visibility.Visible;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show the exhibition
        private void showExhibition()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = this.contentButton4;
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Visible;

            // ComboBoxes
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("neues Exponat");
            if (this.exhibition.getExhibits().Count != 0) // There is and exhibit and it has exhibits in it
            {
                foreach (Exhibit exhibit in this.exhibition.getExhibits())
                {
                    this.comboBox1.Items.Add(exhibit.getName());
                }
            }
            this.comboBox1.Visibility = Visibility.Visible;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show the start screen
        private void showStart()
        {
            // Labels
            this.textBlock1.Text = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = this.contentButton1;
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = this.contentButton2;
            this.button2.Visibility = Visibility.Visible;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = this.contentButton5;
            this.button5.Visibility = Visibility.Visible;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Visibility = Visibility.Hidden;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        private void updateLabels()
        {
            this.textBlock1.Text = this.contentLabel1;
            this.textBlock2.Text = this.contentLabel2;
        }

        private void updateButtons()
        {
            this.button1.Content = this.contentButton1;
            this.button2.Content = this.contentButton2;
            this.button3.Content = this.contentButton3;
            this.button4.Content = this.contentButton4;
            this.button5.Content = this.contentButton5;
        }

        private void updateTextBoxes()
        {
            this.textBox1.Text = this.contentTextBox1;
            this.textBox2.Text = this.contentTextBox2;
        }

        private void updateComboBoxes()
        {
        }

        private void updateImage(BitmapImage image)
        {
            this.image1.Source = image;
            this.image1.Visibility = Visibility.Visible;
        }

        private void showLowMedHigh()
        {
            // EMPTY INSTANCES ARE FOR DEFAULT USE ONLY ! ! !
            Exhibition DEFAULT_EXHIBITION = new Exhibition(); 
            Exhibit DEFAULT_EXHIBIT = new Exhibit();

            this.comboBox2.Items.Clear();
            this.comboBox2.Items.Add("niedrig"); // Low
            this.comboBox2.Items.Add("normal"); // Medium
            this.comboBox2.Items.Add("hoch"); // High

            switch (this.setting)
            { 
                case Setting.None:
                    break;
                case Setting.UserPosition:
                    break;
                case Setting.BackgroundImage:
                    break;
                case Setting.Overview:
                    break;
                case Setting.Threshold:
                    this.comboBox2.SelectedIndex = detLessIsMore(DEFAULT_EXHIBITION.getThreshold(), this.exhibition.getThreshold());
                    break;
                case Setting.SelectionTime:
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBITION.getSelectionTime(), this.exhibition.getSelectionTime());
                    break;
                case Setting.LockTime:
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBITION.getLockTime(), this.exhibition.getLockTime());
                    break;
                case Setting.SlideTime:
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBITION.getSlideTime(), this.exhibition.getSlideTime());
                    break;
                case Setting.EndWait:
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBITION.getEndWait(), this.exhibition.getEndWait());
                    break;
                case Setting.KernelSize:
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBIT.getKernelSize(), this.TMP_EXHIBIT.getKernelSize());
                    break;
                case Setting.KernelWeight:
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBIT.getKernelWeight(), this.TMP_EXHIBIT.getKernelWeight());
                    break;
                case Setting.Position:
                    break;
                default:
                    break;
            }            
            this.comboBox2.Visibility = Visibility.Visible;
            this.button2.Visibility = Visibility.Hidden;
        }
        #endregion

        #region EVENTS
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.headline)
            {
                case Headline.Start: //"hidden"
                    break;
                case Headline.Exhibition:
                    this.TMP_EXHIBIT_INDEX = this.comboBox1.SelectedIndex - 1;

                    switch(this.comboBox1.SelectedIndex)
                    {
                        case -1: // No item selected: "initialization"
                            break;
                        case 0: // First item selected: "new exhibit"
                            this.contentLabel1 = "NEUES EXPONAT";
                            this.contentButton1 = "laden";
                            this.contentButton2 = "erstellen";
                            this.headline = Headline.NewExhibit;
                            updateLayout();
                            break;
                        default: // Existing item selected: "exhibit XY"
                            this.contentLabel1 = this.comboBox1.SelectedItem.ToString();
                            this.contentButton1 = "bearbeiten";
                            this.contentButton2 = "löschen";
                            this.headline = Headline.LoadExhibit;
                            updateLayout();
                            break;
                    }
                    break;
                case Headline.LoadExhibit: //"hidden"
                    break;
                case Headline.NewExhibit: //"hidden"
                    break;
                case Headline.EditExhibit:
                    switch (this.comboBox1.SelectedIndex)
                    {
                        case -1: // No item selected: "initialization" 
                            this.contentButton2 = "     ";
                            this.contentButton3 = "Text laden";
                            this.contentButton4 = "Eigenschaften";
                            this.contentButton5 = "OK";
                            break;
                        case 0: // First item selected: "new image"
                            this.contentButton2 = "laden";
                            break;
                        default: // Existing image selected: "?. image"
                            foreach (KeyValuePair<string, BitmapImage> image in this.TMP_EXHIBIT.getImages())
                            {
                                if (image.Key.Contains(this.comboBox1.SelectedItem.ToString()))
                                {
                                    this.updateImage(image.Value);
                                    break;
                                }
                            }
                            this.contentButton2 = "löschen";
                            break;
                    }
                    updateButtons();
                    break;
                case Headline.ExhibitionPlane: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDef: //"hidden"
                    break;
                case Headline.ExhibitionPlaneVal: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDone: //"hidden"
                    break;
                case Headline.NewName: //"hidden"
                    break;
                case Headline.ExhibitDef: //"hidden"
                    break;
                case Headline.ExhibitVal: //"hidden"
                    break;
                case Headline.ExhibitDone: //"hidden"
                    break;
                case Headline.ExhibitionSettings:
                    switch (this.comboBox1.SelectedIndex)
                    {
                        case 0: //UserPosition
                            this.image1.Visibility = Visibility.Hidden;
                            this.comboBox2.Visibility = Visibility.Hidden;
                            this.contentButton2 = "einstellen";
                            this.button2.Visibility = Visibility.Visible;
                            this.button3.Visibility = Visibility.Hidden;
                            this.setting = Setting.UserPosition;
                            break;
                        case 1: //BackgroundImage
                            updateImage(this.exhibition.getBackgroundImage().Value);
                            this.comboBox2.Visibility = Visibility.Hidden;
                            this.contentButton2 = "laden";
                            this.button2.Visibility = Visibility.Visible;
                            this.button3.Visibility = Visibility.Hidden;
                            this.setting = Setting.BackgroundImage;
                            break;
                        case 2: //Overview
                            updateImage(this.exhibition.getOverview().Value);
                            this.comboBox2.Visibility = Visibility.Hidden;
                            this.contentButton2 = "laden";
                            this.button2.Visibility = Visibility.Visible;
                            this.button3.Visibility = Visibility.Hidden;
                            this.setting = Setting.Overview;
                            break;
                        case 3: //Threshold
                            this.image1.Visibility = Visibility.Hidden;
                            this.setting = Setting.Threshold;
                            this.showLowMedHigh();
                            break;
                        case 4: //SelectionTime
                            this.image1.Visibility = Visibility.Hidden;
                            this.setting = Setting.SelectionTime;
                            this.showLowMedHigh();
                            break;
                        case 5: //LockTime
                            this.image1.Visibility = Visibility.Hidden;
                            this.setting = Setting.LockTime;
                            this.showLowMedHigh();
                            break;
                        case 6: //SlideTime
                            this.image1.Visibility = Visibility.Hidden;
                            this.setting = Setting.SlideTime;
                            this.showLowMedHigh();
                            break;
                        case 7: //EndWait
                            this.image1.Visibility = Visibility.Hidden;
                            this.setting = Setting.EndWait;
                            this.showLowMedHigh();
                            break;
                        default:
                            break;
                    }
                    updateButtons();
                    break;
                case Headline.ExhibitSettings:
                    switch (this.comboBox1.SelectedIndex)
                    {
                        case 0: //Position
                            this.contentButton2 = "ändern";
                            this.button2.Visibility = Visibility.Visible;
                            this.button3.Visibility = Visibility.Hidden;
                            this.setting = Setting.Position;
                            updateButtons();
                            break;
                        case 1: //KernelSize
                            this.setting = Setting.KernelSize;
                            this.showLowMedHigh();
                            break;
                        case 2: //KernelWeight
                            this.setting = Setting.KernelWeight;
                            this.showLowMedHigh();
                            break;
                        default:
                            break;
                    }
                    updateButtons();
                    break;
                default:
                    break;
            }
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.headline)
            {
                case Headline.Start: //"hidden"
                    break;
                case Headline.Exhibition: //"hidden"
                    break;
                case Headline.LoadExhibit: //"hidden"
                    break;
                case Headline.NewExhibit: //"hidden"
                    break;
                case Headline.EditExhibit: //"hidden"
                    break;
                case Headline.ExhibitionPlane: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDef: //"hidden"
                    break;
                case Headline.ExhibitionPlaneVal: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDone: //"hidden"
                    break;
                case Headline.NewName: //"hidden"
                    break;
                case Headline.ExhibitDef: //"hidden"
                    break;
                case Headline.ExhibitVal: //"hidden"
                    break;
                case Headline.ExhibitDone: //"hidden"
                    break;
                case Headline.ExhibitionSettings:
                    this.contentButton3 = "ändern";
                    this.button3.Visibility = Visibility.Visible;
                    break;
                case Headline.ExhibitSettings:   
                    this.contentButton3 = "ändern";
                    this.button3.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch (this.headline)
            { 
                case Headline.Start: //"load existing exhibition"
                    if (this.loadConfigDialog.ShowDialog() == true) // Temporary file path has been set
                    {
                        this.exhibition = this.fileHandler.loadExhibition(this.TMP_PATH);
                        this.TMP_PATH = null;

                        this.contentLabel1 = this.exhibition.getName().ToUpper();
                        this.contentButton4 = "Einstellungen";
                        this.contentButton5 = "schließen";
                        this.headline = Headline.Exhibition;
                        updateLayout();
                    }
                    break;
                case Headline.Exhibition: //"hidden"
                    break;
                case Headline.LoadExhibit: //"edit exhibit"
                    this.TMP_EXHIBIT = this.exhibition.getExhibit(this.TMP_EXHIBIT_INDEX);
                    this.TMP_EXHIBIT_INDEX = this.comboBox1.SelectedIndex - 1;

                    this.contentLabel1 = this.TMP_EXHIBIT.getName().ToUpper() + " - BEARBEITEN";
                    this.contentTextBox1 = this.TMP_EXHIBIT.getDescription();
                    this.headline = Headline.EditExhibit;
                    updateLayout();
                    break;
                case Headline.NewExhibit: //"load existing exhibit"
                    if (this.loadConfigDialog.ShowDialog() == true) // Temporary file path has been set
                    {
                        this.TMP_EXHIBIT = this.fileHandler.loadExhibit(this.TMP_PATH);
                        this.TMP_PATH = null;
                        this.exhibition.addExhibit(this.TMP_EXHIBIT);
                        this.TMP_EXHIBIT_INDEX = this.exhibition.getExhibits().Count - 1;

                        this.contentLabel1 = this.TMP_EXHIBIT.getName().ToUpper() + " - BEARBEITEN";
                        this.contentTextBox1 = this.TMP_EXHIBIT.getDescription();
                        this.headline = Headline.EditExhibit;
                        updateLayout();
                    }
                    else // Temporary file path hat not been set
                    { }
                    break;
                case Headline.EditExhibit: //"hidden"
                    break;
                case Headline.ExhibitionPlane: //"load exisiting definition of the exhibition plane"
                    if (this.loadConfigDialog.ShowDialog() == true) // Temporary file path has been set
                    {
                        this.TMP_EXHIBITION_PLANE = this.fileHandler.loadExhibitionPlane(this.TMP_PATH);
                        this.TMP_PATH = null;
                        this.exhibition = new Exhibition(this.TMP_NAME, this.TMP_EXHIBITION_PLANE);

                        this.contentLabel1 = this.exhibition.getName().ToUpper();
                        this.contentButton4 = "Einstellungen";
                        this.contentButton5 = "schließen";
                        this.headline = Headline.Exhibition;
                        updateLayout();
                    }
                    break;
                case Headline.ExhibitionPlaneDef: //"hidden"
                    break;
                case Headline.ExhibitionPlaneVal: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDone: //"hidden"
                    break;
                case Headline.NewName: //"hidden"
                    break;
                case Headline.ExhibitDef: //"hidden"
                    break;
                case Headline.ExhibitVal: //"hidden"
                    break;
                case Headline.ExhibitDone: //"hidden"
                    break;
                case Headline.ExhibitionSettings: //"hidden"
                    break;
                case Headline.ExhibitSettings: //"hidden"
                    break;
                default:
                    break;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            switch (this.headline)
            {
                case Headline.Start: //"define new exhibition"
                    this.contentLabel1 = "AUSSTELLUNGSNAMEN BESTIMMEN";
                    this.contentLabel2 = "Bitte Ausstellungsnamen eingeben.";
                    this.contentButton5 = "OK";
                    this.headline = Headline.NewName;
                    updateLayout();
                    break;
                case Headline.Exhibition: //"hidden"
                    break;
                case Headline.LoadExhibit: //"remove exhibit from exhibition's list of exhibits"
                    this.exhibition.removeExhibit(this.TMP_EXHIBIT_INDEX);

                    this.contentLabel1 = this.exhibition.getName().ToUpper();
                    this.contentButton4 = "Einstellungen";
                    this.contentButton5 = "schließen";
                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case Headline.NewExhibit: //"define new exhibit"
                    this.contentLabel1 = "EXPONATNAMEN BESTIMMEN";
                    this.contentLabel2 = "Bitte Exponatsnamen eingeben.";
                    this.contentButton5 = "OK";
                    this.headline = Headline.NewName;
                    updateLayout();
                    break;
                case Headline.EditExhibit: //"delete image from exhibit"
                    switch (this.comboBox1.SelectedIndex)
                    { 
                        case -1: // No item selected
                            break;
                        case 0: // "new image"
                            if (this.loadImageDialog.ShowDialog() == true)
                            {
                                this.TMP_EXHIBIT.addImage(this.fileHandler.loadImage(this.TMP_PATH));
                                this.TMP_PATH = null;
                                updateLayout();
                            }
                            else
                            { }
                            break;
                        default: // Any other image selected
                            KeyValuePair<string, BitmapImage> img = new KeyValuePair<string,BitmapImage>();

                            foreach (KeyValuePair<string, BitmapImage> image in this.TMP_EXHIBIT.getImages())
                            {
                                if (image.Key.Contains(this.comboBox1.SelectedItem.ToString()))
                                {
                                    img = image;
                                }
                            }
                            if (img.Key != null) // Image found
                            {
                                this.TMP_EXHIBIT.removeImage(img);
                                this.image1.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                MessageBox.Show("Bild konnte nicht gelöscht werden.");
                            }
                            updateLayout();
                            break;
                    }
                    break;
                case Headline.ExhibitionPlane:// "define new exhibition plane"
                    startTracking();

                    this.contentLabel2 = this.INSTRUCTIONS_EXHIBITION_PLANE;
                    this.headline = Headline.ExhibitionPlaneDef;
                    updateLayout();
                    break;
                case Headline.ExhibitionPlaneDef: //"hidden"
                    break;
                case Headline.ExhibitionPlaneVal: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDone: //"hidden"
                    break;
                case Headline.NewName: //"hidden"
                    break;
                case Headline.ExhibitDef: //"hidden"
                    break;
                case Headline.ExhibitVal: //"hidden"
                    break;
                case Headline.ExhibitDone: //"hidden"
                    break;
                case Headline.ExhibitionSettings:
                    if (this.setting == Setting.UserPosition)
                    {
                        startTracking();

                        this.TMP_NAME = "Benutzer";

                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSDEFINITION";
                        this.contentLabel2 = "Bitte stellen Sie sich auf die zukünftige Benutzerposition.";
                        this.contentButton4 = "zurück";
                        this.contentButton5 = "Start";
                        this.headline = Headline.ExhibitDef;
                    }
                    else if (this.setting == Setting.BackgroundImage)
                    {
                        if (this.loadImageDialog.ShowDialog() == true)
                        {
                            this.exhibition.setBackgroundImage(this.fileHandler.loadImage(this.loadImageDialog.FileName));
                        }
                    }
                    else if (this.setting == Setting.Overview)
                    {
                        if (this.loadImageDialog.ShowDialog() == true)
                        {
                            this.exhibition.setOverview(this.fileHandler.loadImage(this.loadImageDialog.FileName));
                        }
                    }
                    updateLayout();
                    break;
                case Headline.ExhibitSettings:
                    startTracking();

                    this.TMP_NAME = this.TMP_EXHIBIT.getName();

                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSDEFINITION";
                    this.contentLabel2 = this.INSTRUCTIONS_EXHIBIT;
                    this.contentButton4 = "zurück";
                    this.contentButton5 = "OK";
                    this.headline = Headline.ExhibitDef;
                    updateLayout();
                    break;
                default:
                    break;
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            switch (this.headline)
            {
                case Headline.Start: //"hidden"
                    break;
                case Headline.Exhibition: //"hidden"
                    break;
                case Headline.LoadExhibit: //"hidden"
                    break;
                case Headline.NewExhibit: //"hidden"
                    break;
                case Headline.EditExhibit: //"load text from txt-file"
                    if (this.loadTextDialog.ShowDialog() == true)
                    {
                        StreamReader streamReader = new StreamReader(this.loadTextDialog.FileName);
                        this.contentTextBox1 = (streamReader.ReadToEnd());
                    }
                    updateLayout();
                    break;
                case Headline.ExhibitionPlane: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDef: //"hidden"
                    break;
                case Headline.ExhibitionPlaneVal: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDone: //"hidden"
                    break;
                case Headline.NewName: //"hidden"
                    break;
                case Headline.ExhibitDef: //"hidden"
                    break;
                case Headline.ExhibitVal: //"hidden"
                    break;
                case Headline.ExhibitDone: //"hidden"
                    break;
                case Headline.ExhibitionSettings: //"Set current attribute"
                    setAttribute();
                    break;
                case Headline.ExhibitSettings: //"Set current attribute"
                    setAttribute();
                    break;
                default:
                    break;
            }             
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            switch (this.headline)
            {
                case Headline.Start: //"hidden"
                    break;
                case Headline.Exhibition: //"open exhibition's properties"
                    this.contentLabel1 = this.exhibition.getName() + " - EINSTELLUNGEN";
                    this.contentButton5 = "OK";
                    this.headline = Headline.ExhibitionSettings;
                    updateLayout();
                    break;
                case Headline.LoadExhibit: //"hidden"
                    break;
                case Headline.NewExhibit: //"hidden"
                    break;
                case Headline.EditExhibit: //"open exhibit's properties"
                    this.contentLabel1 = this.TMP_EXHIBIT.getName() + " - EINSTELLUNGEN";
                    this.contentButton5 = "OK";
                    this.headline = Headline.ExhibitSettings;
                    updateLayout();
                    break;
                case Headline.ExhibitionPlane: //"back to the start"
                    this.headline = Headline.Start;
                    updateLayout();
                    break;
                case Headline.ExhibitionPlaneDef: //"abort calibration or go back to exhibition plane"
                    if (this.calibrating)
                    {
                        stopCalibration();

                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBITION_PLANE;
                        this.contentButton4 = "zurück";
                        this.button5.Visibility = Visibility.Visible;
                        updateLayout();
                    }
                    else
                    {
                        stopTracking();

                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENBESTIMMUNG";
                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBITION_PLANE;
                        this.contentButton1 = "laden";
                        this.contentButton2 = "bestimmen";
                        this.contentButton4 = "abbrechen";
                        this.headline = Headline.ExhibitionPlane;
                        updateLayout();
                    }
                    break;
                case Headline.ExhibitionPlaneVal: //"abort validation or go back to exhibition plane"
                    if (this.calibrating)
                    {
                        stopCalibration();

                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBITION_PLANE;
                        this.contentButton4 = "zurück";
                        this.button5.Visibility = Visibility.Visible;
                        updateLayout();
                    }
                    else
                    {
                        stopTracking();

                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENBESTIMMUNG";
                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBITION_PLANE;
                        this.contentButton1 = "laden";
                        this.contentButton2 = "bestimmen";
                        this.contentButton4 = "abbrechen";
                        this.headline = Headline.ExhibitionPlane;
                        updateLayout();
                    }
                    break;
                case Headline.ExhibitionPlaneDone: //"hidden"
                    break;
                case Headline.NewName: //"hidden"
                    break;
                case Headline.ExhibitDef: //"back to the exhibition"
                    if (this.calibrating)
                    {
                        stopCalibration();

                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBIT;
                        this.contentButton4 = "zurück";
                        this.button5.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        stopTracking();

                        this.contentLabel1 = this.exhibition.getName().ToUpper();
                        this.contentButton4 = "Einstellungen";
                        this.contentButton5 = "schließen";
                        this.headline = Headline.Exhibition;
                        updateLayout();
                    }
                    break;
                case Headline.ExhibitVal: //"back to the exhibition"
                    if (this.calibrating)
                    {
                        stopCalibration();

                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBIT;
                        this.contentButton4 = "zurück";
                        this.button5.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        stopTracking();

                        this.contentLabel1 = this.exhibition.getName().ToUpper();
                        this.contentButton4 = "Einstellungen";
                        this.contentButton5 = "schließen";
                        this.headline = Headline.Exhibition;
                        updateLayout();
                    }
                    break;
                case Headline.ExhibitDone: //"hidden"
                    break;
                case Headline.ExhibitionSettings: //"hidden"
                    break;
                case Headline.ExhibitSettings: //"hidden"
                    break;
                default:
                    break;
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e) 
        {
            switch (this.headline)
            {
                default:
                    break;
                case Headline.Start: //"close the application"
                    closeAllThreads();
                    break;
                case Headline.Exhibition: //"close the application"
                    if (this.exhibition.getPath() == null)
                    {
                        if (this.saveConfigDialog.ShowDialog() == true)
                        {
                            this.exhibition.setPath(saveConfigDialog.FileName);
                        }
                    }
                    this.fileHandler.saveExhibition(this.exhibition);
                    closeAllThreads();
                    break;
                case Headline.LoadExhibit: //"hidden"
                    break;
                case Headline.NewExhibit: //"hidden"
                    break;
                case Headline.EditExhibit: //"editing done"
                    this.TMP_EXHIBIT.setDescription(this.textBox1.Text);

                    if (this.TMP_EXHIBIT_INDEX != -1) // Existing exhibit
                    {
                        this.exhibition.setExhibit(this.TMP_EXHIBIT_INDEX, this.TMP_EXHIBIT);
                        this.fileHandler.saveExhibit(this.TMP_EXHIBIT);

                        this.contentLabel1 = this.exhibition.getName().ToUpper();
                        this.contentButton4 = "Einstellungen";
                        this.contentButton5 = "schließen";
                        this.headline = Headline.Exhibition;
                        updateLayout();
                    }
                    else // New exhibit
                    {                        
                        if (this.saveConfigDialog.ShowDialog() == true)
                        {
                            this.TMP_EXHIBIT.setPath(this.saveConfigDialog.FileName);
                            this.TMP_PATH = null;
                            this.exhibition.addExhibit(this.TMP_EXHIBIT);

                            this.fileHandler.saveExhibit(this.TMP_EXHIBIT);

                            this.contentLabel1 = this.exhibition.getName().ToUpper();
                            this.contentButton4 = "Einstellungen";
                            this.contentButton5 = "schließen";
                            this.headline = Headline.Exhibition;
                            updateLayout();
                        }
                    }
                    break;
                case Headline.ExhibitionPlane: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDef: //"start definition of exhibition plane"
                    if (!this.calibrating)
                    {
                        startPlaneDefinition();

                        this.contentButton4 = "Abbruch";
                        this.contentButton5 = "Start";
                        this.button5.Visibility = Visibility.Hidden;
                        updateButtons();
                    }
                    break;
                case Headline.ExhibitionPlaneVal: //"start validation of exhibition plane"
                    if (!this.calibrating)
                    {
                        startPlaneDefinition();

                        this.contentButton4 = "Abbruch";
                        this.button5.Visibility = Visibility.Hidden;
                        updateButtons();
                    }
                    break;
                case Headline.ExhibitionPlaneDone: //"abort validation of exhibition plane"
                    stopTracking();
                    this.exhibition = new Exhibition(this.TMP_NAME, this.TMP_EXHIBITION_PLANE);

                    MessageBox.Show("Bitte stellen Sie umgehend die Benutzerposition ein.");
                    
                    this.contentLabel1 = this.exhibition.getName().ToUpper();
                    this.contentButton4 = "Einstellungen";
                    this.contentButton5 = "schließen";
                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case Headline.NewName: //"safe name and continue to next view"
                    if (this.exhibition == null) // New exhibition
                    {
                        this.TMP_NAME = this.textBox2.Text;

                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENBESTIMMUNG";
                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBITION_PLANE;
                        this.contentButton1 = "laden";
                        this.contentButton2 = "bestimmen";
                        this.contentButton4 = "zurück";
                        this.contentButton5 = "OK";
                        this.headline = Headline.ExhibitionPlane;
                    }
                    else // New exhibit
                    {
                        startTracking();

                        this.TMP_NAME = this.textBox2.Text;

                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSBESTIMMUNG";
                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBIT;
                        this.contentButton4 = "zurück";
                        this.contentButton5 = "Start";
                        this.headline = Headline.ExhibitDef;
                    }
                    updateLayout();
                    break;
                case Headline.ExhibitDef: //"start definition of an exhibit or the user position"
                    if (!this.calibrating && this.setting == Setting.None)
                    {
                        startPositionDefinition();

                        this.contentButton4 = "Abbruch";
                        this.button5.Visibility = Visibility.Hidden;
                        updateButtons();
                    }
                    else if (!this.calibrating && this.setting == Setting.UserPosition)
                    {
                        startUserPositionDefinition();

                        this.contentButton4 = "Abbruch";
                        this.button5.Visibility = Visibility.Hidden;
                        updateButtons();
                    }
                    break;
                case Headline.ExhibitVal: //"start validation of an exhibit or accept user position"
                    if (!this.calibrating && this.setting == Setting.None) // Validation
                    {
                        startPositionDefinition();

                        this.contentButton4 = "Abbruch";
                        this.button5.Visibility = Visibility.Hidden;
                        updateButtons();
                    }
                    else // Acceptance of user position
                    {
                        stopTracking();

                        this.contentLabel1 = this.exhibition.getName() + " - EINSTELLUNGEN";
                        this.contentButton5 = "OK";
                        this.headline = Headline.ExhibitionSettings;
                        updateLayout();
                    }
                    break;
                case Headline.ExhibitDone: //"abort validation of exhibition plane"        
                    if (this.setting == Setting.UserPosition)
                    {
                        MessageBox.Show("Bestimmung der Benutzerposition (nicht) erfolgreich");
                        this.exhibition.setUserPosition(new Point3D(1, 1, 1));

                        this.contentLabel1 = this.exhibition.getName() + " - EINSTELLUNGEN";
                        this.contentButton5 = "OK";
                        this.headline = Headline.ExhibitionSettings;                   
                    }
                    else if (this.setting == Setting.Position)
                    {
                        MessageBox.Show("Neubestimmung der Exponatposition (nicht) erfolgreich");
                        this.TMP_EXHIBIT.setPosition(new Point3D(1, 1, 1));

                        this.contentLabel1 = this.TMP_EXHIBIT.getName() + " - EINSTELLUNGEN";
                        this.headline = Headline.ExhibitSettings;
                    }
                    else //((int)this.setting == 0) //New Exhibit: Settings.None
                    {
                        Point3D position = this.geometryHandler.getCenter(this.TMP_POSITION, this.TMP_POSITION_2);
                        this.TMP_EXHIBIT = new Exhibit(this.TMP_NAME, position);

                        this.contentLabel1 = this.TMP_EXHIBIT.getName();
                        this.contentTextBox1 = this.TMP_EXHIBIT.getDescription();
                        this.headline = Headline.EditExhibit;
                    }

                    this.setting = Setting.None;
                    updateLayout();
                    break;
                case Headline.ExhibitionSettings: //"safe and go back to exhibition"
                    if (this.exhibition.getPath() != null) // Config-file already exists
                    {
                        this.fileHandler.saveExhibition(this.exhibition);
                    }

                    this.contentLabel1 = this.exhibition.getName().ToUpper();
                    this.contentButton4 = "Einstellungen";
                    this.contentButton5 = "schließen";
                    this.headline = Headline.Exhibition;
                    this.setting = Setting.None;
                    updateLayout();
                    break;
                case Headline.ExhibitSettings: //"go to exhibit"
                    if (this.TMP_EXHIBIT.getPath() != null) // Config-file already exists
                    {
                        this.fileHandler.saveExhibit(this.TMP_EXHIBIT);
                    } 

                    this.headline = Headline.EditExhibit;
                    this.setting = Setting.None;
                    updateLayout();
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch (this.headline)
            {
                case Headline.Start: //"hidden"
                    break;
                case Headline.Exhibition: //"hidden"
                    break;
                case Headline.LoadExhibit: //"hidden"
                    break;
                case Headline.NewExhibit: //"hidden"
                    break;
                case Headline.EditExhibit: //"update itself"
                    this.contentTextBox1 = this.textBox1.Text;
                    break;
                case Headline.ExhibitionPlane: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDef: //"hidden"
                    break;
                case Headline.ExhibitionPlaneVal: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDone: //"hidden"
                    break;
                case Headline.NewName: //"hidden"
                    break;
                case Headline.ExhibitDef: //"hidden"
                    break;
                case Headline.ExhibitVal: //"hidden"
                    break;
                case Headline.ExhibitDone: //"hidden"
                    break;
                case Headline.ExhibitionSettings: //"hidden"
                    break;
                case Headline.ExhibitSettings: //"hidden"
                    break;
                default:
                    break;
            }
        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch (this.headline)
            {
                case Headline.Start: //"hidden"
                    break;
                case Headline.Exhibition: //"hidden"
                    break;
                case Headline.LoadExhibit: //"hidden"
                    break;
                case Headline.NewExhibit: //"hidden"
                    break;
                case Headline.EditExhibit: //"hidden"
                    break;
                case Headline.ExhibitionPlane: //"hidden"
                    break;
                case Headline.ExhibitionPlaneDef: //"hidden"
                    break;
                case Headline.ExhibitionPlaneVal:// "hidden"
                    break;
                case Headline.ExhibitionPlaneDone: //"hidden"
                    break;
                case Headline.NewName: //"Enter Name of Exhibition / Exhibit"
                    if (this.textBox2.Text != "") // There is some input
                    {
                        this.button5.Visibility = Visibility.Visible; // Show "OK"-button
                    }
                    else
                    {
                        this.button5.Visibility = Visibility.Hidden; // Hide "OK"-button
                    }
                    break;
                case Headline.ExhibitDef: //"hidden"
                    break;
                case Headline.ExhibitVal: //"hidden"
                    break;
                case Headline.ExhibitDone: //"hidden"
                    break;
                case Headline.ExhibitionSettings: //"hidden"
                    break;
                case Headline.ExhibitSettings: //"hidden"
                    break;
                default:
                    break;
            }
        }

        void saveTextDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.TMP_PATH = this.saveTextDialog.FileName;
        }

        void saveConfigDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.TMP_PATH = this.saveConfigDialog.FileName;
        }

        void loadImageDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.TMP_PATH = this.loadImageDialog.FileName;
        }

        void loadTextDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.TMP_PATH = this.loadTextDialog.FileName;
        }

        void loadConfigDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.TMP_PATH = this.loadConfigDialog.FileName;
        }
        #endregion

        #region ATTRIBUTES
        // Less is less applies for times
        private int detLessIsLess(double defaultValue, double value)
        { 
            if (value == (defaultValue * this.LOW)) // 50% of default value
                return 0;
            else if (value == defaultValue)
                return 1;
            else if (value == (defaultValue * this.HIGH)) // 150% of default value
                return 2;
            else // Any other value
                return -1;
        }

        private int detLessIsLess(int defaultValue, int value)
        {
            if (value == (defaultValue * this.LOW)) // 50% of default value
                return 0;
            else if (value == defaultValue)
                return 1;
            else if (value == (defaultValue * this.HIGH)) // 150% of default value
                return 2;
            else // Any other value
                return -1;
        }

        // Less is more applies for thresholds
        private int detLessIsMore(double defaultValue, double value)
        {
            if (value == (defaultValue * this.HIGH)) // 150% of default value
                return 0;
            else if (value == defaultValue)
                return 1;
            else if (value == (defaultValue * this.LOW)) // 50% of default value
                return 2;
            else // Any other value
                return -1;
        }

        private int detLessIsMore(int defaultValue, int value)
        {
            if (value == (defaultValue * this.HIGH)) // 150% of default value
                return 0;
            else if (value == defaultValue)
                return 1;
            else if (value == (defaultValue * this.LOW)) // 50% of default value
                return 2;
            else // Any other value
                return -1;
        }

        private void setAttribute()
        {
            // EMPTY INSTANCES ARE FOR DEFAULT USE ONLY ! ! !
            Exhibition DEFAULT_EXHIBITION = new Exhibition();
            Exhibit DEFAULT_EXHIBIT = new Exhibit();
            
            double factor = 1.0;
            if (this.comboBox2.SelectedIndex == 0) //Low
            {
                factor = this.LOW;
            }
            else if (this.comboBox2.SelectedIndex == 2) //High
            {
                factor = this.HIGH;
            }

            switch (this.setting)
            {
                case Setting.Threshold:
                    if (factor != 1.0)
                        this.exhibition.setThreshold(DEFAULT_EXHIBITION.getThreshold() * ((this.LOW + this.HIGH) - factor));
                    else
                        this.exhibition.setThreshold(DEFAULT_EXHIBITION.getThreshold());
                    break;
                case Setting.SelectionTime:
                    if (factor != 1.0)
                        this.exhibition.setSelectionTime(DEFAULT_EXHIBITION.getSelectionTime() * factor);
                    else
                        this.exhibition.setSelectionTime(DEFAULT_EXHIBITION.getSelectionTime());
                    break;
                case Setting.LockTime:
                    if (factor != 1.0)
                        this.exhibition.setLockTime(DEFAULT_EXHIBITION.getLockTime() * factor);
                    else
                        this.exhibition.setLockTime(DEFAULT_EXHIBITION.getLockTime());
                    break;
                case Setting.SlideTime:
                    if (factor != 1.0)
                        this.exhibition.setSlideTime(DEFAULT_EXHIBITION.getSlideTime() * factor);
                    else
                        this.exhibition.setSlideTime(DEFAULT_EXHIBITION.getSlideTime());
                    break;
                case Setting.EndWait:
                    if (factor != 1.0)
                        this.exhibition.setEndWait(DEFAULT_EXHIBITION.getEndWait() * factor);
                    else
                        this.exhibition.setEndWait(DEFAULT_EXHIBITION.getEndWait());
                    break;
                case Setting.KernelSize:
                    if (factor != 1.0)
                        this.TMP_EXHIBIT.setKernelSize(DEFAULT_EXHIBIT.getKernelSize() * factor);
                    else
                        this.TMP_EXHIBIT.setKernelSize(DEFAULT_EXHIBIT.getKernelSize());
                    break;
                case Setting.KernelWeight:
                    if (factor != 1.0)
                        this.TMP_EXHIBIT.setKernelWeight(DEFAULT_EXHIBIT.getKernelWeight() * factor);
                    else
                        this.TMP_EXHIBIT.setKernelWeight(DEFAULT_EXHIBIT.getKernelWeight());
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region DEFINITION AND VALIDATION
        private void definePlane()
        {
            this.calibrationHandler = new CalibrationHandler(this.SAMPLING_VECTORS); // Initiate calibrator
            List<GeometryHandler.Vector> vectors = sampleVectors(this.SAMPLING_POINTS, this.SAMPLING_POSITIONS, this.SAMPLING_VECTORS, 2); // Sampled vectors
            List<Point3D> corners = this.calibrationHandler.definePlane(vectors, this.SAMPLING_POSITIONS, 2); // Calibration-points
            
            switch (this.headline)
            {
                case Headline.ExhibitionPlaneDef: 
                    this.TMP_EXHIBITION_PLANE = new GeometryHandler.Plane(corners);

                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENVALIDIERUNG";
                    this.contentLabel2 = this.INSTRUCTIONS_EXHIBITION_PLANE;
                    this.contentButton4 = "zurück";
                    this.contentButton5 = "Start";
                    this.headline = Headline.ExhibitionPlaneVal;
            
                    stopCalibration();
                    break;
                case Headline.ExhibitionPlaneVal:
                    this.TMP_EXHIBITION_PLANE_2 = new GeometryHandler.Plane(corners);

                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENBESTIMMUNG";
                    if (this.calibrationHandler.validatePlane(this.TMP_EXHIBITION_PLANE, this.TMP_EXHIBITION_PLANE_2))
                    {
                        this.exhibition = new Exhibition(this.TMP_NAME, this.calibrationHandler.makePlane(this.TMP_EXHIBITION_PLANE, this.TMP_EXHIBITION_PLANE_2));

                        this.contentLabel2 = "Eckpunkte erfolgreich validiert.";
                        this.contentButton4 = "zurück";
                        this.contentButton5 = "OK";
                        this.headline = Headline.ExhibitionPlaneDone;

                        stopCalibration();
                        stopTracking();
                    }
                    else
                    {
                        this.contentLabel2 = "Eckpunkte konnten nicht validiert werden." + '\n' + '\n' + "Erneut definieren?";
                        this.contentButton4 = "zurück";
                        this.contentButton5 = "OK";
                        this.headline = Headline.ExhibitionPlaneDef;

                        stopCalibration();
                    }

                    break;
                default:
                    MessageBox.Show("definePlane()-Problem!");
                    break;
            }
        }

        private void definePosition()
        {
            this.calibrationHandler = new CalibrationHandler(this.SAMPLING_VECTORS, this.exhibition.getThreshold()); // Initiate calibrator
            List<GeometryHandler.Vector> vectors = sampleVectors(1, this.SAMPLING_POSITIONS, this.SAMPLING_VECTORS, 2); // Sampled vectors
            Point3D position = this.calibrationHandler.definePosition(this.exhibition.getExhibitionPlane(), vectors, this.SAMPLING_POSITIONS, 2); // Calibration-points

            switch (this.setting)
            { 
                case Setting.None: // Defining of validating exhibit's position
                    if (this.headline == Headline.ExhibitDef)
                    { 
                        this.TMP_POSITION = position;
                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSVALIDIERUNG";
                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBIT;
                        this.contentButton4 = "zurück";
                        this.contentButton5 = "Start";
                        this.headline = Headline.ExhibitVal;

                        stopCalibration();
                    }
                    else if (this.headline == Headline.ExhibitVal)
                    {
                        this.TMP_POSITION_2 = position;
                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSBESTIMMUNG";
                        if (this.calibrationHandler.validatePoint(this.TMP_POSITION, this.TMP_POSITION_2) == 3) // All three axis are within threshold
                        {
                            this.contentLabel2 = "Position erfolgreich validiert.";
                            this.contentButton4 = "zurück";
                            this.contentButton5 = "OK";
                            this.headline = Headline.ExhibitDone;

                            stopCalibration();
                            stopTracking();
                        }
                        else
                        {
                            this.contentLabel2 = "Position konnte nicht validiert werden." + '\n' + '\n' + "Erneut definieren?";
                            this.contentButton4 = "zurück";
                            this.contentButton5 = "OK";
                            this.headline = Headline.ExhibitDef;

                            stopCalibration();
                        }
                    }
                    else
                    {
                        MessageBox.Show("definePosition()-Problem: Setting.None, Headline?");

                        stopCalibration();
                        stopTracking();
                    }
                    break;
                case Setting.Position: // Re-defining exhibit's position
                    if (this.headline == Headline.ExhibitDef)
                    {
                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSVALIDIERUNG";
                        this.contentLabel2 = this.INSTRUCTIONS_EXHIBIT;
                        this.contentButton4 = "zurück";
                        this.contentButton5 = "Start";
                        this.headline = Headline.ExhibitVal;

                        stopCalibration();
                    }
                    else if (this.headline == Headline.ExhibitVal)
                    {
                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EINSTELLUNGEN";
                        this.headline = Headline.ExhibitSettings;

                        stopCalibration();
                        stopTracking();
                    }
                    else
                    {
                        MessageBox.Show("definePosition()-Problem: Setting.Position, Headline?");

                        stopCalibration();
                        stopTracking();
                    }
                    break;
                default:
                    break;
            }
        }

        private void defineUserPosition()
        {
            this.exhibition.setUserPosition(sampleUserPosition());

            this.contentLabel2 = "Benutzerposition erfolgreich bestimmt."; // "User position defined successfully."
            this.contentButton5 = "OK";
            this.headline = Headline.ExhibitVal;
            
            stopCalibration();
        }

        private List<GeometryHandler.Vector> sampleVectors(int points, int positions, int samplesPerPosition, int returnMode) // Amounts of points to define, positions to point from (at least 2!), samples per position and return mode: 0 = only pointing-samples, 1 = only aiming-samples, 2 = both samples
        {
            List<GeometryHandler.Vector> allVectors = new List<GeometryHandler.Vector>();
            List<GeometryHandler.Vector> pointingVectors = new List<GeometryHandler.Vector>();
            List<GeometryHandler.Vector> aimingVectors = new List<GeometryHandler.Vector>();
            int samplingInterval = 1000 / samplesPerPosition; // 100ms break between samples

            for (int position = 0; position != positions; ++position) // For each corner
            {
                // Position to go to
                this.contentLabel2 = "Begeben Sie sich auf die " + (position + 1) + ". von " + positions + " Position."; // Change to position #
                Thread.Sleep(this.SAMPLING_BREAK);

                for (int point = 0; point != points; ++point) // For each position
                {
                    // Position to point at
                    if (points == 1) // Define position
                    {
                        this.contentLabel2 = "Zeigen Sie auf das Exponat."; // Point to corner #
                    }
                    else
                    {
                        this.contentLabel2 = " Zeigen Sie auf die " + (point + 1) + ". Ecke."; // Point to corner #
                    }
                    Thread.Sleep(this.SAMPLING_BREAK);

                    for (int sample = 0; sample != samplesPerPosition; ++sample)
                    {
                        this.contentLabel2 = "Messung: Nicht bewegen!"; // "Measurment: Do not move!"
                        allVectors = takeSample(); // Take poining- and aiming sample simultaniously
                        pointingVectors.Add(allVectors[0]); // Add pointing-vector to pointing-vectors
                        aimingVectors.Add(allVectors[1]); // Add aiming-vector to aiming-vectors
                        Thread.Sleep(samplingInterval); // Wait for [samplingInterval]ms
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
                    foreach (GeometryHandler.Vector pointingVector in pointingVectors) // Add all pointing-vectors
                    {
                        allVectors.Add(pointingVector);
                    }
                    foreach (GeometryHandler.Vector aimingVector in aimingVectors) // Add all aiming-vectors
                    {
                        allVectors.Add(aimingVector);
                    }
                    return allVectors;
                default:
                    return allVectors;
            }
        }

        private Point3D sampleUserPosition()
        {
            List<Point3D> userPositions = new List<Point3D>();
            Point3D userPosition = new Point3D();
            int samplingInterval = 2500 / this.SAMPLING_VECTORS; // 250ms break between samples

            this.contentLabel2 = "Bitte begeben Sie sich auf die Benutzerposition."; // Move to user position
            Thread.Sleep(this.SAMPLING_BREAK);

            for (int sample = 0; sample != this.SAMPLING_VECTORS; ++sample)
            {
                this.contentLabel2 = "Messung: Hin und her bewegen!"; // "Measurement: Move about!"
                userPositions.Add(takeUserHipSample()); // Add hip position
                Thread.Sleep(samplingInterval); // Wait for [samplingInterval]ms
            }

            userPosition = this.geometryHandler.getCenter(userPositions);

            return userPosition;
        }

        private List<GeometryHandler.Vector> takeSample()
        {
            List<GeometryHandler.Vector> vectors = new List<GeometryHandler.Vector>();

            vectors.Add(takePointingSample());
            vectors.Add(takeAimingSample());
            
            return vectors;
        }

        private GeometryHandler.Vector takePointingSample()
        {
            GeometryHandler.Vector vector = new GeometryHandler.Vector();

            if (trackableUser(Fubi.getClosestUserID()))
            {
                while (!this.geometryHandler.vectorOK(vector))
                {
                    vector.reset(jointsToTrack[0], jointsToTrack[1]); // (RIGHT_ELBOW, RIGHT_HAND)
                }
            }
            else
            {
                Thread.Sleep(10); // Wait for 10ms
            }

            return vector;
        }

        private GeometryHandler.Vector takeAimingSample()
        {
            GeometryHandler.Vector vector = new GeometryHandler.Vector();

            if (trackableUser(Fubi.getClosestUserID()))
            {
                while (!this.geometryHandler.vectorOK(vector))
                {
                    vector.reset(jointsToTrack[4], jointsToTrack[1]); // (HEAD, RIGHT_HAND)
                }
            }
            else
            {
                Thread.Sleep(10); // Wait for 10ms
            }

            return vector;
        }

        private Point3D takeUserHipSample()
        {
            float x, y, z;
            Point3D hipLeft = new Point3D();
            Point3D hipRight = new Point3D();

            if (trackableUser(Fubi.getClosestUserID()))
            {
                // Track left hip
                Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID(), FubiUtils.SkeletonJoint.LEFT_HIP, out x, out y, out z, out confidence, out timestamp);
                hipLeft = new Point3D((double)x, (double)y, (double)z);
                // Track right hip
                Fubi.getCurrentSkeletonJointPosition(Fubi.getClosestUserID(), FubiUtils.SkeletonJoint.RIGHT_HIP, out x, out y, out z, out confidence, out timestamp);
                hipRight = new Point3D((double)x, (double)y, (double)z);
            }
            else
            {
                Thread.Sleep(10); // Wait for 10ms
            }

            // Return center of the hip
            return this.geometryHandler.getCenter(hipLeft, hipRight);
        }
        #endregion

        private void closeAllThreads()
        {

            if (calibrating)
            {
                stopCalibration();
            }
            if (tracking)
            {
                stopTracking();
            }
            this.Close();
        }
    }
}
