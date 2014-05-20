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
        // Menu states determine the layout (visibility, labeling and functions)
        private enum Headline {
            Start = 0,
            Exhibition, //1
            ExhibitionDef, //2
            ExhibitionVal, //3
            ExhibitionDone, //4
            ExhibitionPlane, //5
            ExhibitionPlaneDef, //6
            ExhibitionPlaneVal, //7
            ExhibitionPlaneDone, //8
            Exhibit, //9
            ExhibitDef, //10
            ExhibitVal, //11
            ExhibitDone, //12
            Settings //13
        };
        private Headline headline;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region LAYOUT
        private void updateLayout()
        {
            switch ((int)this.headline)
            { 
                default: //0: Start
                    loadStart();
                    break;
                case 1: //Exhibition
                    loadExhibition();
                    break;
                case 2: //ExhibitionDef
                    loadExhibitionDef();
                    break;
                case 3: //ExhibitionVal
                    loadExhibitionVal();
                    break;
                case 4: //ExhibitionDone
                    loadExhibitionDone();
                    break;
                case 5: //ExhibitionPlane
                    loadExhibitionPlane();
                    break;
                case 6: //ExhibitionPlaneDef
                    loadExhibitionPlaneDef();
                    break;
                case 7: //ExhibitionPlaneVal
                    loadExhibitionPlaneVal();
                    break;
                case 8: //ExhibitionDone
                    loadExhibitionPlaneDone();
                    break;
                case 9: //Exhibit
                    loadExhibit();
                    break;
                case 10: //ExhibitDef
                    loadExhibitDef();
                    break;
                case 11: //ExhibitVal
                    loadExhibitVal();
                    break;
                case 12: //ExhibitDone
                    loadExhibitDone();
                    break;
                case 13: //Settings
                    loadSettings();
                    break;
            }
        }

        private void loadSettings()
        {
            // Labels
            this.label1.Content = "EINSTELLUNGEN";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = "laden";
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Content = "neu";
            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibitDone()
        {
            // Labels
            this.label1.Content = "EXPONAT XY - ERGEBNIS";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "- Position erfolgreich definiert." + '\n' + "oder" + '\n' + "- Position nicht erfolgreich definiert.";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Content = "OK";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibitVal()
        {
            // Labels
            this.label1.Content = "EXPONAT XY - VALIDIERUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "[Instruktionen]";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Content = "OK";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibitDef()
        {
            // Labels
            this.label1.Content = "EXPONAT XY - DEFINITION";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "[Instruktionen]";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Content = "OK";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibit()
        {
            // Labels
            this.label1.Content = "EXPONAT XY";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            if (this.comboBox1.SelectedIndex == 0) // Load new image
            {
                this.button1.Visibility = Visibility.Hidden;

                this.button2.Content = "laden";
                this.button2.Visibility = Visibility.Visible;

                this.button5.Visibility = Visibility.Hidden;

                this.textBox1.Text = "";
            }
            else // Edit existing image
            {
                this.button1.Visibility = Visibility.Hidden;

                this.button2.Content = "ändern";
                this.button2.Visibility = Visibility.Visible;

                this.button5.Content = "löschen";
                this.button5.Visibility = Visibility.Visible;

                this.textBox1.Text = "[Text des " + this.comboBox1.SelectedIndex + ". Bildes]";
            }

            this.button3.Content = "laden";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Content = "OK";
            this.button4.Visibility = Visibility.Visible;

            // Boxes
            this.comboBox1.Visibility = Visibility.Visible;

            this.textBox1.Visibility = Visibility.Visible;
        }

        private void loadExhibitionPlaneDone()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNGSEBENE - ERGEBNIS";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "- Ebene erfolgreich definiert." + '\n' + "oder" + '\n' + "- Ebene nicht erfolgreich definiert.";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Content = "OK";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Visibility = Visibility.Hidden;
            
            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibitionPlaneVal()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNGSEBENE - VALIDIERUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "[Instruktionen]";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Content = "OK";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibitionPlaneDef()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNGSEBENE - DEFINITION";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Content = "[Instruktionen]";
            this.label2.Visibility = Visibility.Visible;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Content = "OK";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Content = "zurück";
            this.button4.Visibility = Visibility.Visible;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibitionPlane()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNGSEBENE";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = "laden";
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = "neu";
            this.button2.Visibility = Visibility.Visible;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibitionDone()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNG - ERGEBNIS";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibitionVal()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNG - VALIDIERUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibitionDef()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNG - DEFINITION";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadExhibition()
        {
            // Labels
            this.label1.Content = "AUSSTELLUNG";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Visibility = Visibility.Hidden;

            this.button2.Visibility = Visibility.Hidden;

            this.button3.Visibility = Visibility.Hidden;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Visibility = Visibility.Hidden;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }

        private void loadStart()
        {
            // Labels
            this.label1.Content = "START";
            this.label1.Visibility = Visibility.Visible;

            this.label2.Visibility = Visibility.Hidden;

            // Buttons
            this.button1.Content = "laden";
            this.button1.Visibility = Visibility.Visible;

            this.button2.Content = "neu";
            this.button2.Visibility = Visibility.Visible;

            this.button3.Content = "beenden";
            this.button3.Visibility = Visibility.Visible;

            this.button4.Visibility = Visibility.Hidden;

            this.button5.Content = "Einstellungen";
            this.button5.Visibility = Visibility.Visible;

            // Boxes
            this.comboBox1.Visibility = Visibility.Hidden;

            this.textBox1.Visibility = Visibility.Hidden;
        }
        #endregion

        #region EVENTS
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateLayout();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            { 
                default:
                    break;
                case 0: //Start
                    break;
                case 1: //Exhibition
                    break;
                case 2: //ExhibitionDef
                    break;
                case 3: //ExhibitionVal
                    break;
                case 4: //ExhibitionDone
                    break;
                case 5: //ExhibitionPlane
                    break;
                case 6: //ExhibitionPlaneDef
                    break;
                case 7: //ExhibitionPlaneVal
                    break;
                case 8: //ExhibitionPlaneDone
                    break;
                case 9: //Exhibit
                    break;
                case 10: //ExhibitDef
                    break;
                case 11: //ExhibitVal
                    break;
                case 12: //ExhibitDone
                    break;
                case 13: //Settings
                    break;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start
                    break;
                case 1: //Exhibition
                    break;
                case 2: //ExhibitionDef
                    break;
                case 3: //ExhibitionVal
                    break;
                case 4: //ExhibitionDone
                    break;
                case 5: //ExhibitionPlane
                    break;
                case 6: //ExhibitionPlaneDef
                    break;
                case 7: //ExhibitionPlaneVal
                    break;
                case 8: //ExhibitionPlaneDone
                    break;
                case 9: //Exhibit
                    break;
                case 10: //ExhibitDef
                    break;
                case 11: //ExhibitVal
                    break;
                case 12: //ExhibitDone
                    break;
                case 13: //Settings
                    break;
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (this.headline == Headline.Start)
                this.Close();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start
                    break;
                case 1: //Exhibition
                    break;
                case 2: //ExhibitionDef
                    break;
                case 3: //ExhibitionVal
                    break;
                case 4: //ExhibitionDone
                    break;
                case 5: //ExhibitionPlane
                    break;
                case 6: //ExhibitionPlaneDef
                    break;
                case 7: //ExhibitionPlaneVal
                    break;
                case 8: //ExhibitionPlaneDone
                    break;
                case 9: //Exhibit
                    break;
                case 10: //ExhibitDef
                    break;
                case 11: //ExhibitVal
                    break;
                case 12: //ExhibitDone
                    break;
                case 13: //Settings
                    break;
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start
                    break;
                case 1: //Exhibition
                    break;
                case 2: //ExhibitionDef
                    break;
                case 3: //ExhibitionVal
                    break;
                case 4: //ExhibitionDone
                    break;
                case 5: //ExhibitionPlane
                    break;
                case 6: //ExhibitionPlaneDef
                    break;
                case 7: //ExhibitionPlaneVal
                    break;
                case 8: //ExhibitionPlaneDone
                    break;
                case 9: //Exhibit
                    break;
                case 10: //ExhibitDef
                    break;
                case 11: //ExhibitVal
                    break;
                case 12: //ExhibitDone
                    break;
                case 13: //Settings
                    break;
            } switch ((int)this.headline)
            {
                default:
                    break;
                case 0: //Start
                    this.Close();
                    break;
                case 1: //Exhibition
                    break;
                case 2: //ExhibitionDef
                    break;
                case 3: //ExhibitionVal
                    break;
                case 4: //ExhibitionDone
                    break;
                case 5: //ExhibitionPlane
                    break;
                case 6: //ExhibitionPlaneDef
                    break;
                case 7: //ExhibitionPlaneVal
                    break;
                case 8: //ExhibitionPlaneDone
                    break;
                case 9: //Exhibit
                    break;
                case 10: //ExhibitDef
                    break;
                case 11: //ExhibitVal
                    break;
                case 12: //ExhibitDone
                    break;
                case 13: //Settings
                    break;
            }
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
    }
}
