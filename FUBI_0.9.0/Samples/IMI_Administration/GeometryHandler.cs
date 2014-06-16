﻿using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace IMI_Administration
{
    public class GeometryHandler
    {
        #region VECTOR
        public struct Vector
        {

            public Point3D Start;
            public Point3D End;
            public Vector3D Direction;

            public Vector(Point3D start, Point3D end)
            {
                this.Start = start;
                this.End = end;
                this.Direction = end - start;
            }

            public Vector(Point3D start, Vector3D dir)
            {
                this.Start = start;
                this.End = start + dir;
                this.Direction = dir;
            }

            public void reset(Point3D start, Point3D end)
            {
                this.Start = start;
                this.End = end;
                this.Direction = end - start;
            }

            public static bool operator==(Vector lhs, Vector rhs)
            {
                if (lhs.Start == rhs.Start && lhs.End == rhs.End)
                    return true;
                else
                    return false;
            }

            public static bool operator!=(Vector lhs, Vector rhs)
            {
                if (lhs.Start != rhs.Start || lhs.End != rhs.End)
                    return true;
                else
                    return false;
            }

            public void setDirection(Point3D start, Point3D end)
            {
                this.Direction.X = end.X - start.X;
                this.Direction.Y = end.Y - start.Y;
                this.Direction.Z = end.Z - start.Z;
            }

            public Point3D getTarget(double lambda)
            {
                Point3D target = new Point3D();

                target.X = Start.X + (lambda * Direction.X);
                target.Y = Start.Y + (lambda * Direction.Y);
                target.Z = Start.Z + (lambda * Direction.Z);

                return target;
            }
        }
        #endregion
        
        #region PLANE
        public struct Plane
        {
            public Point3D Start;
            public Point3D End1;
            public Point3D End2;
            public Vector3D Direction1;
            public Vector3D Direction2;

            public Plane(Point3D start, Point3D end1, Point3D end2)
            {
                this.Start = start;
                this.End1 = end1;
                this.End2 = end2;
                this.Direction1 = end1 - start;
                this.Direction2 = end2 - start;
            }

            public Plane(Point3D start, Vector3D dir1, Vector3D dir2)
            {
                this.Start = start;
                this.End1 = start + dir1;
                this.End2 = start + dir2;
                this.Direction1 = dir1;
                this.Direction2 = dir2;
            }

            public Plane(List<Point3D> corners)
            {
                this.Start = corners[0];
                this.End1 = corners[1];
                this.End2 = corners[2];
                this.Direction1 = corners[1] - corners[0];
                this.Direction2 = corners[2] - corners[0];
            }
        }
        #endregion

        #region DECLARATIONS AND INITIALIZATIONS
        private StatisticsHandler statisticsHandler = new StatisticsHandler();
        // Calibration and Validation
        private double threshold = 100.0; //Default := 100.0mm
        private double classWeight = 0.7;
        private List<Point3D> pointingPts;
        private List<Point3D> aimingPts;
        private List<Point3D> classification = new List<Point3D>(); // Biased (classified) axis
        private List<Point3D> combinedPts = new List<Point3D>(); // Biased (classified) axis
        #endregion

        #region GET AND SET
        private double getThreshold()
        {
            return this.threshold;
        }

        private double getClassWeight()
        {
            return this.classWeight;
        }

        private void setThreshold(double threshold)
        {
            this.threshold = threshold;
        }

        private void setClassWeight(double classWeight)
        {
            this.classWeight = classWeight;
        }
        #endregion

        #region INTERSECTIONS
        public List<Point3D> vectorsIntersectProj(Vector vectorA, Vector vectorB)
        {
            Point3D xy = vectorsIntersectProjXY(vectorA, vectorB); // xy.Z = 0
            Point3D yz = vectorsIntersectProjYZ(vectorA, vectorB); // yz.X = 0
            Point3D xz = vectorsIntersectProjXZ(vectorA, vectorB); // xz.Y = 0
            List<Point3D> testPoints = new List<Point3D>();

            testPoints.Add(xy);
            testPoints.Add(yz);
            testPoints.Add(xz);

            return testPoints;
        }

        public Point3D vectorsIntersectProjXY(Vector vectorA, Vector vectorB)
        {
            Vector3D a1 = new Vector3D(vectorA.Start.X, vectorA.Start.Y, 1);
            Vector3D a2 = new Vector3D(vectorA.End.X, vectorA.End.Y, 1);
            Vector3D a = Vector3D.CrossProduct(a1, a2);
            Vector3D b1 = new Vector3D(vectorB.Start.X, vectorB.Start.Y, 1);
            Vector3D b2 = new Vector3D(vectorB.End.X, vectorB.End.Y, 1);
            Vector3D b = Vector3D.CrossProduct(b1, b2);
            Vector3D i = Vector3D.CrossProduct(a, b);

            Point3D xy = new Point3D(i.X / i.Z, i.Y / i.Z, 0);
            return xy;
        }

        public Point3D vectorsIntersectProjYZ(Vector vectorA, Vector vectorB)
        {
            Vector3D a1 = new Vector3D(1, vectorA.Start.Y, vectorA.Start.Z);
            Vector3D a2 = new Vector3D(1, vectorA.End.Y, vectorA.End.Z);
            Vector3D a = Vector3D.CrossProduct(a1, a2);
            Vector3D b1 = new Vector3D(1, vectorB.Start.Y, vectorB.Start.Z);
            Vector3D b2 = new Vector3D(1, vectorB.End.Y, vectorB.End.Z);
            Vector3D b = Vector3D.CrossProduct(b1, b2);
            Vector3D i = Vector3D.CrossProduct(a, b);

            Point3D yz = new Point3D(0, i.Y / i.X, i.Z / i.X);
            return yz;
        }

        public Point3D vectorsIntersectProjXZ(Vector vectorA, Vector vectorB)
        {
            Vector3D a1 = new Vector3D(vectorA.Start.X, 1, vectorA.Start.Z);
            Vector3D a2 = new Vector3D(vectorA.End.X, 1, vectorA.End.Z);
            Vector3D a = Vector3D.CrossProduct(a1, a2);
            Vector3D b1 = new Vector3D(vectorB.Start.X, 1, vectorB.Start.Z);
            Vector3D b2 = new Vector3D(vectorB.End.X, 1, vectorB.End.Z);
            Vector3D b = Vector3D.CrossProduct(b1, b2);
            Vector3D i = Vector3D.CrossProduct(a, b);

            Point3D xz = new Point3D(i.X / i.Y, 0, i.Z / i.Y);
            return xz;
        }
        
        public List<Point3D> vectorsIntersectFoot(Vector vectorA, Vector vectorB)
        {
            List<Point3D> feet = new List<Point3D>();
            Vector3D crossAB = Vector3D.CrossProduct(vectorA.Direction, vectorB.Direction);
            Vector3D startA = new Vector3D(vectorA.Start.X, vectorA.Start.Y, vectorA.Start.Z);
            Vector3D startB = new Vector3D(vectorB.Start.X, vectorB.Start.Y, vectorB.Start.Z);
            Vector3D normalA = Vector3D.CrossProduct(vectorA.Direction, crossAB);
            Vector3D normalB = Vector3D.CrossProduct(vectorB.Direction, crossAB);

            double factorA1 = Vector3D.DotProduct(startB, normalB);
            double factorA2 = Vector3D.DotProduct(startA, normalB);
            double factorA3 = Vector3D.DotProduct(vectorA.Direction, normalB);
            double factorA = (factorA1 - factorA2) / factorA3;
            Vector3D footA = (factorA * vectorA.Direction) + startA;
            feet.Add(new Point3D(footA.X, footA.Y, footA.Z));

            double factorB1 = Vector3D.DotProduct(startA, normalA);
            double factorB2 = Vector3D.DotProduct(startB, normalA);
            double factorB3 = Vector3D.DotProduct(vectorB.Direction, normalA);
            double factorB = (factorB1 - factorB2) / factorB3;
            Vector3D footB = (factorB * vectorB.Direction) + startB;
            feet.Add(new Point3D(footB.X, footB.Y, footB.Z));

            return feet;
        }

        // Returns the intersection of given vector and given plane
        public Point3D intersectVectorPlane(Point3D point, Vector3D vector, List<Point3D> corners)
        {
            Point3D intersection = new Point3D();

            // DO STUFF HERE

            return intersection;
        }
        #endregion

        #region ROUTINES
        // Check for correctly sampled vector
        public bool vectorOK(Vector v)
        {
            if (v.Direction.X == 0 & v.Direction.Z == 0)
                return false;
            else
                return true;
        }

        public string getString(Point3D p)
        {
            string point = (p.X).ToString() + '\t' + (p.Y).ToString() + '\t' + (p.Z).ToString();
            return point;
        }

        // Returns a vector's properties as string (Start, End, Direction)
        public string getString(Vector v)
        { 
            string vector = (v.Start.X).ToString() + '\t' + (v.Start.Y).ToString() + '\t' + (v.Start.Z).ToString() + '\t' + (v.End.X).ToString() + '\t' + (v.End.Y).ToString() + '\t' + (v.End.Z).ToString() + '\t' + (v.Direction.X).ToString() + '\t' + (v.Direction.Y).ToString() + '\t' + (v.Direction.Z).ToString();
            return vector;
        }

        public string getDirection(Vector v)
        {
            string direction = ((int)v.Direction.X).ToString() + "; " + ((int)v.Direction.Y).ToString() + "; " + ((int)v.Direction.Z).ToString();
            return direction;
        }

        public double maxAxis(Vector v)
        {
            return Math.Max(Math.Max(v.Direction.X, v.Direction.Y), v.Direction.Z);
        }

        public double minAxis(Vector v)
        {
            return Math.Min(Math.Min(v.Direction.X, v.Direction.Y), v.Direction.Z);
        }

        // Returns the "centers" from equivalent points from lists of equal length
        public List<Point3D> getCenters(List<Point3D> pointsA, List<Point3D> pointsB)
        {
            List<Point3D> centers = new List<Point3D>();

            for (int point = 0; point != pointsA.Count; ++point)
            { 
                centers.Add(getCenter(pointsA[point], pointsB[point]));
            }

            return centers;
        }
        
        // Returns the "center" from a list of points
        public Point3D getCenter(List<Point3D> points)
        {
            double check = 0;
            int i = 0;
            foreach (Point3D point in points)
            {
                if (i == 0 || i % 3 == 0)
                {
                    check += point.X;
                }
                ++i;
            }
            check /= (points.Count / 3);

            return statisticsHandler.getAvg(points);
        }

        // Returns the "center" of two points
        public Point3D getCenter(Point3D p1, Point3D p2)
        {
            List<Point3D> points = new List<Point3D>();
            points.Add(p1);
            points.Add(p2);

            return getCenter(points);
        }

        // Returns a List of "average" vectors from a List of vectors for X samples per Position
        public List<Vector> getAvgVector(List<Vector> vectors, int samplesPerPosition)
        {
            List<Vector> avgs = new List<Vector>();
            List<Vector> tmp = new List<Vector>();
            int cnt = 0;

            foreach (Vector v in vectors)
            {
                tmp.Add(v);
                ++cnt;
                if (cnt % samplesPerPosition == 0)
                {
                    avgs.Add(getAvgVector(tmp));
                    tmp.Clear();
                }
            }

            return avgs;
        }

        // Returns the "average" vector from a List of vectors
        public Vector getAvgVector(List<Vector> vectors)
        {
            List<Point3D> starts = new List<Point3D>();
            List<Point3D> ends = new List<Point3D>();
            
            foreach (Vector vector in vectors)
            {
                starts.Add(vector.Start);
                ends.Add(vector.End);
            }

            return new Vector(getCenter(starts), getCenter(ends));
        }

        public void makePlane(Point3D start, Point3D end1, Point3D end2)
        {

        }

        private int withinThreshold(Point3D a, Point3D b)
        {
            int passes = 0;
            Point3D avg = new Point3D();
            Point3D tmp = new Point3D();

            avg.X = (a.X + b.X) / 2;
            avg.Y = (a.Y + b.Y) / 2;
            avg.Z = (a.Z + b.Z) / 2;

            if (Math.Abs(Math.Abs(a.X) - Math.Abs(avg.X)) < this.threshold && Math.Abs(Math.Abs(b.X) - Math.Abs(avg.X)) < this.threshold)
            {
                tmp.X = 1;
                ++passes;
            }
            if (Math.Abs(Math.Abs(a.Y) - Math.Abs(avg.Y)) < this.threshold && Math.Abs(Math.Abs(b.Y) - Math.Abs(avg.Y)) < this.threshold)
            {
                tmp.Y = 1;
                ++passes;
            }
            if (Math.Abs(Math.Abs(a.Z) - Math.Abs(avg.Z)) < this.threshold && Math.Abs(Math.Abs(a.Z) - Math.Abs(avg.Z)) < this.threshold)
            {
                tmp.Z = 1;
                ++passes;
            }

            /*if (this.passes.ContainsKey(tmp))
            {
                ++this.passes[tmp];
            }*/
            return passes;
        }
        #endregion

        #region WEIGHING
        public List<Point3D> classifyCombined(List<Point3D> pointing, List<Point3D> aiming)
        {
            this.pointingPts = pointing;
            this.aimingPts = aiming;
            Point3D tmp = new Point3D();

            this.classification.Clear();
            for (int i = 0; i != this.aimingPts.Count; ++i)
            {
                tmp = classifyPoints(this.pointingPts[i], this.aimingPts[i]); // Check IF a classification is necessary and possible
                this.classification.Add(tmp); // Add result of classification for respective pair of points
            }

            return this.combinedPts;
        }

        private Point3D classifyPoints(Point3D pointing, Point3D aiming)
        {
            Point3D tmp = new Point3D();

            tmp.X = classifyBig(pointing.X, aiming.X);
            tmp.Y = classifyBig(pointing.Y, aiming.Y);
            tmp.Z = classifyBig(pointing.Z, aiming.Z);
            
            return tmp;
        }
        
        private double classifySmall(double pointing, double aiming)
        {
            double bias = smallerValue(pointing, aiming);
            double diff = Math.Abs(Math.Abs(pointing) - Math.Abs(aiming));

            if (bias == pointing && diff > this.threshold) // Poiting IS bias-value AND difference is above than current threshold
            {
                return 1.0;
            }
            else if (bias == aiming && diff > this.threshold) // Aiming IS bias-value AND difference is above than threschold
            {
                return 2.0;
            }
            else // Neither is a bias-value, BUT difference is within threshold
            {
                return 0.0;
            }
        }

        private double classifyBig(double pointing, double aiming)
        {
            double bias = biggerValue(pointing, aiming);
            double diff = Math.Abs(Math.Abs(pointing) - Math.Abs(aiming));

            if (bias == pointing && diff > this.threshold) // Poiting IS bias-value AND difference is above than current threshold
            {
                return 1.0;
            }
            else if (bias == aiming && diff > this.threshold) // Aiming IS bias-value AND difference is above than threschold
            {
                return 2.0;
            }
            else // Neither is a bias-value, BUT difference is within threshold
            {
                return 0.0;
            }
        }

        private double smallerValue(double lhs, double rhs)
        {
            double orient = Math.Min(Math.Abs(lhs), Math.Abs(rhs));

            if (orient == Math.Abs(lhs))
                return lhs;
            else
                return rhs;
        }

        private double biggerValue(double lhs, double rhs)
        {
            double orient = Math.Max(Math.Abs(lhs), Math.Abs(rhs));

            if (orient == Math.Abs(lhs))
                return lhs;
            else
                return rhs;
        }

        private void weighCombined()
        {
            Point3D tmp = new Point3D();

            this.combinedPts.Clear();
            for (int i = 0; i != this.classification.Count; ++i)
            {
                tmp = weighPoints(this.pointingPts[i], this.aimingPts[i], this.classification[i]);
                this.combinedPts.Add(tmp);
            }
        }

        private Point3D weighPoints(Point3D pointing, Point3D aiming, Point3D classification)
        {
            Point3D tmp = new Point3D();

            tmp.X = weighAxisValue(pointing.X, aiming.X, classification.X);
            tmp.Y = weighAxisValue(pointing.Y, aiming.Y, classification.Y);
            tmp.Z = weighAxisValue(pointing.Z, aiming.Z, classification.Z);

            return tmp;
        }

        private double weighAxisValue(double pointing, double aiming, double classification)
        {
            double weightedAxis;
            /*
            if ([statement]) // Automatic classification enabled
            {
                setClassWeight(pointing, aiming); // Calculate weight for each pointing- / aiming-pair
            }
            */
            if (classification == 1) // Biased toward pointing
            {
                weightedAxis = (pointing * this.classWeight) + (aiming * (1 - this.classWeight));
            }
            else if (classification == 2) // Biased toward aiming
            {
                weightedAxis = (aiming * this.classWeight) + (pointing * (1 - this.classWeight));
            }
            else // No bias := average
            {
                weightedAxis = (pointing + aiming) / 2;
            }
            
            return weightedAxis;
        }

        private void setClassWeight(double pointing, double aiming) // For dynamic weighing of axis' values
        {
            double diff = Math.Abs(Math.Abs(pointing) - Math.Abs(aiming));

            this.classWeight = Math.Min(0.9, (Math.Max(0.6, (1 - (this.threshold / diff))))); // Min-,Max-Function: classWeight is always bewteen 0.6 and 0.9
        }
        #endregion
    }
}