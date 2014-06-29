using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;

namespace IMI
{
    public partial class Exhibition
    {
        #region DECLARATIONS
        // Elements
        private GeometryHandler.Plane exhibitionPlane;
        private List<Exhibit> exhibits;
        // Attributes
        private string name;
        private string path;
        private KeyValuePair<string, BitmapImage> backgroundImage;
        private KeyValuePair<string, BitmapImage> overview;
        private Point3D userPosition; // User's head position
        private double threshold = 100.0; // Default radius := 100mm (only for exhibit-definition. Not for exhibition plane-definition)
        private int selectionTime = 1000; // Default dwelltime for selection := 1s
        private int lockTime = 3000; // Default time for locking an exhibit := 3s
        private int slideTime = 5000; // Default time a slide is shown := 5s
        private int endWait = 10000; // Default waiting before back to main view := 10s
        #endregion

        #region CONSTRUCTORS
        public Exhibition() // ONLY FOR IMMEDIATE AND PROPER INSTANTIATION (LOADING FROM CONFIG-FILE OR DEFAUL-VALUES) ! ! !
        {
            this.exhibitionPlane = new GeometryHandler.Plane();
        }

        public Exhibition(string name) // ONLY FOR IMMEDIATE AND PROPER INSTANTIATION (LOADING FROM CONFIG-FILE OR DEFAULT-VALUES) ! ! !
        {
            this.name = name;
            this.exhibitionPlane = new GeometryHandler.Plane();
        }

        public Exhibition(string name, List<Point3D> plane)
        {
            this.name = name;
            this.exhibitionPlane = new GeometryHandler.Plane(plane);
            this.exhibits = new List<Exhibit>();
        }
        public Exhibition(string name, string path, List<Point3D> plane)
        {
            this.name = name;
            this.path = path;
            this.exhibitionPlane = new GeometryHandler.Plane(plane);
            this.exhibits = new List<Exhibit>();
        }

        public Exhibition(string name, GeometryHandler.Plane plane)
        {
            this.name = name;
            this.exhibitionPlane = plane;
            this.exhibits = new List<Exhibit>();
        }

        public Exhibition(string name, string path, GeometryHandler.Plane plane)
        {
            this.name = name;
            this.path = path;
            this.exhibitionPlane = plane;
            this.exhibits = new List<Exhibit>();
        }

        public Exhibition(string name, List<Point3D> plane, List<Exhibit> exhibits)
        {
            this.name = name;
            this.exhibitionPlane = new GeometryHandler.Plane(plane);
            this.exhibits = exhibits;
        }

        public Exhibition(string name, string path, List<Point3D> plane, List<Exhibit> exhibits)
        {
            this.name = name;
            this.path = path;
            this.exhibitionPlane = new GeometryHandler.Plane(plane);
            this.exhibits = exhibits;
        }

        public Exhibition(string name, GeometryHandler.Plane plane, List<Exhibit> exhibits)
        {
            this.name = name;
            this.exhibitionPlane = plane;
            this.exhibits = exhibits;
        }

        public Exhibition(string name, string path, GeometryHandler.Plane plane, List<Exhibit> exhibits)
        {
            this.name = name;
            this.path = path;
            this.exhibitionPlane = plane;
            this.exhibits = exhibits;
        }
        #endregion

        #region SAVE AND LOAD
        public string getName()
        {
            return this.name;
        }

        public string getPath()
        {
            return this.path;
        }

        public KeyValuePair<string, BitmapImage> getBackgroundImage()
        {
            return this.backgroundImage;
        }

        public KeyValuePair<string, BitmapImage> getOverview()
        {
            return this.overview;
        }

        public Point3D getUserPosition()
        {
            return this.userPosition;
        }

        public double getThreshold()
        {
            return this.threshold;
        }

        public int getSelectionTime()
        {
            return this.selectionTime;
        }

        public int getLockTime()
        {
            return this.lockTime;
        }

        public int getSlideTime()
        {
            return this.slideTime;
        }

        public int getEndWait()
        {
            return this.endWait;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public void setPath(string path)
        {
            this.path = path;
        }

        public void setBackgroundImage(KeyValuePair<string, BitmapImage> backgroundImage)
        {
            this.backgroundImage = backgroundImage;
        }

        public void setOverview(KeyValuePair<string, BitmapImage> overview)
        {
            this.overview = overview;
        }

        public void setUserPosition(Point3D userPosition)
        {
            this.userPosition = userPosition;
        }

        public void setThreshold(double threshold)
        {
            this.threshold = threshold;
        }

        public void setSelectionTime(int selectionTime)
        {
            this.selectionTime = selectionTime;
        }

        public void setLockTime(int lockTime)
        {
            this.lockTime = lockTime;
        }

        public void setSlideTime(int slideTime)
        {
            this.slideTime = slideTime;
        }

        public void setEndWait(int endWait)
        {
            this.endWait = endWait;
        }
        #endregion

        #region EXHIBITIONPLANE
        public GeometryHandler.Plane getExhibitionPlane()
        {
            return this.exhibitionPlane;
        }

        public void setExhibitionPlane(GeometryHandler.Plane plane)
        {
            this.exhibitionPlane = plane;
        }
        #endregion

        #region EXHIBITS
        public List<Exhibit> getExhibits()
        {
            return this.exhibits;
        }

        public Exhibit getExhibit(int index)
        {
            return this.exhibits[index];
        }

        public void addExhibit(Exhibit exhibit)
        {
            this.exhibits.Add(exhibit);
        }

        public void setExhibits(List<Exhibit> exhibits)
        {
            this.exhibits = exhibits;
        }

        public void setExhibit(int index, Exhibit exhibit)
        {
            this.exhibits[index] = exhibit; // Replace old with new exhibit
        }

        public void removeExhibit(int index)
        {
            this.exhibits.RemoveAt(index);
        }

        public void removeExhibit(Exhibit exhibit)
        {
            this.exhibits.Remove(exhibit);
        }
        #endregion
    }
}
