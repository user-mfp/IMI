using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;
using System;
using System.Drawing;

namespace PostValidator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DECLARATIONS
        //--- Debugging ---//
        private string debug1;
        private int pass0 = 0;
        private int pass1 = 0;
        private int pass2 = 0;
        private int pass3 = 0;
        private int passX0 = 0;
        private int passX1 = 0;
        private int passX2 = 0;
        private int passX3 = 0;
        private int passY0 = 0;
        private int passY1 = 0;
        private int passY2 = 0;
        private int passY3 = 0;
        private int passZ0 = 0;
        private int passZ1 = 0;
        private int passZ2 = 0;
        private int passZ3 = 0;
        private int pall = 0;
        private int pXall = 0;
        private int pYall = 0;
        private int pZall = 0;

        //--- Reading ---//
        private string folder = @"D:\Master\TestFolder\2014-1-28_defPlane\Rohdaten\";
        //private string pointingPth = @"D:\Master\TestFolder\2014-2-12_defPlane\Rohdaten\";
        private string completePth = "all.txt";
        //private string cleanedPth = "clean.txt";
        
        //--- Parsing ---//
        private List<Point3D> pointingPts = new List<Point3D>();
        private List<Point3D> lookingPts = new List<Point3D>();
        private List<Point3D> mismatchPts = new List<Point3D>();

        //--- Constants ---//
        private int stretch = 7;
        private double threshold = 0.0;
        #endregion

        #region INITIALIZATION
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region DEBUGGING
        private void updateView()
        {
            this.label1.Content = this.debug1;
        }
        #endregion

        #region EVALUATE
        private void evaluate()
        {
            List<string[]> data = readTxt();

            parseLines(data);

            drawBitmap();
        }
        #endregion

        #region READING
        private List<string[]> readTxt()
        {
            List<string[]> data = new List<string[]>();
            switch (this.comboBox1.SelectedIndex)
            {
                case 0: // Pointing
                    data.Add(System.IO.File.ReadAllLines(this.folder + "PointingX" + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "PointingY" + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "PointingZ" + this.completePth));
                    break;
                case 1: // Looking
                    data.Add(System.IO.File.ReadAllLines(this.folder + "lookingX" + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "lookingY" + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "lookingZ" + this.completePth));
                    break;
                case 2: // Mismatch
                    data.Add(System.IO.File.ReadAllLines(this.folder + "MismatchX" + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "MismatchY" + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "MismatchZ" + this.completePth));
                    break;
                default:
                    break;
            }
            return data;
        }
        #endregion

        #region PARSING
        private void parseLines(List<string[]> data)
        {
            Point3D tmpPnt = new Point3D();
            switch (this.comboBox1.SelectedIndex)
            {
                case 0:
                    if (this.pointingPts.Count == 0)
                    {
                        for (int i = 0; i != data[0].Length; ++i)
                        {
                            tmpPnt.X = Double.Parse(data[0][i]);
                            tmpPnt.Y = Double.Parse(data[1][i]);
                            tmpPnt.Z = Double.Parse(data[2][i]);
                            this.pointingPts.Add(tmpPnt);
                        }
                    }
                    break;
                case 1:
                    if (this.lookingPts.Count == 0)
                    {
                        for (int i = 0; i != data[0].Length; ++i)
                        {
                            tmpPnt.X = Double.Parse(data[0][i]);
                            tmpPnt.Y = Double.Parse(data[1][i]);
                            tmpPnt.Z = Double.Parse(data[2][i]);
                            this.lookingPts.Add(tmpPnt);
                        }
                    }
                    break;
                case 2:
                    if (this.mismatchPts.Count == 0)
                    {
                        for (int i = 0; i != data[0].Length; ++i)
                        {
                            tmpPnt.X = Double.Parse(data[0][i]);
                            tmpPnt.Y = Double.Parse(data[1][i]);
                            tmpPnt.Z = Double.Parse(data[2][i]);
                            this.mismatchPts.Add(tmpPnt);
                        }
                    }
                    break;
                default:
                    break;
            }
            updateView();
        }
        #endregion

        #region DRAWING
        private void drawBitmap()
        {
            Bitmap tmpBmp;

            switch (this.comboBox1.SelectedIndex)
            { 
                case 0:
                    tmpBmp = new Bitmap(this.pointingPts.Count * this.stretch, this.pointingPts.Count * this.stretch);
                    for (int pt1 = 0; pt1 != this.pointingPts.Count; ++pt1)
                    {
                        for (int pt2 = 0; pt2 != this.pointingPts.Count; ++pt2)
                        {
                            if (pt1 == pt2)
                            {
                                for (int i1 = 0; i1 != this.stretch; ++i1)
                                {
                                    for (int i2 = 0; i2 != this.stretch; ++i2)
                                    {
                                        tmpBmp.SetPixel((pt1 * this.stretch) + i1, (pt2 * this.stretch) + i2, Color.White);
                                    }
                                }
                            }
                            else
                            {
                                for (int i1 = 0; i1 != this.stretch; ++i1)
                                {
                                    for (int i2 = 0; i2 != this.stretch; ++i2)
                                    {
                                        tmpBmp.SetPixel((pt1 * this.stretch) + i1, (pt2 * this.stretch) + i2, visualize(withinthreshold(this.pointingPts[pt1], this.pointingPts[pt2])));
                                    }
                                }
                            }
                        }
                    }
                    tmpBmp.Save(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_pointing_" + this.threshold + ".BMP");
                    makeStats();
                    break;
                case 1:
                    tmpBmp = new Bitmap(this.lookingPts.Count * this.stretch, this.lookingPts.Count * this.stretch);
                    for (int pt1 = 0; pt1 != this.lookingPts.Count; ++pt1)
                    {
                        for (int pt2 = 0; pt2 != this.lookingPts.Count; ++pt2)
                        {
                            if (pt1 == pt2)
                            {
                                for (int i1 = 0; i1 != this.stretch; ++i1)
                                {
                                    for (int i2 = 0; i2 != this.stretch; ++i2)
                                    {
                                        tmpBmp.SetPixel((pt1 * this.stretch) + i1, (pt2 * this.stretch) + i2, Color.White);
                                    }
                                }
                            }
                            else
                            {
                                for (int i1 = 0; i1 != this.stretch; ++i1)
                                {
                                    for (int i2 = 0; i2 != this.stretch; ++i2)
                                    {
                                        tmpBmp.SetPixel((pt1 * this.stretch) + i1, (pt2 * this.stretch) + i2, visualize(withinthreshold(this.lookingPts[pt1], this.lookingPts[pt2])));
                                    }
                                }
                            }
                        }
                    }
                    tmpBmp.Save(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_looking_" + this.threshold + ".BMP");
                    makeStats();
                    break;
                case 2:
                    tmpBmp = new Bitmap(this.mismatchPts.Count * this.stretch, this.mismatchPts.Count * this.stretch);
                    for (int pt1 = 0; pt1 != this.mismatchPts.Count; ++pt1)
                    {
                        for (int pt2 = 0; pt2 != this.mismatchPts.Count; ++pt2)
                        {
                            if (pt1 == pt2)
                            {
                                for (int i1 = 0; i1 != this.stretch; ++i1)
                                {
                                    for (int i2 = 0; i2 != this.stretch; ++i2)
                                    {
                                        tmpBmp.SetPixel((pt1 * this.stretch) + i1, (pt2 * this.stretch) + i2, Color.White);
                                    }
                                }
                            }
                            else
                            {
                                for (int i1 = 0; i1 != this.stretch; ++i1)
                                {
                                    for (int i2 = 0; i2 != this.stretch; ++i2)
                                    {
                                        tmpBmp.SetPixel((pt1 * this.stretch) + i1, (pt2 * this.stretch) + i2, visualize(withinthreshold(this.mismatchPts[pt1], this.mismatchPts[pt2])));
                                    }
                                }
                            }
                        }
                    }
                    tmpBmp.Save(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_mismatch_" + this.threshold + ".BMP");
                    makeStats();
                    break;
                default:
                    break;
            }
        }

        private Color visualize(int passes)
        {
            switch (passes)
            { 
                case 0:
                    return Color.Red;
                case 1:
                    return Color.Orange;
                case 2:
                    return Color.Yellow;
                case 3:
                    return Color.Green;
                default:
                    return Color.Black;
            }
        }
        #endregion

        #region ROUTINES
        private int withinthreshold(Point3D a, Point3D b)
        {
            ++this.pall;
            int passes = 0;

            if (Math.Abs(a.X - b.X) < this.threshold)
                ++passes;
            if (Math.Abs(a.Y - b.Y) < this.threshold)
                ++passes;
            if (Math.Abs(a.Z - b.Z) < this.threshold)
                ++passes;

            switch (passes)
            {
                case 0:
                    ++this.pass0;
                    break;
                case 1:
                    ++this.pass1;
                    break;
                case 2:
                    ++this.pass2;
                    break;
                case 3:
                    ++this.pass3;
                    break;
                default:
                    break;
            }
            return passes;
        }

        private void makeStats()
        {
            // NumberOfPoints
            double nop = Math.Max(Math.Max(this.pointingPts.Count, this.lookingPts.Count), this.mismatchPts.Count);
            // NumberOfValidations = number of pixels
            double nov = Math.Pow((nop * this.stretch), 2);
            // Pixels on diagonal
            double diagPx = nop * Math.Pow(this.stretch, 2);
            // Pixels of validation
            double valiPx = nov - diagPx;
            // 1% of all validated pixels
            double passPer = valiPx / 100.0;
            // Number of number of passes
            double pass0 = this.pass0 / passPer;
            double pass1 = this.pass1 / passPer;
            double pass2 = this.pass2 / passPer;
            double pass3 = this.pass3 / passPer;
            double passes = pass0 + pass1 + pass2 + pass3;

            List<string> lines = new List<string>();
            switch (this.comboBox1.SelectedIndex)
            { 
                case 0:
                    System.IO.StreamWriter pointingFile = new System.IO.StreamWriter(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_pointing_" + this.threshold + ".TXT");
                    lines.Add("Pointing");
                    lines.Add("");
                    lines.Add("Points:" + '\t' + this.pointingPts.Count);
                    lines.Add("threshold:" + '\t' + this.threshold + '\t' + "mm");
                    lines.Add("3 Passes:" + '\t' + pass3 + '\t' + "%");
                    lines.Add("2 Passes:" + '\t' + pass2 + '\t' + "%");
                    lines.Add("1 Pass:" + '\t' + pass1 + '\t' + "%");
                    lines.Add("0 Passes:" + '\t' + pass0 + '\t' + "%");

                    foreach (string line in lines)
                    {
                        pointingFile.WriteLine(line);
                    }
                    pointingFile.Close();
                    break;
                case 1:
                    System.IO.StreamWriter lookingFile = new System.IO.StreamWriter(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_looking_" + this.threshold + ".TXT");
                    lines.Add("Looking");
                    lines.Add("");
                    lines.Add("Points:" + '\t' + this.lookingPts.Count);
                    lines.Add("threshold:" + '\t' + this.threshold + '\t' + "mm");
                    lines.Add("3 Passes:" + '\t' + pass3 + '\t' + "%");
                    lines.Add("2 Passes:" + '\t' + pass2 + '\t' + "%");
                    lines.Add("1 Pass:" + '\t' + pass1 + '\t' + "%");
                    lines.Add("0 Passes:" + '\t' + pass0 + '\t' + "%");

                    foreach (string line in lines)
                    {
                        lookingFile.WriteLine(line);
                    }
                    lookingFile.Close();
                    break;
                case 2:
                    System.IO.StreamWriter mitmatchFile = new System.IO.StreamWriter(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_mismatch_" + this.threshold + ".TXT");
                    lines.Add("Mismatch");
                    lines.Add("");
                    lines.Add("Points:" + '\t' + this.mismatchPts.Count);
                    lines.Add("threshold:" + '\t' + this.threshold + '\t' + "mm");
                    lines.Add("3 Passes:" + '\t' + pass3 + '\t' + "%");
                    lines.Add("2 Passes:" + '\t' + pass2 + '\t' + "%");
                    lines.Add("1 Pass:" + '\t' + pass1 + '\t' + "%");
                    lines.Add("0 Passes:" + '\t' + pass0 + '\t' + "%");

                    foreach (string line in lines)
                    {
                        mitmatchFile.WriteLine(line);
                    }
                    mitmatchFile.Close();
                    break;
                default:
                    break;
            }

            this.debug1 += "3 Passes:" + '\t' + Math.Round(pass3, 3) + "%" + '\n' + "2 Passes:" + '\t' + Math.Round(pass2, 3) + "%" + '\n' + "1 Pass:" + '\t' + Math.Round(pass1, 3) + "%" + '\n' + "0 Passes:" + '\t' + Math.Round(pass0, 3) + "%" + '\n';
            
            this.pass0 = 0;
            this.pass1 = 0;
            this.pass2 = 0;
            this.pass3 = 0;
            this.pall = 0;
        }
        #endregion

        #region EVENTS
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch (this.comboBox1.SelectedIndex) // Mode of operation
            { 
                case 0: // Pointing
                    this.debug1 = "Pointing" + '\n';
                    evaluate();
                    break;
                case 1: // Looking
                    this.debug1 = "Looking" + '\n';
                    evaluate();
                    break;
                case 2: // Mismatch
                    this.debug1 = "Mismatch" + '\n';
                    evaluate();
                    break;
                default:
                    this.debug1 = "Choose Mode!";
                    break;
            }

            updateView();
        }

        private void comboBox2_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (this.comboBox2.SelectedIndex)
            {
                case 0:
                    this.threshold = 100.0;
                    break;
                case 1:
                    this.threshold = 110.0;
                    break;
                case 2:
                    this.threshold = 120.0;
                    break;
                case 3:
                    this.threshold = 130.0;
                    break;
                case 4:
                    this.threshold = 140.0;
                    break;
                case 5:
                    this.threshold = 150.0;
                    break;
                case 6:
                    this.threshold = 160.0;
                    break;
                case 7:
                    this.threshold = 170.0;
                    break;
                case 8:
                    this.threshold = 180.0;
                    break;
                case 9:
                    this.threshold = 190.0;
                    break;
                case 10:
                    this.threshold = 200.0;
                    break;
                default:
                    this.threshold = 0.0;
                    break;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            for (int mode = 0; mode != 3; ++mode)
            {
                this.comboBox1.SelectedIndex = mode;
                for (int threshold = 0; threshold != 11; ++threshold)
                {
                    this.comboBox2.SelectedIndex = threshold;
                    evaluate();
                }
            }
        }
        #endregion
    }
}
