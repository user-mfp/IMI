using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Threading;
using System.Data;
using System.Globalization;

using FubiNET;

namespace Fubi_WPF_GUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static byte[] buffer = new byte[4 * 640 * 480];

        private Thread fubiThread;
        private bool running = false;

        bool clearRecognizersOnNextUpdate = false;
        bool switchSensorOnNextUpdate = false;
        bool switchFingerSensorOnNextUpdate = false;
        
        private bool controlMouse = false;

        private Dictionary<uint, Dictionary<uint, bool>> currentGestures = new Dictionary<uint, Dictionary<uint, bool>>();
        private Dictionary<uint, Dictionary<uint, bool>> currentFingerGestures = new Dictionary<uint, Dictionary<uint, bool>>();
        private Dictionary<uint, Dictionary<uint, bool>> currentPredefinedGestures = new Dictionary<uint, Dictionary<uint, bool>>();

        private FubiMouse FubiMouse = new FubiMouse();
        KeyboardListener kListener = new KeyboardListener();

        private delegate void NoArgDelegate();

        private DataTable jointsToRenderDT = new DataTable();

        private RecognizerStatsWindow statsWindow = null;

        private Microsoft.Win32.OpenFileDialog openRecDlg = null;

        public MainWindow()
        {
            // Set culuture to have a common number format, ...
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            InitializeComponent();

            string[] mods = Enum.GetNames(typeof(FubiUtils.DepthImageModification));
            string selectedName = Enum.GetName(typeof(FubiUtils.DepthImageModification), FubiUtils.DepthImageModification.UseHistogram);
            foreach (string mod in mods)
            {
                int index = depthModComboBox.Items.Add(mod);
                if (index > -1 && mod == selectedName)
                {
                    depthModComboBox.SelectedIndex = index;
                }
            }

            string[] imageTypes = Enum.GetNames(typeof(FubiUtils.ImageType));
            string selectedType = Enum.GetName(typeof(FubiUtils.ImageType), FubiUtils.ImageType.Depth);
            foreach (string imageType in imageTypes)
            {
                int index = imageStreamComboBox.Items.Add(imageType);
                if (index > -1 && imageType == selectedType)
                {
                    imageStreamComboBox.SelectedIndex = index;
                }
            }

            jointsToRenderDT.Columns.Add("Name", typeof(string));
            jointsToRenderDT.Columns.Add("IsChecked", typeof(bool));
            string[] jointTypes = Enum.GetNames(typeof(FubiUtils.JointsToRender));
            foreach (string jointType in jointTypes)
            {
                jointsToRenderDT.Rows.Add(jointType, true);
            }
            jointsToRenderCB.DataContext = jointsToRenderDT;

            fubiThread = new Thread(fubiMain);
            running = true;
            fubiThread.Start();
            kListener.KeyUp += new RawKeyEventHandler(this.keyPressedHandler);
        }

        private void initFubi()
        {
            List<string> availableSensors = new List<string>();
            availableSensors.Add(Enum.GetName(typeof(FubiUtils.SensorType), FubiUtils.SensorType.NONE));
            FubiUtils.SensorType type = FubiUtils.SensorType.NONE;
            int avSensors = Fubi.getAvailableSensors();
            if ((avSensors & (int)FubiUtils.SensorType.OPENNI2) != 0)
            {
                type = FubiUtils.SensorType.OPENNI2;
                availableSensors.Add(Enum.GetName(typeof(FubiUtils.SensorType), type));
            }
            if ((avSensors & (int)FubiUtils.SensorType.KINECTSDK) != 0)
            {
                if (type == FubiUtils.SensorType.NONE)
                    type = FubiUtils.SensorType.KINECTSDK;
                availableSensors.Add(Enum.GetName(typeof(FubiUtils.SensorType), FubiUtils.SensorType.KINECTSDK));
            }
            if ((avSensors & (int)FubiUtils.SensorType.OPENNI1) != 0)
            {
                if (type == FubiUtils.SensorType.NONE)
                    type = FubiUtils.SensorType.OPENNI1;
                availableSensors.Add(Enum.GetName(typeof(FubiUtils.SensorType), FubiUtils.SensorType.OPENNI1));
            }
            string selectedName = Enum.GetName(typeof(FubiUtils.SensorType), type);
            foreach (string sType in availableSensors)
            {
                int index = sensorSelectionComboBox.Items.Add(sType);
                if (index > -1 && sType == selectedName)
                {
                    sensorSelectionComboBox.SelectedIndex = index;
                }
            }
            switchSensorOnNextUpdate = false;

            List<string> availableFingerSensors = new List<string>();
            string selectedFingerSName = Enum.GetName(typeof(FubiUtils.FingerSensorType), FubiUtils.FingerSensorType.NONE);
            availableFingerSensors.Add(selectedFingerSName);
            int avFSensors = Fubi.getAvailableFingerSensorTypes();
            FubiUtils.FingerSensorType fType = FubiUtils.FingerSensorType.NONE;
            if ((avFSensors & (int)FubiUtils.FingerSensorType.LEAP) != 0)
            {
                string name = Enum.GetName(typeof(FubiUtils.FingerSensorType), FubiUtils.FingerSensorType.LEAP);
                if (type == FubiUtils.SensorType.NONE)
                {
                    selectedFingerSName = name;
                    fType = FubiUtils.FingerSensorType.LEAP;
                }
                availableFingerSensors.Add(name);
            }
            foreach (string sType in availableFingerSensors)
            {
                int index = fingerSensorComboBox.Items.Add(sType);
                if (index > -1 && sType == selectedFingerSName)
                {
                    fingerSensorComboBox.SelectedIndex = index;
                }
            }
            switchFingerSensorOnNextUpdate = false;            
                

            if (!Fubi.init(new FubiUtils.SensorOptions(new FubiUtils.StreamOptions(640, 480, 30), new FubiUtils.StreamOptions(640, 480), new FubiUtils.StreamOptions(-1, -1, -1), type, FubiUtils.SkeletonProfile.ALL), new FubiUtils.FilterOptions()))
            {
                type = FubiUtils.SensorType.NONE;
                Fubi.init(new FubiUtils.SensorOptions(new FubiUtils.StreamOptions(640, 480, 30), new FubiUtils.StreamOptions(640, 480), new FubiUtils.StreamOptions(-1), type, FubiUtils.SkeletonProfile.ALL), new FubiUtils.FilterOptions());
            }

            if (fType != FubiUtils.FingerSensorType.NONE)
                Fubi.initFingerSensor(fType);

            if (type == FubiUtils.SensorType.OPENNI1)
                button4.IsEnabled = true;
            else
                button4.IsEnabled = false;

            float minCutOff, velCutOff, slope;
            Fubi.getFilterOptions(out minCutOff, out velCutOff, out slope);
            minCutOffControl.Value = (int)minCutOff;
            cutOffSlopeControl.Value = slope;

            Fubi.loadRecognizersFromXML("MouseControlRecognizers.xml");
        }

        private void releaseFubi()
        {
            Fubi.release();
        }

        private void updateFubi()
        {
            if (clearRecognizersOnNextUpdate)
            {
                Fubi.clearUserDefinedRecognizers();
                if (Fubi.getNumUserDefinedCombinationRecognizers() == 0 && Fubi.getNumUserDefinedRecognizers() == 0)
                {
                    button3.IsEnabled = false;
                }
                clearRecognizersOnNextUpdate = false;
            }

            FubiUtils.ImageType streamType = (FubiUtils.ImageType)Enum.Parse(typeof(FubiUtils.ImageType), imageStreamComboBox.SelectedItem.ToString());

            if (switchSensorOnNextUpdate)
            {
                FubiUtils.SensorType newType = (FubiUtils.SensorType)Enum.Parse(typeof(FubiUtils.SensorType), sensorSelectionComboBox.SelectedItem.ToString());

                FubiUtils.StreamOptions rgbOptions = (streamType == FubiUtils.ImageType.IR) ? new FubiUtils.StreamOptions(-1, -1, -1) : new FubiUtils.StreamOptions();
                FubiUtils.StreamOptions irOptions = (streamType == FubiUtils.ImageType.IR) ? new FubiUtils.StreamOptions() : new FubiUtils.StreamOptions(-1, -1, -1);
                if (!Fubi.switchSensor(new FubiUtils.SensorOptions(new FubiUtils.StreamOptions(), rgbOptions, irOptions, newType, FubiUtils.SkeletonProfile.ALL, true, registerStreams_checkBox.IsChecked == true)))
                {
                    newType = FubiUtils.SensorType.NONE;
                    MessageBox.Show(this, "Error starting sensor! \nDid you connect the sensor and install the correct driver? \nTry selecting a different sensor.",
                        "Error starting sensor", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (newType == FubiUtils.SensorType.OPENNI1)
                    button4.IsEnabled = true;
                else
                    button4.IsEnabled = false;

                switchSensorOnNextUpdate = false;
            }

            if (switchFingerSensorOnNextUpdate)
            {
                FubiUtils.FingerSensorType newfType = (FubiUtils.FingerSensorType)Enum.Parse(typeof(FubiUtils.FingerSensorType), fingerSensorComboBox.SelectedItem.ToString());
                if (!Fubi.initFingerSensor(newfType))
                {
                    newfType = FubiUtils.FingerSensorType.NONE;
                    MessageBox.Show(this, "Error starting finger sensor! \nDid you connect the sensor and install the correct driver?",
                        "Error starting finger sensor", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (Fubi.getCurrentSensorType() == FubiUtils.SensorType.NONE && newfType == FubiUtils.FingerSensorType.LEAP)
                {
                    // For leap we automatically select a blank stream if no other sensor is present
                    string blankName = Enum.GetName(typeof(FubiUtils.ImageType), FubiUtils.ImageType.Blank);
                    imageStreamComboBox.SelectedItem = blankName;
                    streamType = (FubiUtils.ImageType)Enum.Parse(typeof(FubiUtils.ImageType), blankName);
                }
                switchFingerSensorOnNextUpdate = false;
            }

            label1.Content = "User Count: " + Fubi.getNumUsers().ToString() + " - Hand Count: " + Fubi.getNumHands();

            // Update Fubi and get the debug image
            int width = 0, height = 0;
            FubiUtils.ImageNumChannels channels = FubiUtils.ImageNumChannels.C4;
            
            int renderOptions = 0;
            if (shapeCheckBox.IsChecked == true)
                renderOptions |= (int)FubiUtils.RenderOptions.Shapes;
            if (skeletonCheckBox.IsChecked == true)
                renderOptions |= (int)FubiUtils.RenderOptions.Skeletons;
            if (userCaptionscheckBox.IsChecked == true)
                renderOptions |= (int)FubiUtils.RenderOptions.UserCaptions;
            if (backgroundCheckBox.IsChecked == true)
                renderOptions |= (int)FubiUtils.RenderOptions.Background;
            if (swapRAndBcheckBox.IsChecked == true)
                renderOptions |= (int)FubiUtils.RenderOptions.SwapRAndB;
            if (fingerShapecheckBox.IsChecked == true)
                renderOptions |= (int)FubiUtils.RenderOptions.FingerShapes;
            if (detailedFaceCheckBox.IsChecked == true)
                renderOptions |= (int)FubiUtils.RenderOptions.DetailedFaceShapes;
            if (bodyMeasuresCheckBox.IsChecked == true)
                renderOptions |= (int)FubiUtils.RenderOptions.BodyMeasurements;

            if (orientRadioButton.IsChecked == true)
            {
                if (globalRadioButton.IsChecked == true)
                {
                    renderOptions |= (int)FubiUtils.RenderOptions.GlobalOrientCaptions;
                }
                else
                {
                    renderOptions |= (int)FubiUtils.RenderOptions.LocalOrientCaptions;
                }
            }
            else if (posRadioButton.IsChecked == true)
            {
                if (globalRadioButton.IsChecked == true)
                {
                    renderOptions |= (int)FubiUtils.RenderOptions.GlobalPosCaptions;
                }
                else
                {
                    renderOptions |= (int)FubiUtils.RenderOptions.LocalPosCaptions;
                }
            }
            if (filteredRadioButton.IsChecked == true)
                renderOptions |= (int) FubiUtils.RenderOptions.UseFilteredValues;

            int jointsToRender = 0;
            var query = from r in jointsToRenderDT.AsEnumerable()
                        where r.Field<bool>("IsChecked") == true
                        select new
                        {
                            Name = r["Name"]
                        };
            foreach (var r in query)
            {
                FubiUtils.JointsToRender selection = (FubiUtils.JointsToRender) Enum.Parse(typeof(FubiUtils.JointsToRender), r.Name.ToString());
                if (selection != FubiUtils.JointsToRender.ALL_JOINTS)
                    jointsToRender |= (int) selection;
            }

            FubiUtils.DepthImageModification depthMods = (FubiUtils.DepthImageModification) Enum.Parse(typeof(FubiUtils.DepthImageModification), depthModComboBox.SelectedItem.ToString(), true);


            if (streamType == FubiUtils.ImageType.Color)
            {
                Fubi.getRgbResolution(out width, out height);
                channels = FubiUtils.ImageNumChannels.C3;
            }
            else if (streamType == FubiUtils.ImageType.IR)
            {
                Fubi.getIRResolution(out width, out height);
                channels = FubiUtils.ImageNumChannels.C3;
            }
            else if (streamType == FubiUtils.ImageType.Blank)
            {
                width = 640; height = 480;
                channels = FubiUtils.ImageNumChannels.C4;
            }
            else
            {
                Fubi.getDepthResolution(out width, out height);
                channels = FubiUtils.ImageNumChannels.C4;
            }
            
            // Display the debug image
            if (width > 0 && height > 0)
            {
                WriteableBitmap wb = image1.Source as WriteableBitmap;
                if (wb != null && (wb.Width != width || wb.Height != height || wb.Format.BitsPerPixel != (int)channels * 8))
                {
                    wb = null;
                    buffer = new byte[(int)channels * width * height];
                }
                if (wb == null)
                {
                    PixelFormat format = PixelFormats.Bgra32;
                    if (channels == FubiUtils.ImageNumChannels.C3)
                        format = PixelFormats.Rgb24;
                    else if (channels == FubiUtils.ImageNumChannels.C1)
                        format = PixelFormats.Gray8;
                    wb = new WriteableBitmap(width, height, 0, 0, format, BitmapPalettes.Gray256);
                    image1.Source = wb;
                }

                if (streamType == FubiUtils.ImageType.Blank)
                {
                    // Special case: reset the array, as this one only adds the tracking info
                    for (int i = 0; i < (int)channels * width * height; ++i)
                        buffer[i] = 0;
                }

                Fubi.getImage(buffer, streamType, channels, FubiUtils.ImageDepth.D8, renderOptions, jointsToRender, depthMods);

                int stride = wb.PixelWidth * (wb.Format.BitsPerPixel / 8);
                wb.WritePixels(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight), buffer, stride, 0);
            }
            
            //Check gestures for all users
            ushort numUsers = Fubi.getNumUsers();
            for (uint i = 0; i < numUsers; i++)
            {
                uint id = Fubi.getUserID(i);
                if (id > 0)
                {
                    bool changedSomething = false;
                    // Print gestures
                    if (checkBox1.IsChecked == true)
                    {
                        if (!currentGestures.ContainsKey(id))
                            currentGestures[id] = new Dictionary<uint, bool>();
                        // Only user defined gestures
                        for (uint p = 0; p < Fubi.getNumUserDefinedRecognizers(); ++p)
                        {
                            FubiUtils.RecognitionCorrectionHint correctionHint;
                            FubiUtils.RecognitionResult res = Fubi.recognizeGestureOn(p, id, out correctionHint);
                            if (res == FubiUtils.RecognitionResult.RECOGNIZED)
                            {
                                // Gesture recognized
                                if (!currentGestures[id].ContainsKey(p) || !currentGestures[id][p])
                                {
                                    // Gesture start
                                    textBox1.Text += "User" + id + ": START OF " + Fubi.getUserDefinedRecognizerName(p) + " -->\n";
                                    currentGestures[id][p] = true;
                                    changedSomething = true;
                                }
                            }
                            else if (currentGestures[id].ContainsKey(p) && currentGestures[id][p])
                            {
                                // Gesture end
                                textBox1.Text += "User" + id + ": --> END OF " + Fubi.getUserDefinedRecognizerName(p) + "\n";
                                currentGestures[id][p] = false;
                                changedSomething = true;
                            }
                            else if (res == FubiUtils.RecognitionResult.NOT_RECOGNIZED)
                            {
                                if (correctionHint.m_joint != FubiUtils.SkeletonJoint.NUM_JOINTS)
                                {
                                    string msg = FubiUtils.createCorrectionHintMsg(correctionHint);

                                    if (msg.Length > 0)
                                    {
                                        Console.Write(msg);
                                    }
                                }
                            }
                        }

                        if (PredefinedCheckBox.IsChecked == true)
                        {
                            if (!currentPredefinedGestures.ContainsKey(id))
                                currentPredefinedGestures[id] = new Dictionary<uint, bool>();

                            for (uint p = 0; p < (uint)FubiPredefinedGestures.Postures.NUM_POSTURES; ++p)
                            {
                                if (Fubi.recognizeGestureOn((FubiPredefinedGestures.Postures)p, id) == FubiUtils.RecognitionResult.RECOGNIZED)
                                {
                                    if (!currentPredefinedGestures[id].ContainsKey(p) || !currentPredefinedGestures[id][p])
                                    {
                                        // Gesture recognized
                                        textBox1.Text += "User" + id + ": START OF" + FubiPredefinedGestures.getPostureName((FubiPredefinedGestures.Postures)p) + "\n";
                                        currentPredefinedGestures[id][p] = true;
                                        changedSomething = true;
                                    }
                                }
                                else if (currentPredefinedGestures[id][p])
                                {
                                    textBox1.Text += "User" + id + ": --> END OF " + FubiPredefinedGestures.getPostureName((FubiPredefinedGestures.Postures)p) + "\n";
                                    currentPredefinedGestures[id][p] = false;
                                    changedSomething = true;
                                }
                            }
                        }

                        if (changedSomething)
                            textBox1.ScrollToEnd();
                    }

                    // Print combinations
                    for (uint pc = 0; pc < Fubi.getNumUserDefinedCombinationRecognizers(); ++pc)
                    {
                        // User defined combinations
                        string name = Fubi.getUserDefinedCombinationRecognizerName(pc);
                        FubiUtils.RecognitionCorrectionHint correctionHint;
                        FubiUtils.RecognitionResult res = Fubi.getCombinationRecognitionProgressOn(name, id, out correctionHint);
                        if (res == FubiUtils.RecognitionResult.RECOGNIZED)
                        {
                            // combination recognized
                            if (checkBox2.IsChecked == true)
                                textBox2.Text += "User" + id + ": " + name + "\n";
                            if (statsWindow != null)
                            {
                                if (statsWindow.recognitions.ContainsKey(id) == false)
                                    statsWindow.recognitions[id] = new Dictionary<string, double>();
                                statsWindow.recognitions[id][name] = Fubi.getCurrentTime();
                            }
                        }
                        else
                        {
                            if (statsWindow != null)
                            {
                                if (res == FubiUtils.RecognitionResult.NOT_RECOGNIZED && correctionHint.m_joint != FubiUtils.SkeletonJoint.NUM_JOINTS)
                                {
                                    string msg = FubiUtils.createCorrectionHintMsg(correctionHint);

                                    if (msg.Length > 0)
                                    {
                                        if (msg.EndsWith("\n"))
                                            msg = msg.Remove(msg.Length - 1);
                                        if (statsWindow.hints.ContainsKey(id) == false)
                                            statsWindow.hints[id] = new Dictionary<string, string>();
                                        statsWindow.hints[id][name] = msg;
                                    }
                                }
                                else
                                {
                                    if (statsWindow.hints.ContainsKey(id) == false)
                                        statsWindow.hints[id] = new Dictionary<string, string>();
                                    statsWindow.hints[id][name] = "";
                                }
                            }

                            Fubi.enableCombinationRecognition(Fubi.getUserDefinedCombinationRecognizerName(pc), id, true);
                        }
                    }

                    for (uint pc = 0; pc < (uint)FubiPredefinedGestures.Combinations.NUM_COMBINATIONS; ++pc)
                    {
                        if (checkBox2.IsChecked == true && PredefinedCheckBox.IsChecked == true)
                        {
                            if (Fubi.getCombinationRecognitionProgressOn((FubiPredefinedGestures.Combinations)pc, id) == FubiUtils.RecognitionResult.RECOGNIZED)
                            {
                                // Combination recognized
                                textBox2.Text += "User" + id + ": " + FubiPredefinedGestures.getCombinationName((FubiPredefinedGestures.Combinations)pc) + "\n";
                            }
                            else
                                Fubi.enableCombinationRecognition((FubiPredefinedGestures.Combinations)pc, id, true);
                        }
                    }
                    if (checkBox2.IsChecked == true)
                        textBox2.ScrollToEnd();
                }
            }

            //Now for all hands
            ushort numhands = Fubi.getNumHands();
            for (uint i = 0; i < numhands; i++)
            {
                uint id = Fubi.getHandID(i);
                if (id > 0)
                {
                    bool changedSomething = false;
                    // Print gestures
                    if (checkBox1.IsChecked == true)
                    {
                        if (!currentFingerGestures.ContainsKey(id))
                            currentFingerGestures[id] = new Dictionary<uint, bool>();

                        // Only user defined gestures
                        for (uint p = 0; p < Fubi.getNumUserDefinedRecognizers(); ++p)
                        {
                            string pName = Fubi.getUserDefinedRecognizerName(p);
                            if (Fubi.recognizeGestureOnHand(pName, id) == FubiUtils.RecognitionResult.RECOGNIZED)
                            {
                                // Gesture recognized
                                if (!currentFingerGestures.ContainsKey(p) || !currentFingerGestures[id][p])
                                {
                                    // Gesture start
                                    textBox1.Text += "Hand" + id + ": START OF " + pName  + " -->\n";
                                    currentFingerGestures[id][p] = true;
                                    changedSomething = true;
                                }
                            }
                            else if (currentFingerGestures[id].ContainsKey(p) && currentFingerGestures[id][p])
                            {
                                // Gesture end
                                textBox1.Text += "Hand" + id + ": --> END OF " + pName + "\n";
                                currentFingerGestures[id][p] = false;
                                changedSomething = true;
                            }
                        }

                        if (changedSomething)
                            textBox1.ScrollToEnd();
                    }

                    // Print combinations
                    for (uint pc = 0; pc < Fubi.getNumUserDefinedCombinationRecognizers(); ++pc)
                    {
                        // User defined gestures
                        string name = Fubi.getUserDefinedCombinationRecognizerName(pc);
                        FubiUtils.RecognitionCorrectionHint correctionHint;
                        FubiUtils.RecognitionResult res = Fubi.getCombinationRecognitionProgressOnHand(name, id, out correctionHint);
                        if (res == FubiUtils.RecognitionResult.RECOGNIZED)
                        {
                            // Combination recognized
                            if (checkBox2.IsChecked == true)
                                textBox2.Text += "Hand" + id + ": " + name + "\n";
                            if (statsWindow != null)
                            {
                                if (statsWindow.handRecognitions.ContainsKey(id) == false)
                                    statsWindow.handRecognitions[id] = new Dictionary<string, double>();
                                statsWindow.handRecognitions[id][name] = Fubi.getCurrentTime();
                            }
                        }
                        else
                        {
                            if (res == FubiUtils.RecognitionResult.NOT_RECOGNIZED && statsWindow != null && correctionHint.m_joint != FubiUtils.SkeletonJoint.NUM_JOINTS)
                            {
                                string msg = FubiUtils.createCorrectionHintMsg(correctionHint);

                                if (msg.Length > 0)
                                {
                                    if (statsWindow.handHints.ContainsKey(id) == false)
                                        statsWindow.handHints[id] = new Dictionary<string, string>();
                                    statsWindow.handHints[id][name] = msg;
                                }
                            }
                            Fubi.enableCombinationRecognitionHand(name, id, true);
                        }
                    }

                    if (checkBox2.IsChecked == true)
                        textBox2.ScrollToEnd();
                }
            }

            uint closestId = Fubi.getClosestUserID();
            if (closestId > 0)
            {
                // For printing the user orientation
                //float[] mat = new float[9];
                //float confidence;
                //double timeStamp;
                //Fubi.getCurrentSkeletonJointOrientation(closestId, FubiUtils.SkeletonJoint.Torso, mat, out confidence, out timeStamp);
                //float rx, ry, rz;
                //FubiUtils.Math.rotMatToRotation(mat, out rx, out ry, out rz);
                //label1.Content = "UserOrient:" + String.Format("{0:0.#}", rx) + "/" + String.Format("{0:0.#}", ry) + "/" + String.Format("{0:0.#}", rz);


                if (controlMouse)
                {
                    float x, y;
                    FubiMouse.applyHandPositionToMouse(closestId, out x, out y, leftHandRadioButton.IsChecked == true);
                    label2.Content = "X:" + x + " Y:" + y;
                }
                
                if (checkBox4.IsChecked == true)
                {
                    FubiPredefinedGestures.Combinations activationGesture = FubiPredefinedGestures.Combinations.WAVE_RIGHT_HAND_OVER_SHOULDER;
                    // TODO use left hand waving
                    if (Fubi.getCombinationRecognitionProgressOn(activationGesture, closestId, false) == FubiUtils.RecognitionResult.RECOGNIZED)
                    {
                        if (controlMouse)
                            stopMouseEmulation();
                        else
                            startMouseEmulation();
                    }
                    else
                    {
                        Fubi.enableCombinationRecognition(activationGesture, closestId, true);
                    }
                }
            }
        }
      
        private void fubiMain()
        {
            DispatcherOperation currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(this.initFubi), null);
            while (currentOP.Status != DispatcherOperationStatus.Completed
                    && currentOP.Status != DispatcherOperationStatus.Aborted)
            {
                Thread.Sleep(100); // Wait for init to finish
            }
            
            while (running)
            {
                Fubi.updateSensor();

                currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(this.updateFubi), null);
                Thread.Sleep(29); // Time it should at least take to get new data
                while (currentOP.Status != DispatcherOperationStatus.Completed
                    && currentOP.Status != DispatcherOperationStatus.Aborted)
                {
                    Thread.Sleep(2); // If the update unexpectedly takes longer
                }
            }

            currentOP = this.Dispatcher.BeginInvoke(new NoArgDelegate(this.releaseFubi), null);
            while (currentOP.Status != DispatcherOperationStatus.Completed
                    && currentOP.Status != DispatcherOperationStatus.Aborted)
            {
                Thread.Sleep(100); // Wait for release to finish
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            running = false;

            kListener.Dispose();

            fubiThread.Join(2000);

            this.Dispatcher.InvokeShutdown();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (openRecDlg == null)
            {
                openRecDlg = new Microsoft.Win32.OpenFileDialog();
                openRecDlg.FileName = "SampleRecognizers"; // Default file name 
                openRecDlg.DefaultExt = ".xml"; // Default file extension 
                openRecDlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension 
            }

            // Show open file dialog box 
            Nullable<bool> result = openRecDlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true && openRecDlg.FileName != null)
            {
                // Open document 
                Fubi.loadRecognizersFromXML(openRecDlg.FileName);

                // Enable clear button if we have loaded some recognizers
                button3.IsEnabled = (Fubi.getNumUserDefinedCombinationRecognizers() > 0 || Fubi.getNumUserDefinedRecognizers() > 0);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            // Enable/Disable mouse emulation
            if (controlMouse)
            {
                stopMouseEmulation();
            }
            else
            {
                startMouseEmulation();
            }
        }

        private void keyPressedHandler(object sender, RawKeyEventArgs args)
        {
            // ESC stops mouse emulation
            if (args.Key == Key.Escape)
            {
                this.Dispatcher.BeginInvoke(new NoArgDelegate(this.stopMouseEmulation), null);
            }
        }

        private void startMouseEmulation()
        {
            controlMouse = true;
            button2.Content = "Stop MouseEmu (ESC)";
            label2.Content = "X:0 Y:0";
            FubiMouse.autoCalibrateMapping(leftHandRadioButton.IsChecked == true);
        }

        private void stopMouseEmulation()
        {
            controlMouse = false;
            button2.Content = "Start Mouse Emulation";
            label2.Content = "";
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            // Clear user defined recognizers
            clearRecognizersOnNextUpdate = true;
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            Fubi.resetTracking();
        }

        private void sensorSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switchSensorOnNextUpdate = true;
        }

        private void minCutOffControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded == true)
            {
                float minCutOff, velCutOff, slope;
                Fubi.getFilterOptions(out minCutOff, out velCutOff, out slope);
                Fubi.setFilterOptions((float)minCutOffControl.Value, velCutOff, slope);
            }
        }

        private void cutOffSlopeControl_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded == true)
            {
                float minCutOff, velCutOff, slope;
                Fubi.getFilterOptions(out minCutOff, out velCutOff, out slope);
                Fubi.setFilterOptions(minCutOff, velCutOff, (float)cutOffSlopeControl.Value);
            }
        }

        private void Expander_Changed(object sender, RoutedEventArgs e)
        {
            if (IsLoaded == true)
            {
                double w = 180;
                double h = 203;

                if (MenuExpander.IsExpanded)
                {
                    h += MenuTab.Height;
                    w = 660;
                }

                if (LogExpander.IsExpanded)
                {
                    h += LogGrid.Height;
                    w = 660;
                }

                this.MinWidth = w;
                this.MinHeight = h;
            }
        }

        private void imageStreamComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded == true)
            {
                try
                {
                    FubiUtils.SensorType sensorType = (FubiUtils.SensorType)Enum.Parse(typeof(FubiUtils.SensorType), sensorSelectionComboBox.SelectedItem.ToString());
                    if (sensorType != FubiUtils.SensorType.NONE)
                    {
                        FubiUtils.ImageType newType = (FubiUtils.ImageType)Enum.Parse(typeof(FubiUtils.ImageType), imageStreamComboBox.SelectedItem.ToString());
                        int width = 0, height = 0;
                        if (newType == FubiUtils.ImageType.Color)
                            Fubi.getRgbResolution(out width, out height);
                        else if (newType == FubiUtils.ImageType.IR)
                            Fubi.getIRResolution(out width, out height);
                        if (width < 0 || height < 0)
                            switchSensorOnNextUpdate = true;
                    }
                }
                catch (Exception) { }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox) sender;
            if ((FubiUtils.JointsToRender)Enum.Parse(typeof(FubiUtils.JointsToRender), box.Tag.ToString()) == FubiUtils.JointsToRender.ALL_JOINTS)
            {
                foreach (DataRow d in jointsToRenderDT.AsEnumerable())
                {
                    d.SetField<bool>("IsChecked", true);
                }
            }                
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            if ((FubiUtils.JointsToRender)Enum.Parse(typeof(FubiUtils.JointsToRender), box.Tag.ToString()) == FubiUtils.JointsToRender.ALL_JOINTS)
            {
                foreach (DataRow d in jointsToRenderDT.AsEnumerable())
                {
                    d.SetField<bool>("IsChecked", false);
                }
            }
        }

        private void jointsToRenderCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            jointsToRenderCB.SelectedItem = null;
        }

        private void openRecStats_click(object sender, RoutedEventArgs e)
        {
            if (statsWindow == null)
            {
                statsWindow = new RecognizerStatsWindow();
                statsWindow.Owner = this;
                statsWindow.Closed += new EventHandler(statsWindow_Closed);
                statsWindow.Left = this.Left + this.Width;
                statsWindow.Top = this.Top;
                statsWindow.Show();
            }
            else
                statsWindow.Close();
        }

        private void statsWindow_Closed(object sender, EventArgs e)
        {
            statsWindow = null;
            openRecStats.IsChecked = false;
        }

        private void registerStreams_checkBox_Changed(object sender, RoutedEventArgs e)
        {
            switchSensorOnNextUpdate = true;
        }

        private void fingerSensorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switchFingerSensorOnNextUpdate = true;
        }

        private void fSensorOffset_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded == true)
            {
                Fubi.setFingerSensorOffsetPosition((float)xOffsetControl.Value, (float)yOffsetControl.Value, (float)zOffsetControl.Value);
            }
        }
    }
}