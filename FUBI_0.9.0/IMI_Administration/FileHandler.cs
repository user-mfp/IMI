using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace IMI_Administration
{
    class FileHandler
    {
        #region DECLARATIONS
        // Dialogs
        private OpenFileDialog loadConfigDialog;
        private OpenFileDialog loadTextDialog;
        private OpenFileDialog loadImageDialog;
        private SaveFileDialog saveConfigDialog;
        private SaveFileDialog saveTextDialog;

        // Exhibition
        private Exhibition exhibition;
        private GeometryHandler.Plane exhibitionPlane;
        private Exhibit exhibit;
        #endregion

        #region INITIALIZATION
        public FileHandler()
        { 
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
        #endregion

        #region LOADING
        public Exhibition loadExhibition()
        {
            // TODO
            // - read exhibition's config-file(s)
            // - build whole exhibition with exhibition plane and all of the exhibits

            return this.exhibition;
        }

        public void loadExhibitionPlane()
        { 
        
        }

        public void loadExhibit()
        { 
        
        }

        public void loadDescription()
        { 
        
        }

        public void loadImage()
        { 
        
        }
        #endregion

        #region SAVING
        public void saveExhibition(Exhibition exhibition)
        {
            string name;
            string path;
            GeometryHandler.Plane exhibitionPlane;
            List<Exhibit> exhibits;
        }

        public void saveExhibitionPlane(GeometryHandler.Plane exhibitionPlane)
        {

        }

        public void saveExhibit(Exhibit exhibit)
        {
            string name;
            Point3D position;
            string path;
            string description;
            List<Image> images;
            List<string> imagePaths;
        }

        public void saveDescription(string description)
        {

        }

        public void saveImages()
        { 
            
        }

        public void saveImage()
        {

        }
        #endregion
    }
}
