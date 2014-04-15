using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace WpfApplication3.lib
{
    class Calibrator
    {
        #region DECLARATIONS
        private DataLogger dataLogger;
        private GeometryHandler geometryHandler;

        int samplesPerPosition; // Number of samples taken per position pointed from 
        #endregion

        #region CONSTRUCTOR
        public Calibrator(int samples)
        {
            this.dataLogger = new DataLogger(); // TODO: Initialize properly!
            this.geometryHandler = new GeometryHandler();
            this.samplesPerPosition = samples; 
        }
        #endregion 

        #region DEFINITIONS
        public List<Point3D> definePlane(List<GeometryHandler.Vector> samples, int positions, int samplesPerPosition, int mode) // Get all sampled vectors, pointing and aiming; mode: 0 = only pointing-samples, 1 = only aiming-samples, 2 = both samples
        {
            // It needs THREE Points to define a Plane
            List<Point3D> pointCorners = new List<Point3D>(3);
            List<Point3D> aimCorners = new List<Point3D>(3);
            // Pointing variables
            List<GeometryHandler.Vector> pointingSamples; // All or first half of "samples"-List (depending on mode)
            List<GeometryHandler.Vector> pointAvgVectors;
            List<Point3D> pointFootPoints = new List<Point3D>(); // Pointing intersections from foot point-algorithm
            List<Point3D> pointProjPoints = new List<Point3D>(); // Pointing intersections from projection-algorithm
            // Aiming variables
            List<GeometryHandler.Vector> aimingSamples; // All or second half of "samples"-List (depending on mode)
            List<GeometryHandler.Vector> aimAvgVectors;
            List<Point3D> aimFootPoints = new List<Point3D>(); // Aiming intersections from foot point-algorithm
            List<Point3D> aimProjPoints = new List<Point3D>(); // Aiming intersections from projection-algorithm

            switch (mode)
            { 
                case 0: // Only pointing-samples
                    pointingSamples = samples;
                    pointAvgVectors = this.geometryHandler.getAvgVector(pointingSamples, samplesPerPosition);
                    sortAvgVectors(ref pointAvgVectors, 3); // 3 corners
                    setFootPoints(ref pointFootPoints, pointAvgVectors);
                    setProjPoints(ref pointProjPoints, pointAvgVectors);
                    break;
                case 1: // Only aiming-samples
                    aimingSamples = samples;
                    aimAvgVectors = this.geometryHandler.getAvgVector(aimingSamples, samplesPerPosition);
                    sortAvgVectors(ref aimAvgVectors, 3); // 3 corners
                    setFootPoints(ref aimFootPoints, aimAvgVectors);
                    setProjPoints(ref aimProjPoints, aimAvgVectors);
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
                    setFootPoints(ref pointFootPoints, pointAvgVectors);
                    setProjPoints(ref pointProjPoints, pointAvgVectors);

                    aimAvgVectors = this.geometryHandler.getAvgVector(aimingSamples, samplesPerPosition);
                    sortAvgVectors(ref aimAvgVectors, 3); // 3 corners
                    setFootPoints(ref aimFootPoints, aimAvgVectors);
                    setProjPoints(ref aimProjPoints, aimAvgVectors);
                    break;
                default: // Undefined mode
                    break;
            }
            //setCorners(out pointCorners, pointFootPoints);
            return null;// pointFootPoints;
        }

        private void setFootPoints(ref List<Point3D> points, List<GeometryHandler.Vector> avgVectors)
        {
            List<Point3D> tmp = new List<Point3D>();

            for (int vectorA = 0; vectorA != (avgVectors.Count - 1); ++vectorA) // For each vector, except the last
            {
                for (int vectorB = (vectorA + 1); vectorB != avgVectors.Count; ++vectorB) // and every following vector
                {
                    if (avgVectors[vectorA] != avgVectors[vectorB]) // Vectors are not equal(vectorA.Start != vectorB.Start && vectorA.End != vectorB.End)
                    {
                        foreach (Point3D intersection in this.geometryHandler.vectorsIntersectFoot(avgVectors[vectorA], avgVectors[vectorB]))
                        {
                            points.Add(intersection); // Add intersections to List
                        }
                    }
                }
            }

            //points = tmp;
        }

        private void setProjPoints(ref List<Point3D> points, List<GeometryHandler.Vector> avgVectors)
        {
            for (int vectorA = 0; vectorA != (avgVectors.Count - 1); ++vectorA) // For each vector, except the last
            {
                for (int vectorB = (vectorA + 1); vectorB != avgVectors.Count; ++vectorB) // and every following vector
                {
                    if (avgVectors[vectorA] != avgVectors[vectorB]) // Vectors are not equal(vectorA.Start != vectorB.Start && vectorA.End != vectorB.End)
                    {
                        foreach (Point3D intersection in this.geometryHandler.vectorsIntersectProj(avgVectors[vectorA], avgVectors[vectorB]))
                        {
                            points.Add(intersection); // Add intersections to List
                        }
                    }
                }
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
            this.dataLogger = new DataLogger(@"D:\Master\TestFolder\2014-3-4_defPlane\Me.txt");
            // Headline
            this.dataLogger.newPargraph("leer");
        }
        #endregion
    }
}
