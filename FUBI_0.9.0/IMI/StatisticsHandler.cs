using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace IMI
{
    public partial class StatisticsHandler
    {
        #region ROUTINES
        public Point3D getAvg(List<Point3D> points)
        {
            Point3D center = new Point3D();
            List<double> x_values = new List<double>();
            List<double> y_values = new List<double>();
            List<double> z_values = new List<double>();

            foreach (Point3D point in points)
            {
                x_values.Add(point.X);
                y_values.Add(point.Y);
                z_values.Add(point.Z);
            }

            center.X = getAvg(x_values);
            center.Y = getAvg(y_values);
            center.Z = getAvg(z_values);

            return center;
        }

        public double getAvg(List<double> values)
        {
            double avg = 0;

            foreach (double value in values)
            {
                avg += value;
            }

            return avg / values.Count;
        }

        public double getVar(List<double> values)
        {
            double var = 0;
            double avg = getAvg(values);

            foreach (double value in values)
            {
                var += Math.Pow((value - avg), 2);
            }

            return var / values.Count;
        }

        public Point3D getStdAbw(List<Point3D> points)
        {
            Point3D stdAbw = new Point3D();
            List<double> x_values = new List<double>();
            List<double> y_values = new List<double>();
            List<double> z_values = new List<double>();

            foreach (Point3D point in points)
            {
                x_values.Add(point.X);
                y_values.Add(point.Y);
                z_values.Add(point.Z);
            }

            stdAbw.X = getStdAbw(x_values);
            stdAbw.Y = getStdAbw(y_values);
            stdAbw.Z = getStdAbw(z_values);

            return stdAbw;
        }

        public double getStdAbw(List<double> values)
        {
            return Math.Sqrt(getVar(values));
        }

        public double getEmpVar(List<double> values)
        {
            double var = getVar(values);
            double m = values.Count / (values.Count - 1.0);

            return m * var;
        }

        public Point3D getEmpStdAbw(List<Point3D> points)
        {
            Point3D empStdAbw = new Point3D();
            List<double> x_values = new List<double>();
            List<double> y_values = new List<double>();
            List<double> z_values = new List<double>();

            foreach (Point3D point in points)
            {
                x_values.Add(point.X);
                y_values.Add(point.Y);
                z_values.Add(point.Z);
            }

            empStdAbw.X = getEmpStdAbw(x_values);
            empStdAbw.Y = getEmpStdAbw(y_values);
            empStdAbw.Z = getEmpStdAbw(z_values);

            return empStdAbw;
        }

        public double getEmpStdAbw(List<double> values)
        {
            return Math.Sqrt(getEmpVar(values));
        }

        public Point3D getStdErr(List<Point3D> points)
        {
            Point3D empStdAbw = new Point3D();
            List<double> x_values = new List<double>();
            List<double> y_values = new List<double>();
            List<double> z_values = new List<double>();

            foreach (Point3D point in points)
            {
                x_values.Add(point.X);
                y_values.Add(point.Y);
                z_values.Add(point.Z);
            }

            empStdAbw.X = getStdErr(x_values);
            empStdAbw.Y = getStdErr(y_values);
            empStdAbw.Z = getStdErr(z_values);

            return empStdAbw;
        }

        public double getStdErr(List<double> values)
        {
            return getEmpStdAbw(values) / Math.Sqrt(values.Count);
        }
        #endregion
    }
}
