using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;

namespace InOutSimulator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Init Layout
            this.state = State.start;
        }

        #region DECLARATIONS
        //Layout
        private enum State {
            start = 0, 
            exhibition, 
            exhibitionPlane, 
            exhibit 
        };
        private State state;
        private string textBox1Text;
        private string label1Content;
        private string button1Content;
        private string button2Content;

        //Simulation
        private string testPath = @"D:\Master\TestFolder\InOutSimulator\test.xml";
        private List<Point3D> corners;
        #endregion

        #region LAYOUT
        private void updateLayout()
        {
            this.textBox1.Text = this.textBox1Text; 
            this.label1.Content = this.label1Content;
        }

        private void showStart()
        {
            this.Title = "InOutSimulator - Start";

            //Boxes
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("Exhibition");
            this.comboBox1.Items.Add("Exhibition Plane");
            this.comboBox1.Items.Add("Exhibit");

            this.textBox1.Text = "";

            //Buttons
            this.button1.Content = this.button1Content;
            this.button2.Content = this.button2Content;

            //Label
            this.label1.Content = "";
        }

        private void showExhibition()
        {
            this.Title = "InOutSimulator - Exhibition";

            //Boxes
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("Add Exhibition Plane");
            this.comboBox1.Items.Add("Add Exhibit");

            this.textBox1.Text = "";

            //Buttons
            this.button1.Content = this.button1Content;
            this.button2.Content = this.button2Content;

            //Label
            this.label1.Content = "";
        }

        private void showExhibitionPlane()
        {
            this.Title = "InOutSimulator - Exhibition Plane";

            //Boxes
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("Add Corners");

            this.textBox1.Text = "";

            //Buttons
            this.button1.Content = this.button1Content;
            this.button2.Content = this.button2Content;

            //Label
            this.label1.Content = "";
        }

        private void showExhibit()
        {
            this.Title = "InOutSimulator - Exhibit";

            //Boxes
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("Exhibition");
            this.comboBox1.Items.Add("Exhibition Plane");
            this.comboBox1.Items.Add("Exhibit");

            this.textBox1.Text = "";

            //Buttons
            this.button1.Content = this.button1Content;
            this.button2.Content = this.button2Content;

            //Label
            this.label1.Content = "";
        }
        #endregion

        #region OUT
        private void writeTest()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(this.testPath, settings);
            writer.WriteStartDocument();
            // Start-Tag des Stammelements
            writer.WriteStartElement("Exhibition");
            // Start-Tag von 'ExhibitionPlane'
            writer.WriteStartElement("ExhibitionPlane");
            writer.WriteElementString("Corner", "0.0   0.0 0.0");
            writer.WriteElementString("Corner", "1.0   0.0 0.0");
            writer.WriteElementString("Corner", "0.0   1.0 0.0");
            // End-Tag von 'ExhibitionPlane'
            writer.WriteEndElement();
            // Element 'Exhibit' mit Attributen
            writer.WriteStartElement("Exhibits");
            writer.WriteStartElement("Exhibit");
            writer.WriteAttributeString("Position", "0.5    0.5 0.0");
            writer.WriteAttributeString("Bild", "D:\\Master\\TestFolder\\InOutSimulator\\center.jpg");
            writer.WriteAttributeString("Text", "D:\\Master\\TestFolder\\InOutSimulator\\center.txt");
            writer.WriteValue("Center");
            writer.WriteEndElement();
            writer.WriteStartElement("Exhibit");
            writer.WriteAttributeString("Position", "0.0    0.0 0.0");
            writer.WriteAttributeString("Bild", "D:\\Master\\TestFolder\\InOutSimulator\\zero.jpg");
            writer.WriteAttributeString("Text", "D:\\Master\\TestFolder\\InOutSimulator\\zero.txt");
            writer.WriteValue("Zero");
            writer.WriteEndElement();
            writer.WriteEndElement();
            // End-Tag des Stammelements
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        private void writeExhibition()
        {

        }

        private void writeExhibitionPlane()
        {

        }

        private void writeExhibit()
        {

        }
        #endregion

        #region IN
        private void readTest()
        {
            int exhibit = 0;
            List<Point3D> exhibitionPlane = new List<Point3D>();
            Dictionary<int, Point3D> exhibitsPosition = new Dictionary<int, Point3D>();
            Dictionary<int, string> exhibitsImage = new Dictionary<int, string>();
            Dictionary<int, string> exhibitsText = new Dictionary<int, string>();
            XmlReader reader = XmlReader.Create(this.testPath);
            while (reader.Read())
            {
                // prüfen, ob es sich aktuell um ein Element handelt
                if (reader.NodeType == XmlNodeType.Element)
                {
                    // alle relevanten Elemente untersuchen
                    switch (reader.Name)
                    {
                        case "ExhibitionPlane":
                            break;
                        case "Corner":
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "Position")
                                    exhibitionPlane.Add(Point3D.Parse(reader.Value));
                            }
                            break;
                        case "Exhibits":
                            break;
                        case "Exhibit":
                            if (reader.HasAttributes)
                            {
                                // Attributsliste durchlaufen
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "Position")
                                        exhibitsPosition.Add(exhibit, Point3D.Parse(reader.Value));
                                    else if (reader.Name == "Bild")
                                        exhibitsImage.Add(exhibit, reader.Value);
                                    else if (reader.Name == "Text")
                                        exhibitsText.Add(exhibit, reader.Value);
                                }
                            }
                            ++exhibit;
                            break;
                    }
                }
            }
            label1Content = "Corners: " + exhibitionPlane.Count.ToString() + '\n' + "0. ExPos: " + exhibitsPosition[0].ToString(); 
            updateLayout();
        }

        private void readExhibition()
        {

        }

        private void readExhibitionPlane()
        {

        }

        private void readExhibit()
        {

        }
        #endregion

        #region EVENTS
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.state)
            { 
                case 0: //Start
                    writeTest();
                    break;
                case 1: //Exhibition
                    break;
                case 2: //ExhibitionPlane
                    break;
                case 3: //Exhibit
                    break;
                default:
                    break;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.state)
            {
                case 0: //Start
                    readTest();
                    break;
                case 1: //Exhibition
                    break;
                case 2: //ExhibitionPlane
                    break;
                case 3: //Exhibit
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
