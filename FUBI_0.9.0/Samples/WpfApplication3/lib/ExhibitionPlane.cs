using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace WpfApplication3.lib
{
    public class ExhibitionPlane
    {
        #region DECLARATIONS
        /// <summary>
        /// DECLARATIONS
        /// </summary>
        
        private List<Point3D> corners;
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// CONSTRUCTORS
        /// </summary>

        public ExhibitionPlane()
        {
            this.corners = new List<Point3D>();
        }

        public ExhibitionPlane(List<Point3D> corners)
        {
            this.corners = corners;
        }
        #endregion

        #region SETTERS AND GETTERS
        /// <summary>
        /// SETTER AND GETTER
        /// </summary>
        
        public void setCorners(List<Point3D> corners)
        {
            this.corners = corners;
        }

        public List<Point3D> getCorners()
        {
            return this.corners;
        }
        #endregion

        #region MODIFICATIONS
        /// <summary>
        /// MODIFICATIONS
        /// </summary>
        /// 
        #endregion
    }
}
