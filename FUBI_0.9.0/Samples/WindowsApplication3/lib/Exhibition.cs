using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace IMI.lib
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
            this.exhibits = new List<ExhibitionExhibit>(null);
        }

        public Exhibition(List<Point3D> plane, List<ExhibitionExhibit> exhibits)
        {
            this.plane = new GeometryHandler.Plane();
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
