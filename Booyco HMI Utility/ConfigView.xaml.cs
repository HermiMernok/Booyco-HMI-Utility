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
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : UserControl, INotifyPropertyChanged
    {
        PropertyGroupDescription groupDescription = new PropertyGroupDescription("Group");
        PropertyGroupDescription SubgroupDescription = new PropertyGroupDescription("SubGroup");
        ParametersDisplay ParametersDisplay = new ParametersDisplay();
        CollectionView parametrsGroup;
        GeneralFunctions generalFunctions;

        private DispatcherTimer dispatcherTimer;

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

        private void GetDefaultParametersFromFile()
        {
            ExcelFileManagement excelFileManagement = new ExcelFileManagement();
            string _parameterPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/CommanderParametersFile.xlsx";

            Parameters = new List<Parameters>();
            Parameters = excelFileManagement.ParametersfromFile(_parameterPath);

            Disp_Parameters = new ObservableCollection<ParametersDisplay>();
            Disp_Parameters = ParametersToDisplay(parameters);

            parametrsGroup = (CollectionView)CollectionViewSource.GetDefaultView(Disp_Parameters);
            parametrsGroup.GroupDescriptions.Add(groupDescription);
            parametrsGroup.GroupDescriptions.Add(SubgroupDescription);

            Save_ParaMetersToFile();
        }

        static int Configchunks = 0;

        private void Save_ParaMetersToFile()
        {
            byte[] paraMeterBytes = new byte[parameters.Count * 4];

            for (int i = 0; i < parameters.Count; i++)
            {
                Array.Copy(BitConverter.GetBytes(parameters[i].CurrentValue), 0, paraMeterBytes, i * 4, 4);
            }

            string hex = BitConverter.ToString(paraMeterBytes).Replace("-", string.Empty);
            File.WriteAllText("Parameters.mer", hex);

            int fileChunck = 512;
            int bytesleft = 0;
            int ConfigfileSize = 0;
            bytesleft = ConfigfileSize = paraMeterBytes.Length;

            Configchunks = (int)Math.Round(ConfigfileSize / (double)fileChunck);
            int shifter = 0;
            for (int i = 0; i <= Configchunks; i++)
            {
                byte[] bootchunk = Enumerable.Repeat((byte)0xFF, 522).ToArray();
                byte[] bytes = BitConverter.GetBytes(i);
                byte[] bytes2 = BitConverter.GetBytes(Configchunks);
                bootchunk[0] = (byte)'[';
                bootchunk[1] = (byte)'&';
                bootchunk[2] = (byte)'P';
                bootchunk[3] = (byte)'D';
                bootchunk[4] = bytes[0];
                bootchunk[5] = bytes[1];
                bootchunk[6] = bytes2[0];
                bootchunk[7] = bytes2[1];

                if (bytesleft > fileChunck)
                    Array.Copy(paraMeterBytes, shifter, bootchunk, 8, fileChunck);
                else if (bytesleft > 0)
                    Array.Copy(paraMeterBytes, shifter, bootchunk, 8, bytesleft);

                bootchunk[520] = 0;
                bootchunk[521] = (byte)']';
                ConfigSendList.Add(bootchunk);
                shifter += fileChunck;
                bytesleft -= fileChunck;
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            this.Visibility = Visibility.Collapsed;
            ConfigSendStop = true;
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Save_ParaMetersToFile();

            //BootStart = true;
            ConfigSendReady = false;
            ConfigSentIndex = 0;
            ConfigSentAckIndex = -1;
            byte[] bootmessage = new byte[522];
            if (ConfigureThread != null && ConfigureThread.IsAlive)
            {

            }
            else
            {
                ConfigureThread = new Thread(ConfigSendDo)
                {
                    IsBackground = true,
                    Name = "BootloaderThread"
                };
                ConfigureThread.Start();
            }

            ConfigStatus = "Asking device to boot...";
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&BB00]");


        }

        public ObservableCollection<ParametersDisplay> ParametersToDisplay(List<Parameters> parameters)
        {
            ObservableCollection<ParametersDisplay> parametersDisplays = new ObservableCollection<ParametersDisplay>();
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
                        EnumIndx = enumIndx,
                        Group = Parameters[i].Group,
                        SubGroup = parameters[i].SubGroup,
                        Description = parameters[i].Description
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
                        LablEdit = EditLbl,
                        Group = parameters[i].Group,
                        SubGroup = parameters[i].SubGroup,
                        Description = parameters[i].Description
                    });
                }              

            }

            return parametersDisplays;
        }

        public ParametersDisplay DisplayParameterUpdate(Parameters parameters, int Index)
        {
            string valueString = "";
            Visibility btnvisibility = Visibility.Collapsed;
            Visibility drpDwnVisibility = Visibility.Collapsed;
            bool EditLbl = true;
            int enumIndx = -1;

            if (parameters.Ptype == 0)
            {
                valueString = parameters.CurrentValue.ToString();
                enumIndx = -1;
                btnvisibility = Visibility.Visible;
                drpDwnVisibility = Visibility.Collapsed;
                EditLbl = false;
            }
            else if (parameters.Ptype == 1)
            {
                valueString = (parameters.CurrentValue == 1) ? "true" : "false";
                enumIndx = -1;
                btnvisibility = Visibility.Visible;
                drpDwnVisibility = Visibility.Collapsed;
                EditLbl = true;
            }
            else if (parameters.Ptype == 2)
            {
                valueString = parameters.parameterEnums[parameters.CurrentValue];
                enumIndx = parameters.CurrentValue;
                btnvisibility = Visibility.Visible;
                drpDwnVisibility = Visibility.Visible;
                EditLbl = true;
            }

            ParametersDisplay newDisp_Parameter_value = new ParametersDisplay()
            {
                OriginIndx = Index,
                Name = parameters.Name,
                Value = valueString,
                BtnVisibility = btnvisibility,
                dropDownVisibility = drpDwnVisibility,
                LablEdit = EditLbl,
                parameterEnums = parameters.parameterEnums,
                EnumIndx = enumIndx,
                Group = parameters.Group,
                SubGroup = parameters.SubGroup,
                Description = parameters.Description
            };

            return newDisp_Parameter_value;
        }

        #region properties
        private List<Parameters> parameters;

        public List<Parameters> Parameters
        {
            get { return parameters; }
            set { parameters = value; OnPropertyChanged("Parameters"); }
        }

        private ObservableCollection<ParametersDisplay> disp_parameters;

        public ObservableCollection<ParametersDisplay> Disp_Parameters
        {
            get { return disp_parameters; }
            set { disp_parameters = value; OnPropertyChanged("Disp_Parameters"); }
        }
        #endregion

        #region Datagrid functions
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
                
                Disp_Parameters[DGparameters.SelectedIndex] = DisplayParameterUpdate(parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx], Disp_Parameters[DGparameters.SelectedIndex].OriginIndx);
                //Disp_Parameters = ParametersToDisplay(parameters);
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
                Disp_Parameters[DGparameters.SelectedIndex] = DisplayParameterUpdate(parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx], Disp_Parameters[DGparameters.SelectedIndex].OriginIndx);
                //Disp_Parameters = ParametersToDisplay(parameters);
                DGparameters.SelectedIndex = temp;
            }
        }

        private void RowDoubleClick(object sender, RoutedEventArgs e)
        {
            var row = (DataGridRow)sender;
            //if (row.DetailsVisibility == Visibility.Collapsed)
            //{
            //    DataLogIsExpanded = true;

            //  //  Expander_Expanded(sender, e);
            //}
            //else
            //{
            //    DataLogIsExpanded = false;
            // //   Expander_Collapsed(sender, e);
            //}
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        }
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DGparameters.SelectedIndex != -1 && parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].Ptype == 4)
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = generalFunctions.StringConditioner(textBox.Text);
                textBox.SelectionStart = textBox.Text.Length;
                String str = textBox.Text;
                byte[] NameBytes = new byte[20];

                NameBytes = Encoding.ASCII.GetBytes(textBox.Text);
                for (int i = 19; i > 0; i--)
                {
                    if((19-i) < NameBytes.Length)
                        parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx-i].CurrentValue = NameBytes[19-i];
                    else
                        parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx - i].CurrentValue = (byte)' ';
                }

            }
            else if(DGparameters.SelectedIndex != -1 && parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].Ptype == 0)
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = generalFunctions.StringNumConditioner(textBox.Text);
                textBox.SelectionStart = textBox.Text.Length;
                parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue = Convert.ToInt32(Disp_Parameters[DGparameters.SelectedIndex].Value);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue = comboBox.SelectedIndex;           
            //Disp_Parameters = ParametersToDisplay(parameters);
        }
        private static Thread ConfigureThread;
        
        #endregion

        List<byte[]> ConfigSendList = new List<byte[]>();

        public static string ConfigStatus { get; set; }

        public static bool ConfigSendReady { get; set; }

        public static bool ConfigSendDone { get; set; }

        public static int ConfigPersentage { get; set; }

        public static int ConfigSentIndex { get; set; }

        public static bool ConfigSendStop { get; set; }

        public static bool bootContinue;

        public static int ConfigSentAckIndex { get; set; }

        public static void ConfigSendParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'P'))
            {

                #region Configure ready to start
                if (message[3] == 'a' && message[6] == ']')
                {
                    ConfigStatus = "Device ready to boot...";
                    GlobalSharedData.ServerStatus = "Config ready message recieved";
                    GlobalSharedData.BroadCast = false;
                    ConfigSendReady = true;
                    WiFiconfig.SelectedIP = endPoint.ToString();
                }
                #endregion

                #region Configure next index
                if (message[3] == 'D')
                {
                    if (message[4] == 'a' && message[9] == ']')
                    {
                        bootContinue = true;
                        ConfigSentAckIndex = BitConverter.ToUInt16(message, 5);
                        //ConfigStatus = "Device bootloading packet " + ConfigSentAckIndex.ToString() + " of " + bootchunks.ToString() + "...";
                        GlobalSharedData.ServerStatus = "Config acknowledgment message recieved";

                    }
                }
                #endregion

                #region Configure complete message
                if (message[3] == 's' && message[6] == ']')
                {
                    ConfigStatus = "Device bootloading done...";
                    ConfigSendDone = true;
                    ConfigSendReady = false;
                    Thread.Sleep(20);
                    GlobalSharedData.ServerMessageSend = WiFiconfig.HeartbeatMessage;
                    GlobalSharedData.ServerStatus = "Config acknowledgment message recieved";
                }
                #endregion

                #region Configure error message
                if (message[3] == 'e' && message[8] == ']')
                {
                    if (BitConverter.ToUInt16(message, 4) == 0xFFFF)
                    {
                        ConfigSentIndex = 0;
                        ConfigSentAckIndex = -1;
                        ConfigStatus = "Waiting for device, please be patient... " + ConfigSentAckIndex.ToString() + "...";
                    }
                    else
                    {
                        ConfigSentAckIndex--;
                        ConfigStatus = "Waiting for device, please be patient... " + ConfigSentAckIndex.ToString() + "...";
                    }

                }
                #endregion
            }
        }

        private void ConfigSendDo()
        {
            //BootBtnEnabled = false;
            while (!WiFiconfig.endAll && !ConfigSendStop)
            {
                //Thread.Sleep(100);
                if (ConfigSendReady)
                {

                    if (ConfigSentIndex == 0 && ConfigSentAckIndex == -1)
                    {
                        GlobalSharedData.ServerMessageSend = ConfigSendList.ElementAt(ConfigSentIndex);
                        ConfigSentIndex++;
                    }

                    if (ConfigSentIndex < ConfigSendList.Count && ConfigSentAckIndex == ConfigSentIndex - 1)
                    {
                        GlobalSharedData.ServerMessageSend = ConfigSendList.ElementAt(ConfigSentIndex);
                        ConfigSentIndex++;
                    }

                    if (ConfigSentIndex == ConfigSendList.Count)
                    {
                        Console.WriteLine("====================Bootloading done======================");
                        //WIFIcofig.ServerMessageSend = 
                        //BootReady = false;
                        break;
                    }
                }

            }
            //BootBtnEnabled = true;
            ConfigSendStop = false;
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.Visibility == Visibility.Visible) //when the view is opened
            {
                GetDefaultParametersFromFile();
            }
            else //when the view is closed
            {
                ConfigSendStop = true;
            }
        }
    }
}

