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

namespace IMI_Administration
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DECLARATIONS
        // Headlines determine the layout (visibility, labeling and functions)
        private enum Headline {
            Start = 0,
            Exhibition, //1
            LoadExhibit, //2
            NewExhibit, //3
            EditExhibit, //4
            ExhibitionPlane, //5
            ExhibitionPlaneDef, //6
            ExhibitionPlaneVal, //7
            ExhibitionPlaneDone, //8
            NewImage, //9
            ExhibitDef, //10
            ExhibitVal, //11
            ExhibitDone, //12
            Settings //13
        };
        private Headline headline;
        // Updateable contents of widgets
        private string contentLabel1;
        private string contentLabel2;
        private string contentButton1;
        private string contentButton2;
        private string contentButton3;
        private string contentButton4;
        private string contentButton5;
        private string contentTextBox1;
        // Exhibition
        private Exhibition exhibition;
        // For temporary use only!
        private Exhibit tmpExhibit;
        private GeometryHandler.Plane tmpExhibitionPlane;
        // Handler
        private GeometryHandler geometryHandler;
        private FileHandler fileHandler;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            // Initialize handlers
            this.geometryHandler = new GeometryHandler();
            this.fileHandler = new FileHandler();

            // Initialize layout
            this.headline = 0;
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
                    showNewImage();
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
                    showSettings();
                    break;
            }
        }
        
        // ... show the exhibition's settings
        private void showSettings()
        {
            // Labels
            this.label1.Content = "SETTINGS";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = "laden";
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Content = "neu";
            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }
        
        // ... show the outcome of definition and validation of an exhibit's position
        private void showExhibitDone()
        {
            // Labels
            this.label1.Content = "EXHIBIT DONE";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "- Position erfolgreich definiert." + '\n' + "oder" + '\n' + "- Position nicht erfolgreich definiert.";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;
          
            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = "OK";
            this.button5.Visibility = Visibility.Visible;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }
        
        // ... show dialogue for an exhibit's position's validation
        private void showExhibitVal()
        {
            // Labels
            this.label1.Content = "EXHIBIT VAL";
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

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }
        
        // ... show dialogue for an exhibit's position's validation
        private void showExhibitDef()
        {
            // Labels
            this.label1.Content = "EXHIBIT DEF";
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

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        // ... show ???
        private void showNewImage()
        {
            // Labels
            this.label1.Content = "NEW IMAGE";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Content = "button2";
            this.button2.Visibility = Visibility.Visible;

            this.button3.Content = "button3";
            this.button3.Visibility = Visibility.Visible;

            this.button5.Content = "button4";
            this.button5.Visibility = Visibility.Visible;

            this.button4.Content = "button5";
            this.button4.Visibility = Visibility.Visible;

            // Boxes
            this.comboBox1.Visibility = Visibility.Visible;

            this.textBox1.Visibility = Visibility.Visible;
        }

        // ... show the outcome of definition and validation of an exhibition plane
        private void showExhibitionPlaneDone()
        {
            // Labels
            this.label1.Content = "EXHIBITION PLANE DONE";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "- Ebene erfolgreich definiert." + '\n' + "oder" + '\n' + "- Ebene nicht erfolgreich definiert.";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = "OK";
            this.button5.Visibility = Visibility.Visible;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        // ... show dialogue for an exhibition plane's validation
        private void showExhibitionPlaneVal()
        {
            // Labels
            this.label1.Content = "EXHIBITION PLANE VAL";
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

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        // ... show dialogue for an exhibition plane's definition
        private void showExhibitionPlaneDef()
        {
            // Labels
            this.label1.Content = "EXHIBITION PLANE DEF";
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

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        // ... show the exhibition plane
        private void showExhibitionPlane()
        {
            // Labels
            this.label1.Content = "EXHIBITION PLANE";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = "laden";
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = "neu";
            this.button2.Visibility = Visibility.Visible;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible; ;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        // ... show the editing of an exhibit
        private void showEditExhibit()
        {
            // Labels
            this.label1.Content = "EDIT EXHIBIT";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Content = this.contentButton2;
            this.button2.Visibility = Visibility.Visible;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "Text laden";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Content = "OK";
            this.button5.Visibility = Visibility.Visible;

            // Boxes
            this.comboBox1.Visibility = Visibility.Visible;

            this.textBox1.Visibility = Visibility.Visible;
        }

        // ... show a new exhibit
        private void showNewExhibit()
        {
            // Labels
            this.label1.Content = "NEW EXHIBIT";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Content = "Laden";
            this.button2.Visibility = Visibility.Visible;

            this.button3.Content = "Erstellen";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Visible;

            this.textBox1.Visibility = Visibility.Visible;
        }

        // ... show an existing exhibit
        private void showLoadExhibit()
        {
            // Labels
            this.label1.Content = "LOAD EXHIBIT";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Content = "Bearbeiten";
            this.button2.Visibility = Visibility.Visible;

            this.button3.Content = "Löschen";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Visible;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        // ... show the exhibition
        private void showExhibition()
        {
            // Boxes
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("neues Exponat");
            for (int i = 1; i != 4; ++i)
            {
                this.comboBox1.Items.Add(i.ToString() + ". Exponat");
            }
            this.comboBox1.Visibility = Visibility.Visible;

            this.textBox1.Visibility = Visibility.Hidden;

            // Labels
            this.label1.Content = "EXHIBITION";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Content = "Einstellungen";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Visibility = Visibility.Hidden;
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

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
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
                    if (this.comboBox1.SelectedIndex == 0)
                    {
                        this.headline = Headline.NewExhibit;
                    }
                    else
                    {
                        this.headline = Headline.LoadExhibit;
                    }
                    break;
                case 2: //LoadExhibit: "hidden"
                    if (this.comboBox1.SelectedIndex == 0)
                    {
                        this.headline = Headline.NewExhibit;
                    }
                    else
                    {
                        this.headline = Headline.LoadExhibit;
                    }
                    break;
                case 3: //NewExhibit: "hidden"
                    if (this.comboBox1.SelectedIndex == 0)
                    {
                        this.headline = Headline.NewExhibit;
                    }
                    else
                    {
                        this.headline = Headline.LoadExhibit;
                    }
                    break;
                case 4: //EditExhibit: "hidden"
                    if (this.comboBox1.SelectedIndex == 0)
                    {
                        this.contentButton2 = "Laden";
                    }
                    else
                    {
                        this.contentButton2 = "Löschen";
                    }
                    break;
                case 5: //ExhibitionPlane: "hidden"
                    break;
                case 6: //ExhibitionPlaneDef: "hidden"
                    break;
                case 7: //ExhibitionPlaneVal: "hidden"
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewImage: "hidden"
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
            updateLayout();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            { 
                default:
                    break;
                case 0: //Start: "load existing exhibition"
                    MessageBox.Show("Dialog: Ausstellung laden");
                    this.headline = Headline.Exhibition;
                    break;
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "hidden"
                    break;
                case 5: //ExhibitionPlane: "load exisiting definition of the exhibition plane"
                    MessageBox.Show("Dialog: Ausstellungsebene laden");
                    this.headline = Headline.Exhibition;
                    break;
                case 6: //ExhibitionPlaneDef: "hidden"
                    break;
                case 7: //ExhibitionPlaneVal: "hidden"
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewImage: "hidden"
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
            updateLayout();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start: "define new exhibition"
                    MessageBox.Show("Neue Ausstellung definieren");
                    this.headline = Headline.ExhibitionPlane;
                    break;
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "edit existing exhibit"
                    MessageBox.Show("Exponat bearbeiten");
                    this.headline = Headline.EditExhibit;
                    break;
                case 3: //NewExhibit: "load existing exhibit"
                    MessageBox.Show("Dialog: Exponat laden");
                    this.headline = Headline.EditExhibit;
                    break;
                case 4: //EditExhibit: ""
                    break;
                case 5: //ExhibitionPlane: "define new exhibition plane"
                    MessageBox.Show("Neue Ausstellungsebene definieren");
                    this.headline = Headline.ExhibitionPlaneDef;
                    break;
                case 6: //ExhibitionPlaneDef: "hidden"
                    break;
                case 7: //ExhibitionPlaneVal: "hidden"
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewImage: "hidden"
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
            updateLayout();
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
                case 2: //LoadExhibit: "remove exhibit from exhibition's list of exhibits"
                    MessageBox.Show("Bestehendes Exponat löschen");
                    this.headline = Headline.Exhibition;
                    break;
                case 3: //NewExhibit: "define new exhibit"
                    MessageBox.Show("Neues Exponat definieren");
                    this.headline = Headline.ExhibitDef;
                    break;
                case 4: //EditExhibit:
                    break;
                case 5: //ExhibitionPlane:
                    break;
                case 6: //ExhibitionPlaneDef:
                    break;
                case 7: //ExhibitionPlaneVal:
                    break;
                case 8: //ExhibitionPlaneDone:
                    break;
                case 9: //NewImage:
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
            updateLayout();             
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
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "load a text-file"
                    MessageBox.Show("Dialog: Text laden");
                    break;
                case 5: //ExhibitionPlane: "back to the start"
                    MessageBox.Show("Zurück zum Start");
                    this.headline = Headline.Start;
                    break;
                case 6: //ExhibitionPlaneDef: "back to exhibition plane"
                    MessageBox.Show("Zurück zur Ausstellungsebene");
                    this.headline = Headline.ExhibitionPlane;
                    break;
                case 7: //ExhibitionPlaneVal: "back to exhibition plane"
                    MessageBox.Show("Zurück zur Ausstellungsebene");
                    this.headline = Headline.ExhibitionPlane;
                    break;
                case 8: //ExhibitionPlaneDone: "hidden"
                    break;
                case 9: //NewImage: "hidden"
                    break;
                case 10: //ExhibitDef: "back to the exhibition"
                    MessageBox.Show("Zurück zur Ausstellung");
                    this.headline = Headline.Exhibition;
                    break;
                case 11: //ExhibitVal: "back to the exhibition"
                    MessageBox.Show("Zurück zur Ausstellung");
                    this.headline = Headline.Exhibition;
                    break;
                case 12: //ExhibitDone: "hidden"
                    break;
                case 13: //Settings: "back to the exhibition"
                    MessageBox.Show("Zurück zur Ausstellung");
                    this.headline = Headline.Exhibition;
                    break;
            }
            updateLayout();
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
                case 1: //Exhibition: "hidden"
                    break;
                case 2: //LoadExhibit: "hidden"
                    break;
                case 3: //NewExhibit: "hidden"
                    break;
                case 4: //EditExhibit: "editing done"
                    MessageBox.Show("Exponat speichern und zurück zur Ausstellung");
                    this.headline = Headline.Exhibition;
                    break;
                case 5: //ExhibitionPlane: "hidden"
                    break;
                case 6: //ExhibitionPlaneDef: "start or abort definition of exhibition plane"
                    MessageBox.Show("Start oder Abbruch der Definition der Ausstellungsebene");
                    this.headline = Headline.ExhibitionPlaneVal;
                    break;
                case 7: //ExhibitionPlaneVal: "abort validation of exhibition plane"
                    MessageBox.Show("Start oder Abbruch der Validierung der Ausstellungsebene");
                    this.headline = Headline.ExhibitionPlaneDone;
                    break;
                case 8: //ExhibitionPlaneDone: "abort validation of exhibition plane"
                    MessageBox.Show("Validierung der Ausstellungsebene (nicht) erfolgreich");
                    this.headline = Headline.Exhibition;
                    break;
                case 9: //NewImage: "hidden"
                    break;
                case 10: //ExhibitDef: "start or abort definition of exhibit"
                    MessageBox.Show("Start oder Abbruch der Definition des Exponats");
                    this.headline = Headline.ExhibitVal;
                    break;
                case 11: //ExhibitVal: "abort validation of exhibit"
                    MessageBox.Show("Start oder Abbruch der Validierung des Exponats");
                    this.headline = Headline.ExhibitDone;
                    break;
                case 12: //ExhibitDone: "abort validation of exhibition plane"
                    MessageBox.Show("Validierung des Exponats (nicht) erfolgreich");
                    this.headline = Headline.Exhibition;
                    break;
                case 13: //Settings:
                    break;
            }
            updateLayout();
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
