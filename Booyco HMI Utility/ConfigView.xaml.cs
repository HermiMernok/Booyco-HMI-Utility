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
using System.Windows.Threading;

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
            generalFunctions = new GeneralFunctions();
            InitializeComponent();

        }
        GeneralFunctions generalFunctions;
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
            Visibility btnvisibility = Visibility.Collapsed;
            Visibility drpDwnVisibility = Visibility.Collapsed;
            bool EditLbl = true;
            int enumIndx = -1;

            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].Ptype == 0)
                {
                    valueString = parameters[i].CurrentValue.ToString();
                    enumIndx = -1;
                    btnvisibility = Visibility.Visible;
                    drpDwnVisibility = Visibility.Collapsed;
                    EditLbl = false;
                }
                else if (parameters[i].Ptype == 1)
                {
                    valueString = (parameters[i].CurrentValue == 1) ? "true" : "false";
                    enumIndx = -1;
                    btnvisibility = Visibility.Visible;
                    drpDwnVisibility = Visibility.Collapsed;
                    EditLbl = true;
                }
                else if (parameters[i].Ptype == 2)
                {
                    valueString = parameters[i].parameterEnums[parameters[i].CurrentValue];
                    enumIndx = parameters[i].CurrentValue;
                    btnvisibility = Visibility.Visible;
                    drpDwnVisibility = Visibility.Visible;
                    EditLbl = true;
                }
                else if (parameters[i].Ptype == 4)
                {
                    VehicleName += Convert.ToChar(parameters[i].CurrentValue);
                    enumIndx = -1;
                    btnvisibility = Visibility.Collapsed;
                    drpDwnVisibility = Visibility.Collapsed;
                    EditLbl = false;
                }

                if (!parameters[i].Name.Contains("Name") && !parameters[i].Name.Contains("Reserved"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = parameters[i].Name,
                        Value = valueString,
                        BtnVisibility = btnvisibility,
                        dropDownVisibility = drpDwnVisibility,
                        LablEdit = EditLbl,
                        parameterEnums = parameters[i].parameterEnums,
                        EnumIndx = enumIndx
                    });
                }
                else if (parameters[i].Name.Contains("Name 20"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = "Name",
                        Value = VehicleName,
                        BtnVisibility = btnvisibility,
                        dropDownVisibility = drpDwnVisibility,
                        LablEdit = EditLbl
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

        private void min_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DGparameters.SelectedIndex != -1)
            {
                int temp = DGparameters.SelectedIndex;
                if (parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue > parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].MinimumValue)
                {
                    parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue--;
                }
                else if (parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue == parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].MinimumValue)
                {
                    parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue = parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].MaximumValue;
                }
                Disp_Parameters = ParametersToDisplay(parameters);
                DGparameters.SelectedIndex = temp;
            }
        }

        private void max_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DGparameters.SelectedIndex != -1)
            {
                int temp = DGparameters.SelectedIndex;
                if (parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue < parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].MaximumValue)
                {
                    parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue++;
                }
                else if (parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue == parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].MaximumValue)
                {
                    parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue = parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].MinimumValue;
                }
                Disp_Parameters = ParametersToDisplay(parameters);
                DGparameters.SelectedIndex = temp;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DGparameters.SelectedIndex != -1 && parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].Ptype == 4)
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = generalFunctions.StringConditioner(textBox.Text);
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (DGparameters.SelectedIndex != -1 && comboBox.SelectedIndex != -1)
            {                
                int temp = DGparameters.SelectedIndex;
                if(comboBox.SelectedIndex < parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].MaximumValue && comboBox.SelectedIndex > parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].MinimumValue)
                {
                    parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue = comboBox.SelectedIndex;
                    Disp_Parameters[DGparameters.SelectedIndex].EnumIndx = parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue;
                }
                    

                //Disp_Parameters = ParametersToDisplay(parameters);
                DGparameters.SelectedIndex = temp;
            }
        }
    }
}

