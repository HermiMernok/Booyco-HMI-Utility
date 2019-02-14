using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using System.IO;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : UserControl, INotifyPropertyChanged
    {

        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion
           
        public ConfigView()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            this.Visibility = Visibility.Collapsed;
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            ExcelFileManagement excelFileManagement = new ExcelFileManagement();
            string _parameterPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/CommanderParametersFile.xlsx";
            Parameters = excelFileManagement.ParametersfromFile(_parameterPath);
            Disp_Parameters = ParametersToDisplay(parameters);

        }

        public List<ParametersDisplay> ParametersToDisplay(List<Parameters> parameters)
        {
            List<ParametersDisplay> parametersDisplays = new List<ParametersDisplay>();
            string VehicleName = "";
            string valueString = "";

            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Ptype == 0)
                {
                    valueString = parameters[i].CurrentValue.ToString();
                }
                else if (parameters[i].Ptype == 1)
                {
                    valueString = (parameters[i].CurrentValue == 1) ? "true" : "false";
                }
                else if (parameters[i].Ptype == 2)
                {
                    valueString = parameters[i].parameterEnums[parameters[i].CurrentValue];
                }
                else if (parameters[i].Ptype == 4)
                {
                    VehicleName += Convert.ToChar(parameters[i].CurrentValue);
                }

                if (!parameters[i].Name.Contains("Name") && !parameters[i].Name.Contains("Reserved"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = parameters[i].Name,
                        Value = valueString
                    });
                }
                else if (parameters[i].Name.Contains("Name 20"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = "Name",
                        Value = VehicleName
                    });
                }

            }

            return parametersDisplays;
        }

        #region properties
        private List<Parameters> parameters;

        public List<Parameters> Parameters
        {
            get { return parameters; }
            set { parameters = value; OnPropertyChanged("Parameters"); }
        }

        private List<ParametersDisplay> disp_parameters;

        public List<ParametersDisplay> Disp_Parameters
        {
            get { return disp_parameters; }
            set { disp_parameters = value; OnPropertyChanged("Disp_Parameters"); }
        }
        #endregion

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void min_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void max_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

