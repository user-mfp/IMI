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
        private int pass0;
        private int pass1;
        private int pass2;
        private int pass3;
        private int pall;
        private Dictionary<Point3D, int> passes = new Dictionary<Point3D, int>();

        //--- Reading ---//
        private string folder = @"D:\Master\TestFolder\CAL_0\Rohdaten\PostEval\";
        private string curPnt; // Current point
        //private string pointingPth = @"D:\Master\TestFolder\2014-2-12_defPlane\Rohdaten\";
        private string completePth = ".txt";
        //private string cleanedPth = "clean.txt";
        
        //--- Parsing ---//
        private List<Point3D> pointingPts = new List<Point3D>();
        private List<Point3D> aimingPts = new List<Point3D>();
        private List<Point3D> combinedPts = new List<Point3D>();

        //--- Constants ---//
        private int stretch = 10;
        private double threshold;
        #endregion

        #region INITIALIZATION
        public MainWindow()
        {
            InitializeComponent();
            initPassesDict();
        }

        private void initPassesDict()
        {
            // No passes at all
            this.passes.Add(new Point3D(0, 0, 0), 0);
            // Only one pass
            this.passes.Add(new Point3D(1, 0, 0), 0);
            this.passes.Add(new Point3D(0, 1, 0), 0);
            this.passes.Add(new Point3D(0, 0, 1), 0);
            // Two passes
            this.passes.Add(new Point3D(1, 1, 0), 0);
            this.passes.Add(new Point3D(1, 0, 1), 0);
            this.passes.Add(new Point3D(0, 1, 1), 0);
            // Three passes
            this.passes.Add(new Point3D(1, 1, 1), 0);

            // Set passes' counter to 0
            pass0 = pass1 = pass2 = pass3 = pall = 0;
        }

        private void evalPassesDict()
        {
            int passes;
            Point3D tmp = new Point3D();

            foreach (KeyValuePair<Point3D, int> pair in this.passes)
            {
                tmp = pair.Key;
                passes = (int)(tmp.X + tmp.Y + tmp.Z);

                switch (passes)
                { 
                    case 0:
                        pass0 += pair.Value;
                        break;
                    case 1:
                        pass1 += pair.Value;
                        break;
                    case 2:
                        pass2 += pair.Value;
                        break;
                    case 3:
                        pass3 += pair.Value;
                        break;
                    default:
                        break;
                }
                
                this.pall += pair.Value;
            }
            int i = 0;
        }

        private void resetPassesDict()
        {
            this.passes.Clear();
            initPassesDict();
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

            evalPassesDict();
            
            makeStats();

            resetPassesDict();
        }
        #endregion

        #region READING
        private List<string[]> readTxt()
        {
            List<string[]> data = new List<string[]>();
            switch (this.comboBox1.SelectedIndex)
            {
                case 0: // Pointing
                    data.Add(System.IO.File.ReadAllLines(this.folder + "PointingX" + this.curPnt + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "PointingY" + this.curPnt + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "PointingZ" + this.curPnt + this.completePth));
                    break;
                case 1: // Aiming
                    data.Add(System.IO.File.ReadAllLines(this.folder + "AimingX" + this.curPnt + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "AimingY" + this.curPnt + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "AimingZ" + this.curPnt + this.completePth));
                    break;
                case 2: // Combined
                    data.Add(System.IO.File.ReadAllLines(this.folder + "CombinedX" + this.curPnt + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "CombinedY" + this.curPnt + this.completePth));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "CombinedZ" + this.curPnt + this.completePth));
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
                    this.pointingPts.Clear();                    
                    for (int i = 0; i != data[0].Length; ++i)
                    {
                        tmpPnt.X = Double.Parse(data[0][i]);
                        tmpPnt.Y = Double.Parse(data[1][i]);
                        tmpPnt.Z = Double.Parse(data[2][i]);
                        this.pointingPts.Add(tmpPnt);
                    }                    
                    break;
                case 1:
                    this.aimingPts.Clear();
                    for (int i = 0; i != data[0].Length; ++i)
                    {
                        tmpPnt.X = Double.Parse(data[0][i]);
                        tmpPnt.Y = Double.Parse(data[1][i]);
                        tmpPnt.Z = Double.Parse(data[2][i]);
                        this.aimingPts.Add(tmpPnt);
                    }
                    break;
                case 2:
                    this.combinedPts.Clear();
                    for (int i = 0; i != data[0].Length; ++i)
                    {
                        tmpPnt.X = Double.Parse(data[0][i]);
                        tmpPnt.Y = Double.Parse(data[1][i]);
                        tmpPnt.Z = Double.Parse(data[2][i]);
                        this.combinedPts.Add(tmpPnt);
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
                    tmpBmp.Save(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Pointing" + this.curPnt + "_" + this.threshold + ".BMP");
                    break;
                case 1:
                    tmpBmp = new Bitmap(this.aimingPts.Count * this.stretch, this.aimingPts.Count * this.stretch);
                    for (int pt1 = 0; pt1 != this.aimingPts.Count; ++pt1)
                    {
                        for (int pt2 = 0; pt2 != this.aimingPts.Count; ++pt2)
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
                                        tmpBmp.SetPixel((pt1 * this.stretch) + i1, (pt2 * this.stretch) + i2, visualize(withinthreshold(this.aimingPts[pt1], this.aimingPts[pt2])));
                                    }
                                }
                            }
                        }
                    }
                    tmpBmp.Save(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Aiming" + this.curPnt + "_" + this.threshold + ".BMP");
                    break;
                case 2:
                    tmpBmp = new Bitmap(this.combinedPts.Count * this.stretch, this.combinedPts.Count * this.stretch);
                    for (int pt1 = 0; pt1 != this.combinedPts.Count; ++pt1)
                    {
                        for (int pt2 = 0; pt2 != this.combinedPts.Count; ++pt2)
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
                                        tmpBmp.SetPixel((pt1 * this.stretch) + i1, (pt2 * this.stretch) + i2, visualize(withinthreshold(this.combinedPts[pt1], this.combinedPts[pt2])));
                                    }
                                }
                            }
                        }
                    }
                    tmpBmp.Save(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Combined" + this.curPnt + "_" + this.threshold + ".BMP");
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
            int passes = 0;
            Point3D tmp = new Point3D();

            if (Math.Abs(a.X - b.X) < this.threshold)
            {
                tmp.X = 1;
                ++passes;
            }
            if (Math.Abs(a.Y - b.Y) < this.threshold)
            {
                tmp.Y = 1;
                ++passes;
            }
            if (Math.Abs(a.Z - b.Z) < this.threshold)
            {
                tmp.Z = 1;
                ++passes;
            }

            if (this.passes.ContainsKey(tmp))
            {
                ++this.passes[tmp];
            }
            return passes;
        }

        private void makeStats()
        {
            // NumberOfPoints
            double nop = Math.Max(Math.Max(this.pointingPts.Count, this.aimingPts.Count), this.combinedPts.Count);
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
                    System.IO.StreamWriter pointingFile = new System.IO.StreamWriter(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Pointing" + this.curPnt + "_" + this.threshold + ".TXT");
                    lines.Add("Pointing to Point #" + this.curPnt);
                    lines.Add("");
                    lines.Add("Points:" + '\t' + this.pointingPts.Count);
                    lines.Add("Threshold:" + '\t' + this.threshold + '\t' + "mm");
                    lines.Add("");
                    lines.Add("3 Passes:" + '\t' + this.pass3 + '\t' + pass3 + '\t' + "%");
                    lines.Add("(X,Y,Z):" + '\t' + this.passes[new Point3D(1, 1, 1)] + '\t' + (this.passes[new Point3D(1, 1, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("");
                    lines.Add("2 Passes:" + '\t' + this.pass2 + '\t' + pass2 + '\t' + "%");
                    lines.Add("(X,Y,_):" + '\t' + this.passes[new Point3D(1, 1, 0)] + '\t' + (this.passes[new Point3D(1, 1, 0)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(X,_,Z):" + '\t' + this.passes[new Point3D(1, 0, 1)] + '\t' + (this.passes[new Point3D(1, 0, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(_,Y,Z):" + '\t' + this.passes[new Point3D(0, 1, 1)] + '\t' + (this.passes[new Point3D(0, 1, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("");
                    lines.Add("1 Pass: " + '\t' + this.pass1 + '\t' + pass1 + '\t' + "%");
                    lines.Add("(X,_,_):" + '\t' + this.passes[new Point3D(1, 0, 0)] + '\t' + (this.passes[new Point3D(1, 0, 0)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(_,Y,_):" + '\t' + this.passes[new Point3D(0, 1, 0)] + '\t' + (this.passes[new Point3D(0, 1, 0)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(_,_,Z):" + '\t' + this.passes[new Point3D(0, 0, 1)] + '\t' + (this.passes[new Point3D(0, 0, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("");
                    lines.Add("0 Passes:" + '\t' + this.pass0 + '\t' + pass0 + '\t' + "%");
                    lines.Add("(_,_,_):" + '\t' + this.passes[new Point3D(0, 0, 0)] + '\t' + (this.passes[new Point3D(0, 0, 0)] / passPer).ToString() + '\t' + "%");

                    foreach (string line in lines)
                    {
                        pointingFile.WriteLine(line);
                    }
                    pointingFile.Close();
                    break;
                case 1:
                    System.IO.StreamWriter aimingFile = new System.IO.StreamWriter(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Aiming" + this.curPnt + "_" + this.threshold + ".TXT");
                    lines.Add("Aiming to Point #" + this.curPnt);
                    lines.Add("");
                    lines.Add("Points:" + '\t' + this.aimingPts.Count);
                    lines.Add("Threshold:" + '\t' + this.threshold + '\t' + "mm");
                    lines.Add("");
                    lines.Add("3 Passes:" + '\t' + this.pass3 + '\t' + pass3 + '\t' + "%");
                    lines.Add("(X,Y,Z):" + '\t' + this.passes[new Point3D(1, 1, 1)] + '\t' + (this.passes[new Point3D(1, 1, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("");
                    lines.Add("2 Passes:" + '\t' + this.pass2 + '\t' + pass2 + '\t' + "%");
                    lines.Add("(X,Y,_):" + '\t' + this.passes[new Point3D(1, 1, 0)] + '\t' + (this.passes[new Point3D(1, 1, 0)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(X,_,Z):" + '\t' + this.passes[new Point3D(1, 0, 1)] + '\t' + (this.passes[new Point3D(1, 0, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(_,Y,Z):" + '\t' + this.passes[new Point3D(0, 1, 1)] + '\t' + (this.passes[new Point3D(0, 1, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("");
                    lines.Add("1 Pass: " + '\t' + this.pass1 + '\t' + pass1 + '\t' + "%");
                    lines.Add("(X,_,_):" + '\t' + this.passes[new Point3D(1, 0, 0)] + '\t' + (this.passes[new Point3D(1, 0, 0)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(_,Y,_):" + '\t' + this.passes[new Point3D(0, 1, 0)] + '\t' + (this.passes[new Point3D(0, 1, 0)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(_,_,Z):" + '\t' + this.passes[new Point3D(0, 0, 1)] + '\t' + (this.passes[new Point3D(0, 0, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("");
                    lines.Add("0 Passes:" + '\t' + this.pass0 + '\t' + pass0 + '\t' + "%");
                    lines.Add("(_,_,_):" + '\t' + this.passes[new Point3D(0, 0, 0)] + '\t' + (this.passes[new Point3D(0, 0, 0)] / passPer).ToString() + '\t' + "%");

                    foreach (string line in lines)
                    {
                        aimingFile.WriteLine(line);
                    }
                    aimingFile.Close();
                    break;
                case 2:
                    System.IO.StreamWriter mitmatchFile = new System.IO.StreamWriter(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Combined" + this.curPnt + "_" + this.threshold + ".TXT");
                    lines.Add("Combined of Point #" + this.curPnt);
                    lines.Add("");
                    lines.Add("Points:" + '\t' + this.combinedPts.Count);
                    lines.Add("Threshold:" + '\t' + this.threshold + '\t' + "mm");
                    lines.Add("");
                    lines.Add("3 Passes:" + '\t' + this.pass3 + '\t' + pass3 + '\t' + "%");
                    lines.Add("(X,Y,Z):" + '\t' + this.passes[new Point3D(1, 1, 1)] + '\t' + (this.passes[new Point3D(1, 1, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("");
                    lines.Add("2 Passes:" + '\t' + this.pass2 + '\t' + pass2 + '\t' + "%");
                    lines.Add("(X,Y,_):" + '\t' + this.passes[new Point3D(1, 1, 0)] + '\t' + (this.passes[new Point3D(1, 1, 0)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(X,_,Z):" + '\t' + this.passes[new Point3D(1, 0, 1)] + '\t' + (this.passes[new Point3D(1, 0, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(_,Y,Z):" + '\t' + this.passes[new Point3D(0, 1, 1)] + '\t' + (this.passes[new Point3D(0, 1, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("");
                    lines.Add("1 Pass: " + '\t' + this.pass1 + '\t' + pass1 + '\t' + "%");
                    lines.Add("(X,_,_):" + '\t' + this.passes[new Point3D(1, 0, 0)] + '\t' + (this.passes[new Point3D(1, 0, 0)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(_,Y,_):" + '\t' + this.passes[new Point3D(0, 1, 0)] + '\t' + (this.passes[new Point3D(0, 1, 0)] / passPer).ToString() + '\t' + "%");
                    lines.Add("(_,_,Z):" + '\t' + this.passes[new Point3D(0, 0, 1)] + '\t' + (this.passes[new Point3D(0, 0, 1)] / passPer).ToString() + '\t' + "%");
                    lines.Add("");
                    lines.Add("0 Passes:" + '\t' + this.pass0 + '\t' + pass0 + '\t' + "%");
                    lines.Add("(_,_,_):" + '\t' + this.passes[new Point3D(0, 0, 0)] + '\t' + (this.passes[new Point3D(0, 0, 0)] / passPer).ToString() + '\t' + "%");

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
                case 1: // Aiming
                    this.debug1 = "Aiming" + '\n';
                    evaluate();
                    break;
                case 2: // Combined
                    this.debug1 = "Combined" + '\n';
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

        private void comboBox3_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (this.comboBox3.SelectedIndex)
            { 
                case 0:
                    this.curPnt = "1";
                    break;
                case 1:
                    this.curPnt = "2";
                    break;
                case 2:
                    this.curPnt = "3";
                    break;
                case 3:
                    this.curPnt = "4";
                    break;
                default:
                    this.curPnt = "?";
                    break;
            }
        }
        #endregion
    }
}
