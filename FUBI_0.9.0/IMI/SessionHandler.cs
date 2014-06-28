using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System;


namespace IMI
{
    public partial class SessionHandler
    {
        #region DECLARATIONS
        private uint id;
        private Point3D userPosition;
        private double radius;
        private Dictionary<Point3D, int> lookup;

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

        #region EXHIBIT POSITION
        private void lookUpTable(Point3D position)
        {

        }

        public void makeLookupTable(List<Exhibit> exhibits, GeometryHandler.Plane exhibitionPlane)
        {
            DateTime start = DateTime.Now;
            this.lookup = new Dictionary<Point3D, int>();
            List<Point3D> positions = makeLookupPositions(exhibitionPlane);
            List<double> weightsForPosition = new List<double>();

            foreach (Point3D position in positions) // Lookup-key
            {
                weightsForPosition.Clear();

                foreach (Exhibit exhibit in exhibits)
                { 
                    weightsForPosition.Add(getKernelWeight(position, exhibit.getPosition(), exhibit.getKernelSize(), exhibit.getKernelWeight()));
                }

                this.lookup.Add(position, getMaxIndex(weightsForPosition));
            }
            DateTime finish = DateTime.Now;
            TimeSpan duration = finish - start;
            int success = 0;
        }

        private List<Point3D> makeLookupPositions(GeometryHandler.Plane plane)
        {
            List<Point3D> planePositions = new List<Point3D>();
            Point3D tmpPos = new Point3D();
            double stepSize = 0.002; // ~3mm steps, at least in lab's exhibition plane(1800x1800mm)
            double lambdaDir1;
            double lambdaDir2;

            for (lambdaDir1 = 0.0; lambdaDir1 < 2.0; lambdaDir1 += stepSize) // Over twice the first direction of the plane, make 1000(=: 2.0/stepSize) steps
            {
                for (lambdaDir2 = 0.0; lambdaDir2 < 2.0; lambdaDir2 += stepSize) // Over twice the second direction of the plane, make 1000(=: 2.0/stepSize) steps
                {
                    tmpPos = plane.Start + (lambdaDir1 * plane.Direction1) + (lambdaDir2 * plane.Direction2);
                    planePositions.Add(tmpPos);
                }
            }

            return planePositions;
        }

        private double getKernelWeight(Point3D targetPosition, Point3D position, double kernelRadius, double kernelWeight)
        {
            double distance = this.geometryHandler.getDistance(targetPosition, position);
            double relDist = kernelRadius - distance; // relDist gets negative for positions, which are not within the exhibits kernel
            // Triangular
            return kernelWeight * relDist;
        }

        private int getMaxIndex(List<double> weights)
        {
            int index = 0;
            double maxWeight = 0.0;

            for (int i = 0; i != weights.Count; ++i)
            {
                if (weights[i] > maxWeight)
                {
                    maxWeight = weights[i];
                    index = i;
                }
            }

            return index;
        }

        private int getLookupExhibit()
        {
            return 0;
        }
        #endregion
    }
}
