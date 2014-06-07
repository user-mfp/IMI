using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Xml;

namespace IMI_Administration
{
    class FileHandler
    {
        #region DECLARATIONS
        // Writer and reader
        private XmlWriterSettings xmlWriterSettings;

        // Dialogs
        private OpenFileDialog loadConfigDialog;
        private OpenFileDialog loadTextDialog;
        private OpenFileDialog loadImageDialog;
        private SaveFileDialog saveConfigDialog;
        private SaveFileDialog saveTextDialog;
        private string TMP_PATH;

        private string exhibitionFolder;
        
        // Exhibition
        private Exhibition TMP_EXHIBITION;
        private GeometryHandler.Plane TMP_EXHIBITION_PLANE;
        private Exhibit TMP_EXHIBIT;
        #endregion

        #region CONSTRUCTORS
        public FileHandler()
        { 
            // Initialize settings for XmlWriter
            this.xmlWriterSettings = new XmlWriterSettings();
            this.xmlWriterSettings.Indent = true;
            // Initialize dialogs
            this.loadConfigDialog = new OpenFileDialog();
            this.loadConfigDialog.Filter = "Config-Files|*.xml";
            this.loadConfigDialog.Title = "Konfigurationsdatei laden";
            this.loadTextDialog = new OpenFileDialog();
            this.loadTextDialog.Filter = "Text-Files|*.txt";
            this.loadTextDialog.Title = "Textdatei laden";
            this.loadImageDialog = new OpenFileDialog();
            this.loadImageDialog.Filter = "Image-Files|*.jpg|*.png|*.bmp";
            this.loadImageDialog.Title = "Bilddatei laden";
            this.saveConfigDialog = new SaveFileDialog();
            this.saveConfigDialog.Filter = "Config-Files|*.xml";
            this.saveConfigDialog.Title = "Konfigurationsdatei speichern";
            this.saveTextDialog = new SaveFileDialog(); ;
            this.saveTextDialog.Filter = "Text-Files|*.txt";
            this.saveTextDialog.Title = "Textdatei speichern";
        }

        public FileHandler(string exhibitionFolder)
        {
            // Initialize settings for XmlWriter
            this.xmlWriterSettings = new XmlWriterSettings();
            this.xmlWriterSettings.Indent = true;
            // Initialize dialogs
            this.loadConfigDialog = new OpenFileDialog();
            this.loadConfigDialog.Filter = "Config-Files|*.xml";
            this.loadConfigDialog.Title = "Konfigurationsdatei laden";
            this.loadTextDialog = new OpenFileDialog();
            this.loadTextDialog.Filter = "Text-Files|*.txt";
            this.loadTextDialog.Title = "Textdatei laden";
            this.loadImageDialog = new OpenFileDialog();
            this.loadImageDialog.Filter = "Image-Files|*.jpg|*.png|*.bmp";
            this.loadImageDialog.Title = "Bilddatei laden";
            this.saveConfigDialog = new SaveFileDialog();
            this.saveConfigDialog.Filter = "Config-Files|*.xml";
            this.saveConfigDialog.Title = "Konfigurationsdatei speichern";
            this.saveTextDialog = new SaveFileDialog(); ;
            this.saveTextDialog.Filter = "Text-Files|*.txt";
            this.saveTextDialog.Title = "Textdatei speichern";
            // Set the exhibition's main folder
            this.exhibitionFolder = exhibitionFolder;
        }
        #endregion

        #region LOADING
        public Exhibition loadExhibition()
        {
            // TODO
            // - read exhibition's config-file(s)
            // - build whole exhibition with exhibition plane and all of the exhibits

            return this.TMP_EXHIBITION;
        }

        public GeometryHandler.Plane loadExhibitionPlane()
        {
            return this.TMP_EXHIBITION_PLANE;
        }

        public Exhibit loadExhibit()
        {
            return this.TMP_EXHIBIT;
        }

        public string loadDescription()
        {
            return this.TMP_EXHIBIT.getDescription();
        }

        public Dictionary<string, Image> loadImages()
        {
            Dictionary<string, Image> images = new Dictionary<string, Image>();
            KeyValuePair<string, Image> image = new KeyValuePair<string, Image>();



            return images;
        }

        public KeyValuePair<string, Image> loadImage()
        {
            string imagePath = "";
            Image image = new Image();
            KeyValuePair<string, Image> imagePair = new KeyValuePair<string, Image>(imagePath, image);

            return imagePair;
        }
        #endregion

        #region SAVING
        public void saveExhibition(Exhibition exhibition)
        {
            this.saveConfigDialog.ShowDialog(); // Open dialog...
            this.TMP_PATH = this.saveConfigDialog.FileName; // ... to get file's path
            this.exhibitionFolder = this.TMP_PATH.Substring(0, (this.TMP_PATH.LastIndexOf('.'))); // Find the exhibition's folder including the exhibition's name at the end
            
            XmlWriter exhibitionWriter = XmlWriter.Create(this.TMP_PATH, this.xmlWriterSettings); // Create XmlWriter for file's path
            exhibitionWriter.WriteStartDocument(); // Start writing the file

            //<Exhibition>
            exhibitionWriter.WriteStartElement("Exhibition");
            exhibitionWriter.WriteAttributeString("Name", exhibition.getName());
            exhibitionWriter.WriteAttributeString("Path", this.TMP_PATH);
            exhibitionWriter.WriteAttributeString("Threshold", exhibition.getThreshold().ToString().Replace(',', '.'));
            exhibitionWriter.WriteAttributeString("SelectionTime", exhibition.getSelectionTime().ToString());
            exhibitionWriter.WriteAttributeString("LockTime", exhibition.getLockTime().ToString());
            exhibitionWriter.WriteAttributeString("SlideTime", exhibition.getSlideTime().ToString());
            exhibitionWriter.WriteAttributeString("endWait", exhibition.getEndWait().ToString());

                //<UserHeadPosition>
            exhibitionWriter.WriteStartElement("UserHeadPosition");
            exhibitionWriter.WriteAttributeString("Position", exhibition.getUserHeadPosition().ToString().Replace(',', '.').Replace(';', ' '));
            exhibitionWriter.WriteEndElement();
                //</UserHEadPosition>
            
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

                foreach (KeyValuePair<string, Image> image in exhibit.getImages())
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
