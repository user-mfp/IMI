using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;
using Microsoft.Win32;

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

            initMembers();

            //Init Layout
            this.state = State.start;
        }

        private void initMembers()
        {
            safeFileDialog = new SaveFileDialog();
            safeFileDialog.Filter = "XML-Files|*.xml";
            safeFileDialog.Title = "Datei Speichern";

            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML-Files|*.xml";
            openFileDialog.Title = "Datei Speichern";

            this.corners.Add(new Point3D(0.1, 0.1, 0.1));
            this.corners.Add(new Point3D(1000.1, 0.1, 0.1));
            this.corners.Add(new Point3D(0.1, 1000.1, 0.1));
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
        private SaveFileDialog safeFileDialog;
        private OpenFileDialog openFileDialog;
        private string testPath = @"D:\Master\TestFolder\InOutSimulator\test.xml";
        private string exhibitionTestPath = @"D:\Master\TestFolder\InOutSimulator\exhibitionTest.xml";
        private string exhibitionPlaneTestPath = @"D:\Master\TestFolder\InOutSimulator\exhibitionPlaneTest.xml";
        private string exhibit1TestPath = @"D:\Master\TestFolder\InOutSimulator\exhibit1Test.xml";
        private string exhibit2TestPath = @"D:\Master\TestFolder\InOutSimulator\exhibit2Test.xml";
        private Point3D center = new Point3D(500.1, 500.1, 0.1);
        private Point3D point = new Point3D(111.222, 333.444, 555.666);
        private List<Point3D> corners = new List<Point3D>();
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
            writer.WriteElementString("Corner", this.corners[0].ToString().Replace(',', '.').Replace(';', ' '));
            writer.WriteElementString("Corner", this.corners[1].ToString().Replace(',', '.').Replace(';', ' '));
            writer.WriteElementString("Corner", this.corners[2].ToString().Replace(',', '.').Replace(';', ' '));
            // End-Tag von 'ExhibitionPlane'
            writer.WriteEndElement();
            // Element 'Exhibit' mit Attributen
            writer.WriteStartElement("Exhibits");
            writer.WriteStartElement("Exhibit");
            writer.WriteAttributeString("Position", this.center.ToString().Replace(',', '.').Replace(';', ' '));
            writer.WriteAttributeString("Bild", "D:\\Master\\TestFolder\\InOutSimulator\\center.jpg");
            writer.WriteAttributeString("Text", "D:\\Master\\TestFolder\\InOutSimulator\\center.txt");
            writer.WriteValue("Center");
            writer.WriteEndElement();
            writer.WriteStartElement("Exhibit");
            writer.WriteAttributeString("Position", this.point.ToString().Replace(',', '.').Replace(';', ' '));
            writer.WriteAttributeString("Bild", "D:\\Master\\TestFolder\\InOutSimulator\\point.jpg");
            writer.WriteAttributeString("Text", "D:\\Master\\TestFolder\\InOutSimulator\\point.txt");
            writer.WriteValue("Point");
            writer.WriteEndElement();
            writer.WriteEndElement();
            // End-Tag des Stammelements
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        private void writeExhibition()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(this.exhibitionTestPath, settings);
            writer.WriteStartDocument();
            // Start-Tag des Stammelements
            writer.WriteStartElement("Exhibition");
            // Start-Tag von 'ExhibitionPlane'
            writer.WriteStartElement("ExhibitionPlane");
            writer.WriteAttributeString("Path", this.exhibitionPlaneTestPath);
            // End-Tag von 'ExhibitionPlane'
            writer.WriteEndElement();

            writeExhibitionPlane();

            // Element 'Exhibit' mit Attributen
            writer.WriteStartElement("Exhibits");
            writer.WriteStartElement("Exhibit");
            writer.WriteAttributeString("Path", this.exhibit1TestPath);
            writer.WriteEndElement();
            writer.WriteStartElement("Exhibit");
            writer.WriteAttributeString("Path", this.exhibit2TestPath);
            writer.WriteEndElement();
            writer.WriteEndElement();
            // End-Tag des Stammelements
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        private void writeExhibitionPlane()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(this.exhibitionPlaneTestPath, settings);
            writer.WriteStartDocument();
            // Start-Tag von 'ExhibitionPlane'
            writer.WriteStartElement("ExhibitionPlane");
            writer.WriteElementString("Corner", this.corners[0].ToString().Replace(',', '.').Replace(';', ' '));
            writer.WriteElementString("Corner", this.corners[1].ToString().Replace(',', '.').Replace(';', ' '));
            writer.WriteElementString("Corner", this.corners[2].ToString().Replace(',', '.').Replace(';', ' '));
            // End-Tag von 'ExhibitionPlane'
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        private void writeExhibit(int index)
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

        private void readExhibit(int index)
        {

        }
        #endregion

        #region EVENTS
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch ((int)this.state)
            { 
                case 0: //Start
                    this.safeFileDialog.ShowDialog();
                    this.testPath = safeFileDialog.FileName;
                    writeTest();
                    break;
                case 1: //Exhibition
                    writeExhibition();
                    break;
                case 2: //ExhibitionPlane
                    writeExhibitionPlane();
                    break;
                case 3: //Exhibit
                    writeExhibit(0);
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
                    this.openFileDialog.ShowDialog();
                    this.testPath = this.openFileDialog.FileName;
                    readTest();
                    break;
                case 1: //Exhibition
                    readExhibition();
                    break;
                case 2: //ExhibitionPlane
                    readExhibitionPlane();
                    break;
                case 3: //Exhibit
                    readExhibit(0);
                    break;
                default:
                    break;
            }
        }


        private void comboBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (this.comboBox1.SelectedIndex)
            { 
                case 0: //Start
                    this.state = State.start;
                    break;
                case 1:
                    this.state = State.exhibition;
                    break;
                case 2:
                    this.state = State.exhibitionPlane;
                    break;
                case 3:
                    this.state = State.exhibit;
                    break;
            }
        }
        #endregion
    }
}
