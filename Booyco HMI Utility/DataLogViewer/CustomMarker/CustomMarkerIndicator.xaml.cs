using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;

using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System;
using System.ComponentModel;
using Booyco_HMI_Utility;
using Booyco_HMI_Utility.CustomMarker;

namespace Demo.WindowsPresentation.CustomMarkers
{
   /// <summary>
   /// Interaction logic for CustomMarkerDemo.xaml
   /// </summary>
   public partial class CustomMarkerIndicator
   {
      Popup Popup;
      Label Label;
      GMapMarker Marker;
      MainWindow MainWindow;
        GMapControl mapControl;
        bool IsBrakeZoneVisible = false;
        public MarkerEntry CurrentMarker;

        public CustomMarkerIndicator(GMapControl _mapControl, MarkerEntry marker)
      {
        this.InitializeComponent();
        this.mapControl = _mapControl;
        this.Marker = marker.MapMarker;

            CurrentMarker = new MarkerEntry();

            CurrentMarker.Heading = marker.Heading;
            CurrentMarker.Scale = marker.Scale;
            CurrentMarker.PresenceZoneSize = marker.PresenceZoneSize;
            CurrentMarker.WarningZoneSize = marker.WarningZoneSize;
            CurrentMarker.CriticalZoneSize = marker.CriticalZoneSize;
            CurrentMarker.Zone = marker.Zone;
            CurrentMarker.title = marker.title;

            RotateTransform IndicatorTransfrom = new RotateTransform(CurrentMarker.Heading);
              PathIndicator.LayoutTransform = IndicatorTransfrom;
           // Grind_Marker.LayoutTransform = IndicatorTransfrom;
            Rectangle_ProhibitZone.LayoutTransform = IndicatorTransfrom;
             Rectangle_Vehicle.LayoutTransform = IndicatorTransfrom;
            if (CurrentMarker.Zone == 1)
            {
                PathIndicator.Fill = Brushes.Blue;

            }
            else if (CurrentMarker.Zone == 2)
            {
                PathIndicator.Fill = Brushes.Yellow;
            }
            else if (CurrentMarker.Zone == 3)
            {
                PathIndicator.Fill = Brushes.Red;
            }
            else if (CurrentMarker.Zone == 10)
            {
                PathIndicator.Fill = Brushes.Black;
            }
            else if (CurrentMarker.Zone == 20)
            {
                PathIndicator.Fill = Brushes.Green;
            }
            else
            {
                PathIndicator.Fill = Brushes.Transparent;
            }
              
            

            //CustomMarkerAngle = (Double) (Heading + 180);

        this.SizeChanged += new SizeChangedEventHandler(CustomMarkerIndicator_SizeChanged);
            //this.MouseEnter += new MouseEventHandler(CustomMarkerIndicator_MouseEnter);
            //this.MouseLeave += new MouseEventHandler(CustomMarkerIndicator_MouseLeave);

            //Popup.Placement = PlacementMode.Mouse;
            //{
            //    Label.Background = Brushes.Gray;
            //    Label.Foreground = Brushes.White;
            //    Label.BorderBrush = Brushes.Black;
            //    Label.BorderThickness = new Thickness(2);
            //    Label.Padding = new Thickness(5);
            //    Label.FontSize = 12;
            //    Label.Content = marker.title;
            //}
            UpdateSizes();

      }

