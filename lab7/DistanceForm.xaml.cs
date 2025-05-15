
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
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

            cbb_spatial.Items.Add("Intersects");
            cbb_spatial.Items.Add("Contains");
            cbb_spatial.Items.Add("Within");
            cbb_spatial.Items.Add("Touches");
            cbb_spatial.Items.Add("Undefined");
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

<<<<<<< HEAD

=======
>>>>>>> b3c1fe2211463783016389046af75c99cf7099e9
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
                       
                        LoadFieldsFromSelectedLayer(pointsLayer);
                    } 
                    else
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This is not a feature layer.", "Info");
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

                QueryFilter queryFilter = new QueryFilter
                {
                    WhereClause = whereClause
                };
                var rowCursor = polygonsLayer.Search(queryFilter);
                while (rowCursor.MoveNext())
                {
                    var feature = rowCursor.Current as Feature;
                    selectedPolygon = feature.GetShape();
                }


                SpatialRelationship sr = SpatialRelationship.Intersects;
               
                switch (spatialRelate)
                {
                    case "intersects":
                        sr = SpatialRelationship.Intersects;
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Applying expression with Intersects", "Info");
                        break;
                    case "within":
                        sr = SpatialRelationship.Within;
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Applying expression with Within", "Info");
                        break;
                    case "contains":
                        sr = SpatialRelationship.Contains;
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Applying expression with Contains", "Info");
                        break;
                    case "touches":
                        sr = SpatialRelationship.Touches;
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Applying expression with Touches", "Info");
                        break;
                    case "undefined":
                        sr = SpatialRelationship.Undefined;
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Applying expression with Undefined", "Info");
                        break;
                    default:
                        
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No matching expression found. Applying default expression Intersects", "Info");
                        break;
                }

                string pointAttributeClause = $"{pointFieldName} = '{pointValue}'";
                var spatialFilter = new SpatialQueryFilter
                {
                    FilterGeometry = selectedPolygon,
                    SpatialRelationship = sr,
                    WhereClause = pointAttributeClause
                };

                var pointTable = pointsLayer.GetTable();
                var matchingObjectIDs = new List<long>();
                using (var cursor = pointTable.Search(spatialFilter, false))
                {
                    while (cursor.MoveNext())
                    {
                        matchingObjectIDs.Add(cursor.Current.GetObjectID());
                    }
                }

                if (matchingObjectIDs.Count == 0)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No matching points found with given spatial and attribute filters.", "Info");
                    return;
                }

                var objectIDidFilter = new QueryFilter
                {
                    ObjectIDs = matchingObjectIDs
                };

                var selection = pointsLayer.Select(spatialFilter, SelectionCombinationMethod.New);
                var objectIds = selection.GetObjectIDs();


                // Spatial join
                var outputFeatureClass = @"H:\ArcGIS\Projects\DVG304_IUPG5\DVG304_IUPG5.gdb\result_point_feature";

                var parameters = Geoprocessing.MakeValueArray(pointsLayer, outputFeatureClass);
                Geoprocessing.ExecuteToolAsync("management.CopyFeatures", parameters);

                
                // Clear the selection
                pointsLayer.ClearSelection();
                polygonsLayer.ClearSelection();
                pointsLayer.SetVisibility(false);
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
            }
        }

        private void cmbPointValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPointValues.SelectedItem is string selectedField && cmbPointValues.SelectedItem is string valuePoints && pointsLayer != null)
            {
                pointValue = cmbPointValues.SelectedItem.ToString();
            }
        }

        private void cbb_spatial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbb_spatial.SelectedIndex != 0)
            {
                spatialRelate = cbb_spatial.SelectedItem.ToString().ToLower();
            }
        }

        private void btnRunSpatial_Click(object sender, RoutedEventArgs e)
        {
            SpatialFilter();

            lbxSpatial.Items.Clear();
            lbxSpatial.Items.Add("Selected polygon: " + (string)polygonValue);
            lbxSpatial.Items.Add("Spat.rel: " + spatialRelate);
            lbxSpatial.Items.Add("Selected points: " + (string)pointValue);

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
