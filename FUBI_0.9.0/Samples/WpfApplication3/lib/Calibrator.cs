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

        int samples; // Number of samples taken per position pointed from 
        #endregion

        #region CONSTRUCTOR
        public Calibrator(int samples)
        {
            this.dataLogger = new DataLogger(); // TODO: Initialize properly!
            this.geometryHandler = new GeometryHandler();
            this.samples = samples; 
        }
        #endregion

        #region DEFINITIONS
        public List<Point3D> definePlane(List<GeometryHandler.Vector> samples)
        {
            int pointings = samples.Count / this.samples; // Pointing from a positions to b corners gives a*b pointings => up to a*b intersections
            List<GeometryHandler.Vector> tmpVec = new List<GeometryHandler.Vector>();
            List<GeometryHandler.Vector> vectors = new List<GeometryHandler.Vector>();
            List<Point3D> footPoints = new List<Point3D>();
            List<Point3D> projPoints = new List<Point3D>();

            for (int pointing = 0; pointing != pointings; ++pointing)
            {
                for (int sample = pointing * this.samples; sample != (pointing + 1) * this.samples; ++sample)
                {
                    tmpVec.Add(samples[sample]);
                }
                vectors.Add(this.geometryHandler.getAvgVector(tmpVec));
            }

            for (int vectorA = 0; vectorA != (vectors.Count - 1); ++vectorA) // For each vector, except the last
            {
                for (int vectorB = (vectorA + 1); vectorB != vectors.Count; ++vectorB) // Every following vector
                {
                    if (vectors[vectorA] != vectors[vectorB]) //(vectorA.Start != vectorB.Start && vectorA.End != vectorB.End)
                    {
                        // TODO: Stuff
                        foreach (Point3D intersection in this.geometryHandler.vectorsLotfuesse(vectors[vectorA], vectors[vectorB]))
                        {
                            footPoints.Add(intersection);
                        }
                        // TODO: Stuff
                        //foreach (Point3D intersection in this.geometryHandler.vectorsIntersectTest(vectors[vectorA], vectors[vectorB]))
                        //{
                        //    projPoints.Add(intersection);
                        //}
                    }
                }
            }

            Point3D footAve = this.geometryHandler.getCenter(footPoints);
            //Point3D projAve = this.geometryHandler.getCenter(projPoints);
            return footPoints;
        }

        private Point3D definePointInSpace(List<GeometryHandler.Vector> vectors)
        {
            Point3D point = new Point3D();

            // TODO: Stuff

            return point;
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

        public void evaluateVectorInSpace()
        { 
            
        }

        public void evaluateVectorOnPlane()
        {

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
