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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;

using GMap.NET.WindowsPresentation;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        private bool isWindow = false;
        public bool CloseRequest = false;
        public MapView()
        {
            InitializeComponent();
        }

        public void AddMapMarker(GMapMarker _gpsMarker)
        {
         
            MainMap.Markers.Add(_gpsMarker);
            


        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {


            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            // choose your provider here
            //MainMap.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
              MainMap.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;

            // whole world zoom
            MainMap.MinZoom = 0;
            MainMap.MaxZoom = 30;
            MainMap.Zoom = 19;

            // lets the map use the mousewheel to zoom
            MainMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;

            // lets the user drag the map
            MainMap.CanDragMap = true;
            //MainMap.ShowCenter = false;

            // lets the user drag the map with the left mouse button
            MainMap.DragButton = MouseButton.Left;

            double textBoxLat = -25.882784;
            double textBoxLng = 28.163630;

            MainMap.Position = new GMap.NET.PointLatLng(textBoxLat, textBoxLng);


            MainMap.ShowCenter = false;
      
        }
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            
             ProgramFlow.ProgramWindow = (int)ProgramFlowE.Dataview;
            CloseRequest = true;

        }
        private void ButtonNewWindow_Click(object sender, RoutedEventArgs e)
        {

            GlobalSharedData.ViewMode = true;
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Dataview;
        }



    }



}
