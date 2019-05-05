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

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for ImageFilesView.xaml
    /// </summary>
    public partial class ImageFilesView : UserControl
    {
        private uint SelectVID = 0;

        public ImageFilesView()
        {
            InitializeComponent();
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAppend_Click(object sender, RoutedEventArgs e)
        {

        }


        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
            {
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.ConfigureMenuView;
            }
            else
            {
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.FileMenuView;
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.AudioFilesView;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ParametersView;
        }
                private void ButtonNext_MouseEnter(object sender, MouseEventArgs e)
        {
            RectangleArrowRight.Fill = new SolidColorBrush(Color.FromRgb(60, 6, 6));
            ImageParameter.Opacity = 1;
        }

        private void ButtonNext_MouseLeave(object sender, MouseEventArgs e)
        {
            RectangleArrowRight.Fill = new SolidColorBrush(Color.FromRgb(140, 9, 9));
            ImageParameter.Opacity = 0.6;
        }

        private void ButtonPrevious_MouseEnter(object sender, MouseEventArgs e)
        {
            RectangleArrowLeft.Fill = new SolidColorBrush(Color.FromRgb(60, 6, 6));
            ImagePicture.Opacity = 1;
        }

        private void ButtonPrevious_MouseLeave(object sender, MouseEventArgs e)
        {
            RectangleArrowLeft.Fill = new SolidColorBrush(Color.FromRgb(140, 9, 9));
            ImagePicture.Opacity = 0.6;
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                
                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                {
                    DataGridImageFiles.Columns[2].Visibility = Visibility.Visible;
                    ButtonNew.Visibility = Visibility.Visible;
                    ButtonAppend.Visibility = Visibility.Visible;
                    Grid_Progressbar.Visibility = Visibility.Visible;
                    ButtonNext.Visibility = Visibility.Collapsed;
                    ButtonPrevious.Visibility = Visibility.Collapsed;
                    WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                    SelectVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                    
                }
                else
                {
                    DataGridImageFiles.Columns[2].Visibility = Visibility.Collapsed;
                    ButtonNew.Visibility = Visibility.Collapsed;
                    ButtonAppend.Visibility = Visibility.Collapsed;
                    ButtonNext.Visibility = Visibility.Collapsed;
                    ButtonPrevious.Visibility = Visibility.Collapsed;
                    Grid_Progressbar.Visibility = Visibility.Collapsed;
                }

            }
            else
            {
               
            }

        }
    }
}
