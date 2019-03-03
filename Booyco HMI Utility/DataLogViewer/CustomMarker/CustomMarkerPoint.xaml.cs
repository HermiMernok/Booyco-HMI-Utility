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
using Booyco_HMI_Utility.CustomMarker;

namespace Demo.WindowsPresentation.CustomMarkers
{
    /// <summary>
    /// Interaction logic for CustomMarkerDemo.xaml
    /// </summary>
    public partial class CustomMarkerPoint
   {
        Popup Popup;
        Label Label;
        GMapMarker Marker;
    
        GMapControl mapControl;

            public CustomMarkerPoint(GMapControl _mapControl, MarkerEntry marker)
        {
                this.InitializeComponent();
                this.mapControl = _mapControl;
                this.Marker = marker.MapMarker;

                Popup = new Popup();
            Label = new Label();

            if (marker.Zone == 1)
            {
                PointOfIntersection.Fill = Brushes.Blue;
            }
            else if (marker.Zone == 2)
            {
                PointOfIntersection.Fill = Brushes.Yellow;
            }
            else if (marker.Zone == 3)
            {
                PointOfIntersection.Fill = Brushes.Red;
            }

            this.SizeChanged += new SizeChangedEventHandler(CustomMarkerPoint_SizeChanged);
            this.MouseEnter += new MouseEventHandler(CustomMarkerPoint_MouseEnter);
            this.MouseLeave += new MouseEventHandler(CustomMarkerPoint_MouseLeave);

            Popup.Placement = PlacementMode.Mouse;
            {
                Label.Background = Brushes.Gray;
                Label.Foreground = Brushes.White;
                Label.BorderBrush = Brushes.Black;
                Label.BorderThickness = new Thickness(2);
                Label.Padding = new Thickness(5);
                Label.FontSize = 12;
                Label.Content = marker.title;
            }
            Popup.Child = Label;
        }

        void CustomMarkerPoint_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

        void CustomMarkerPointr_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                Point p = e.GetPosition(mapControl);
                Marker.Position = mapControl.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }

        void CustomMarkerPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        void CustomMarkerPoint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        void CustomMarkerPoint_MouseLeave(object sender, MouseEventArgs e)
        {
         //   Marker.ZIndex -= 10000;
            Popup.IsOpen = false;
        }

        void CustomMarkerPoint_MouseEnter(object sender, MouseEventArgs e)
        {
           // Marker.ZIndex += 10000;
            Popup.IsOpen = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}