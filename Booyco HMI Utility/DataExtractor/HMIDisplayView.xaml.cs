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
    /// Interaction logic for HMIDisplayView.xaml
    /// </summary>
    public partial class HMIDisplayView : UserControl
    {

        public bool CloseRequest = false;
        public HMIDisplayView()
        {
            InitializeComponent();
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Dataview;
            CloseRequest = true;
        }

        private void ButtonNewWindow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
