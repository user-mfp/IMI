using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace WpfApplication3.lib
{
    public class Exhibition
    {
        #region DECLARATIONS
        /// <summary>
        /// DECLARATIONS
        /// </summary>
        
        private GeometryHandler.Plane plane;
        private List<ExhibitionExhibit> exhibits;
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// CONSTRUCTORS
        /// </summary>

        public Exhibition()
        {
            this.plane = new GeometryHandler.Plane();
            this.exhibits = new List<ExhibitionExhibit>();
        }

        public Exhibition(List<Point3D> plane, List<ExhibitionExhibit> exhibits)
        {
            this.plane = new GeometryHandler.Plane(plane);
            this.exhibits = exhibits;
        }

        public Exhibition(GeometryHandler.Plane plane, List<ExhibitionExhibit> exhibits)
        {
            this.plane = plane;
            this.exhibits = exhibits;
        }
        #endregion

        #region SETTERS AND GETTERS
        /// <summary>
        /// SETTERS AND GETTERS
        /// </summary>
        
        public void setExhibit(List<ExhibitionExhibit> exhibits)
        {
            this.exhibits = exhibits;
        }

        public List<ExhibitionExhibit> getExhibits()
        {
            return this.exhibits;
        }

        public ExhibitionExhibit getExhibit(int index)
        {
            return this.exhibits[index];
        }

        public void setExhibitionPlane(List<Point3D> corners)
        {
            this.plane.Start = corners[0];
            this.plane.End1 = corners[1];
            this.plane.End2 = corners[2];
            this.plane.Direction1 = corners[1] - corners[0];
            this.plane.Direction2 = corners[2] - corners[0];
        }

        public void setExhibitionPlane(GeometryHandler.Plane plane)
        {
            this.plane = plane;
        }
        #endregion

        #region ROUTINES
        /// <summary>
        /// ROUTINES
        /// </summary>

        public void newExhibit()
        { 
            /* TO DO:
             * - define corners/center and size
             * - define text
             */ 
        }

        public void addExhibit(ExhibitionExhibit exhibit)
        {
            this.exhibits.Add(exhibit);
        }
        #endregion
    }
}
