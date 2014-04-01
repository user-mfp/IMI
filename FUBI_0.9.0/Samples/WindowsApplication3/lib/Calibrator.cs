using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace IMI.lib
{
    class Calibrator
    {
        #region DECLARATIONS
        private DataLogger dataLogger;
        #endregion

        #region DEFINITIONS
        public List<Point3D> definePlane(List<GeometryHandler.Vector> samples)
        {
            List<Point3D> points = new List<Point3D>();

            // DO STUFF

            return points;
        }

        private Point3D definePointInSpace(List<GeometryHandler.Vector> vectors)
        {
            Point3D point = new Point3D();

            // DO STUFF

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
            this.dataLogger.newPargraph("");
        }
        #endregion
    }
}
