using System.Collections.Generic;
using System;

namespace IMI
{
    public partial class DataLogger
    {
        #region DECLARATIONS
        private string path;
        private List<List<string>> paragraphs;
        private DateTime startSession;
        #endregion

        #region DECLARATION
        public DataLogger(string path)
        {
            this.path = path;
            this.paragraphs = new List<List<string>>();
        }
        #endregion

        #region ROUTINES
        private void newParagraph()
        {
            List<string> paragraph = new List<string>();

            paragraph.Add("");

            this.paragraphs.Add(paragraph);
        }

        private void newParagraph(string head)
        {
            List<string> paragraph = new List<string>();

            paragraph.Add(head);
            paragraph.Add("");

            this.paragraphs.Add(paragraph);
        }

        private void addLineToParagraph(int index, string line)
        {
            this.paragraphs[index].Add(line);
        }

        private void writeFile(string extension)
        {
            // Build full path and filename
            string fullPath = this.path + extension; 
            // Start writing the file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fullPath))
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
            this.paragraphs.Clear(); // Empty all lines
        }
        #endregion

        #region SESSION
        public void newSession()
        {
            this.startSession = DateTime.Now;

            this.path = @"C:\IMI-DATA\Statistiken\";

            string head = "Time"
                + '\t' + "Event"
                + '\t' + "ExhibitID"
                + '\t' + "UserID"
                + '\t' + "Visitors";
            newParagraph(head);

            string line = DateTime.Now.ToString("HH.mm.ss.fffffff")
                + '\t' + "Start Session"
                + '\t' + "" // No "Exhibit", yet
                + '\t' + "" // No "UserID", yet 
                + '\t' + ""; // No "Visitors", yet
            addLineToParagraph(0, line);
        }

        public void addEventToSession(string sessionEvent, int exhibit, int visitors, int user)
        {
            if (this.paragraphs.Count != 0) // Something to write
            {
                string line = DateTime.Now.ToString("HH.mm.ss.fffffff")
                    + '\t' + sessionEvent
                    + '\t' + exhibit
                    + '\t' + user
                    + '\t' + visitors;
                addLineToParagraph(0, line);
            }
        }

        public void addEventToSession(string sessionEvent, string exhibit, int visitors, int user)
        {
            if (this.paragraphs.Count != 0) // Something to write
            {
                string line = DateTime.Now.ToString("HH.mm.ss.fffffff")
                    + '\t' + sessionEvent
                    + '\t' + exhibit
                    + '\t' + user
                    + '\t' + visitors;
                addLineToParagraph(0, line);
            }
        }

        public void endSession()
        {
            if (this.paragraphs.Count != 0) // Something to write
            {
                TimeSpan timeSpan = DateTime.Now - this.startSession;

                string line = DateTime.Now.ToString("HH.mm.ss.fffffff")
                    + '\t' + "End Session"
                    + '\t' + "" // No "Exhibit", anymore
                    + '\t' + "" // No "UserID", anymore 
                    + '\t' + ""; // No "Visitors", anymore
                addLineToParagraph(0, line);

                line = timeSpan.ToString()
                    + '\t' + "Duration"
                    + '\t' + "" // No "Exhibit", anymore
                    + '\t' + "" // No "UserID", anymore 
                    + '\t' + ""; // No "Visitors", anymore
                addLineToParagraph(0, line);

                writeSessionFile();
            }
        }

        public void writeSessionFile()
        {
            string extension = "Session(" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ").txt";

            writeFile(extension);
        }
        #endregion
    }
}
