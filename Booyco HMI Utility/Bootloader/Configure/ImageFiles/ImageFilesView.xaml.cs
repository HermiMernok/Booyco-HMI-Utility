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

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ConfigureMenuView;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.AudioFilesView;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ParametersView;
        }
    }
}
