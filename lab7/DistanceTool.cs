using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Collections.Generic;

namespace lab7
{
    internal class DistanceTool : MapTool
    {
        private Map map;
        private DistanceForm distanceForm;  
        private MapPoint firstClickedPoint;
        private MapPoint secondClickedPoint;
        private List<MapPoint> clickedPoints = [];
        private int clickCount = 0;

        public DistanceTool()
        {
            System.Windows.MessageBox.Show("Distance tool active");
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;
            distanceForm = new DistanceForm();
            distanceForm.Visibility = System.Windows.Visibility.Visible;
            firstClickedPoint = null;
            secondClickedPoint = null;
            map = MapView.Active.Map;
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                e.Handled = true;
                base.OnToolMouseDown(e);
            }
                
        }

        private async Task AddMarker(MapPoint point)
        {
            await QueuedTask.Run(() =>
            {
                CIMMarker marker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.GreenRGB, 8.0, SimpleMarkerStyle.Pushpin);
                CIMPointSymbol pointSymbolFromMarker = SymbolFactory.Instance.ConstructPointSymbol(marker);
                var symbolReference = pointSymbolFromMarker.MakeSymbolReference();
                
                var graphic = new CIMPointGraphic
                {
                    Symbol = symbolReference,
                    Location = point,
                    
                };
                var overlay = MapView.Active.AddOverlay(graphic);
            });
        }

        private async Task CalculateDistance()
        {
            if (firstClickedPoint != null && secondClickedPoint != null)
            {
                double distance = await QueuedTask.Run(() =>
                {
                    return GeometryEngine.Instance.GeodesicDistance(firstClickedPoint, secondClickedPoint);
                });

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    System.Windows.MessageBox.Show($"Distance between the two points: {distance:F2} meters");
                });
            }
        }


        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs args)
        {
            
            return QueuedTask.Run(() =>
            {
                clickCount++;
                var clickedPoint = MapView.Active.ClientToMap(args.ClientPoint);

                // Första klicket
                if (clickCount == 1)
                {
                    firstClickedPoint = clickedPoint;
                    System.Windows.MessageBox.Show(string.Format("X: {0} Y: {1} Z: {2}",
                        clickedPoint.X, clickedPoint.Y, clickedPoint.Z), "Map Coordinates");
                    AddMarker(firstClickedPoint);

                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        distanceForm.Visibility = System.Windows.Visibility.Visible;
                        distanceForm.txtFirst.Text = firstClickedPoint.X.ToString() + " " + firstClickedPoint.Y.ToString();
                    });
                }
                // Andra klicket
                else if (clickCount == 2)
                {
                    secondClickedPoint = clickedPoint;
                    System.Windows.MessageBox.Show(string.Format("X: {0} Y: {1} Z: {2}",
                        clickedPoint.X, clickedPoint.Y, clickedPoint.Z), "Map Coordinates");
                    AddMarker(secondClickedPoint);
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        distanceForm.Visibility = System.Windows.Visibility.Visible;
                        distanceForm.txtSecond.Text = secondClickedPoint.X.ToString() + " " + secondClickedPoint.Y.ToString();
                        _ = CalculateDistance();
                    });
                }

                // Återställ vid tredje klicket
                if (clickCount > 2)
                {
                    clickCount = 1;
                    firstClickedPoint = clickedPoint;
                    secondClickedPoint = null;

                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        distanceForm.txtFirst.Text = firstClickedPoint.X.ToString() + " " + firstClickedPoint.Y.ToString();
                        distanceForm.txtSecond.Clear();
                    });
                    _ = CalculateDistance();
                }
            });
        }

        protected override Task OnToolActivateAsync(bool active)
        {

            if (active)
            {   
                System.Windows.MessageBox.Show("DistanceTool is activated.");
            }
            

            return base.OnToolActivateAsync(active);
        }
       


        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
