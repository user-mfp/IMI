using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;

namespace IMI_Administration
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
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
            UserHeadPosition, //1
            BackgroundImage, //2
            Threshold, //3
            SelectionTime, //4
            LockTime, //5
            SlideTime, //6
            EndWait, //7
            // Exhibit
            KernelSize, //8
            KernelWeight, //9
            Position //10
        };

        // Canstants for setting attributes
        private double HIGH = 1.5;
        private double LOW = 0.5;
        #endregion

        #region DECLARATIONS
        // Exhibition
        private Exhibition exhibition;
        private Headline headline;
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
            this.setting = Setting.None;
            updateLayout();
        }

        #region LAYOUT
        // Update layout to...
        private void updateLayout()
        {
            switch ((int)this.headline)
            { 
                case 0: //Start
                    this.contentLabel1 = "START";
                    this.contentButton1 = "Ausstellung laden";
                    this.contentButton2 = "Ausstellung erstellen";
                    this.contentButton5 = "beenden";
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
                default:
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
            this.comboBox1.Items.Add("Hintergrundbild");
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
            this.label1.Content = this.contentLabel1; //this.TMP_NAME.ToUpper() + " - POSITIONSBESTIMMUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "- Position erfolgreich bestimmt." + '\n' + "oder" + '\n' + "- Position nicht erfolgreich bestimmt.";
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
            this.label1.Content = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = this.contentLabel2;
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
            this.label1.Content = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = this.contentLabel2;
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
            this.label1.Content = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = this.contentLabel2;
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
            this.label1.Content = this.contentLabel1;// "AUSSTELLUNGSEBENE - BESTIMMUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = this.contentLabel2;// "- Ebene erfolgreich bestimmt." + '\n' + "oder" + '\n' + "- Ebene nicht erfolgreich bestimmt.";
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
            this.label1.Content = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = this.contentLabel2;
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
            this.label1.Content = this.contentLabel1;
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = this.contentLabel2;
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

        // ... show the exhibition plane
        private void showExhibitionPlane()
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
            this.label1.Content = this.contentLabel1;// +" - BEARBEITEN";
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
            this.label1.Content = this.contentLabel1;
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

            switch ((int)this.setting)
            { 
                case 0: //None
                    break;
                case 1: //UserHeadPosition
                    break;
                case 2: //BackgroundImage
                    break;
                case 3: //Threshold
                    this.comboBox2.SelectedIndex = detLessIsMore(DEFAULT_EXHIBITION.getThreshold(), this.exhibition.getThreshold());
                    break;
                case 4: //SelectionTime
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBITION.getSelectionTime(), this.exhibition.getSelectionTime());
                    break;
                case 5: //LockTime
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBITION.getLockTime(), this.exhibition.getLockTime());
                    break;
                case 6: //SlideTime
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBITION.getSlideTime(), this.exhibition.getSlideTime());
                    break;
                case 7: //EndWait
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBITION.getEndWait(), this.exhibition.getEndWait());
                    break;
                case 8: //KernelSize
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBIT.getKernelSize(), this.TMP_EXHIBIT.getKernelSize());
                    break;
                case 9: //KernelWeigth
                    this.comboBox2.SelectedIndex = detLessIsLess(DEFAULT_EXHIBIT.getKernelWeight(), this.TMP_EXHIBIT.getKernelWeight());
                    break;
                case 10: //Position
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
                            this.image1.Visibility = Visibility.Hidden;
                            this.comboBox2.Visibility = Visibility.Hidden;
                            this.contentButton2 = "einstellen";
                            this.button2.Visibility = Visibility.Visible;
                            this.button3.Visibility = Visibility.Hidden;
                            this.setting = Setting.UserHeadPosition;
                            break;
                        case 1: //BackgroundImage
                            updateImage(this.exhibition.getBackgroundImage().Value);
                            this.comboBox2.Visibility = Visibility.Hidden;
                            this.contentButton2 = "laden";
                            this.button2.Visibility = Visibility.Visible;
                            this.button3.Visibility = Visibility.Hidden;
                            this.setting = Setting.BackgroundImage;
                            break;
                        case 2: //Threshold
                            this.image1.Visibility = Visibility.Hidden;
                            this.setting = Setting.Threshold;
                            this.showLowMedHigh();
                            break;
                        case 3: //SelectionTime
                            this.image1.Visibility = Visibility.Hidden;
                            this.setting = Setting.SelectionTime;
                            this.showLowMedHigh();
                            break;
                        case 4: //LockTime
                            this.image1.Visibility = Visibility.Hidden;
                            this.setting = Setting.LockTime;
                            this.showLowMedHigh();
                            break;
                        case 5: //SlideTime
                            this.image1.Visibility = Visibility.Hidden;
                            this.setting = Setting.SlideTime;
                            this.showLowMedHigh();
                            break;
                        case 6: //EndWait
                            this.image1.Visibility = Visibility.Hidden;
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
                    this.contentButton3 = "ändern";
                    this.button3.Visibility = Visibility.Visible;
                    break;
                case 14: //ExhibitSettings     
                    this.contentButton3 = "ändern";
                    this.button3.Visibility = Visibility.Visible;
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
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "edit exhibit"
                    this.TMP_EXHIBIT = this.exhibition.getExhibit(this.TMP_EXHIBIT_INDEX);
                    this.TMP_EXHIBIT_INDEX = this.comboBox1.SelectedIndex - 1;

                    this.contentLabel1 = this.TMP_EXHIBIT.getName().ToUpper() + " - BEARBEITEN";
                    this.contentTextBox1 = this.TMP_EXHIBIT.getDescription();
                    this.headline = Headline.EditExhibit;
                    updateLayout();
                    break;
                case 3: //NewExhibit: "load existing exhibit"
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
                case 4: //EditExhibit: "hidden"
                    break;
                case 5: //ExhibitionPlane: "load exisiting definition of the exhibition plane"
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
                    this.contentLabel1 = "AUSSTELLUNGSNAMEN BESTIMMEN";
                    this.contentLabel2 = "Bitte Ausstellungsnamen eingeben.";
                    this.contentButton5 = "OK";
                    this.headline = Headline.NewName;
                    updateLayout();
                    break;
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "remove exhibit from exhibition's list of exhibits"
                    this.exhibition.removeExhibit(this.TMP_EXHIBIT_INDEX);

                    this.contentLabel1 = this.exhibition.getName().ToUpper();
                    this.contentButton4 = "Einstellungen";
                    this.contentButton5 = "schließen";
                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case 3: //NewExhibit: "define new exhibit"
                    this.contentLabel1 = "EXPONATNAMEN BESTIMMEN";
                    this.contentLabel2 = "Bitte Exponatsnamen eingeben.";
                    this.contentButton5 = "OK";
                    this.headline = Headline.NewName;
                    updateLayout();
                    break;
                case 4: //EditExhibit: "delete image from exhibit"
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
                case 5: //ExhibitionPlane: "define new exhibition plane"
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
                case 13: //ExhibitionSettings:
                    if (this.setting == Setting.UserHeadPosition)
                    {
                        MessageBox.Show("Benutzerposition neu bestimmen");
                        this.TMP_NAME = "Benutzer";

                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSDEFINITION";
                        this.contentLabel2 = "[Instruktionen]";
                        this.contentButton4 = "abbrechen";
                        this.contentButton5 = "OK";
                        this.headline = Headline.ExhibitDef;
                    }
                    else if (this.setting == Setting.BackgroundImage)
                    {
                        if (this.loadImageDialog.ShowDialog() == true)
                        {
                            this.exhibition.setBackgroundImage(this.fileHandler.loadImage(this.loadImageDialog.FileName));
                        }
                    }
                    updateLayout();
                    break;
                case 14: //ExhibitSettings:
                    MessageBox.Show("Exponatposition neu bestimmen");
                    this.TMP_NAME = this.TMP_EXHIBIT.getName();

                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSDEFINITION";
                    this.contentLabel2 = "[Instruktionen]";
                    this.contentButton4 = "abbrechen";
                    this.contentButton5 = "OK";
                    this.headline = Headline.ExhibitDef;
                    updateLayout();
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
                    if (this.loadTextDialog.ShowDialog() == true)
                    {
                        StreamReader streamReader = new StreamReader(this.loadTextDialog.FileName);
                        this.contentTextBox1 = (streamReader.ReadToEnd());
                    }
                    updateLayout();
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
                case 13: //ExhibitionSettings: "hidden"
                    setAttribute();
                    break;
                case 14: //ExhibitSettings: "hidden"
                    setAttribute();
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
                    this.contentButton5 = "OK";
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
                    
                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENDEFINITION";
                    this.contentLabel2 = "[Instruktionen]";
                    this.contentButton1 = "laden";
                    this.contentButton2 = "bestimmen";
                    this.contentButton4 = "abbrechen";
                    this.headline = Headline.ExhibitionPlane;
                    updateLayout();
                    break;
                case 7: //ExhibitionPlaneVal: "back to exhibition plane"
                    MessageBox.Show("Zurück zur Ausstellungsebene");

                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENDEFINITION";
                    this.contentLabel2 = "[Instruktionen]";
                    this.contentButton1 = "laden";
                    this.contentButton2 = "bestimmen";
                    this.contentButton4 = "abbrechen";
                    this.headline = Headline.ExhibitionPlane;
                    updateLayout();
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewName: "hidden"
                    break;
                case 10: //ExhibitDef: "back to the exhibition"
                    MessageBox.Show("Zurück zur Ausstellung");
                    this.contentLabel1 = this.exhibition.getName().ToUpper();
                    this.contentButton4 = "Einstellungen";
                    this.contentButton5 = "schließen";
                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case 11: //ExhibitVal: "back to the exhibition"
                    MessageBox.Show("Zurück zur Ausstellung");
                    this.contentLabel1 = this.exhibition.getName().ToUpper();
                    this.contentButton4 = "Einstellungen";
                    this.contentButton5 = "schließen";
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
                case 5: //ExhibitionPlane: "hidden"
                    break;
                case 6: //ExhibitionPlaneDef: "start or abort definition of exhibition plane"
                    MessageBox.Show("Start oder Abbruch der Definition der Ausstellungsebene");
                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENVALIDIERUNG";
                    this.contentLabel2 = "[Instruktionen]";
                    this.headline = Headline.ExhibitionPlaneVal;
                    updateLayout();
                    break;
                case 7: //ExhibitionPlaneVal: "abort validation of exhibition plane"
                    MessageBox.Show("Start oder Abbruch der Validierung der Ausstellungsebene");
                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENBESTIMMUNG";
                    this.contentLabel2 = "- Ausstellungsebene erfolgreich bestimmt" + '\n' + "oder" + '\n' + "- Ausstellungsebene nicht erfolgreich bestimmt";;
                    this.headline = Headline.ExhibitionPlaneDone;
                    updateLayout();
                    break;
                case 8: //ExhibitionPlaneDone: "abort validation of exhibition plane"
                    //DUMMY-PLANE
                    this.TMP_EXHIBITION_PLANE = new GeometryHandler.Plane(new Point3D(), new Point3D(), new Point3D());
                    //DUMMY-EXHIBITION
                    this.exhibition = new Exhibition(this.TMP_NAME, this.TMP_EXHIBITION_PLANE);

                    MessageBox.Show("Bitte stellen Sie umgehend die Benutzerposition ein.");
                    
                    this.contentLabel1 = this.exhibition.getName().ToUpper();
                    this.contentButton4 = "Einstellungen";
                    this.contentButton5 = "schließen";
                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case 9: //NewName: "safe name and continue to next view"
                    if (this.exhibition == null) // New exhibition
                    {
                        this.TMP_NAME = this.textBox2.Text;

                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - EBENENDEFINITION";
                        this.contentLabel2 = "[Instruktionen]";
                        this.contentButton1 = "laden";
                        this.contentButton2 = "bestimmen";
                        this.contentButton4 = "abbrechen";
                        this.headline = Headline.ExhibitionPlane;
                    }
                    else // New exhibit
                    {
                        this.TMP_NAME = this.textBox2.Text;

                        this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSDEFINITION";
                        this.contentLabel2 = "[Instruktionen]";
                        this.contentButton4 = "abbrechen";
                        this.contentButton5 = "OK";
                        this.headline = Headline.ExhibitDef;
                    }
                    updateLayout();
                    break;
                case 10: //ExhibitDef: "start or abort definition of exhibit"
                    MessageBox.Show("Start oder Abbruch der Definition des Exponats");
                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSVALIDIERUNG";
                    this.contentLabel2 = "[Instruktionen]";
                    this.contentButton4 = "abbrechen";
                    this.contentButton5 = "OK";
                    this.headline = Headline.ExhibitVal;
                    updateLayout();
                    break;
                case 11: //ExhibitVal: "abort validation of exhibit"
                    MessageBox.Show("Start oder Abbruch der Validierung des Exponats");
                    this.contentLabel1 = this.TMP_NAME.ToUpper() + " - POSITIONSBESTIMMUNG";
                    this.contentLabel2 = "- Position des Exponats erfolgreich bestimmt" + '\n' + "oder" + '\n' + "- Position des Exponats nicht erfolgreich bestimmt";
                    this.contentButton4 = "abbrechen";
                    this.contentButton5 = "OK";
                    this.headline = Headline.ExhibitDone;
                    updateLayout();
                    break;
                case 12: //ExhibitDone: "abort validation of exhibition plane"        
                    if ((int)this.setting == 1) //ExhibitionSetting: UserHeadPosition
                    {
                        MessageBox.Show("Bestimmung der Benutzerposition (nicht) erfolgreich");
                        this.exhibition.setUserHeadPosition(new Point3D(1, 1, 1));

                        this.contentLabel1 = this.exhibition.getName() + " - EINSTELLUNGEN";
                        this.contentButton5 = "OK";
                        this.headline = Headline.ExhibitionSettings;                   
                    }
                    else if ((int)this.setting == 10) //ExhibitSettings: Position
                    {
                        MessageBox.Show("Neubestimmung der Exponatposition (nicht) erfolgreich");
                        this.TMP_EXHIBIT.setPosition(new Point3D(1, 1, 1));

                        this.contentLabel1 = this.TMP_EXHIBIT.getName() + " - EINSTELLUNGEN";
                        this.headline = Headline.ExhibitSettings;
                    }
                    else //((int)this.settings == 0) //New Exhibit: Settings.None
                    {
                        // DUMMY-POINT
                        this.TMP_EXHIBIT = new Exhibit(this.TMP_NAME, new Point3D());

                        this.contentLabel1 = this.TMP_EXHIBIT.getName();
                        this.contentTextBox1 = this.TMP_EXHIBIT.getDescription();
                        this.headline = Headline.EditExhibit;
                    }
                        this.setting = Setting.None;
                    updateLayout();
                    break;
                case 13: //ExhibitionSettings: "safe and go back to exhibition"
                    if (this.exhibition.getPath() != null) // Config-file already exists
                    {
                        this.fileHandler.saveExhibition(this.exhibition);
                    }

                    this.contentLabel1 = this.exhibition.getName().ToUpper();
                    this.contentButton4 = "Einstellungen";
                    this.contentButton5 = "schließen";
                    this.headline = Headline.Exhibition;
                    updateLayout();
                    break;
                case 14: //ExhibitSettings: "go to exhibit"
                    if (this.TMP_EXHIBIT.getPath() != null) // Config-file already exists
                    {
                        this.fileHandler.saveExhibit(this.TMP_EXHIBIT);
                    } 

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

            switch ((int)this.setting)
            {
                case 3: //Threshold
                    if (factor != 1.0)
                        this.exhibition.setThreshold(DEFAULT_EXHIBITION.getThreshold() * ((this.LOW + this.HIGH) - factor));
                    else
                        this.exhibition.setThreshold(DEFAULT_EXHIBITION.getThreshold());
                    break;
                case 4: //SelectionTime
                    if (factor != 1.0)
                        this.exhibition.setSelectionTime(DEFAULT_EXHIBITION.getSelectionTime() * factor);
                    else
                        this.exhibition.setSelectionTime(DEFAULT_EXHIBITION.getSelectionTime());
                    break;
                case 5: //LockTime
                    if (factor != 1.0)
                        this.exhibition.setLockTime(DEFAULT_EXHIBITION.getLockTime() * factor);
                    else
                        this.exhibition.setLockTime(DEFAULT_EXHIBITION.getLockTime());
                    break;
                case 6: //SlideTime
                    if (factor != 1.0)
                        this.exhibition.setSlideTime(DEFAULT_EXHIBITION.getSlideTime() * factor);
                    else
                        this.exhibition.setSlideTime(DEFAULT_EXHIBITION.getSlideTime());
                    break;
                case 7: //EndWait
                    if (factor != 1.0)
                        this.exhibition.setEndWait(DEFAULT_EXHIBITION.getEndWait() * factor);
                    else
                        this.exhibition.setEndWait(DEFAULT_EXHIBITION.getEndWait());
                    break;
                case 8: //KernelSize
                    if (factor != 1.0)
                        this.TMP_EXHIBIT.setKernelSize(DEFAULT_EXHIBIT.getKernelSize() * factor);
                    else
                        this.TMP_EXHIBIT.setKernelSize(DEFAULT_EXHIBIT.getKernelSize());
                    break;
                case 9: //KernelWeight
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

        private void closeAllThreads()
        {
            this.Close();
        }
    }
}
