using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Windows.Media.Imaging;
using System;

namespace IMI_Administration
{
    class FileHandler
    {
        #region DECLARATIONS
        // Writer and reader
        private XmlWriterSettings xmlWriterSettings;
        private string exhibitionFolder;
        
        // Exhibition
        private Exhibition TMP_EXHIBITION;
        private Exhibit TMP_EXHIBIT;
        #endregion

        #region CONSTRUCTORS
        public FileHandler()
        { 
            // Initialize settings for XmlWriter
            this.xmlWriterSettings = new XmlWriterSettings();
            this.xmlWriterSettings.Indent = true;
        }

        public FileHandler(string exhibitionFolder)
        {
            // Initialize settings for XmlWriter
            this.xmlWriterSettings = new XmlWriterSettings();
            this.xmlWriterSettings.Indent = true;

            // Set the exhibition's main folder
            this.exhibitionFolder = exhibitionFolder;
        }
        #endregion

        #region LOADING
        public Exhibition loadExhibition(string path)
        {
            this.exhibitionFolder = path.Substring(0, (path.LastIndexOf('.'))); // Find the exhibition's folder including the exhibition's name at the end

            XmlReader exhibitionReader = XmlReader.Create(path); // Create XmlReader for file's path
            while (exhibitionReader.Read())
            {
                if (exhibitionReader.NodeType == XmlNodeType.Element)
                {
                    switch (exhibitionReader.Name)
                    { 
                        case "Exhibition":
                            this.TMP_EXHIBITION = new Exhibition();

                            while (exhibitionReader.MoveToNextAttribute())
                            {
                                switch (exhibitionReader.Name)
                                { 
                                    case "Name":
                                        this.TMP_EXHIBITION.setName(exhibitionReader.Value);
                                        break;
                                    case "Path":
                                        this.TMP_EXHIBITION.setPath(exhibitionReader.Value);
                                        break;
                                    case "BackgroundImage":
                                        this.TMP_EXHIBITION.setBackgroundImage(loadImage(exhibitionReader.Value));
                                        break;
                                    case "Overview":
                                        this.TMP_EXHIBITION.setOverview(loadImage(exhibitionReader.Value));
                                        break;
                                    case "UserPosition":
                                        this.TMP_EXHIBITION.setUserPosition(Point3D.Parse(exhibitionReader.Value));
                                        break;
                                    case "Threshold":
                                        this.TMP_EXHIBITION.setThreshold(double.Parse(exhibitionReader.Value));
                                        break;
                                    case "SelectionTime":
                                        this.TMP_EXHIBITION.setSelectionTime(double.Parse(exhibitionReader.Value));
                                        break;
                                    case "LockTime":
                                        this.TMP_EXHIBITION.setLockTime(double.Parse(exhibitionReader.Value));
                                        break;
                                    case "SlideTime":
                                        this.TMP_EXHIBITION.setSlideTime(double.Parse(exhibitionReader.Value));
                                        break;
                                    case "EndWait":
                                        this.TMP_EXHIBITION.setEndWait(double.Parse(exhibitionReader.Value));
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "ExhibitionPlane":
                            while (exhibitionReader.MoveToNextAttribute())
                            {
                                if (exhibitionReader.Name == "Path")
                                {
                                    this.TMP_EXHIBITION.setExhibitionPlane(loadExhibitionPlane(exhibitionReader.Value));
                                }
                            }
                            break;
                        case "Exhibits": // There are exhibits in the exhibition (exhibtion.getExhibits() != null)
                            this.TMP_EXHIBITION.setExhibits(new List<Exhibit>());
                            break;
                        case "Exhibit":
                            while (exhibitionReader.MoveToNextAttribute())
                            {
                                if (exhibitionReader.Name == "Path")
                                {
                                    this.TMP_EXHIBITION.addExhibit(loadExhibit(exhibitionReader.Value));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return this.TMP_EXHIBITION;
        }

        public GeometryHandler.Plane loadExhibitionPlane(string path)
        {
            List<Point3D> corners = new List<Point3D>();

            XmlReader exhibitionPlaneReader = XmlReader.Create(path); // Create XmlReader for file's path
            while (exhibitionPlaneReader.Read())
            {
                if (exhibitionPlaneReader.NodeType == XmlNodeType.Element)
                {
                    switch (exhibitionPlaneReader.Name)
                    { 
                        case "ExhibitionPlane":                            
                            break;
                        case "Corner":
                            while (exhibitionPlaneReader.MoveToNextAttribute())
                            {
                                if (exhibitionPlaneReader.Name == "Position")
                                {
                                    corners.Add(Point3D.Parse(exhibitionPlaneReader.Value));
                                }
                            }                            
                            break;
                        default:
                            break;
                    }
                }
            }

            return new GeometryHandler.Plane(corners[0], corners[1], corners[2]);
        }

        public Exhibit loadExhibit(string path)
        {
            Dictionary<string, BitmapImage> images = new Dictionary<string, BitmapImage>(); // Prepare Dictionary for storage of coming images

            XmlReader exhibitReader = XmlReader.Create(path); // Create XmlReader for file's path
            while (exhibitReader.Read())
            {
                if (exhibitReader.NodeType == XmlNodeType.Element)
                {
                    switch (exhibitReader.Name)
                    {
                        case "Exhibit":
                            this.TMP_EXHIBIT = new Exhibit();

                            while (exhibitReader.MoveToNextAttribute())
                            {
                                switch (exhibitReader.Name)
                                {
                                    case "Name":
                                        this.TMP_EXHIBIT.setName(exhibitReader.Value);
                                        break;
                                    case "Path":
                                        this.TMP_EXHIBIT.setPath(exhibitReader.Value);
                                        break;
                                    case "KernelSize":
                                        this.TMP_EXHIBIT.setKernelSize(double.Parse(exhibitReader.Value));
                                        break;
                                    case "KernelWeight":
                                        this.TMP_EXHIBIT.setKernelWeight(double.Parse(exhibitReader.Value));
                                        break;
                                    case "Description":
                                        this.TMP_EXHIBIT.setDescription(exhibitReader.Value);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                        case "Position":
                            while (exhibitReader.MoveToNextAttribute())
                            {
                                if (exhibitReader.Name == "Position")
                                {
                                    this.TMP_EXHIBIT.setPosition(Point3D.Parse(exhibitReader.Value));
                                }
                            }
                            break;
                        case "Images": // There are images in the exhibit
                            break;
                        case "Image":
                            while (exhibitReader.MoveToNextAttribute())
                            {
                                if (exhibitReader.Name == "Path")
                                {
                                    KeyValuePair<string, BitmapImage> image = loadImage(exhibitReader.Value);
                                    images.Add(image.Key, image.Value);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            this.TMP_EXHIBIT.setImages(images);

            return this.TMP_EXHIBIT;
        }

        public KeyValuePair<string, BitmapImage> loadImage(string path)
        {
            BitmapImage image = new BitmapImage(new Uri(path));
            return new KeyValuePair<string, BitmapImage>(path, image);
        }
        #endregion

        #region SAVING
        public void saveExhibition(Exhibition exhibition)
        {
            this.exhibitionFolder = exhibition.getPath().Substring(0, (exhibition.getPath().LastIndexOf('.'))); // Find the exhibition's folder including the exhibition's name at the end

            XmlWriter exhibitionWriter = XmlWriter.Create(exhibition.getPath(), this.xmlWriterSettings); // Create XmlWriter for file's path
            exhibitionWriter.WriteStartDocument(); // Start writing the file

            //<Exhibition>
            exhibitionWriter.WriteStartElement("Exhibition");
            exhibitionWriter.WriteAttributeString("Name", exhibition.getName());
            exhibitionWriter.WriteAttributeString("Path", exhibition.getPath());
            if (exhibition.getBackgroundImage().Key != null)
                exhibitionWriter.WriteAttributeString("BackgroundImage", exhibition.getBackgroundImage().Key);
            if (exhibition.getOverview().Key != null)
                exhibitionWriter.WriteAttributeString("Overview", exhibition.getOverview().Key);          
            exhibitionWriter.WriteAttributeString("UserPosition", exhibition.getUserPosition().ToString().Replace(',', '.').Replace(';', ' '));
            exhibitionWriter.WriteAttributeString("Threshold", exhibition.getThreshold().ToString().Replace(',', '.'));
            exhibitionWriter.WriteAttributeString("SelectionTime", exhibition.getSelectionTime().ToString());
            exhibitionWriter.WriteAttributeString("LockTime", exhibition.getLockTime().ToString());
            exhibitionWriter.WriteAttributeString("SlideTime", exhibition.getSlideTime().ToString());
            exhibitionWriter.WriteAttributeString("EndWait", exhibition.getEndWait().ToString());
            
            saveExhibitionPlane(exhibition.getExhibitionPlane());
            
                //<ExhibitionPlane>
            exhibitionWriter.WriteStartElement("ExhibitionPlane");
            exhibitionWriter.WriteAttributeString("Path", this.exhibitionFolder + "_Plane.xml");
            exhibitionWriter.WriteEndElement(); 
                //</ExhibitionPlane>

            if (exhibition.getExhibits() != null) // There are exhibits in the exhibition
            {
                //<Exhibits>
                exhibitionWriter.WriteStartElement("Exhibits");

                foreach (Exhibit exhibit in exhibition.getExhibits())
                {
                    if (exhibit.getPath() == null) // No Path yet
                        exhibit.setPath(this.exhibitionFolder + "_" + exhibit.getName() + ".xml");
                    
                    saveExhibit(exhibit);

                    //<Exhibit>
                    exhibitionWriter.WriteStartElement("Exhibit");
                    exhibitionWriter.WriteAttributeString("Name", exhibit.getName());
                    exhibitionWriter.WriteAttributeString("Path", exhibit.getPath());
                    exhibitionWriter.WriteEndElement(); 
                    //</Exhibit>
                }

                exhibitionWriter.WriteEndElement();
                //</Exhibits>
            }

            exhibitionWriter.WriteEndElement(); 
            //</Exhibition>

            exhibitionWriter.WriteEndDocument(); // Stop writing the file
            exhibitionWriter.Close(); // Close the file
        }

        public void saveExhibitionPlane(GeometryHandler.Plane exhibitionPlane)
        {
            string path = this.exhibitionFolder + "_Plane.xml";
            XmlWriter exhibitionPlaneWriter = XmlWriter.Create(path, this.xmlWriterSettings) ; 

            //<ExhibitionPlane>
            exhibitionPlaneWriter.WriteStartElement("ExhibitionPlane");
                //<Corner> #1
            exhibitionPlaneWriter.WriteStartElement("Corner");
            exhibitionPlaneWriter.WriteAttributeString("Position", exhibitionPlane.Start.ToString().Replace(',', '.').Replace(';', ' '));
            exhibitionPlaneWriter.WriteEndElement();
                //</Corner> #1
                //<Corner> #2
            exhibitionPlaneWriter.WriteStartElement("Corner");
            exhibitionPlaneWriter.WriteAttributeString("Position", exhibitionPlane.End1.ToString().Replace(',', '.').Replace(';', ' '));
            exhibitionPlaneWriter.WriteEndElement(); 
                //</Corner> #2
                //<Corner> #3
            exhibitionPlaneWriter.WriteStartElement("Corner");
            exhibitionPlaneWriter.WriteAttributeString("Position", exhibitionPlane.End2.ToString().Replace(',', '.').Replace(';', ' '));
            exhibitionPlaneWriter.WriteEndElement(); 
                //</Corner> #3
            exhibitionPlaneWriter.WriteEndElement();
            //</ExhibitionPlane>

            exhibitionPlaneWriter.WriteEndDocument(); // Stop writing the file
            exhibitionPlaneWriter.Close(); // Close the file
        }

        public void saveExhibit(Exhibit exhibit)
        {
            XmlWriter exhibitWriter = XmlWriter.Create(exhibit.getPath(), this.xmlWriterSettings); // Create XmlWriter for file's path
            exhibitWriter.WriteStartDocument(); // Start writing the file

            //<Exhibit>
            exhibitWriter.WriteStartElement("Exhibit");
            exhibitWriter.WriteAttributeString("Name", exhibit.getName());
            exhibitWriter.WriteAttributeString("Path", exhibit.getPath());
            exhibitWriter.WriteAttributeString("KernelSize", exhibit.getKernelSize().ToString().Replace(',', '.'));
            exhibitWriter.WriteAttributeString("KernelWeight", exhibit.getKernelWeight().ToString().Replace(',', '.'));

            if (exhibit.getDescription() != null) // The exhibit has a description
            {
                exhibitWriter.WriteAttributeString("Description", exhibit.getDescription());
            }

                //<Position>
            exhibitWriter.WriteStartElement("Position");
            exhibitWriter.WriteAttributeString("Position", exhibit.getPosition().ToString().Replace(',', '.').Replace(';', ' '));
            exhibitWriter.WriteEndElement();
                //</Position>

            if (exhibit.getImages() != null) // The exhibit has images
            {
                //<Images>
                exhibitWriter.WriteStartElement("Images");

                foreach (KeyValuePair<string, BitmapImage> image in exhibit.getImages())
                { 
                    //<Image>
                    exhibitWriter.WriteStartElement("Image");
                    exhibitWriter.WriteAttributeString("Path", image.Key);
                    exhibitWriter.WriteEndElement();
                    //</Image>
                }

                exhibitWriter.WriteEndElement();
                //</Images>
            }
            exhibitWriter.WriteEndElement();
            //</Exhibit>

            exhibitWriter.WriteEndDocument(); // Stop writing the file
            exhibitWriter.Close(); // Close the file
        }
        #endregion
    }
}
