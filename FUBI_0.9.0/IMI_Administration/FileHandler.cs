using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

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
        public void loadExhibition()
        { 
        
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

        #endregion
    }
}
