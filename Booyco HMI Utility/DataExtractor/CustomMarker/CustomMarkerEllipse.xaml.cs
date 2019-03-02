using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Booyco_HMI_Utility.CustomMarker;

namespace Demo.WindowsPresentation.CustomMarkers
{
    /// <summary>
    /// Interaction logic for Cross.xaml
    /// </summary>
    public partial class CustomMarkerEllipse
    {
        Popup Popup;
        Label Label;
        GMapMarker Marker;    
        GMapControl mapControl;

       public CustomMarkerEllipse(GMapControl _mapControl, MarkerEntry marker)
        {
                this.InitializeComponent();
                this.mapControl = _mapControl;
                this.Marker = marker.MapMarker;

                Popup = new Popup();
            Label = new Label();

            this.SizeChanged += new SizeChangedEventHandler(MarkerEllipseControl_SizeChanged);
            this.MouseEnter += new MouseEventHandler(MarkerellipseControl_MouseEnter);
            this.MouseLeave += new MouseEventHandler(MarkerEllipseControl_MouseLeave);

            //ScaleTransform EllipseTransfrom = new ScaleTransform(14,14);
            //EllipseMarker.RenderTransform = EllipseTransfrom;
        
            EllipseMarker.Width = marker.Width/marker.Scale;
            EllipseMarker.Height = marker.Height/marker.Scale;

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

        void MarkerEllipseControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

        void MarkerEllipseControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //  Marker.ZIndex -= 10000;
            EllipseMarker.Opacity = 1;
            Popup.IsOpen = false;
        }

        void MarkerellipseControl_MouseEnter(object sender, MouseEventArgs e)
        {
            EllipseMarker.Opacity = 0.5;
            //  Marker.ZIndex += 10000;
            Popup.IsOpen = true;
        }

      

        private void EllipseMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Popup.IsOpen)
            {
                Popup.IsOpen = false;
            }
            else
            {
                Popup.IsOpen = true;
            }
        }

        private void EllipseMarker_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
