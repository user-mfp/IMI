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

namespace WpfApplication1
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

        private OpenFileDialog openImage;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.openImage = new OpenFileDialog();
            this.openImage.Title = "Bild auswählen";
            this.openImage.Filter = "Alle unterstützen Grafiken|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png";

            if (this.openImage.ShowDialog() == true)
            {
                this.image1.Source = new BitmapImage(new Uri(this.openImage.FileName));
            }
        }
    }
}
