using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace IMI_Administration
{
    class Exhibition
    {
        #region DECLARATIONS
        private string name;
        private string path;
        private GeometryHandler.Plane exhibitionPlane;
        private List<Exhibit> exhibits;
        #endregion

        #region CONSTRUCTORS
        public Exhibition(string name, List<Point3D> plane)
        {
            this.name = name;
            this.path = "";
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
            this.path = "";
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
            this.path = "";
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
            this.path = "";
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

        public void setName(string name)
        {
            this.name = name;
        }

        public void setPath(string path)
        {
            this.path = path;
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

        public void replaceExhibit(int index, Exhibit exhibit)
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
