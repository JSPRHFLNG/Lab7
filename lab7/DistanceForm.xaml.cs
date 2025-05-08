using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace lab7
{
    /// <summary>
    /// Interaction logic for DistanceForm.xaml
    /// </summary>
    public partial class DistanceForm : Window
    {
        //public Map map;
        public DistanceForm()
        {
            InitializeComponent();
            //map = MapView.Active.Map;
        }

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
           
        }

        private void btnSecond_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility= Visibility.Collapsed;
        }
    }
}
