using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System;
using XNA = Microsoft.Xna.Framework;
using System.Windows;
using System.Windows.Forms;

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
        private XNA.Vector3 planeDir1;
        private XNA.Vector3 planeDir2;

        // Screen
        private Point3D screenSize;
        private Point3D canvasSize;
        private double planeCanvasRatio;

        private GeometryHandler geometryHandler = new GeometryHandler();
        #endregion

        #region CONSTRUCTOR
        public SessionHandler(uint id, Point3D userPosition, double radius, GeometryHandler.Plane plane, Point3D screenSize)
        {
            this.id = id;
            this.userPosition = userPosition;
            this.radius = radius;
            this.screenSize = screenSize;

            initPlane(plane);
            initScreen();
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
            // Plane
            this.planeStart = new XNA.Vector3((float)plane.Start.X, (float)plane.Start.Y, (float)plane.Start.Z);
            this.planeEnd1 = new XNA.Vector3((float)plane.End1.X, (float)plane.End1.Y, (float)plane.End1.Z);
            this.planeEnd2 = new XNA.Vector3((float)plane.End2.X, (float)plane.End2.Y, (float)plane.End2.Z);
            this.planeDir1 = new XNA.Vector3((float)plane.Direction1.X, (float)plane.Direction1.Y, (float)plane.Direction1.Z);
            this.planeDir2 = new XNA.Vector3((float)plane.Direction2.X, (float)plane.Direction2.Y, (float)plane.Direction2.Z);
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
            Dictionary<int, int> checkup = new Dictionary<int, int>();
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

            foreach (KeyValuePair<Point3D, int> position in this.lookup)
            { 
                if (checkup.ContainsKey(position.Value) == false)
                {
                    checkup.Add(position.Value, 1);
                }
                else
                {
                    ++checkup[position.Value];
                }
            }
        }

        private Point3D getClosestPosition(Point3D target)
        {
            Point3D tmp = new Point3D();
            double min = this.radius;

            foreach (KeyValuePair<Point3D, int> position in this.lookup)
            {
                double distance = this.geometryHandler.getDistance(target, position.Key);
                if (distance < min)
                {
                    tmp = position.Key;
                    min = distance;
                }
            }
            return tmp;
        }


        private int getClosestIndex(Point3D target)
        {
            int tmp = 99;
            double min = this.radius;

            foreach (KeyValuePair<Point3D, int> position in this.lookup)
            {
                double distance = this.geometryHandler.getDistance(target, position.Key);
                if (distance < min)
                {
                    tmp = position.Value;
                    min = distance;
                }
            }
            return tmp;
        }

        private List<Point3D> makeLookupPositions(GeometryHandler.Plane plane)
        {
            List<Point3D> planePositions = new List<Point3D>();
            Point3D tmpPos = plane.Start;
            double steps = 1000; // ~3mm steps, at least in lab's exhibition plane(1800x1800mm)
            double lambdaDir1;
            double lambdaDir2;

            planePositions.Add(tmpPos);
            for (int stepDir1 = 0; stepDir1 != steps; ++stepDir1) // Over (twice) the first direction of the plane, make 1000(=: 2.0/stepSize) steps
            {
                for (int stepDir2 = 0; stepDir2 != steps; ++stepDir2) // Over (twice) the second direction of the plane, make 1000(=: 2.0/stepSize) steps
                {
                    lambdaDir1 = (stepDir1 + 1) / steps;
                    lambdaDir2 = (stepDir2 + 1) / steps;
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

        #region SCREEN
        private void initScreen()
        {
            this.canvasSize = new Point3D();
            float planeDir1L = this.planeDir1.Length(); // Depth of plane
            float planeDir2L = this.planeDir2.Length(); // Width of plane
            float planeDirMax = Math.Max(planeDir1L, planeDir2L);

            if (planeDirMax == planeDir1L) // Depth the plane is longest dimension
            {
                this.canvasSize.Y = this.screenSize.Y;
                this.planeCanvasRatio = canvasSize.Y / planeDir1L;
                this.canvasSize.X = this.screenSize.X * this.planeCanvasRatio;
            }
            else if (planeDirMax == planeDir2L) // Width of plane is longest dimension
            {
                this.canvasSize.X = this.screenSize.X;
                this.planeCanvasRatio = canvasSize.X / planeDir2L;
                this.canvasSize.Y = this.screenSize.Y * this.planeCanvasRatio;
            }
            else // Neither is longest dimension of the two
            {
                // WTF?
            }
        }

        public Point3D getCanvasSize()
        {
            return this.canvasSize;
        }

        public double getPlaneCanvasRatio()
        {
            return this.planeCanvasRatio;
        }

        private Point getClosestLambdas(Point3D target)
        {
            int tmp = 0;
            int index = 0;
            double min = this.radius;

            foreach (KeyValuePair<Point3D, int> position in this.lookup)
            {
                double distance = this.geometryHandler.getDistance(target, position.Key);

                if (distance < min)
                {
                    index = tmp;
                    min = distance;
                }

                ++tmp;
            }

            int stepsDir1 = index / 1000;
            int stepsDir2 = index % 1000;

            Point lambdas = new Point();
            lambdas.Y = stepsDir1 / 1000.0;
            lambdas.X = stepsDir2 / 1000.0;

            return lambdas;
        }

        public Point3D getCanvasPosition(Point3D position)
        {
            Point3D canvasPosition = new Point3D();
            
            Point lambdas = getClosestLambdas(position);
            canvasPosition.X = this.canvasSize.X - (lambdas.X * this.canvasSize.X); // Position on screen in relation to origin (upper left corner)
            canvasPosition.Y = this.canvasSize.Y - (lambdas.Y * this.canvasSize.Y); // Position on screen in relation to origin (upper left corner)

            return canvasPosition;
        }

        public double getSizeOnCanvas(double planeSize)
        {
            double canvasSize = 0;

            // TODO: RETURN CORRECT DIMENSIONS

            return canvasSize;
        }
        #endregion
    }
}
