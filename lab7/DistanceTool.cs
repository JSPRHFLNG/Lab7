using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab7
{
    internal class DistanceTool : MapTool
    {

        private DistanceForm distanceForm;     
        private MapPoint firstClickedPoint;
        private MapPoint secondClickedPoint;
        private int clickCount = 0;

        public DistanceTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;
            distanceForm = new DistanceForm();         
            distanceForm.Visibility = System.Windows.Visibility.Visible;
            firstClickedPoint = null;
            secondClickedPoint = null;
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
                    Location = point
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

                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        distanceForm.Visibility = System.Windows.Visibility.Visible;
                        distanceForm.txtSecond.Text = secondClickedPoint.X.ToString() + " " + secondClickedPoint.Y.ToString();
                        CalculateDistance();
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
                    CalculateDistance();
                }
            });
        }



        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
