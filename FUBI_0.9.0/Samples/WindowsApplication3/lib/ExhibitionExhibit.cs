using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace IMI.lib
{
    public class ExhibitionExhibit
    {
        #region DECLARATIONS
        /// <summary>
        /// DECLARATIONS
        /// </summary>

        // Corners of exhibit's bounding box
        private List<Point3D> corners;
        // Center of exhibit on Plane
        private Point3D center;
        // Size of exhibit's space: height for bounding box or radius of kernel
        private float size;
        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// CONSTRUCTORS
        /// </summary>

        public ExhibitionExhibit(List<Point3D> corners, float height)
        {
            this.corners = corners;
            this.size = height;
        }

        public ExhibitionExhibit(Point3D center, float radius)
        {
            this.center = center;
            this.size = radius;
        }
        #endregion

        #region SETTERS AND GETTERS
        /// <summary>
        /// SETTERS AND GETTERS
        /// </summary>
        
        public void setCorners(List<Point3D> corners)
        {
            this.corners = corners;
        }

        public void setCenter(Point3D center)
        {
            this.center = center;
        }

        public void setSize(float size)
        {
            this.size = size;
        }

        public List<Point3D> getCorners()
        {
            return this.corners;
        }

        public Point3D getCenter()
        {
            return this.center;
        }

        public float getSize()
        {
            return this.size;
        }
        #endregion
    }
}
