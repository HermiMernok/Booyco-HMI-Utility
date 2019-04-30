using Booyco_HMI_Utility.CustomObservableCollection;
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
    /// Interaction logic for AudioFilesView.xaml
    /// </summary>
    public partial class AudioFilesView : UserControl
    {
        private RangeObservableCollection<AudioEntry> AudioFileList;

        public AudioFilesView()
        {
            InitializeComponent();
            AudioFileList = new RangeObservableCollection<AudioEntry>();
            DataGridAudioFiles.AutoGenerateColumns = false;
            DataGridAudioFiles.ItemsSource = AudioFileList;
     
            ReadAudioFiles();
                
        }

        void ReadAudioFiles()
        {
            AudioFileList.Add(new AudioEntry
            {
                ID = 1,
                Name = "Test File",
                Progress = 50
            });
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            AudioFileList.Last().Progress++;
        }

        private void ButtonAppend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            this.Visibility = Visibility.Collapsed;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Configure;
  
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
    }
}