        double VehicleWidth = 1;
        double VehicleHeight = 3;
        double ProhibitWidth = 4;
        double ProhibitHeight = 5;
        //double VehicleWidth = 3;
        //double VehicleHeight = 4;
        //double ProhibitWidth = 8;
        //double ProhibitHeight = 10;
        public void UpdateSizes()
        {
            Rectangle_ProhibitZone.Width = ProhibitWidth/2 / CurrentMarker.Scale;
            Rectangle_ProhibitZone.Height = ProhibitHeight/2 / CurrentMarker.Scale;

            Rectangle_Vehicle.Width = VehicleWidth/2 / CurrentMarker.Scale;
            Rectangle_Vehicle.Height = VehicleHeight/2 / CurrentMarker.Scale;

            Rectangle_PresenceZone.Height = (CurrentMarker.PresenceZoneSize / 2) / CurrentMarker.Scale;
            Rectangle_PresenceZone.Width = ProhibitWidth/2 / CurrentMarker.Scale;

            Rectangle_WarningZone.Height = (CurrentMarker.WarningZoneSize / 2) / CurrentMarker.Scale;
            Rectangle_WarningZone.Width = ProhibitWidth / 2 / CurrentMarker.Scale;
            Rectangle_CriticalZone.Height = (CurrentMarker.CriticalZoneSize / 2) / CurrentMarker.Scale;
            Rectangle_CriticalZone.Width = ProhibitWidth / 2 / CurrentMarker.Scale;
            Label_PopupInfo.Content = CurrentMarker.title;
            RectangleTransform_Crtical.Y = -Rectangle_CriticalZone.Height / 2;
            RectangleTransform_Warning.Y = -Rectangle_WarningZone.Height / 2;
            RectangleTransform_Presence.Y = -Rectangle_PresenceZone.Height / 2;
            //Rectangle_PresenceZone.LayoutTransform = IndicatorTransfrom;
            //Rectangle_WarningZone.LayoutTransform = IndicatorTransfrom;
            //Rectangle_CriticalZone.LayoutTransform = IndicatorTransfrom;
            Rectangle_RotateTransform_Warning.Angle = CurrentMarker.Heading;
            Rectangle_RotateTransform_Presence.Angle = CurrentMarker.Heading;
            Rectangle_RotateTransform_Critical.Angle = CurrentMarker.Heading;
            Ellipse_PresenceZone.Width = CurrentMarker.PresenceZoneSize / CurrentMarker.Scale;
            Ellipse_PresenceZone.Height = CurrentMarker.PresenceZoneSize / CurrentMarker.Scale;

            Ellipse_WarningZone.Width = CurrentMarker.WarningZoneSize / CurrentMarker.Scale;
            Ellipse_WarningZone.Height = CurrentMarker.WarningZoneSize / CurrentMarker.Scale;
            Ellipse_WarningZoneBackground.Width = CurrentMarker.WarningZoneSize / CurrentMarker.Scale;
            Ellipse_WarningZoneBackground.Height = CurrentMarker.WarningZoneSize / CurrentMarker.Scale;

            Ellipse_CriticalZone.Width = CurrentMarker.CriticalZoneSize / CurrentMarker.Scale;
            Ellipse_CriticalZone.Height = CurrentMarker.CriticalZoneSize / CurrentMarker.Scale;
            Ellipse_CriticalZoneBackground.Width = CurrentMarker.CriticalZoneSize / CurrentMarker.Scale;
            Ellipse_CriticalZoneBackground.Height = CurrentMarker.CriticalZoneSize / CurrentMarker.Scale;
        }
        private Double _CustomMarkerAngle;
        public Double CustomMarkerAngle
        {
            get { return _CustomMarkerAngle; }
            set
            {
                _CustomMarkerAngle = value;
                OnPropertyChanged("CustomMarkerAngle");
            }
        }

      void CustomMarkerIndicator_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         Marker.Offset = new Point(-e.NewSize.Width/2, -e.NewSize.Height/2);
      }

      void CustomMarkerIndicator_MouseMove(object sender, MouseEventArgs e)
      {
         if(e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
         {
            Point p = e.GetPosition(mapControl);
            Marker.Position = mapControl.FromLocalToLatLng((int) p.X, (int) p.Y);
         }
      }

      void CustomMarkerIndicator_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
      {
            //if(!IsMouseCaptured)
            //{
            //   Mouse.Capture(this);
            //}
            if (!Label_PopupInfo.IsVisible)
            {
                Label_PopupInfo.Visibility = Visibility.Visible;
            }
            else
            {
                Label_PopupInfo.Visibility = Visibility.Collapsed;
               
            }

      }

      void CustomMarkerIndicator_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
      {
            //if(IsMouseCaptured)
            //{
            //   Mouse.Capture(null);
            //}
          //  Popup.IsOpen = false;

        }

        void CustomMarkerIndicator_MouseLeave(object sender, MouseEventArgs e)
      {
            PathIndicator.Opacity = 1;
        // Marker.ZIndex -= 10000;
      
            Ellipse_CriticalZone.Visibility = Visibility.Collapsed;
            Ellipse_WarningZoneBackground.Visibility = Visibility.Collapsed;
            Ellipse_WarningZone.Visibility = Visibility.Collapsed;
            Ellipse_CriticalZoneBackground.Visibility = Visibility.Collapsed;
            Ellipse_PresenceZone.Visibility = Visibility.Collapsed;
            if (!IsBrakeZoneVisible)
            {
                Rectangle_CriticalZone.Visibility = Visibility.Collapsed;
                Rectangle_WarningZone.Visibility = Visibility.Collapsed;
                Rectangle_PresenceZone.Visibility = Visibility.Collapsed;
            }

        }

      void CustomMarkerIndicator_MouseEnter(object sender, MouseEventArgs e)
      {
            PathIndicator.Opacity = 0.5;
            //Marker.ZIndex += 10000;
        
            Ellipse_CriticalZone.Visibility = Visibility.Visible;
            Ellipse_WarningZoneBackground.Visibility = Visibility.Visible;
            Ellipse_WarningZone.Visibility = Visibility.Visible;
            Ellipse_CriticalZoneBackground.Visibility = Visibility.Visible;
            Ellipse_PresenceZone.Visibility = Visibility.Visible;
        
            Rectangle_CriticalZone.Visibility = Visibility.Visible;
            Rectangle_WarningZone.Visibility = Visibility.Visible;
            Rectangle_PresenceZone.Visibility = Visibility.Visible;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

   

        private void PathIndicator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsBrakeZoneVisible)
            {
                IsBrakeZoneVisible = true;
            }
            else
            {
                IsBrakeZoneVisible = false;
            }
               
        }
    }
}