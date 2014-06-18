using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Microsoft.Xna.Framework;
using System;

namespace IMI_Administration
{
    class CalibrationHandler
    {
        #region DECLARATIONS
        private DataLogger dataLogger;
        private GeometryHandler geometryHandler;

        int samplesPerPosition; // Number of samples taken per position pointed from
        double threshold = 100; // Default := 100mm
        #endregion

        #region CONSTRUCTOR
        public CalibrationHandler(int samplingVectors)
        {
            this.dataLogger = new DataLogger(@"C:\Users\Haßleben\Desktop\IMI-DATA\Debug\"); // TODO: Initialize properly!
            this.geometryHandler = new GeometryHandler();
            this.samplesPerPosition = samplingVectors; 
        }

        public CalibrationHandler(int samplingVectors, double threshold)
        {
            this.dataLogger = new DataLogger(@"C:\Users\Haßleben\Desktop\IMI-DATA\Debug\"); // TODO: Initialize properly!
            this.geometryHandler = new GeometryHandler();
            this.samplesPerPosition = samplingVectors;
            this.threshold = threshold;
        }
        #endregion 

        #region THRESHOLD
        public double getThreshold()
        {
            return this.threshold;
        }

        public void setThreshold(double threshold)
        {
            this.threshold = threshold;
        }
        #endregion

        #region DEFINITION
        public List<Point3D> definePlane(List<GeometryHandler.Vector> samples, int positions, int mode) // Get all sampled vectors, pointing and aiming; mode: 0 = only pointing-samples, 1 = only aiming-samples, 2 = both samples
        {
            int points = pointsToDefine(samples.Count, positions, mode);
            sortAvgSamples(ref samples, points, mode);

            List<Point3D> pointFootPoints = new List<Point3D>(); // Pointing intersections from foot point-algorithm
            List<Point3D> aimFootPoints = new List<Point3D>(); // Aiming intersections from foot point-algorithm
            List<Point3D> combinedPoints = new List<Point3D>();

            switch (mode)
            { 
                case 0: // Only (pointing-)samples
                    setFootPoints(ref pointFootPoints, samples, points);
                    return pointFootPoints;
                case 1: // Only (aiming-)samples
                    setFootPoints(ref aimFootPoints, samples, points);
                    return aimFootPoints;
                case 2: // Both kinds of samples                    
                    List<GeometryHandler.Vector> pointAvgVectors = new List<GeometryHandler.Vector>();
                    List<GeometryHandler.Vector> aimAvgVectors = new List<GeometryHandler.Vector>();
                    int cnt = 0;
                    foreach(GeometryHandler.Vector sample in samples)
                    {
                        if (cnt < (samples.Count / 2))
                            pointAvgVectors.Add(sample);
                        else
                            aimAvgVectors.Add(sample);
                        ++cnt;
                    }

                    setFootPoints(ref pointFootPoints, pointAvgVectors, points); 
                    setFootPoints(ref aimFootPoints, aimAvgVectors, points);

                    combinedPoints = this.geometryHandler.classifyCombined(pointFootPoints, aimFootPoints);
                    return combinedPoints;
                default: // Undefined mode
                    return combinedPoints; // Empty
            }
        }

        private void setFootPoints(ref List<Point3D> footPoints, List<GeometryHandler.Vector> avgVectors, int points)
        {
            int positions = avgVectors.Count / points;
            List<List<GeometryHandler.Vector>> vectors = new List<List<GeometryHandler.Vector>>();
            List<List<Point3D>> feet = new List<List<Point3D>>();

            int avgCount = 0;
            for (int point = 0; point != points; ++point)
            {
                vectors.Add(new List<GeometryHandler.Vector>()); // Add a list for all vectors to particular point from all positions
                for (int position = 0; position != positions; ++position)
                {
                    vectors[point].Add(avgVectors[avgCount]); // Fill list with vectors from particular position to one point
                    ++avgCount;
                }

                feet.Add(new List<Point3D>()); // Add a list for all foot points concerning one point
                for (int vectorA = 0; vectorA != (vectors[point].Count - 1); ++vectorA) // For each vector, except the last
                {
                    for (int vectorB = (vectorA + 1); vectorB != vectors[point].Count; ++vectorB) // and every following vector
                    {
                        if (vectors[point][vectorA] != vectors[point][vectorB]) // Vectors are not equal(vectorA.Start != vectorB.Start && vectorA.End != vectorB.End)
                        {
                            foreach (Point3D foot in this.geometryHandler.vectorsIntersectFoot(vectors[point][vectorA], vectors[point][vectorB]))
                            {
                                feet[point].Add(foot); // Add foot point to list
                            }
                        }
                    }
                }

                footPoints.Add(this.geometryHandler.getCenter(feet[point])); // Add the center of all foot points for particular point to list of foot points
            }
        }

        private void sortAvgVectors(ref List<GeometryHandler.Vector> vectors, int points)
        {
            int positions = vectors.Count / points;
            int vecIndex; // Order for 3 points and 2 positions: (1st point)0, 3, 6; (2nd point)1, 4, 7; (3rd point)2, 5, 8
            List<GeometryHandler.Vector> tmp = new List<GeometryHandler.Vector>();

            for (int point = 0; point != points; ++point)
            {
                for (int position = 0; position != positions; ++position)
                {
                    vecIndex = point + (points * position); 
                    tmp.Add(vectors[vecIndex]);
                }
            }

            vectors = tmp;
        }

        private int pointsToDefine(int samples, int positions, int mode)
        {
            int points; // Either 3 corers or 1 position
            
            if (mode == 2)
            {
                points = samples / (positions * this.samplesPerPosition * mode);
            }
            else
            {
                points = samples / (positions * this.samplesPerPosition);
            }

            return points;
        }

        private void sortAvgSamples(ref List<GeometryHandler.Vector> samples, int points, int mode)
        {
            // Pointing variables
            List<GeometryHandler.Vector> pointingSamples; // All or first half of "samples"-List (depending on mode)
            List<GeometryHandler.Vector> pointAvgVectors;
            // Aiming variables
            List<GeometryHandler.Vector> aimingSamples; // All or second half of "samples"-List (depending on mode)
            List<GeometryHandler.Vector> aimAvgVectors;

            switch (mode)
            { 
                case 0: // Only pointing-samples
                    pointingSamples = samples;
                    pointAvgVectors = this.geometryHandler.getAvgVector(pointingSamples, samplesPerPosition);
                    sortAvgVectors(ref pointAvgVectors, points);
                    samples = pointAvgVectors;
                    break;
                case 1: // Only aiming-samples
                    aimingSamples = samples;
                    aimAvgVectors = this.geometryHandler.getAvgVector(aimingSamples, samplesPerPosition);
                    sortAvgVectors(ref aimAvgVectors, points);
                    samples = aimAvgVectors;
                    break;
                case 2: // Both kinds of samples
                    pointingSamples = new List<GeometryHandler.Vector>();
                    aimingSamples = new List<GeometryHandler.Vector>();
                    int cnt = 0;
                    foreach(GeometryHandler.Vector sample in samples)
                    {
                        if (cnt < (samples.Count / 2))
                            pointingSamples.Add(sample);
                        else
                            aimingSamples.Add(sample);
                        ++cnt;
                    }
                    
                    pointAvgVectors = this.geometryHandler.getAvgVector(pointingSamples, samplesPerPosition);
                    sortAvgVectors(ref pointAvgVectors, points);                    
                    aimAvgVectors = this.geometryHandler.getAvgVector(aimingSamples, samplesPerPosition);
                    sortAvgVectors(ref aimAvgVectors, points);
                    
                    samples.Clear();
                    foreach(GeometryHandler.Vector pAvgVec in pointAvgVectors)
                    {
                        samples.Add(pAvgVec);
                    }
                    foreach (GeometryHandler.Vector aAvgVec in aimAvgVectors)
                    {
                        samples.Add(aAvgVec);
                    }
                    break;
                default: // Undefined mode
                    break;
            }
        }

        public Point3D definePosition(GeometryHandler.Plane plane, List<GeometryHandler.Vector> samples, int positions, int mode)
        {
            int points = pointsToDefine(samples.Count, positions, mode);
            sortAvgSamples(ref samples, points, mode);

            List<Point3D> pointPositions = new List<Point3D>(); // Pointing intersections from pointOnPlane-calculation
            List<Point3D> aimPositions = new List<Point3D>(); // Aiming intersections from pointOnPlane-calculation
            List<Point3D> combinedPoints = new List<Point3D>();
            Point3D combinedPoint = new Point3D();

            switch (mode)
            {
                case 0: // Only (pointing-)samples
                    foreach (GeometryHandler.Vector vector in samples)
                    {
                        combinedPoints.Add(this.geometryHandler.intersectVectorPlane(plane, vector));
                    }
                    break;
                case 1: // Only (aiming-)samples
                    foreach (GeometryHandler.Vector vector in samples)
                    {
                        combinedPoints.Add(this.geometryHandler.intersectVectorPlane(plane, vector));
                    }
                    break;
                case 2: // Both kinds of samples                    
                    List<GeometryHandler.Vector> pointAvgVectors = new List<GeometryHandler.Vector>();
                    List<GeometryHandler.Vector> aimAvgVectors = new List<GeometryHandler.Vector>();
                    
                    int cnt = 0;
                    foreach (GeometryHandler.Vector sample in samples)
                    {
                        if (cnt < (samples.Count / 2))
                            pointPositions.Add(this.geometryHandler.intersectVectorPlane(plane, sample));
                        else
                            aimPositions.Add(this.geometryHandler.intersectVectorPlane(plane, sample));
                        ++cnt;
                    }

                    combinedPoints = this.geometryHandler.classifyCombined(pointPositions, aimPositions);
                    break;
                default: // Undefined mode
                    break; // Empty
            }
            combinedPoint = this.geometryHandler.getCenter(combinedPoints);

            return combinedPoint;
        }
        #endregion

        #region VALIDATION
        public bool validatePlane(List<Point3D> corners1, List<Point3D> corners2)
        {
            int sum = 0;

            for (int i = 0; i != corners1.Count; ++i)
            {
                sum += validatePoint(corners1[i], corners2[i]);
            }

            if (sum > 7) // 8 or 9 axis' values within threshold
                return true;
            else
                return false;
        }

        public bool validatePlane(GeometryHandler.Plane plane1, GeometryHandler.Plane plane2)
        {
            int sum = withinThreshold(plane1.Start, plane2.Start) + withinThreshold(plane1.End1, plane2.End1) + withinThreshold(plane1.End2, plane2.End2);

            if (sum > 7) // 8 or 9 axis' values within threshold
                return true;
            else
                return false;
        }

        public int validatePoint(Point3D point1, Point3D point2)
        {
            return withinThreshold(point1, point2);
        }

        private int withinThreshold(Point3D point1, Point3D point2)
        {
            int passes = 0;
            Point3D avg = this.geometryHandler.getCenter(point1, point2);

            if (Math.Abs(Math.Abs(point1.X) - Math.Abs(avg.X)) < this.threshold && Math.Abs(Math.Abs(point2.X) - Math.Abs(avg.X)) < this.threshold)
            {
                ++passes;
            }
            if (Math.Abs(Math.Abs(point1.Y) - Math.Abs(avg.Y)) < this.threshold && Math.Abs(Math.Abs(point2.Y) - Math.Abs(avg.Y)) < this.threshold)
            {
                ++passes;
            }
            if (Math.Abs(Math.Abs(point1.Z) - Math.Abs(avg.Z)) < this.threshold && Math.Abs(Math.Abs(point2.Z) - Math.Abs(avg.Z)) < this.threshold)
            {
                ++passes;
            }

            return passes;
        }
        #endregion

        #region DATA-LOGGING
        private void initLogger()
        {
            this.dataLogger.newPargraph(); 
        }

        private void logVectors(List<GeometryHandler.Vector> vectors, int index)
        {
            this.dataLogger.newPargraph("Start.X" + '\t' + "Start.Y" + '\t' + "Start.Z" + '\t' + "End.X" + '\t' + "End.Y" + '\t' + "End.Z" + '\t' + "Direction.X" + '\t' + "Direction.Y" + '\t' + "Direction.Z");
            
            foreach (GeometryHandler.Vector vector in vectors)
            {
                this.dataLogger.addLineToParagraph(index, this.geometryHandler.getString(vector));
            }
        }

        private void logPoints(List<Point3D> points, int index)
        { 
            this.dataLogger.newPargraph("Point.X" + '\t' + "Point.Y" + '\t' + "Point.Z");
       
            foreach (Point3D point in points)
            {
                this.dataLogger.addLineToParagraph(index, this.geometryHandler.getString(point));
            }
        }
        #endregion
    }
}
