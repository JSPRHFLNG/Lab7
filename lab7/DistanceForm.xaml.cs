using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Microsoft.Win32;
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

        // Variabler
        public Map map;
        public FeatureLayer firstLayer;
        public FeatureLayer secondLayer;

        public DistanceForm()
        {
            InitializeComponent();
            map = MapView.Active.Map;
        }

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
           
        }

        private void btnSecond_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility= Visibility.Collapsed;
        }


        // LÄS IN DATA
        public string ChooseFile()
        {
            OpenFileDialog openFiles = new OpenFileDialog();
            openFiles.InitialDirectory = @"H:\GIS_Applikationer\Lab1\Lab1\data\";
            openFiles.Filter = "Shapefiles (*.shp)|*.shp|All files (*.*)|*.*";

            if (openFiles.ShowDialog() == true)
            {
                return openFiles.FileName;
            }
            return null;
        }

        // Punkt data
        private async void btnSelectPointData_Click(object sender, RoutedEventArgs e)
        {
            
            string selectedFilePath = ChooseFile();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                return;
            }
            var progress = new ProgressDialog("Adding layer to map...", "Cancel", 100, true);
            progress.Show();
            
            try
            {                
                map = MapView.Active.Map;
                await QueuedTask.Run(() =>
                {
                    Layer layer = LayerFactory.Instance.CreateLayer(new Uri(selectedFilePath), map);
                    if (layer is FeatureLayer featureLayer)
                    {
                        firstLayer = featureLayer;
                    } 
                    else
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This is not a feature layer..");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding layer: " + ex.Message);
            }
            finally
            {
                progress.Hide();
            }
        }
        //Polygon data
        private async void btnSelectPolygonData_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilePath = ChooseFile();
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                return;
            }
            var progress = new ProgressDialog("Adding layer to map...", "Cancel", 100, true);
            progress.Show();

            try
            {
                map = MapView.Active.Map;
                await QueuedTask.Run(() =>
                {
                    Layer layer = LayerFactory.Instance.CreateLayer(new Uri(selectedFilePath), map);
                    if (layer is FeatureLayer featureLayer)
                    {
                        secondLayer = featureLayer;
                    }
                    else
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This is not a feature layer..");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding layer: " + ex.Message);
            }
            finally
            {
                progress.Hide();
            }
        }
    }
}
