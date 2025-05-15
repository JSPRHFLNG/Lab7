using ActiproSoftware.Products.Ribbon;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.CommonControls;
using ArcGIS.Desktop.Mapping;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace lab7
{
    /// <summary>
    /// Interaction logic for DistanceForm.xaml
    /// </summary>
    public partial class DistanceForm : Window
    {

        // Variabler
        public Map map;
        public FeatureLayer polygonsLayer;
        public FeatureLayer pointsLayer;
        private string polygonFieldName;
        private string pointFieldName;
        private string polygonValue;
        private string pointValue;

        private string spatialRelate;

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
        public string ChoosePolygonFeatureFile()
        {
            OpenFileDialog openFiles = new OpenFileDialog();
            openFiles.InitialDirectory = @"H:\ArcGIS\Projects\DVG304_IUPG7\data";
            openFiles.Filter = "Shapefiles (*.shp)|*.shp|All files (*.*)|*.*";

            if (openFiles.ShowDialog() == true)
            {
                return openFiles.FileName;
            }
            return null;
        }

        public string ChoosePointFeatureFile()
        {
            OpenFileDialog openFiles = new OpenFileDialog();
            openFiles.InitialDirectory = @"H:\ArcGIS\Projects\DVG304_IUPG7\data";
            openFiles.Filter = "Shapefiles (*.shp)|*.shp|All files (*.*)|*.*";

            if (openFiles.ShowDialog() == true)
            {
                return openFiles.FileName;
            }
            return null;
        }

        // Fält
        public void LoadFieldsFromSelectedLayer(FeatureLayer selectedLayer)
        {
            QueuedTask.Run(() =>
            {
                var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                inspector.LoadSchema(selectedLayer);

                if(selectedLayer == polygonsLayer)
                {
                    //this.Dispatcher.Invoke(() => cmbPolygonFields.Items.Clear());

                    foreach (var attribute in inspector)
                    {
                        if (!attribute.IsSystemField && !attribute.IsGeometryField)
                        {
                            var fldName = attribute.FieldName;
                            this.Dispatcher.Invoke(() => cmbPolygonFields.Items.Add(fldName));
                        }
                    }
                }
                if(selectedLayer == pointsLayer)
                {
                    //this.Dispatcher.Invoke(() => cmbPointFields.Items.Clear());

                    foreach (var attribute in inspector)
                    {
                        if (!attribute.IsSystemField && !attribute.IsGeometryField)
                        {
                            var fldName = attribute.FieldName;
                            this.Dispatcher.Invoke(() => cmbPointFields.Items.Add(fldName));
                        }
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
                    var query = new QueryFilter();
                    
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
                        if(layer == polygonsLayer)
                        {
                            //cmbPolygonValues.Items.Clear();
                            foreach (var val in sortedValues)
                                cmbPolygonValues.Items.Add(val);
                        }
                        if(layer == pointsLayer)
                        {
                            //cmbPointsValues.Items.Clear();
                            foreach (var val in sortedValues)
                                cmbPointValues.Items.Add(val);
                        }
                        
                    });
                }
            });
        }

        private void ApplyAttributeFilter(FeatureLayer layer, string field, string value)
        {
            QueuedTask.Run(() =>
            {
                string whereClause = $"{field} = '{value}'";
                var queryFilter = new QueryFilter
                {
                    WhereClause = whereClause
                };
                layer.ClearSelection();
                layer.Select(queryFilter, SelectionCombinationMethod.New);
            });
        }



        //Polygon data
        private async void btnSelectPolygonData_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilePath = ChoosePolygonFeatureFile();
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
                        polygonsLayer = featureLayer;
                        LoadFieldsFromSelectedLayer(polygonsLayer);
                        //FilterDATA();
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


        // Punkt data
        private async void btnSelectPointData_Click(object sender, RoutedEventArgs e)
        {
            
            string selectedFilePath = ChoosePointFeatureFile();
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
                        pointsLayer = featureLayer;
                        //FilterDATA();
                        LoadFieldsFromSelectedLayer(pointsLayer);
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


        private void SpatialFilter()
        {
            QueuedTask.Run(() =>
            {
                var map = MapView.Active.Map;
                if (map == null)
                    return;



                var selectedPolygon = default(ArcGIS.Core.Geometry.Geometry);
                string whereClause = $"{polygonFieldName} = '{polygonValue}'";
                MessageBox.Show(whereClause.ToString());
                QueryFilter queryFilter = new QueryFilter
                {
                    WhereClause = whereClause
                };
                var rowCursor = polygonsLayer.Search(queryFilter);
                while (rowCursor.MoveNext())
                {
                    var feature = rowCursor.Current as ArcGIS.Core.Data.Feature;
                    selectedPolygon = feature.GetShape();
                }

                SpatialRelationship sr = SpatialRelationship.Intersects;
                switch (spatialRelate)
                {
                    case "Intersects":
                        sr = SpatialRelationship.Intersects;
                        break;
                    case "Within":
                        sr = SpatialRelationship.Within;
                        break;
                    case "Contains":
                        sr = SpatialRelationship.Contains;
                        break;
                    case "Touches":
                        sr = SpatialRelationship.Touches;
                        break;
                }

                var spatialFilter = new ArcGIS.Core.Data.SpatialQueryFilter
                {
                    FilterGeometry = selectedPolygon,
                    SpatialRelationship = sr
                };

                var selection = pointsLayer.Select(spatialFilter, SelectionCombinationMethod.New);
                var objectIds = selection.GetObjectIDs();


                // Spatial join
                var outputFeatureClass = @"H:\ArcGIS\Projects\DVG304_IUPG7\data\filteredData";
                Geoprocessing.ExecuteToolAsync("management.CopyFeatures", new string[] { string.Join(",", objectIds), outputFeatureClass });
                
                // Clear the selection
                pointsLayer.ClearSelection();
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Spatial filter applied successfully! ", "Info");
            });
        }



        // Combobox


        private void cmbPolygonFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPolygonFields.SelectedItem is string selectedField && polygonsLayer != null)
            {
                polygonFieldName = cmbPolygonFields.SelectedItem.ToString();
                LoadValuesForField(polygonsLayer, polygonFieldName);
            }
        }
        private void cmbPointFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPointFields.SelectedItem is string selectedField && polygonsLayer != null)
            {
                pointFieldName = cmbPointFields.SelectedItem.ToString();
                LoadValuesForField(pointsLayer, pointFieldName);
            }

        }

        

        private void cmbPolygonValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPolygonValues.SelectedItem is string selectedField && cmbPolygonValues.SelectedItem is string valuePolygons && polygonsLayer != null)
            {
                polygonValue = cmbPolygonValues.SelectedItem.ToString();
                //ApplyAttributeFilter(polygonsLayer, polygonFieldName, polygonValue);
            }

        }

        private void cmbPointValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPointValues.SelectedItem is string selectedField && cmbPointValues.SelectedItem is string valuePoints && pointsLayer != null)
            {
                pointValue = cmbPointValues.SelectedItem.ToString();
                //ApplyAttributeFilter(pointsLayer, pointFieldName, pointValue);
            }
        }

        private void cmbSpatial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSpatial.SelectedItem is string selectedField)
            {
                spatialRelate = selectedField;
                
            }
        }

        private void btnRunSpatial_Click(object sender, RoutedEventArgs e)
        {
            SpatialFilter();
        }

        private void btnHighlightPolygons_Click(object sender, RoutedEventArgs e)
        {
            ApplyAttributeFilter(polygonsLayer, polygonFieldName, polygonValue);
        }

        private void btnHighlightPoints_Click(object sender, RoutedEventArgs e)
        {
            ApplyAttributeFilter(pointsLayer, pointFieldName, pointValue);
        }
    }
}
