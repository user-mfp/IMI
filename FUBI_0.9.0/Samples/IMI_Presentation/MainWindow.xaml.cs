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


namespace IMI_Presentation
{
    public partial class MainWindow : Window
    {
        #region ENUMS AND CONSTANTS
        // The mode determines the layout (visibility, labeling and functions)
        private enum Mode
        {
            Start = 0,
            Standby,
            Navigation,
            Presentation
        };
        #endregion

        #region DECLARATIONS
        private Exhibition exhibition;
        private Exhibit TMP_EXHIBIT;

        private Mode mode;
        private string contentLabel1;
        private string contentLabel2;

        private GeometryHandler geometryHandler;
        private FileHandler fileHandler;
        #endregion

        #region INITIALIZATIONS
        public MainWindow()
        {
            InitializeComponent();

            this.mode = Mode.Start;
        }
        #endregion

        #region LAYOUT
        private void updateLayout()
        { 
        
        }

        private void showStart()
        { 
        
        }

        private void showStandby()
        { 
        
        }

        private void showNavigation()
        { 
        
        }

        private void showPresentation()
        { 
        
        }

        private void loadImage1(string path)
        { 
            
        }

        private void loadImage2(string path)
        { 
        
        }
        #endregion

        #region EVENTS
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch (this.mode)
            { 
                case Mode.Start:
                    MessageBox.Show("Ausstellungs laden...");
                    break;
                case Mode.Standby:
                    break;
                case Mode.Navigation:
                    break;
                case Mode.Presentation:
                    break;
                default:
                    break;
            }
            
        }
        #endregion
    }
}
