using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace IMI_Administration
{
    class CalibrationHandler
    {
        #region DECLARATIONS
        private DataLogger dataLogger;
        private GeometryHandler geometryHandler;

        int samplesPerPosition; // Number of samples taken per position pointed from 
        #endregion

        #region CONSTRUCTOR
        public CalibrationHandler(int samplingVectors)
        {
            this.dataLogger = new DataLogger(@"C:\Users\Haßleben\Desktop\IMI-DATA\Debug\"); // TODO: Initialize properly!
            this.geometryHandler = new GeometryHandler();
            this.samplesPerPosition = samplingVectors; 
        }
        #endregion 

        #region DEFINITIONS
        public List<Point3D> definePlane(List<GeometryHandler.Vector> samples, int positions, int samplesPerPosition, int mode) // Get all sampled vectors, pointing and aiming; mode: 0 = only pointing-samples, 1 = only aiming-samples, 2 = both samples
        {
            initLogger();
            // It needs THREE Points to define a Plane
            List<Point3D> pointCorners = new List<Point3D>(4);
            List<Point3D> aimCorners = new List<Point3D>(4);
            // Pointing variables
            List<GeometryHandler.Vector> pointingSamples; // All or first half of "samples"-List (depending on mode)
            List<GeometryHandler.Vector> pointAvgVectors;
            List<Point3D> pointFootPoints = new List<Point3D>(); // Pointing intersections from foot point-algorithm
            //List<Point3D> pointProjPoints = new List<Point3D>(); // Pointing intersections from projection-algorithm
            // Aiming variables
            List<GeometryHandler.Vector> aimingSamples; // All or second half of "samples"-List (depending on mode)
            List<GeometryHandler.Vector> aimAvgVectors;
            List<Point3D> aimFootPoints = new List<Point3D>(); // Aiming intersections from foot point-algorithm
            //List<Point3D> aimProjPoints = new List<Point3D>(); // Aiming intersections from projection-algorithm
            //List<GeometryHandler.Vector> combineVectors;
            List<Point3D> combinePoints;

            switch (mode)
            { 
                case 0: // Only pointing-samples
                    pointingSamples = samples;
                    pointAvgVectors = this.geometryHandler.getAvgVector(pointingSamples, samplesPerPosition);
                    sortAvgVectors(ref pointAvgVectors, 3); // 3 corners
                    setFootPoints(ref pointFootPoints, pointAvgVectors, 3);
                    //setProjPoints(ref pointProjPoints, pointAvgVectors, 3);
                    break;
                case 1: // Only aiming-samples
                    aimingSamples = samples;
                    aimAvgVectors = this.geometryHandler.getAvgVector(aimingSamples, samplesPerPosition);
                    sortAvgVectors(ref aimAvgVectors, 3); // 3 corners
                    setFootPoints(ref aimFootPoints, aimAvgVectors, 3);
                    //setProjPoints(ref aimProjPoints, aimAvgVectors, 3);
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
                    sortAvgVectors(ref pointAvgVectors, 3); // 3 corners
                    setFootPoints(ref pointFootPoints, pointAvgVectors, 3); // 3 corners
                    //setProjPoints(ref pointProjPoints, pointAvgVectors, 3);

                    aimAvgVectors = this.geometryHandler.getAvgVector(aimingSamples, samplesPerPosition);
                    sortAvgVectors(ref aimAvgVectors, 3); // 3 corners
                    setFootPoints(ref aimFootPoints, aimAvgVectors, 3); // 3 corners
                    //setProjPoints(ref aimProjPoints, aimAvgVectors, 3);

                    combinePoints = this.geometryHandler.getCenters(pointFootPoints, aimFootPoints);

                    this.logVectors(pointAvgVectors, 1); // Index := 1
                    this.logVectors(aimAvgVectors, 2); // Index := 2
                    this.logPoints(pointFootPoints, 3); // Index := 3
                    this.logPoints(aimFootPoints, 4); // Index := 4
                    this.logPoints(combinePoints, 5); // Index := 5
                    this.dataLogger.writeFile();
                    break;
                default: // Undefined mode
                    break;
            }

            return aimFootPoints;
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

        private void setProjPoints(ref List<Point3D> projPoints, List<GeometryHandler.Vector> avgVectors, int points)
        {
            int positions = avgVectors.Count / points;
            List<List<GeometryHandler.Vector>> vectors = new List<List<GeometryHandler.Vector>>();
            List<List<Point3D>> proj = new List<List<Point3D>>();

            int avgCount = 0;
            for (int point = 0; point != points; ++point)
            {
                vectors.Add(new List<GeometryHandler.Vector>()); // Add a list for all vectors to particular point from all positions
                for (int position = 0; position != positions; ++position)
                {
                    vectors[point].Add(avgVectors[avgCount]); // Fill list with vectors from particular position to one point
                    ++avgCount;
                }

                proj.Add(new List<Point3D>()); // Add a list for all foot points concerning one point
                for (int vectorA = 0; vectorA != (vectors[point].Count - 1); ++vectorA) // For each vector, except the last
                {
                    for (int vectorB = (vectorA + 1); vectorB != vectors[point].Count; ++vectorB) // and every following vector
                    {
                        if (vectors[point][vectorA] != vectors[point][vectorB]) // Vectors are not equal(vectorA.Start != vectorB.Start && vectorA.End != vectorB.End)
                        {
                            foreach (Point3D foot in this.geometryHandler.vectorsIntersectProj(vectors[point][vectorA], vectors[point][vectorB]))
                            {
                                proj[point].Add(foot); // Add foot point to list
                            }
                        }
                    }
                }

                projPoints.Add(this.geometryHandler.getCenter(proj[point])); // Add the center of all foot points for particular point to list of foot points
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

        private void setCorners(out List<Point3D> corners, List<Point3D> points)
        {
            int positions = points.Count / 3; // positions = 2; 3; 4;...

            // TODO:
            // - Something useful

            corners = points;
        }

        public void definePointOnPlane(GeometryHandler.Plane plane, GeometryHandler.Vector vector)
        {
            
        }

        public void defineBoundingBox(List<Point3D> points)
        { 
        
        }
        #endregion

        #region EVALUATIONS
        public bool validatePlane(List<Point3D> corners1, List<Point3D> corners2)
        {
            return false;
        }

        private bool validatePoint(Point3D point1, Point3D point2)
        {
            return false;
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
