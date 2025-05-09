using ArcGIS.Core.Data;
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
        public FeatureLayer polyLayer;
        public FeatureLayer pointLayer;

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

        public async Task removeLayerIfExists(String layerName)
        {
            await QueuedTask.Run(() =>
            {
                var layers = MapView.Active.Map.Layers;
                if (layers == null) return; 
                var existingLayer = layers.FirstOrDefault(layer => layer.Name == layerName);
                if (existingLayer != null)
                {
                    MapView.Active.Map.RemoveLayer(existingLayer);
                }
                    
                
            });
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

        // FILTRERA DATA

        private async void FilterDATA()
        {
            if (cmbFields.SelectedItem is not string field ||
                cmbValues.SelectedItem is not string value ||
                secondLayer == null)
            {
                MessageBox.Show("Du måste välja ett fält och ett värde först.");
                return;
            }

            string whereClause = $"{field} = '{value.Replace("'", "''")}'"; // Hanterar eventuella apostrofer

            await QueuedTask.Run(() =>
            {
                var queryFilter = new QueryFilter
                {
                    WhereClause = whereClause
                };

                secondLayer.ClearSelection();
                secondLayer.Select(queryFilter, SelectionCombinationMethod.New);

                var outputPath = @"H:\GIS_Applikationer\Lab1\Lab1\Lab7\selectedPoly.shp";
                var parameters = Geoprocessing.MakeValueArray(secondLayer, outputPath);

                Geoprocessing.ExecuteToolAsync("management.CopyFeatures", parameters);
                _ = removeLayerIfExists("selectedPoly.shp");
            });
        }



        // Fält
        public void LoadFieldsFromSelectedLayer(FeatureLayer selectedLayer)
        {
            QueuedTask.Run(() =>
            {
                var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                inspector.LoadSchema(selectedLayer);

                this.Dispatcher.Invoke(() => cmbFields.Items.Clear());

                foreach (var attribute in inspector)
                {
                    if (!attribute.IsSystemField && !attribute.IsGeometryField)
                    {
                        var fldName = attribute.FieldName;
                        this.Dispatcher.Invoke(() => cmbFields.Items.Add(fldName));
                    }
                }
            });
        }

        // Värden
        public void LoadValuesForField(FeatureLayer layer, string fieldName)
        {
            QueuedTask.Run(() =>
            {
                using (var table = layer.GetTable())
                {
                    var query = new QueryFilter
                    {
                        SubFields = fieldName
                    };

                    var values = new HashSet<string>();
                    using (var cursor = table.Search(query, false))
                    {
                        while (cursor.MoveNext())
                        {
                            var row = cursor.Current;
                            var val = row[fieldName]?.ToString();
                            if (!string.IsNullOrEmpty(val))
                                values.Add(val);
                        }
                    }

                    var sortedValues = values.OrderBy(v => v).ToList();

                    this.Dispatcher.Invoke(() =>
                    {
                        cmbValues.Items.Clear();
                        foreach (var val in sortedValues)
                            cmbValues.Items.Add(val);
                    });
                }
            });
        }

        private void ApplyAttributeFilter(FeatureLayer layer, string field, string value)
        {
            QueuedTask.Run(() =>
            {
                string whereClause = $"{field} = '{value.Replace("'", "''")}'";
                var queryFilter = new QueryFilter
                {
                    WhereClause = whereClause
                };
                layer.ClearSelection();
                layer.Select(queryFilter, SelectionCombinationMethod.New);
            });
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
                        FilterDATA();
                        LoadFieldsFromSelectedLayer(firstLayer);
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
                        LoadFieldsFromSelectedLayer(secondLayer);
                        FilterDATA();
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
        


        // Combobox
        private void cmbFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFields.SelectedItem is string selectedField && firstLayer != null)
            {
                LoadValuesForField(firstLayer, selectedField);
                LoadValuesForField(secondLayer, selectedField);
            }
        }

        private void cmbValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFields.SelectedItem is string fieldFirst && cmbValues.SelectedItem is string valueFirst && firstLayer != null)
            {
                ApplyAttributeFilter(firstLayer, fieldFirst, valueFirst);
            }
            if(cmbFields.SelectedItem is string fieldSecond && cmbValues.SelectedItem is string valueSecond && secondLayer != null)
            {
                ApplyAttributeFilter(secondLayer, fieldSecond, valueSecond);
            }
        }

        private async void btnAddData_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilePath = ChooseFile();
            if (string.IsNullOrEmpty(selectedFilePath))
                return;

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
                        string msg = "Vilken typ av geografiska objekt innehåller shapefilen?\nYes for Polygon and No for Point.";
                        var result = MessageBox.Show(msg, "Datatyp", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Polygon
                            firstLayer = featureLayer;
                            LoadFieldsFromSelectedLayer(firstLayer);
                            //FilterDATA();
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            // Point
                            secondLayer = featureLayer;
                            LoadFieldsFromSelectedLayer(secondLayer);
                            FilterDATA();
                        }
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
