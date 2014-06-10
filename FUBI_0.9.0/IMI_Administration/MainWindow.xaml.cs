﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace IMI_Administration
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DECLARATIONS
        // Exhibition
        private Exhibition exhibition;
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
        private Headline headline;
        // Settings determine the propoerty to be edited (exhibition or exhibit)
        private enum Setting
        {
            // Exhibition
            UserHeadPosition = 0,
            Threshold, //1
            SelectionTime, //2
            LockTime, //3
            SlideTime, //4
            EndWait, //5
            // Exhibit
            KernelSize, //6
            KernelWeight, //7
            Position //8
        };
        private Setting setting;
        // Updateable contents of widgets
        private string contentLabel1;
        private string contentLabel2;
        private string contentButton1;
        private string contentButton2;
        private string contentButton3;
        private string contentButton4;
        private string contentButton5;
        private string contentTextBox1;
        private string contentTextBox2;
        // FOR TEMPORARY USE ONLY!
        private string TMP_NAME;
        private string TMP_PATH;
        private Exhibit TMP_EXHIBIT;
        private int TMP_EXHIBIT_INDEX;
        private GeometryHandler.Plane TMP_EXHIBITION_PLANE;
        // Dialogs
        private OpenFileDialog loadConfigDialog;
        private OpenFileDialog loadTextDialog;
        private OpenFileDialog loadImageDialog;
        private SaveFileDialog saveConfigDialog;
        private SaveFileDialog saveTextDialog;
        // Handler
        private GeometryHandler geometryHandler;
        private FileHandler fileHandler;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            // Initialize dialogs
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

            // Initialize handlers
            this.geometryHandler = new GeometryHandler();
            this.fileHandler = new FileHandler();
            
            // Initialize layout
            this.headline = Headline.Start;
            updateLayout();
        }

        #region LAYOUT
        // Update layout to...
        private void updateLayout()
        {
            switch ((int)this.headline)
            { 
                default: //0: Start
                    showStart();
                    break;
                case 1: //Exhibition
                    showExhibition();
                    break;
                case 2: //LoadExhibit
                    showLoadExhibit();
                    break;
                case 3: //NewExhibit
                    showNewExhibit();
                    break;
                case 4: //EditExhibit
                    showEditExhibit();
                    break;
                case 5: //ExhibitionPlane
                    showExhibitionPlane();
                    break;
                case 6: //ExhibitionPlaneDef
                    showExhibitionPlaneDef();
                    break;
                case 7: //ExhibitionPlaneVal
                    showExhibitionPlaneVal();
                    break;
                case 8: //ExhibitionDone
                    showExhibitionPlaneDone();
                    break;
                case 9: //Exhibit
                    showNewName();
                    break;
                case 10: //ExhibitDef
                    showExhibitDef();
                    break;
                case 11: //ExhibitVal
                    showExhibitVal();
                    break;
                case 12: //ExhibitDone
                    showExhibitDone();
                    break;
                case 13: //Settings
                    showExhibitionSettings();
                    break;
                case 14: //Settings
                    showExhibitSettings();
                    break;
            }
        }
        
        // ... show the exhibition's settings
        private void showExhibitionSettings()
        {
            // Labels
            this.label1.Content = this.contentLabel1;
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
            this.comboBox1.Items.Add("Benutzerposition"); //UserHeadPosition
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
            this.label1.Content = this.contentLabel1;
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
            this.comboBox1.Items.Add("Gewicht"); // KernelWeight
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
            this.label1.Content = this.TMP_NAME.ToUpper() + " - POSITIONSBESTIMMUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "- Position erfolgreich bestimmt." + '\n' + "oder" + '\n' + "- Position nicht erfolgreich bestimmt.";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;
          
            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = "OK";
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
            this.label1.Content = this.TMP_NAME.ToUpper() + " - VALIDIERUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "[Instruktionen]";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = "OK";
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
            this.label1.Content = this.TMP_NAME.ToUpper() + " - DEFINITION";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "[Instruktionen]";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = "OK";
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
            if (this.exhibition == null) // New Exhibition
            {
                this.contentLabel1 = "Ausstellung";
            }
            else // New exhibit
            {
                this.contentLabel1 = "Exponat";
            }

            // Labels
            this.label1.Content = this.contentLabel1.ToUpper();
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "Bitte " + this.contentLabel1 + "snamen eingeben";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = "OK";
            this.button5.Visibility = Visibility.Hidden;

            // ComboBoxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.comboBox2.Visibility = Visibility.Hidden;

            // TextBoxes
            this.textBox1.Visibility = Visibility.Hidden;

            this.textBox2.Text = "";
            this.textBox2.Visibility = Visibility.Visible;

            // Image
            this.image1.Visibility = Visibility.Hidden;
        }

        // ... show the outcome of definition and validation of an exhibition plane
        private void showExhibitionPlaneDone()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNGSEBENE - BESTIMMUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "- Ebene erfolgreich bestimmt." + '\n' + "oder" + '\n' + "- Ebene nicht erfolgreich bestimmt.";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = "OK";
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
            this.label1.Content = "AUSSTELLUNGSEBENE - VALIDIERUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "[Instruktionen]";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = "OK";
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
            this.label1.Content = "AUSSTELLUNGSEBENE - DEFINITION";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "[Instruktionen]";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = "OK";
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

        // ... show the exhibition plane
        private void showExhibitionPlane()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNGSEBENE";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = "LADEN";
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = "NEU";
            this.button2.Visibility = Visibility.Visible;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "zurück";
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
            this.label1.Content = this.contentLabel1 + " - BEARBEITEN";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Content = this.contentButton2;
            this.button2.Visibility = Visibility.Visible;

            this.button3.Content = contentButton3;
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
            this.label1.Content = this.contentLabel1;
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
            this.label1.Content = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = "Bearbeiten";
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = "Löschen";
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
            this.label1.Content = this.exhibition.getName().ToUpper() +"-AUSSTELLUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "Einstellungen";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = "Schließen";
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
            this.label1.Content = "START";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = "Ausstellung laden";
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = "Neu erstellen";
            this.button2.Visibility = Visibility.Visible;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = "Beenden";
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
            Exhibition tmpExhbtn = new Exhibition(); 
            Exhibit tmpExhbt = new Exhibit();

            this.comboBox2.Items.Clear();
            this.comboBox2.Items.Add("niedrig"); // Low
            this.comboBox2.Items.Add("normal"); // Medium
            this.comboBox2.Items.Add("hoch"); // High

            switch ((int)this.setting)
            { 
                case 0: //UserHeadPosition
                    break;
                case 1: //Threshold
                    if (this.exhibition.getThreshold() == (tmpExhbtn.getThreshold() * 1.5)) //150% default := Low
                        this.comboBox2.SelectedIndex = 0;
                    else if (this.exhibition.getThreshold() == tmpExhbtn.getThreshold()) //100% default := Medium
                        this.comboBox2.SelectedIndex = 1;
                    else if (this.exhibition.getThreshold() == (tmpExhbtn.getThreshold() * 0.5)) //50% default := High
                        this.comboBox2.SelectedIndex = 2;
                    break;
                case 2: //SelectionTime
                    if (this.exhibition.getSelectionTime() == (tmpExhbtn.getSelectionTime() * 0.5)) //50% default := Low
                        this.comboBox2.SelectedIndex = 0;
                    else if (this.exhibition.getSelectionTime() == tmpExhbtn.getSelectionTime()) //100% default := Medium
                        this.comboBox2.SelectedIndex = 1;
                    else if (this.exhibition.getSelectionTime() == (tmpExhbtn.getSelectionTime() * 1.5)) //150% default := High
                        this.comboBox2.SelectedIndex = 2;
                    break;
                case 3: //LockTime
                    if (this.exhibition.getLockTime() == (tmpExhbtn.getLockTime() * 0.5)) //50% default := Low
                        this.comboBox2.SelectedIndex = 0;
                    else if (this.exhibition.getLockTime() == tmpExhbtn.getLockTime()) //100% default := Medium
                        this.comboBox2.SelectedIndex = 1;
                    else if (this.exhibition.getLockTime() == (tmpExhbtn.getLockTime() * 1.5)) //150% default := High
                        this.comboBox2.SelectedIndex = 2;
                    break;
                case 4: //SlideTime
                    if (this.exhibition.getSlideTime() == (tmpExhbtn.getSlideTime() * 0.5)) //50% default := Low
                        this.comboBox2.SelectedIndex = 0;
                    else if (this.exhibition.getSlideTime() == tmpExhbtn.getSlideTime()) //100% default := Medium
                        this.comboBox2.SelectedIndex = 1;
                    else if (this.exhibition.getSlideTime() == (tmpExhbtn.getSlideTime() * 1.5)) //150% default := High
                        this.comboBox2.SelectedIndex = 2;
                    break;
                case 5: //EndWait
                    if (this.exhibition.getEndWait() == (tmpExhbtn.getEndWait() * 0.5)) //50% default := Low
                        this.comboBox2.SelectedIndex = 0;
                    else if (this.exhibition.getEndWait() == tmpExhbtn.getEndWait()) //100% default := Medium
                        this.comboBox2.SelectedIndex = 1;
                    else if (this.exhibition.getEndWait() == (tmpExhbtn.getEndWait() * 1.5)) //150% default := High
                        this.comboBox2.SelectedIndex = 2;
                    break;
                case 6: //KernelSize
                    if (this.TMP_EXHIBIT.getKernelSize() == (tmpExhbt.getKernelSize() * 0.5)) //50% default := Low
                        this.comboBox2.SelectedIndex = 0;
                    else if (this.TMP_EXHIBIT.getKernelSize() == tmpExhbt.getKernelSize()) //100% default := Medium
                        this.comboBox2.SelectedIndex = 1;
                    else if (this.TMP_EXHIBIT.getKernelSize() == (tmpExhbt.getKernelSize() * 1.5)) //150% default := High
                        this.comboBox2.SelectedIndex = 2;
                    break;
                case 7: //KernelWeigth
                    if (this.TMP_EXHIBIT.getKernelWeight() == (tmpExhbt.getKernelWeight() * 0.5)) //50% default := Low
                        this.comboBox2.SelectedIndex = 0;
                    else if (this.TMP_EXHIBIT.getKernelWeight() == tmpExhbt.getKernelWeight()) //100% default := Medium
                        this.comboBox2.SelectedIndex = 1;
                    else if (this.TMP_EXHIBIT.getKernelWeight() == (tmpExhbt.getKernelWeight() * 1.5)) //150% default := High
                        this.comboBox2.SelectedIndex = 2;
                    break;
                case 8: //Position
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
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start: "hidden"
                    break;
                case 1: //Exhibition: "hidden"
                    this.TMP_EXHIBIT_INDEX = this.comboBox1.SelectedIndex - 1;

                    switch(this.comboBox1.SelectedIndex)
                    {
                        case -1: // No item selected: "initialization"
                            break;
                        case 0: // First item selected: "new exhibit"
                            this.contentLabel1 = "NEUES EXPONAT";
                            this.contentButton1 = "Laden";
                            this.contentButton2 = "Erstellen";

                            this.headline = Headline.NewExhibit;
                            updateLayout();
                            break;
                        default: // Existing item selected: "exhibit XY"
                            this.contentLabel1 = this.comboBox1.SelectedItem.ToString();

                            this.headline = Headline.LoadExhibit;
                            updateLayout();
                            break;
                    }
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "hidden"
                    switch (this.comboBox1.SelectedIndex)
                    {
                        case -1: // No item selected: "initialization" 
                            this.contentButton2 = "     ";
                            this.contentButton3 = "Text laden";
                            this.contentButton4 = "Eigenschaften";
                            this.contentButton5 = "OK";
                            break;
                        case 0: // First item selected: "new image"
                            this.contentButton2 = "Laden";
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

                            this.contentButton2 = "Löschen";
                            break;
                    }
                    updateButtons();
                    break;
                case 5: //ExhibitionPlane: "hidden"
                    break;
                case 6: //ExhibitionPlaneDef: "hidden"
                    break;
                case 7: //ExhibitionPlaneVal: "hidden"
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewName: "hidden"
                    break;
                case 10: //ExhibitDef: "hidden"
                    break;
                case 11: //ExhibitVal: "hidden"
                    break;
                case 12: //ExhibitDone: "hidden"
                    break;
                case 13: //ExhibitionSettings:
                    switch (this.comboBox1.SelectedIndex)
                    {
                        case 0: //UserHeadPosition
                            this.comboBox2.Visibility = Visibility.Hidden;
                            this.contentButton2 = "einstellen";
                            this.button2.Visibility = Visibility.Visible;
                            this.button3.Visibility = Visibility.Hidden;
                            this.setting = Setting.UserHeadPosition;
                            break;
                        case 1: //Threshold
                            this.setting = Setting.Threshold;
                            this.showLowMedHigh();
                            break;
                        case 2: //SelectionTime
                            this.setting = Setting.SelectionTime;
                            this.showLowMedHigh();
                            break;
                        case 3: //LockTime
                            this.setting = Setting.LockTime;
                            this.showLowMedHigh();
                            break;
                        case 4: //SlideTime
                            this.setting = Setting.SlideTime;
                            this.showLowMedHigh();
                            break;
                        case 5: //EndWait
                            this.setting = Setting.EndWait;
                            this.showLowMedHigh();
                            break;
                        default:
                            break;
                    }
                    updateButtons();
                    break;
                case 14: //ExhibitSettings:
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
            }
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start: "hidden"
                    break;
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "hidden"
                    break;
                case 5: //ExhibitionPlane: "hidden"
                    break;
                case 6: //ExhibitionPlaneDef: "hidden"
                    break;
                case 7: //ExhibitionPlaneVal: "hidden"
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewName: "hidden"
                    break;
                case 10: //ExhibitDef: "hidden"
                    break;
                case 11: //ExhibitVal: "hidden"
                    break;
                case 12: //ExhibitDone: "hidden"
                    break;
                case 13: //ExhibitionSettings                    
                    //this.contentButton3 = "ändern";
                    //this.button3.Visibility = Visibility.Visible;
                    //updateButtons();
                    break;
                case 14: //ExhibitSettings                    
                    //this.contentButton3 = "ändern";
                    //this.button3.Visibility = Visibility.Visible;
                    //updateButtons();
                    break;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            { 
                default:
                    break;
                case 0: //Start: "load existing exhibition"
                    this.loadConfigDialog.ShowDialog();
                    if (this.TMP_PATH != null) // Temporary file path has been set
                    {
                        this.exhibition = this.fileHandler.loadExhibition(this.TMP_PATH);
                        this.TMP_PATH = null;

                        this.headline = Headline.Exhibition;
                        updateLayout();
                    }
                    else // Temporary file path hat not been set
                    { }
                    break;
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "edit exhibit"
                    this.TMP_EXHIBIT = this.exhibition.getExhibit(this.TMP_EXHIBIT_INDEX);
                    this.TMP_EXHIBIT_INDEX = this.comboBox1.SelectedIndex - 1;

                    this.contentLabel1 = this.TMP_EXHIBIT.getName();
                    this.contentTextBox1 = this.TMP_EXHIBIT.getDescription();
                    this.headline = Headline.EditExhibit;
                    updateLayout();
                    break;
                case 3: //NewExhibit: "load existing exhibit"
                    this.loadConfigDialog.ShowDialog();
                    if (this.TMP_PATH != null) // Temporary file path has been set
                    {
                        this.TMP_EXHIBIT = this.fileHandler.loadExhibit(this.TMP_PATH);
                        this.TMP_PATH = null;
                        this.exhibition.addExhibit(this.TMP_EXHIBIT);
                        this.TMP_EXHIBIT_INDEX = this.exhibition.getExhibits().Count - 1;

                        this.contentLabel1 = this.TMP_EXHIBIT.getName();
                        this.contentTextBox1 = this.TMP_EXHIBIT.getDescription();
                        this.headline = Headline.EditExhibit;
                        updateLayout();
                    }
                    else // Temporary file path hat not been set
                    { }
                    break;
                case 4: //EditExhibit: "hidden"
                    break;
                case 5: //ExhibitionPlane: "load exisiting definition of the exhibition plane"
                    this.loadConfigDialog.ShowDialog();
                    if (this.TMP_PATH != null) // Temporary file path has been set
                    {
                        this.TMP_EXHIBITION_PLANE = this.fileHandler.loadExhibitionPlane(this.TMP_PATH);
                        this.TMP_PATH = null;
                        this.exhibition = new Exhibition(this.TMP_NAME, this.TMP_EXHIBITION_PLANE);

                        this.headline = Headline.Exhibition;
                        updateLayout();
                    }
                    else // Temporary file path hat not been set
                    { }
                    break;
                case 6: //ExhibitionPlaneDef: "hidden"
                    break;
                case 7: //ExhibitionPlaneVal: "hidden"
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewName: "hidden"
                    break;
                case 10: //ExhibitDef: "hidden"
                    break;
                case 11: //ExhibitVal: "hidden"
                    break;
                case 12: //ExhibitDone: "hidden"
                    break;
                case 13: //Settings:
                    break;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start: "define new exhibition"
                    this.headline = Headline.NewName;
                    updateLayout();
                    break;
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "remove exhibit from exhibition's list of exhibits"
                    this.exhibition.removeExhibit(this.TMP_EXHIBIT_INDEX);

                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case 3: //NewExhibit: "define new exhibit"
                    this.headline = Headline.NewName;
                    updateLayout();
                    break;
                case 4: //EditExhibit: "delete image from exhibit"
                    switch (this.comboBox1.SelectedIndex)
                    { 
                        case -1: // No item selected
                            break;
                        case 0: // "new image"
                            this.loadImageDialog.ShowDialog();
                            if (this.TMP_PATH != null)
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
                case 5: //ExhibitionPlane: "define new exhibition plane"
                    MessageBox.Show("Neue Ausstellungsebene definieren");
                    this.headline = Headline.ExhibitionPlaneDef;
                    updateLayout();
                    break;
                case 6: //ExhibitionPlaneDef: "hidden"
                    break;
                case 7: //ExhibitionPlaneVal: "hidden"
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewName: "hidden"
                    break;
                case 10: //ExhibitDef: "hidden"
                    break;
                case 11: //ExhibitVal: "hidden"
                    break;
                case 12: //ExhibitDone: "hidden"
                    break;
                case 13: //Settings:
                    break;
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start:
                    break;
                case 1: //Exhibition:
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "load text from txt-file"
                    MessageBox.Show("Text laden");
                    break;
                case 5: //ExhibitionPlane:
                    break;
                case 6: //ExhibitionPlaneDef:
                    break;
                case 7: //ExhibitionPlaneVal:
                    break;
                case 8: //ExhibitionPlaneDone:
                    break;
                case 9: //NewName:
                    break;
                case 10: //ExhibitDef:
                    break;
                case 11: //ExhibitVal:
                    break;
                case 12: //ExhibitDone:
                    break;
                case 13: //Settings:
                    break;
            }             
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start: "hidden"
                    break;
                case 1: //Exhibition: "hidden"
                    this.contentLabel1 = this.exhibition.getName() + " - EINSTELLUNGEN";
                    this.contentButton5 = "OK";

                    this.headline = Headline.ExhibitionSettings;
                    updateLayout();
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "open exhibit's properties"
                    this.contentLabel1 = this.TMP_EXHIBIT.getName() + " - EINSTELLUNGEN";

                    this.headline = Headline.ExhibitSettings;
                    updateLayout();
                    break;
                case 5: //ExhibitionPlane: "back to the start"
                    MessageBox.Show("Zurück zum Start");
                    this.headline = Headline.Start;
                    updateLayout();
                    break;
                case 6: //ExhibitionPlaneDef: "back to exhibition plane"
                    MessageBox.Show("Zurück zur Ausstellungsebene");
                    this.headline = Headline.ExhibitionPlane;
                    updateLayout();
                    break;
                case 7: //ExhibitionPlaneVal: "back to exhibition plane"
                    MessageBox.Show("Zurück zur Ausstellungsebene");
                    this.headline = Headline.ExhibitionPlane;
                    updateLayout();
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewName: "hidden"
                    break;
                case 10: //ExhibitDef: "back to the exhibition"
                    MessageBox.Show("Zurück zur Ausstellung");
                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case 11: //ExhibitVal: "back to the exhibition"
                    MessageBox.Show("Zurück zur Ausstellung");
                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case 12: //ExhibitDone: "hidden"
                    break;
                case 13: //ExhibitionSettings: "hidden"
                    break;
                case 14: //ExhibitSettings: "hidden"
                    break;
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start: "close the application"
                    closeAllThreads();
                    break;
                case 1: //Exhibition: "close the application"
                    this.fileHandler.saveExhibition(this.exhibition);
                    closeAllThreads();
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "editing done"
                    this.TMP_EXHIBIT.setDescription(this.textBox1.Text);

                    if (this.TMP_EXHIBIT_INDEX != -1) // Existing exhibit
                    {
                        this.exhibition.setExhibit(this.TMP_EXHIBIT_INDEX, this.TMP_EXHIBIT);
                        this.fileHandler.saveExhibit(this.TMP_EXHIBIT);

                        this.headline = Headline.Exhibition;
                        updateLayout();
                    }
                    else // New exhibit
                    {
                        this.saveConfigDialog.ShowDialog();
                        if (this.TMP_PATH != null)
                        {
                            this.TMP_EXHIBIT.setPath(this.saveConfigDialog.FileName);
                            this.TMP_PATH = null;
                            this.exhibition.addExhibit(this.TMP_EXHIBIT);

                            this.fileHandler.saveExhibit(this.TMP_EXHIBIT);

                            this.headline = Headline.Exhibition;
                            updateLayout();
                        }
                        else
                        { }
                    }
                    break;
                case 5: //ExhibitionPlane: "hidden"
                    break;
                case 6: //ExhibitionPlaneDef: "start or abort definition of exhibition plane"
                    MessageBox.Show("Start oder Abbruch der Definition der Ausstellungsebene");
                    this.headline = Headline.ExhibitionPlaneVal;
                    updateLayout();
                    break;
                case 7: //ExhibitionPlaneVal: "abort validation of exhibition plane"
                    MessageBox.Show("Start oder Abbruch der Validierung der Ausstellungsebene");
                    this.headline = Headline.ExhibitionPlaneDone;
                    updateLayout();
                    break;
                case 8: //ExhibitionPlaneDone: "abort validation of exhibition plane"
                    //DUMMY-PLANE
                    this.TMP_EXHIBITION_PLANE = new GeometryHandler.Plane(new Point3D(), new Point3D(), new Point3D());
                    //DUMMY-EXHIBITION
                    this.exhibition = new Exhibition(this.TMP_NAME, this.TMP_EXHIBITION_PLANE);

                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case 9: //NewName: "safe name and continue to next view"
                    if (this.exhibition == null) // New exhibition
                    {
                        this.TMP_NAME = this.textBox2.Text;
                        this.headline = Headline.ExhibitionPlane;
                    }
                    else // New exhibit
                    {
                        this.TMP_NAME = this.textBox2.Text;
                        this.headline = Headline.ExhibitDef;
                    }
                    updateLayout();
                    break;
                case 10: //ExhibitDef: "start or abort definition of exhibit"
                    MessageBox.Show("Start oder Abbruch der Definition des Exponats");
                    this.headline = Headline.ExhibitVal;
                    updateLayout();
                    break;
                case 11: //ExhibitVal: "abort validation of exhibit"
                    MessageBox.Show("Start oder Abbruch der Validierung des Exponats");
                    this.headline = Headline.ExhibitDone;
                    updateLayout();
                    break;
                case 12: //ExhibitDone: "abort validation of exhibition plane"
                    
                    MessageBox.Show("Validierung des Exponats (nicht) erfolgreich");
                    // DUMMY-POINT
                    this.TMP_EXHIBIT = new Exhibit(this.TMP_NAME, new Point3D());

                    this.contentLabel1 = this.TMP_EXHIBIT.getName();
                    this.contentTextBox1 = this.TMP_EXHIBIT.getDescription();
                    this.headline = Headline.EditExhibit;
                    updateLayout();
                    break;
                case 13: //ExhibitionSettings: "safe and go back to exhibition"
                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case 14: //ExhibitSettings: "go to exhibit"
                    this.headline = Headline.EditExhibit;
                    updateLayout();
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start: "hidden"
                    break;
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "hidden"
                    this.contentTextBox1 = this.textBox1.Text;
                    break;
                case 5: //ExhibitionPlane: "hidden"
                    break;
                case 6: //ExhibitionPlaneDef: "hidden"
                    break;
                case 7: //ExhibitionPlaneVal: "hidden"
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewName: "hidden"
                    break;
                case 10: //ExhibitDef: "hidden"
                    break;
                case 11: //ExhibitVal: "hidden"
                    break;
                case 12: //ExhibitDone: "hidden"
                    break;
                case 13: //ExhibitionSettings: "hidden"
                    break;
                case 14: //ExhibitSettings: "hidden"
                    break;
            }
        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start: "hidden"
                    break;
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "hidden"
                    break;
                case 5: //ExhibitionPlane: "hidden"
                    break;
                case 6: //ExhibitionPlaneDef: "hidden"
                    break;
                case 7: //ExhibitionPlaneVal: "hidden"
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewName: "Enter Name of Exhibition / Exhibit"
                    if (this.textBox2.Text != "") // There is some input
                    {
                        this.button5.Visibility = Visibility.Visible; // Show "OK"-button
                    }
                    else
                    {
                        this.button5.Visibility = Visibility.Hidden; // Hide "OK"-button
                    }
                    break;
                case 10: //ExhibitDef: "hidden"
                    break;
                case 11: //ExhibitVal: "hidden"
                    break;
                case 12: //ExhibitDone: "hidden"
                    break;
                case 13: //ExhibitionSettings: "hidden"
                    break;
                case 14: //ExhibitSettings: "hidden"
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

        //--- FOR DEBUGGING ONLY ---// 
        private void label1_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((int)this.headline != 13)
                ++this.headline;
            else
                this.headline = 0;

            updateLayout();
        }
        #endregion
        
        private void closeAllThreads()
        {
            this.Close();
        }
    }
}
