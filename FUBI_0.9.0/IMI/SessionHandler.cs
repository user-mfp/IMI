using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System;
using XNA = Microsoft.Xna.Framework;


namespace IMI
{
    public partial class SessionHandler
    {
        #region DECLARATIONS
        private uint id;
        private Point3D userPosition;
        private double radius;
        private Dictionary<Point3D, int> lookup;
        
        // Plane
        private XNA.Plane plane;
        private XNA.Vector3 planeNormal;
        private XNA.Vector3 planeStart;
        private XNA.Vector3 planeEnd1;
        private XNA.Vector3 planeEnd2;

        private GeometryHandler geometryHandler = new GeometryHandler();
        #endregion

        #region CONSTRUCTORS
        public SessionHandler(uint id, Point3D userPosition)
        {
            this.id = id;
            this.userPosition = userPosition;
            this.radius = 300.0;// Default := 300mm
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

        #region PLANE
        public void initPlane(GeometryHandler.Plane plane)
        {
            this.planeStart = new XNA.Vector3((float)plane.Start.X, (float)plane.Start.Y, (float)plane.Start.Z);
            this.planeEnd1 = new XNA.Vector3((float)plane.End1.X, (float)plane.End1.Y, (float)plane.End1.Z);
            this.planeEnd2 = new XNA.Vector3((float)plane.End2.X, (float)plane.End2.Y, (float)plane.End2.Z);

            this.plane = new XNA.Plane(planeStart, planeEnd1, planeEnd2);

            this.planeNormal = this.plane.Normal;
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

        #region LOOKUP
        public int getTarget(GeometryHandler.Vector vector)
        {
            Point3D pos = getPosition(vector);

            return getClosestIndex(pos);
        }

        public Point3D getPosition(List<GeometryHandler.Vector> vectors)
        {
            return this.geometryHandler.weighPosition(getPosition(vectors[0]), getPosition(vectors[1]));
        }

        public Point3D getPosition(GeometryHandler.Vector vector)
        {
            XNA.Vector3 vS = this.geometryHandler.makeVector3(vector.Start);
            XNA.Vector3 vD = this.geometryHandler.makeVector3(vector.Direction);

            double s = (double)XNA.Vector3.Dot(this.plane.Normal, (planeStart - vS)) / XNA.Vector3.Dot(this.plane.Normal, vD);

            Point3D intersection = vector.Start + (s * vector.Direction);

            return intersection;
        }

        public void makeLookupTable(List<Exhibit> exhibits, GeometryHandler.Plane exhibitionPlane)
        {
            this.lookup = new Dictionary<Point3D, int>();
            Dictionary<Point3D, int> checkup = new Dictionary<Point3D, int>();
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
        }

        private Point3D getClosestPosition(Point3D target)
        {
            Point3D tmp = new Point3D();
            double min = this.radius;

            DateTime start = DateTime.Now;
            foreach (KeyValuePair<Point3D, int> position in this.lookup)
            {
                double distance = this.geometryHandler.getDistance(target, position.Key);
                if (distance < min)
                {
                    tmp = position.Key;
                    min = distance;
                }
            }
            DateTime end = DateTime.Now;
            TimeSpan dur = end - start;
            return tmp;
        }


        private int getClosestIndex(Point3D target)
        {
            int tmp = 99;
            double min = this.radius;

            DateTime start = DateTime.Now;
            foreach (KeyValuePair<Point3D, int> position in this.lookup)
            {
                double distance = this.geometryHandler.getDistance(target, position.Key);
                if (distance < min)
                {
                    tmp = position.Value;
                    min = distance;
                }
            }
            DateTime end = DateTime.Now;
            TimeSpan dur = end - start;
            return tmp;
        }

        private List<Point3D> makeLookupPositions(GeometryHandler.Plane plane)
        {
            List<Point3D> planePositions = new List<Point3D>();
            Point3D tmpPos = new Point3D();
            double stepSize = 0.002; // ~3mm steps, at least in lab's exhibition plane(1800x1800mm)
            double lambdaDir1;
            double lambdaDir2;

            for (lambdaDir1 = 0.0; lambdaDir1 < 2.0; lambdaDir1 += stepSize) // Over (twice) the first direction of the plane, make 1000(=: 2.0/stepSize) steps
            {
                for (lambdaDir2 = 0.0; lambdaDir2 < 2.0; lambdaDir2 += stepSize) // Over (twice) the second direction of the plane, make 1000(=: 2.0/stepSize) steps
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
            int index = 99;
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
        #endregion
    }
}
