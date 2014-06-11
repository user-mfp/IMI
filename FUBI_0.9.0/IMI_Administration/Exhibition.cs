using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace IMI_Administration
{
    class Exhibition
    {
        #region DECLARATIONS
        // Elements
        private GeometryHandler.Plane exhibitionPlane;
        private List<Exhibit> exhibits;
        // Attributes
        private string name;
        private string path;
        private Point3D userHeadPosition; // User's head position
        private double threshold = 100.0; // Default radius := 100mm
        private double selectionTime = 1000.0; // Default dwelltime for selection := 1s
        private double lockTime = 3000.0; // Default time for locking an exhibit := 3s
        private double slideTime = 5000.0; // Default time a slide is shown := 5s
        private double endWait = 10000.0; // Default waiting before back to main view := 10s
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

        public Point3D getUserHeadPosition()
        {
            return this.userHeadPosition;
        }

        public double getThreshold()
        {
            return this.threshold;
        }

        public double getSelectionTime()
        {
            return this.selectionTime;
        }

        public double getLockTime()
        {
            return this.lockTime;
        }

        public double getSlideTime()
        {
            return this.slideTime;
        }

        public double getEndWait()
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

        public void setUserHeadPosition(Point3D userHeadPosition)
        {
            this.userHeadPosition = userHeadPosition;
        }

        public void setThreshold(double threshold)
        {
            this.threshold = threshold;
        }

        public void setSelectionTime(double selectionTime)
        {
            this.selectionTime = selectionTime;
        }

        public void setLockTime(double lockTime)
        {
            this.lockTime = lockTime;
        }

        public void setSlideTime(double slideTime)
        {
            this.slideTime = slideTime;
        }

        public void setEndWait(double endWait)
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
