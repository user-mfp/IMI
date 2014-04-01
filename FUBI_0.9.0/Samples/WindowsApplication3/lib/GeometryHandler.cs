using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace IMI.lib
{
    public class GeometryHandler
    {
        #region VECTOR
        /// <summary>
        /// VECTOR
        /// </summary>
        
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
        }
        #endregion

        #region DECLARATIONS
        /// <summary>
        /// DECLARATIONS
        /// </summary>

        private Statistics statistics = new Statistics();
        #endregion
        
        #region ROUTINES
        /// <summary>
        /// ROUTINES
        /// </summary>

        public List<Point3D> vectorsIntersectTest(Vector vectorA, Vector vectorB)
        {
            Point3D xy = vectorsIntersectOnXY(vectorA, vectorB); // xy.Z = 0
            Point3D yz = vectorsIntersectOnYZ(vectorA, vectorB); // yz.X = 0
            Point3D xz = vectorsIntersectOnXZ(vectorA, vectorB); // xz.Y = 0
            List<Point3D> testPoints = new List<Point3D>();

            testPoints.Add(xy);
            testPoints.Add(yz);
            testPoints.Add(xz);

            return testPoints;
        } 

        public Point3D vectorsIntersectOnXY(Vector vectorA, Vector vectorB)
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

        public Point3D vectorsIntersectOnYZ(Vector vectorA, Vector vectorB)
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

        public Point3D vectorsIntersectOnXZ(Vector vectorA, Vector vectorB)
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

        // Returns the two closest points or intersection of two vectors
        public List<Point3D> vectorsIntersect(Vector vectorA, Vector vectorB)
        {
            List<Point3D> feet = new List<Point3D>();
            vectorA.Direction.Normalize();
            vectorB.Direction.Normalize();
            Vector3D projection = new Vector3D((vectorB.Start.X - vectorA.Start.X), (vectorB.Start.Y - vectorA.Start.Y), (vectorB.Start.Z - vectorA.Start.Z));
            double unitDirection = Vector3D.DotProduct(vectorA.Direction, vectorB.Direction);
            
            if (unitDirection != 1) // Vectors are not parallel
            {
                double separationProjectionA = Vector3D.DotProduct(projection, vectorA.Direction);
                double separationProjectionB = Vector3D.DotProduct(projection, vectorB.Direction);

                double lamdaA = ((separationProjectionA - unitDirection) * separationProjectionB) / ((1 - unitDirection) * unitDirection);
                double lamdaB = ((separationProjectionB - unitDirection) * separationProjectionA) / (unitDirection * (unitDirection - 1));

                Point3D footA = Point3D.Add(vectorA.Start, Vector3D.Multiply(lamdaA, vectorA.Direction));
                Point3D footB = Point3D.Add(vectorB.Start, Vector3D.Multiply(lamdaB, vectorB.Direction));

                if (footA != footB) // there is no intersection
                {
                    feet.Add(footA);
                    feet.Add(footB);
                }
                else
                    feet.Add(footA);
                
                return feet;
            }
            else
                return feet;
        }

        // Returns a vector's properties as string
        public string getString(Vector v)
        { 
            string vector = ((int)v.Start.X).ToString() + '\t' + ((int)v.Start.Y).ToString() + '\t' + ((int)v.Start.Z).ToString() + '\t' + ((int)v.End.X).ToString() + '\t' + ((int)v.End.Y).ToString() + '\t' + ((int)v.End.Z).ToString() + '\t' + ((int)v.Direction.X).ToString() + '\t' + ((int)v.Direction.Y).ToString() + '\t' + ((int)v.Direction.Z).ToString();
            return vector;
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

            return statistics.getAvg(points);
        }

        public Point3D getCenter(Point3D p1, Point3D p2)
        {
            List<Point3D> points = new List<Point3D>();
            points.Add(p1);
            points.Add(p2);

            return getCenter(points);
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

        // Returns the intersection of given vector and given plane
        public Point3D intersectVectorPlane(Point3D point, Vector3D vector, List<Point3D> corners)
        { 
            Point3D intersection = new Point3D();

            // DO STUFF HERE

            return intersection;
        }
        
        // Returns, whether given vector intersects with given bounding box or not
        public bool intersectVectorBbox(Point3D point, Vector3D vector, List<Point3D> corners)
        { 
            // DO STUFF HERE

            return false;
        }
        #endregion
    }
}
