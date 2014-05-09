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
        private string completeTxt = "_CW1_s.TXT";
        private string completeBmp = "_CW1_s.BMP";
        
        //--- Parsing ---//
        private List<Point3D> pointingPts = new List<Point3D>();
        private List<Point3D> aimingPts = new List<Point3D>();
        private List<Point3D> combinedPts = new List<Point3D>();

        //--- Constants ---//
        private int stretch = 10;

        //--- Variables ---//
        private double threshold;
        private double classWeight;
        private List<Point3D> classification = new List<Point3D>(); // Only coded dominant (classified) axis
        private List<double> weights = new List<double>(); // Only weights for respective point's axis' values
        private List<Point3D> weightPts = new List<Point3D>(); 
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
            if (this.classWeight == 1) // "No Classification"
            {
                List<string[]> data = readTxt();
                parseLines(data);
                drawBitmap();
                evalPassesDict();
                makeStats();
                resetPassesDict();
            }
            else
            {
                // TODO
                List<List<string[]>> data = readForClassification(); // - read pointing- [0] and aiming-data [1]
                parseForClassification(data); // - parse pointing- and aiming-data and fill pointingPts and aimingPts
                classifyCombined(); // - compare both and write classification
                writeClassificationTxt();
                weighCombined();
                writeWeightedTxt();
            }
        }
        #endregion

        #region READING
        private List<string[]> readTxt()
        {
            List<string[]> data = new List<string[]>();
                        
            data.Add(System.IO.File.ReadAllLines(this.folder + "2014-5-08_Combined_#" + this.curPnt + "_TH" + this.threshold + this.completeTxt));
            /*
            switch (this.comboBox1.SelectedIndex)
            {
                case 0: // Pointing
                    data.Add(System.IO.File.ReadAllLines(this.folder + "PointingX" + this.curPnt + this.completeTxt));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "PointingY" + this.curPnt + this.completeTxt));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "PointingZ" + this.curPnt + this.completeTxt));
                    break;
                case 1: // Aiming
                    data.Add(System.IO.File.ReadAllLines(this.folder + "AimingX" + this.curPnt + this.completeTxt));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "AimingY" + this.curPnt + this.completeTxt));
                    data.Add(System.IO.File.ReadAllLines(this.folder + "AimingZ" + this.curPnt + this.completeTxt));
                    break;
                case 2: // Combined
                    data.Add(System.IO.File.ReadAllLines(this.folder + "2014-5-08_Combined_#" + this.curPnt + "_TH" + this.threshold + this.completeTxt));
                    break;
                default:
                    break;
            }*/
            return data;
        }

        private List<List<string[]>> readForClassification()
        {
            List<List<string[]>> classificationData = new List<List<string[]>>();
            List<string[]> pointingData = new List<string[]>();
            List<string[]> aimingData = new List<string[]>();

            pointingData.Add(System.IO.File.ReadAllLines(this.folder + "PointingX" + this.curPnt + this.completeTxt));
            pointingData.Add(System.IO.File.ReadAllLines(this.folder + "PointingY" + this.curPnt + this.completeTxt));
            pointingData.Add(System.IO.File.ReadAllLines(this.folder + "PointingZ" + this.curPnt + this.completeTxt));
            classificationData.Add(pointingData);

            aimingData.Add(System.IO.File.ReadAllLines(this.folder + "AimingX" + this.curPnt + this.completeTxt));
            aimingData.Add(System.IO.File.ReadAllLines(this.folder + "AimingY" + this.curPnt + this.completeTxt));
            aimingData.Add(System.IO.File.ReadAllLines(this.folder + "AimingZ" + this.curPnt + this.completeTxt));
            classificationData.Add(aimingData);

            return classificationData;
        }
        #endregion

        #region WRITING
        private void writeClassificationTxt()
        {
            string fullPath = this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_" + "Classification_#" + this.curPnt + "_TH" + this.threshold.ToString() + "_CW" + this.comboBox4.SelectedIndex.ToString() + "_b.TXT";
            List<string> classifications = new List<string>();

            foreach (Point3D point in this.classification)
            { 
                classifications.Add(point.X.ToString() + '\t' + point.Y.ToString() + '\t' + point.Z.ToString());
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fullPath))
            {
                foreach (string line in classifications)
                {
                    file.WriteLine(line);
                }
                file.Close();
            }
        }

        private void writeWeightedTxt()
        {
            string wieghtPath = this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_" + "Weights_#" + this.curPnt + "_TH" + this.threshold.ToString() + "_CW" + this.comboBox4.SelectedIndex.ToString() + "_b.TXT";
            List<string> weights = new List<string>();
            Point3D tmp = new Point3D();
            string combinedPath = this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_" + "Combined_#" + this.curPnt + "_TH" + this.threshold.ToString() + "_CW" + this.comboBox4.SelectedIndex.ToString() + "_b.TXT";
            List<string> combined = new List<string>();

            for (int axis = 0; axis != this.weights.Count; ++axis)
            {                
                if(axis % 3 == 0)
                {
                    tmp.X = this.weights[axis];
                }
                else if (axis % 3 == 1)
                {
                    tmp.Y = this.weights[axis];                
                }
                else // (axis % 3 == 2)
                {
                    tmp.Z = this.weights[axis];
                    this.weightPts.Add(tmp);
                }
            }
            this.weights.Clear();
            foreach (Point3D point in this.weightPts)
            {
                weights.Add(point.X.ToString() + '\t' + point.Y.ToString() + '\t' + point.Z.ToString());
            }
            this.weightPts.Clear();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(wieghtPath))
            {
                foreach (string line in weights)
                {
                    file.WriteLine(line);
                }
                file.Close();
            }

            foreach (Point3D point in this.combinedPts)
            {
                combined.Add(point.X.ToString() + '\t' + point.Y.ToString() + '\t' + point.Z.ToString());
            }
            this.combinedPts.Clear();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(combinedPath))
            {
                foreach (string line in combined)
                {
                    file.WriteLine(line);
                }
                file.Close();
            }
        }
        #endregion

        #region PARSING
        private void parse(List<string[]> data)
        {
            Point3D tmp = new Point3D();

            switch (this.comboBox1.SelectedIndex)
            {
                case 0:
                    this.pointingPts.Clear();                    
                    for (int i = 0; i != data[0].Length; ++i)
                    {
                        tmp.X = Double.Parse(data[0][i]);
                        tmp.Y = Double.Parse(data[1][i]);
                        tmp.Z = Double.Parse(data[2][i]);
                        this.pointingPts.Add(tmp);
                    }                    
                    break;
                case 1:
                    this.aimingPts.Clear();
                    for (int i = 0; i != data[0].Length; ++i)
                    {
                        tmp.X = Double.Parse(data[0][i]);
                        tmp.Y = Double.Parse(data[1][i]);
                        tmp.Z = Double.Parse(data[2][i]);
                        this.aimingPts.Add(tmp);
                    }
                    break;
                case 2:
                    this.combinedPts.Clear();
                    for (int i = 0; i != data[0].Length; ++i)
                    {
                        tmp.X = Double.Parse(data[0][i]);
                        tmp.Y = Double.Parse(data[1][i]);
                        tmp.Z = Double.Parse(data[2][i]);
                        this.combinedPts.Add(tmp);
                    }
                    break;
                default:
                    break;
            }
            updateView();
        }

        private void parseLines(List<string[]> data)
        {
            Point3D tmp = new Point3D();

            switch (this.comboBox1.SelectedIndex)
            {
                case 0:
                    this.pointingPts.Clear();
                    for (int i = 0; i != data[0].Length; ++i)
                    {
                        tmp = Point3D.Parse(data[0][i]);
                        this.pointingPts.Add(tmp);
                    }
                    break;
                case 1:
                    this.aimingPts.Clear();
                    for (int i = 0; i != data[0].Length; ++i)
                    {
                        tmp = Point3D.Parse(data[0][i]);
                        this.aimingPts.Add(tmp);
                    }
                    break;
                case 2:
                    this.combinedPts.Clear();
                    for (int i = 0; i != data[0].Length; ++i)
                    {
                        tmp = Point3D.Parse(data[0][i]);
                        this.combinedPts.Add(tmp);
                    }
                    break;
                default:
                    break;
            }
            updateView();
        }

        private void parseForClassification(List<List<string[]>> data)
        {
            Point3D tmp = new Point3D();

            this.pointingPts.Clear(); 
            for (int i = 0; i != data[0][0].Length; ++i)
            {
                tmp.X = Double.Parse(data[0][0][i]);
                tmp.Y = Double.Parse(data[0][1][i]);
                tmp.Z = Double.Parse(data[0][2][i]);
                this.pointingPts.Add(tmp);
            }    

            this.aimingPts.Clear();
            for (int i = 0; i != data[1][0].Length; ++i)
            {
                tmp.X = Double.Parse(data[1][0][i]);
                tmp.Y = Double.Parse(data[1][1][i]);
                tmp.Z = Double.Parse(data[1][2][i]);
                this.aimingPts.Add(tmp);
            }
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
                    tmpBmp.Save(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Pointing" + this.curPnt + "_" + this.threshold + this.completeBmp);
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
                    tmpBmp.Save(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Aiming" + this.curPnt + "_" + this.threshold + this.completeBmp);
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
                    tmpBmp.Save(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Combined" + this.curPnt + "_" + this.threshold + this.completeBmp);
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
                    System.IO.StreamWriter pointingFile = new System.IO.StreamWriter(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Pointing" + this.curPnt + "_" + this.threshold + this.completeTxt);
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
                    System.IO.StreamWriter aimingFile = new System.IO.StreamWriter(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Aiming" + this.curPnt + "_" + this.threshold + this.completeTxt);
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
                    System.IO.StreamWriter mitmatchFile = new System.IO.StreamWriter(this.folder + System.DateTime.Now.ToString("yyyy-M-dd") + "_Combined" + this.curPnt + "_" + this.threshold + this.completeTxt);
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
        
        private void classifyCombined()
        {
            Point3D tmp = new Point3D();

            this.classification.Clear();
            for (int i = 0; i != this.aimingPts.Count; ++i)
            {
                tmp = classifyPoints(this.pointingPts[i], this.aimingPts[i]); // Check IF a classification is possible or necessary
                this.classification.Add(tmp); // Add result of classification for respective pair of points
            }
        }
        
        private Point3D classifyPoints(Point3D pointing, Point3D aiming)
        {
            Point3D tmp = new Point3D();

            tmp.X = classifyAxis(pointing.X, aiming.X);
            tmp.Y = classifyAxis(pointing.Y, aiming.Y);
            tmp.Z = classifyAxis(pointing.Z, aiming.Z);

            return tmp;
        }

        private double classifyAxis(double pointing, double aiming) // Returns 1.0 for pointing-, 2.0 for aiming- and 0.0 for no bias
        {
            // Bias to center
            double bias = smallerValue(pointing, aiming);
            //double bias = biggerValue(pointing, aiming);
            double diff = Math.Abs(Math.Abs(pointing) - Math.Abs(aiming));
            
            if (bias == pointing && diff > this.threshold) // Poiting is closer to 0 AND difference is above than current threshold
            {
                return 1.0;
            }
            else if (bias == aiming && diff > this.threshold) // Aiming is closer to 0 AND difference is above than threschold
            {
                return 2.0;
            }
            else // Either ist closer to 0 BUT difference is within threshold
            {
                return 0.0;
            }

        }

        private double smallerValue(double lhs, double rhs)
        {
            double closer = Math.Min(Math.Abs(lhs), Math.Abs(rhs));

            if (closer == Math.Abs(lhs))
                return lhs;
            else
                return rhs;
        }

        private double biggerValue(double lhs, double rhs)
        {
            double closer = Math.Max(Math.Abs(lhs), Math.Abs(rhs));

            if (closer == Math.Abs(lhs))
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

            tmp.X = weighAxis(pointing.X, aiming.X, classification.X);
            tmp.Y = weighAxis(pointing.Y, aiming.Y, classification.Y);
            tmp.Z = weighAxis(pointing.Z, aiming.Z, classification.Z);

            return tmp;
        }

        private double weighAxis(double pointing, double aiming, double classification)
        {
            double weighted;

            if (this.comboBox4.SelectedIndex == 1) // Automatic classification enabled
            {
                setClassWeight(pointing, aiming); // Calculate weight for each pointing- / aiming-pair
            }

            if (classification == 1) // Biased toward pointing
            {
                weighted = (pointing * this.classWeight) + (aiming * (1 - this.classWeight));
            }
            else if (classification == 2) // Biased toward aiming
            {
                weighted = (aiming * this.classWeight) + (pointing * (1 - this.classWeight));
            }
            else // No bias
            {
                weighted = (pointing + aiming) / 2; // Average
            }

            this.weights.Add(this.classWeight);

            return weighted;
        }

        private void setClassWeight(double pointing, double aiming)
        {
            double diff = Math.Abs(Math.Abs(pointing) - Math.Abs(aiming));

            this.classWeight = Math.Min(0.9, (Math.Max(0.6, (1 - (this.threshold / diff))))); // Min-,Max-Funktion: classWeight is bewteen 0.6 and 0.9
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
            //for (int mode = 0; mode != 3; ++mode)
            //{
            this.comboBox1.SelectedIndex = 2;// mode;
            for (int threshold = 0; threshold != 11; ++threshold)
            {
                this.comboBox2.SelectedIndex = threshold;
                evaluate();
            }
            //}
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

        private void comboBox4_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (this.comboBox4.SelectedIndex)
            { 
                default: // case 0: "No Classification
                    this.classWeight = 1.0;
                    break;
                case 1: // "Auto Classification"
                    this.classWeight = 0.0;
                    this.comboBox1.SelectedIndex = 2;
                    break;
                case 2: // "90% Classification"
                    this.classWeight = 0.9;
                    this.comboBox1.SelectedIndex = 2;
                    break;
                case 3: // "80% Classification"
                    this.classWeight = 0.8;
                    this.comboBox1.SelectedIndex = 2;
                    break;
                case 4: // "70% Classification"
                    this.classWeight = 0.7;
                    this.comboBox1.SelectedIndex = 2;
                    break;
                case 5: // "60% Classification"
                    this.classWeight = 0.6;
                    this.comboBox1.SelectedIndex = 2;
                    break;
            }
        }
        #endregion
    }
}
