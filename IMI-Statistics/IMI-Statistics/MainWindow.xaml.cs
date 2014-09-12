using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace IMI_Statistics
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region DECLARATIONS
        List<string> filePaths;
        #endregion

        #region EVENTS
        // LOAD LOGDATA-FILES
        private void button1_Click(object sender, RoutedEventArgs e) 
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == true)
            {
                this.filePaths = new List<string>();

                foreach (string path in ofd.FileNames)
                {
                    this.filePaths.Add(path);
                }
            }
            else
            {
                ofd = null;
            }
        }

        // ANALYZE LOGDATA-FILES
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (this.filePaths != null)
            {
                MessageBox.Show(this.filePaths.Count.ToString());
            }
            else
            {
                MessageBox.Show("Keine Dateien ausgewählt. Bitte laden sie Dateien.");
            }
        }
        #endregion
    }
}
