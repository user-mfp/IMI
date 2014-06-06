using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace IMI_Administration
{
    class Exhibit
    {
        #region DECLARATIONS
        private string name;
        private Point3D position;
        private string path;
        private string description;
        private List<Image> images;
        private List<string> imagePaths;
        #endregion

        #region CONSTRUCTORS
        public Exhibit(string name, Point3D position)
        {
            this.name = name;
            this.position = position;
        }

        public Exhibit(string name, Point3D position, string path, string description, List<Image> images, List<string> imagePaths)
        {
            this.name = name;
            this.position = position;
            this.path = path;
            this.description = description;
            this.images = images;
            this.imagePaths = imagePaths;
        }
        #endregion

        #region SAVE AND LOAD
        public string getPath()
        {
            return this.path;
        }

        public void setPath(string path)
        {
            this.path = path;
        }        
        #endregion

        #region NAME
        public string getName()
        {
            return this.name;
        }

        public void setName(string name)
        {
            this.name = name;
        }
        #endregion

        #region DESCRIPTION
        public string getDescription()
        {
            return this.description;
        }

        public void setDescription(string description)
        {
            this.description = description;
        }
        #endregion

        #region IMAGES
        public List<Image> getImages()
        {
            return this.images;
        }
        
        public Image getImage(int index)
        {
            return this.images[index];
        }

        public void addImages(List<Image> images)
        {
            foreach (Image image in images)
            {
                this.images.Add(image);
            }
        }

        public void addImage(Image image)
        {
            this.images.Add(image);
        }

        public void changeImage(int index, Image image)
        {
            this.images[index] = image;
        }

        public void setImages(List<Image> images)
        {
            this.images = images;
        }

        public void removeImage(Image image)
        {
            this.images.Remove(image);
        }
        #endregion
    }
}
