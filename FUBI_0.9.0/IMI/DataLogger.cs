using System.Collections.Generic;

namespace IMI
{
    class DataLogger
    {
        #region DECLARATIONS
        private string path;
        private int round;
        private List<List<string>> paragraphs;
        #endregion

        #region ROUTINES
        public DataLogger()
        {
            this.path = @"C:\Users\Haßleben\Desktop\IMI-DATA\Debug\";
            this.round = -1;
            this.paragraphs = new List<List<string>>();
        }

        public DataLogger(string path)
        {
            this.path = path;
            this.round = -1;
            this.paragraphs = new List<List<string>>();
        }

        public DataLogger(string path, int round)
        {
            this.path = path;
            this.round = round;
            this.paragraphs = new List<List<string>>();
        }

        public void newPargraph()
        {
            List<string> paragraph = new List<string>();

            paragraph.Add("");

            this.paragraphs.Add(paragraph);
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
            string fullPath = this.path + System.DateTime.Now.ToString("yyyy-M-dd_HH.mm.ss") + "_" + "_CALIBRATION_" + this.round + ".txt";
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
