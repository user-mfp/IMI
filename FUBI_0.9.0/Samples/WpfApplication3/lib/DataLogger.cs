using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication3.lib
{
    public class DataLogger
    {
        #region DECLARATIONS
        /// <summary>
        /// DECLARATIONS
        /// </summary>
        
        private string path;
        private string vp;
        private int round;
        private List<List<string>> paragraphs;
        #endregion

        #region ROUTINES
        /// <summary>
        /// ROUTINES
        /// </summary>
        
        public DataLogger()
        {
            this.path = @"D:\Master\TestFolder\";
            this.vp = "DEBUG";
            this.round = -1;
            this.paragraphs = new List<List<string>>();
        }

        public DataLogger(string path)
        {
            this.path = path;
            this.vp = "VP";
            this.round = -1;
            this.paragraphs = new List<List<string>>();
        }

        public DataLogger(string path, string vp, int round)
        {
            this.path = path;
            this.vp = vp;
            this.round = round;
            this.paragraphs = new List<List<string>>();
        }

        public void newPargraph(string head)
        {
            List<string> paragraph = new List<string>();

            paragraph.Add(head);
            paragraph.Add("");

            this.paragraphs.Add(paragraph);
        }

        public void addLineToParagraph(int index, string line)
        {
            this.paragraphs[index].Add(line);
        }

        public void writeFile()
        {
            // Build full path and filename
            string fullPath = this.path + System.DateTime.Now.ToString("yyyy-M-dd_HH.mm.ss") + "_" + this.vp + "_defPlane()_" + this.round.ToString() + ".txt";
            // Start writing the file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fullPath)) //@"C:\Users\Public\TestFolder\pointingLines.txt"
            {
                foreach (List<string> paragraph in this.paragraphs)
                {
                    // new line at the end
                    paragraph.Add("");
                    foreach (string line in paragraph)
                    {
                        file.WriteLine(line);
                    }
                }
                file.Close();
            }
        }
        #endregion
    }
}
