﻿using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows;
using System;
using System.Windows.Threading;
using ActiproSoftware.Windows.Extensions;

namespace lab7
{
    internal class DistanceTool : MapTool
    {
        private Map map;
        private DistanceForm distanceForm;  
        private MapPoint firstClickedPoint;
        private MapPoint secondClickedPoint;
        private List<MapPoint> overlays = new List<MapPoint>();
        private int clickCount = 0;

        // Ta bort dem övriga punkterna
        private CIMMarker firstMarker;
        private CIMPointSymbol markerToRemove;

        public DistanceTool()
        {
            System.Windows.MessageBox.Show("Distance tool is now active.");
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
                firstMarker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.RedRGB, 20.0, SimpleMarkerStyle.Pushpin);
                CIMPointSymbol pointSymbolFromMarker = SymbolFactory.Instance.ConstructPointSymbol(firstMarker);
                var symbolReference = pointSymbolFromMarker.MakeSymbolReference();
                
                var graphic = new CIMPointGraphic
                {
                    Symbol = symbolReference,
                    Location = point,
                    
                };
                var overlay = MapView.Active.AddOverlay(graphic);
            });
        }

        private async Task RemoveMarker(CIMPointSymbol marker)
        {
            if (clickCount > 2)
            {
                await QueuedTask.Run(() =>
                {
                    
                });
            }
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
                    System.Windows.MessageBox.Show($"Distance between the two points: {Math.Round(distance, 1)} meters");
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        distanceForm.txtdistance.Text = Math.Round(distance,0).ToString() + " Meters";
                    });

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
                    _=AddMarker(firstClickedPoint);

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
                    _=AddMarker(secondClickedPoint);
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        distanceForm.Visibility = System.Windows.Visibility.Visible;
                        distanceForm.txtSecond.Text = secondClickedPoint.X.ToString() + " " + secondClickedPoint.Y.ToString(); 
                    });
                    _=CalculateDistance();
                    
                }

                // Reset 
                if (clickCount > 2)
                {
                    clickCount = 1;
                    firstClickedPoint = clickedPoint;
                    secondClickedPoint = null;
                    //_=RemoveMarker(secondClickedPoint);
               
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        distanceForm.txtFirst.Text = firstClickedPoint.X.ToString() + " " + firstClickedPoint.Y.ToString();
                        distanceForm.Visibility = System.Windows.Visibility.Visible;
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
