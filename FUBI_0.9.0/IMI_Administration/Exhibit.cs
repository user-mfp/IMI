using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace IMI_Administration
{
    class Exhibit
    {
        #region DECLARATIONS
        // Elements
        private Dictionary<string, System.Drawing.Image> images; // string := imagePath (unique)
        // Attributes
        private string name;
        private Point3D position;
        private string path;
        private string description;
        private double kernelSize = 100.0; // Default radius := 100mm
        private double kernelWeight = 1.0; // Default size := 100%
        #endregion

        #region CONSTRUCTORS
        public Exhibit()  // ONLY FOR IMMEDIATE AND PROPER INSTANTIATION (LOADING FROM CONFIG-FILE)
        {
            this.position = new Point3D();
        }

        public Exhibit(string name)  // ONLY FOR IMMEDIATE AND PROPER INSTANTIATION (LOADING FROM CONFIG-FILE)
        {
            this.name = name;
            this.position = new Point3D();
        }

        public Exhibit(string name, Point3D position)
        {
            this.name = name;
            this.position = position;
        }

        public Exhibit(string name, Point3D position, string path, string description, Dictionary<string, System.Drawing.Image> images)
        {
            this.name = name;
            this.position = position;
            this.path = path;
            this.description = description;
            this.images = images;
        }
        #endregion

        #region SAVE AND LOAD
        public string getName()
        {
            return this.name;
        }

        public Point3D getPosition()
        {
            return this.position;
        }

        public string getPath()
        {
            return this.path;
        }

        public string getDescription()
        {
            return this.description;
        }

        public double getKernelSize()
        {
            return this.kernelSize;
        }

        public double getKernelWeight()
        {
            return this.kernelWeight;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public void setPosition(Point3D position)
        {
            this.position = position;
        }

        public void setPath(string path)
        {
            this.path = path;
        }  

        public void setDescription(string description)
        {
            this.description = description;
        }

        public void setKernelSize(double kernelSize)
        {
            this.kernelSize = kernelSize;
        }

        public void setKernelWeight(double kernelWeight)
        {
            this.kernelWeight = kernelWeight;
        }
        #endregion

        #region IMAGES
        public Dictionary<string, System.Drawing.Image> getImages()
        {
            return this.images;
        }

        public List<System.Drawing.Image> getActualImages(List<string> paths)
        {
            List<System.Drawing.Image> images = new List<System.Drawing.Image>();

            foreach (KeyValuePair<string, System.Drawing.Image> image in this.images)
            {
                images.Add(getActualImage(image.Key));
            }

            return images;
        }

        public System.Drawing.Image getActualImage(string path)
        {
            return this.images[path];
        }

        public void addImages(Dictionary<string, System.Drawing.Image> images)
        {
            foreach (KeyValuePair<string, System.Drawing.Image> image in images)
            {
                addImage(image);
            }
        }

        public void addImage(KeyValuePair<string, System.Drawing.Image> image)
        {
            if (this.images == null) // No images, yet
            {
                this.images = new Dictionary<string, System.Drawing.Image>();
            }

            if (!this.images.ContainsKey(image.Key)) // Image not already in exhibition
            {
                this.images.Add(image.Key, image.Value);
            }
            else
            {
                MessageBox.Show("Dieses Bild in bereits vorhanden.");
            }

        }

        public void changeImage(string path, System.Drawing.Image image)
        {
            this.images[path] = image;
        }

        public void setImages(Dictionary<string, System.Drawing.Image> images)
        {
            this.images = images;
        }

        public void removeImage(KeyValuePair<string, System.Drawing.Image> image)
        {
            this.images.Remove(image.Key);
        }
        #endregion
    }
}
