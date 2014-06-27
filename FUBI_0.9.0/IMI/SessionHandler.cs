using System.Windows.Media.Media3D;
using System.Collections.Generic;

namespace IMI
{
    public partial class SessionHandler
    {
        #region DECLARATIONS
        private uint id;
        private Point3D userPosition;
        private double radius;

        private GeometryHandler geometryHandler = new GeometryHandler();
        #endregion

        #region CONSTRUCTORS
        public SessionHandler(uint id, Point3D userPosition)
        {
            this.id = id;
            this.userPosition = userPosition;
            this.radius = 500.0;// Default := 500mm
        }

        public SessionHandler(uint id, Point3D userPosition, double radius)
        {
            this.id = id;
            this.userPosition = userPosition;
            this.radius = radius;
        }
        #endregion

        #region ID
        public uint getID()
        {
            return this.id;
        }

        public uint getCurrentUserID(Dictionary<uint, Point3D> users)
        {
            uint id = 99;
            double minDist = this.radius;
            double tmpDist = this.radius;

            foreach (KeyValuePair<uint, Point3D> user in users)
            {
                tmpDist = this.geometryHandler.furthestDistance(this.userPosition, user.Value);
                
                if (tmpDist < minDist)
                {
                    minDist = tmpDist;
                    id = user.Key;
                }
            }

            return id;
        }

        public void setID(uint id)
        {
            this.id = id;
        }
        #endregion

        #region USER POSITION
        public Point3D getUserPosition()
        {
            return this.userPosition;
        }

        public void setUserPosition(Point3D userPosition)
        {
            this.userPosition = userPosition;
        }
        #endregion
    }
}
