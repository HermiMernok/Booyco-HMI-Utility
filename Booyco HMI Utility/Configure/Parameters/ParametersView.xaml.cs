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
using System.Text.RegularExpressions;

namespace Booyco_HMI_Utility
{

    public partial class ParametersView : UserControl, INotifyPropertyChanged
    {
        PropertyGroupDescription groupDescription = new PropertyGroupDescription("Group");
        PropertyGroupDescription SubgroupDescription = new PropertyGroupDescription("SubGroup");
        ParametersDisplay ParametersDisplay = new ParametersDisplay();
        CollectionView parametrsGroup;
        public CollectionViewSource parametersGroup = new CollectionViewSource();
        GeneralFunctions generalFunctions;
        private static bool backBtner = false;
        private string ParameterSaveFilename = "";
        private DispatcherTimer updateDispatcherTimer;
        private DispatcherTimer InfoDelay;


        private DispatcherTimer dispatcherTimer;

        static int StoredIndex = -1;
        static bool ParamsReceiveComplete = false;
        static bool ParamsRequestStarted = false;
        static bool ParamsTransmitComplete = false;
        static bool ParamsSendStarted = false;
        static bool RevertInfo = false;

        private static int ParamReceiveProgress = 0;
        private static int ParamTransmitProgress = 0;
        public static int DataIndex { get; set; }
        public static int TotalCount { get; set; }

      
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
     

        public ParametersView()
        {
            DataContext = this;
            generalFunctions = new GeneralFunctions();
            InitializeComponent();

            updateDispatcherTimer = new DispatcherTimer();
            updateDispatcherTimer.Tick += new EventHandler(ConfigReceiveParams);
            updateDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            InfoDelay = new DispatcherTimer();
            InfoDelay.Tick += new EventHandler(InfoDelayFunc);
            InfoDelay.Interval = new TimeSpan(0, 0, 0, 0, 5000);
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {

                SureMessageVis = Visibility.Collapsed;
                ParamsRequestStarted = false;
                ParamsReceiveComplete = false;
                ParamsTransmitComplete = false;
                ParamsSendStarted = false;
                Label_StatusView.Content = "Waiting for user command..";
                ProgressBar_Params.Value = 0;
                Label_ProgressStatusPercentage.Content = "";

                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.File)
                {
                    SendFileButton.Visibility = Visibility.Collapsed;
                    ButtonConfigRefresh.Visibility = Visibility.Collapsed;
                }
                else if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                {
                    //ConfigRefreshButton.Content = "Refresh";
                    SendFileButton.Visibility = Visibility.Visible;
                    ButtonConfigRefresh.Visibility = Visibility.Visible;
                }

                ButtonConfigRefresh_Click(null,null);
            }
            else
            {
                updateDispatcherTimer.Stop();
                ProgressBar_Params.Value = 0;
                ConfigSendReady = false;
                ConfigSendStop = true;
            }
        }

        private void InfoDelayFunc(object sender, EventArgs e)
        {
            RevertInfo = true;
        }    

        private void ConfigReceiveParams(object sender, EventArgs e)
        {
            if (ParamsRequestStarted)
            {
                RevertInfo = false;
                ProgressBar_Params.Value = ParamReceiveProgress;
                Label_ProgressStatusPercentage.Content = "Overall progress: " + (ParamReceiveProgress).ToString() + "%";
                Label_StatusView.Content = "Loading parameters from device: Packet " + DataIndex.ToString() + " of " + TotalCount.ToString() + "...";
                if (ParamsReceiveComplete)
                {

                    UpdateParametersFromDevice();
                    Label_StatusView.Content = "Loading of parameters from device completed...";
                    //updateDispatcherTimer.Stop();
                    ParamsReceiveComplete = false;
                    ParamsRequestStarted = false;
                    ButtonState(true);
                    InfoDelay.Start();
                }
            }
            else if (ParamsSendStarted)
            {
                RevertInfo = false;
                ParamsRequestStarted = false;
                ParamsReceiveComplete = false;
                ParamTransmitProgress = (ConfigSentIndex * 100) / Configchunks;
                ProgressBar_Params.Value = ParamTransmitProgress;
                Label_ProgressStatusPercentage.Content = "Overall progress: " + (ParamTransmitProgress).ToString() + "%";
                Label_StatusView.Content = "Loading parameters to device: Packet " + ConfigSentIndex.ToString() + " of " + Configchunks.ToString() + "...";

                if (ParamsTransmitComplete)
                {
                    Label_StatusView.Content = "Loading of parameters to device completed...";
                    ParamsTransmitComplete = false;
                    ParamsSendStarted = false;
                    ButtonState(true);
                    //updateDispatcherTimer.Stop();
                    InfoDelay.Start();
                }

            }
            //else if ()
            else if (RevertInfo)
            {
                Label_StatusView.Content = "Waiting for user command..";
                ProgressBar_Params.Value = 0;
                Label_ProgressStatusPercentage.Content = "";
                InfoDelay.Stop();
            }
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible) //when the view is opened
            {
                GetDefaultParametersFromFile();
                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                {
                    WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                    ButtonNext.Visibility = Visibility.Visible;
                    ButtonPrevious.Visibility = Visibility.Visible;
                    ButtonConfigRefresh.Visibility = Visibility.Visible;
                    SendFileButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ButtonConfigRefresh.Visibility = Visibility.Collapsed;
                    SendFileButton.Visibility = Visibility.Collapsed;
                    ButtonNext.Visibility = Visibility.Collapsed;
                    ButtonPrevious.Visibility = Visibility.Collapsed;
                }

                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(InfoUpdater);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                dispatcherTimer.Start();
            }
            else //when the view is closed
            {
                ConfigSendStop = true;
                dispatcherTimer.Stop();
            }
        }

        private void InfoUpdater(object sender, EventArgs e)
        {
            if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi && Visibility == Visibility.Visible && (WiFiconfig.clients.Count == 0 || WiFiconfig.clients.Where(t => t.Client.RemoteEndPoint.ToString() == WiFiconfig.SelectedIP).ToList().Count == 0))
            {
                WiFiconfig.ConnectionError = true;
                backBtner = true;
                ConfigSendReady = false;    //todo is this required
            }

            if (backBtner)
            {
                backBtner = false;
                ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
                this.Visibility = Visibility.Collapsed;
                ConfigSendStop = true;
            }
        }

        public ObservableCollection<ParametersDisplay> ParametersToDisplay(List<Parameters> parameters)
        {
            ObservableCollection<ParametersDisplay> parametersDisplays = new ObservableCollection<ParametersDisplay>();
            string VehicleName = "";
            string WiFiSSID = "";
            string WiFiPassword = "";
            string WiFiUnitIP = "";
            string WiFiServerIP = "";
            string WiFiGatewayIP = "";
            string WiFiSubnetMask = "";
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
                    if (parameters[i].Name.Contains("Name"))
                    {
                        VehicleName += Convert.ToChar(parameters[i].CurrentValue);
                        enumIndx = -1;
                        btnvisibility = Visibility.Collapsed;
                        drpDwnVisibility = Visibility.Collapsed;
                        EditLbl = false;
                    }
                    else if (parameters[i].Name.Contains("SSID"))
                    {
                        WiFiSSID += Convert.ToChar(parameters[i].CurrentValue);
                        enumIndx = -1;
                        btnvisibility = Visibility.Collapsed;
                        drpDwnVisibility = Visibility.Collapsed;
                        EditLbl = false;
                    }
                    else if (parameters[i].Name.Contains("Password"))
                    {
                        WiFiPassword += Convert.ToChar(parameters[i].CurrentValue);
                        enumIndx = -1;
                        btnvisibility = Visibility.Collapsed;
                        drpDwnVisibility = Visibility.Collapsed;
                        EditLbl = false;
                    }
                    else if (parameters[i].Name.Contains("Unit IP"))
                    {
                        WiFiUnitIP += Convert.ToChar(parameters[i].CurrentValue);
                        enumIndx = -1;
                        btnvisibility = Visibility.Collapsed;
                        drpDwnVisibility = Visibility.Collapsed;
                        EditLbl = false;
                    }
                    else if (parameters[i].Name.Contains("Server IP"))
                    {
                        WiFiServerIP += Convert.ToChar(parameters[i].CurrentValue);
                        enumIndx = -1;
                        btnvisibility = Visibility.Collapsed;
                        drpDwnVisibility = Visibility.Collapsed;
                        EditLbl = false;
                    }
                    else if (parameters[i].Name.Contains("Gateway IP"))
                    {
                        WiFiGatewayIP += Convert.ToChar(parameters[i].CurrentValue);
                        enumIndx = -1;
                        btnvisibility = Visibility.Collapsed;
                        drpDwnVisibility = Visibility.Collapsed;
                        EditLbl = false;
                    }
                    else if (parameters[i].Name.Contains("Subnet Mask"))
                    {
                        WiFiSubnetMask += Convert.ToChar(parameters[i].CurrentValue);
                        enumIndx = -1;
                        btnvisibility = Visibility.Collapsed;
                        drpDwnVisibility = Visibility.Collapsed;
                        EditLbl = false;
                    }

                }

                if(parameters[i].AccessLevel == (int)AccessLevelEnum.Full && GlobalSharedData.AccessLevel != (int)AccessLevelEnum.Full)
                {
                    EditLbl = true;
                    btnvisibility = Visibility.Collapsed;
                    drpDwnVisibility = Visibility.Collapsed;
                }

                if (!parameters[i].Name.Contains("Name") && !parameters[i].Name.Contains("Reserved") && !parameters[i].Name.Contains("SSID") && !parameters[i].Name.Contains("Password")
                    && !parameters[i].Name.Contains("Unit IP") && !parameters[i].Name.Contains("Server IP") && !parameters[i].Name.Contains("Gateway IP") && !parameters[i].Name.Contains("Subnet Mask"))
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
                else if (parameters[i].Name.Contains("Name 15"))
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
                else if (parameters[i].Name.Contains("SSID 32"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = "WiFi SSID",
                        Value = WiFiSSID,
                        BtnVisibility = btnvisibility,
                        dropDownVisibility = drpDwnVisibility,
                        LablEdit = EditLbl,
                        Group = parameters[i].Group,
                        SubGroup = parameters[i].SubGroup,
                        Description = parameters[i].Description
                    });
                }
                else if (parameters[i].Name.Contains("Password 32"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = "WiFi Password",
                        Value = WiFiPassword,
                        BtnVisibility = btnvisibility,
                        dropDownVisibility = drpDwnVisibility,
                        LablEdit = EditLbl,
                        Group = parameters[i].Group,
                        SubGroup = parameters[i].SubGroup,
                        Description = parameters[i].Description
                    });
                }
                else if (parameters[i].Name.Contains("Unit IP 15"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = "WiFi Unit IP",
                        Value = IPAddressConditioner(WiFiUnitIP),
                        BtnVisibility = btnvisibility,
                        dropDownVisibility = drpDwnVisibility,
                        LablEdit = EditLbl,
                        Group = parameters[i].Group,
                        SubGroup = parameters[i].SubGroup,
                        Description = parameters[i].Description
                    });
                }
                else if (parameters[i].Name.Contains("Server IP 15"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = "WiFi Server IP",
                        Value = IPAddressConditioner(WiFiServerIP),
                        BtnVisibility = btnvisibility,
                        dropDownVisibility = drpDwnVisibility,
                        LablEdit = EditLbl,
                        Group = parameters[i].Group,
                        SubGroup = parameters[i].SubGroup,
                        Description = parameters[i].Description
                    });
                }
                else if (parameters[i].Name.Contains("Gateway IP 15"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = "WiFi Gateway IP",
                        Value = IPAddressConditioner(WiFiGatewayIP),
                        BtnVisibility = btnvisibility,
                        dropDownVisibility = drpDwnVisibility,
                        LablEdit = EditLbl,
                        Group = parameters[i].Group,
                        SubGroup = parameters[i].SubGroup,
                        Description = parameters[i].Description
                    });
                }
                else if (parameters[i].Name.Contains("Subnet Mask 15"))
                {
                    parametersDisplays.Add(new ParametersDisplay
                    {
                        OriginIndx = i,
                        Name = "WiFi Subnet Mask",
                        Value = IPAddressConditioner(WiFiSubnetMask),
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



      
        private List<Parameters> parameters;

        public List<Parameters> Parameters
        {
            get { return parameters; }
            set { parameters = value; OnPropertyChanged("Parameters"); }
        }

        private Visibility _SureMessageVis;

        public Visibility SureMessageVis
        {
            get { return _SureMessageVis; }
            set { _SureMessageVis = value; OnPropertyChanged("SureMessageVis"); }
        }

        private ObservableCollection<ParametersDisplay> disp_parameters;

        public ObservableCollection<ParametersDisplay> Disp_Parameters
        {
            get { return disp_parameters; }
            set { disp_parameters = value; OnPropertyChanged("Disp_Parameters"); }
        }
    
        
        private int FindDispParIndex(int ParameterIndex)
        {
            int j = 0;
            foreach (ParametersDisplay item in Disp_Parameters)
            {
                if (item.Name == parameters[ParameterIndex].Name)
                {
                    return j;
                }
                j++;

            }
            return 32767;
        }

        private int FindParIndex(string str)
        {
            int j = 0;

            foreach (Parameters item in parameters)
            {
                if (item.Name == str)
                {
                    //tt = item.Name;
                    //item.
                    return j;
                }

                j++;

            }
            return 32767;
        }
        private void min_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DGparameters.SelectedIndex != -1)
            {
                ParametersDisplay tempPar = (ParametersDisplay)DGparameters.SelectedItem;
                var SortedIndex = tempPar.OriginIndx;

                //string str = tempPar.Name;
                //int SortedIndex = FindParIndex(tempPar.Name);
                int j = 0;

                if (parameters[SortedIndex].CurrentValue > parameters[SortedIndex].MinimumValue)
                {
                    parameters[SortedIndex].CurrentValue--;
                }
                else if (parameters[SortedIndex].CurrentValue == parameters[SortedIndex].MinimumValue)
                {
                    parameters[SortedIndex].CurrentValue = parameters[SortedIndex].MaximumValue;
                }

                Disp_Parameters[FindDispParIndex(SortedIndex)] = DisplayParameterUpdate(parameters[SortedIndex], SortedIndex);
            }
        }

        private void max_Button_Click(object sender, RoutedEventArgs e)
        {

            if (DGparameters.SelectedIndex != -1)
            {
                ParametersDisplay tempPar = (ParametersDisplay)DGparameters.SelectedItem;
                var SortedIndex = tempPar.OriginIndx;
                int j = 0;

                int DisplayIndex = DGparameters.SelectedIndex;

                if (parameters[SortedIndex].CurrentValue < parameters[SortedIndex].MaximumValue)
                {
                    parameters[SortedIndex].CurrentValue++;
                }
                else if (parameters[SortedIndex].CurrentValue == parameters[SortedIndex].MaximumValue)
                {
                    parameters[SortedIndex].CurrentValue = parameters[SortedIndex].MinimumValue;
                }

                Disp_Parameters[FindDispParIndex(SortedIndex)] = DisplayParameterUpdate(parameters[SortedIndex], SortedIndex);

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

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ParametersDisplay tempPar = new ParametersDisplay();
            var SortedIndex = 0;

            if (DGparameters.SelectedIndex != -1)
            {
                tempPar = (ParametersDisplay)DGparameters.SelectedItem;
                SortedIndex = tempPar.OriginIndx;
            }

           
            if (DGparameters.SelectedIndex != -1 && parameters[SortedIndex].Ptype == 4)
            {
           
                if (tempPar.Name == "Name")
                {
                    TextBox textBox = (TextBox)sender;
                    textBox.Text = generalFunctions.StringConditioner(textBox.Text);
                    textBox.SelectionStart = textBox.Text.Length;
                    String str = textBox.Text;
                    byte[] NameBytes = new byte[15];

                    NameBytes = Encoding.ASCII.GetBytes(textBox.Text);
                    for (int i = 14; i >= 0; i--)
                    {
                        if ((14 - i) < NameBytes.Length)
                            parameters[SortedIndex - i].CurrentValue = NameBytes[14 - i];
                        else
                            parameters[SortedIndex - i].CurrentValue = (byte)' ';
                    }
                }
              
            
                else if ((tempPar.Name == "WiFi Unit IP") || (tempPar.Name == "WiFi Server IP") || (tempPar.Name == "WiFi Gateway IP") || (tempPar.Name == "WiFi Subnet Mask"))
                {
                    TextBox textBox = (TextBox)sender;
                    textBox.Text = IPAddressConditioner(textBox.Text);
                    byte[] NameBytes = new byte[15];

                    NameBytes = Encoding.ASCII.GetBytes(textBox.Text);
                    for (int i = 14; i >= 0; i--)
                    {
                        if ((14 - i) < NameBytes.Length)
                            parameters[SortedIndex - i].CurrentValue = NameBytes[14 - i];
                        else
                            parameters[SortedIndex - i].CurrentValue = (byte)' ';
                    }
                }
         
               
                else if ((tempPar.Name == "WiFi Password") || (tempPar.Name == "WiFi SSID"))
                {
                    TextBox textBox = (TextBox)sender;
                    textBox.Text = generalFunctions.StringConditionerAlphaNum(textBox.Text, 32);
                    byte[] NameBytes = new byte[32];

                    NameBytes = Encoding.ASCII.GetBytes(textBox.Text);
                    for (int i = 31; i > 0; i--)
                    {
                        if ((31 - i) < NameBytes.Length)
                            parameters[SortedIndex - i].CurrentValue = NameBytes[31 - i];
                        else
                            parameters[SortedIndex - i].CurrentValue = (byte)' ';
                    }
                }
               
            }
           

           
            else if (DGparameters.SelectedIndex != -1 && parameters[SortedIndex].Ptype == 0)
            {
                TextBox textBox = (TextBox)sender;
                if (StringTestNum(textBox.Text))
                {
                    try
                    {
                        if ((Convert.ToInt32(Disp_Parameters[FindDispParIndex(SortedIndex)].Value) <= parameters[SortedIndex].MaximumValue) && (Convert.ToInt32(Disp_Parameters[FindDispParIndex(SortedIndex)].Value) >= parameters[SortedIndex].MinimumValue))
                        {
                            parameters[SortedIndex].CurrentValue = Convert.ToInt32(Disp_Parameters[FindDispParIndex(SortedIndex)].Value);
                        }
                        else
                        {
                            Disp_Parameters[FindDispParIndex(SortedIndex)].Value = parameters[SortedIndex].CurrentValue.ToString();
                        }
                        //parameters[SortedIndex].CurrentValue = Convert.ToInt32(Disp_Parameters[FindDispParIndex(SortedIndex)].Value);
                    }
                    catch
                    {
                        Disp_Parameters[FindDispParIndex(SortedIndex)].Value = parameters[SortedIndex].CurrentValue.ToString();
                    }

                }
                else
                {
                    Disp_Parameters[FindDispParIndex(SortedIndex)].Value = parameters[SortedIndex].CurrentValue.ToString();
                }

            }
     
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (DGparameters.SelectedIndex != -1)
            {
                ComboBox comboBox = (ComboBox)sender;
                ParametersDisplay tempPar = (ParametersDisplay)DGparameters.SelectedItem;
                var SortedIndex = tempPar.OriginIndx;

                parameters[SortedIndex].CurrentValue = comboBox.SelectedIndex;

                Disp_Parameters[FindDispParIndex(SortedIndex)] = DisplayParameterUpdate(parameters[SortedIndex], SortedIndex);
            }
        }

     

       
        public static void ConfigSendParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'P'))
            {


                if (message[3] == 'a' && message[6] == ']')
                {
                    ConfigSendReady = true;
                    //ConfigSentIndex = 0; 
                    //ConfigSentAckIndex = -1;
                    ParamsSendStarted = true;
                    ConfigStatus = "Device ready to configure...";
                    GlobalSharedData.ServerStatus = "Config ready message recieved";
                    GlobalSharedData.BroadCast = false;
                    WiFiconfig.SelectedIP = endPoint.ToString();
                }
             


                if (message[3] == 'D')
                {
                    if (message[4] == 'a' && message[9] == ']')
                    {
                        ConfigSentAckIndex = BitConverter.ToUInt16(message, 5);
                        ConfigStatus = "Device receiving packet " + ConfigSentAckIndex.ToString() + " of " + Configchunks.ToString() + "...";
                        GlobalSharedData.ServerStatus = "Config acknowledgment message recieved";

                    }
                }
         

        
                if (message[3] == 's' && message[6] == ']')
                {
                    ConfigStatus = "Device config read done...";
                    ConfigSendDone = true;
                    ParamsTransmitComplete = true;
                    ConfigSendStop = true;
                    ConfigSendReady = false;

                    //Thread.Sleep(20);
                    //GlobalSharedData.ServerMessageSend = WiFiconfig.HeartbeatMessage;
                    //GlobalSharedData.ServerStatus = "Config paramaters sent message recieved";
                }
          

        
                if (message[3] == 'e' && message[8] == ']')
                {
                    if (BitConverter.ToUInt16(message, 4) == 0xFFFF)
                    {
                        ConfigSentIndex = 0;
                        ConfigSentAckIndex = -1;
                        ConfigStatus = "Waiting for device, please be patient... " + ConfigSentAckIndex.ToString() + "...";
                        ConfigSendReady = true;
                    }
                    else
                    {
                        ConfigSentIndex = BitConverter.ToUInt16(message, 4);
                        //ConfigSentIndex--;
                        ConfigSentAckIndex = BitConverter.ToUInt16(message, 4);
                        ConfigStatus = "Waiting for device, please be patient... " + ConfigSentAckIndex.ToString() + "...";
                        Console.WriteLine("Error at Index" + ConfigSentIndex.ToString() + " ACK Index: " + ConfigSentAckIndex.ToString());
                    }

                }
           

      
                if (message[3] == 'x' && message[6] == ']')
                {
                    backBtner = true;
                }
            
                Console.WriteLine("Packet Index:" + ConfigSentIndex.ToString() + " ACK Index: " + ConfigSentAckIndex.ToString());
            }
            else
            {

            }
        }

        public static byte[] ParamReceiveBytes = new byte[800 * 4];

        public static void ConfigReceiveParamsParse(byte[] message, EndPoint endPoint)
        {
            if(ParamsRequestStarted)
            { 
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'p') && (message[3] == 'a'))
            {                
                ParamsRequestStarted = true;
            }
                if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'p') && (message[3] == 'D'))
                {
                    DataIndex = BitConverter.ToUInt16(message, 4);
                    TotalCount = BitConverter.ToUInt16(message, 6);

                    Array.Copy(message, 8, ParamReceiveBytes, (DataIndex - 1) * 512, 512);

                    ParamReceiveProgress = (DataIndex * 100) / TotalCount;

                    if (DataIndex < TotalCount && DataIndex > StoredIndex)
                    {
                        byte[] ParamsReceivechunk = Enumerable.Repeat((byte)0xFF, 10).ToArray();

                        ParamsReceivechunk[0] = (byte)'[';
                        ParamsReceivechunk[1] = (byte)'&';
                        ParamsReceivechunk[2] = (byte)'p';
                        ParamsReceivechunk[3] = (byte)'D';
                        ParamsReceivechunk[4] = (byte)'a';
                        ParamsReceivechunk[5] = message[4];
                        ParamsReceivechunk[6] = message[5];
                        ParamsReceivechunk[7] = 0;
                        ParamsReceivechunk[8] = 0;
                        ParamsReceivechunk[9] = (byte)']';

                        GlobalSharedData.ServerMessageSend = ParamsReceivechunk;
                        Console.WriteLine("DataIndex: " + DataIndex.ToString() + "of " + TotalCount.ToString() + " Indexes");

                    }
                    else if (DataIndex == TotalCount)
                    {
                        GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&ps00]");
                        Console.WriteLine("DataIndex: " + DataIndex.ToString() + "of " + TotalCount.ToString() + " Indexes");
                        ParamsReceiveComplete = true;
                    }

                    StoredIndex = DataIndex;
                }
            }
            else
            {

            }
        }

        private bool UpdateParametersFromDevice()
        {
            Int32 Value = 0;
            for (int i = 0; i < Parameters.Count; i++)
            {
                Value = ParamReceiveBytes[i * 4] | ParamReceiveBytes[(i * 4) + 1] << 8 | ParamReceiveBytes[(i * 4) + 2] << 16 | ParamReceiveBytes[(i * 4) + 3] << 24;

                if (parameters[i].Ptype == 1)
                {
                    if (i == 207)
                    {
                        Thread.Sleep(1);
                    }
                    if (parameters[i].CurrentValue > 1)
                    {
                        parameters[i].CurrentValue = parameters[i].DefaultValue;
                    }
                    else
                    {
                        parameters[i].CurrentValue = Value;
                    }
                }
                else if ((Value <= parameters[i].MaximumValue) && (Value >= parameters[i].MinimumValue))
                {
                    parameters[i].CurrentValue = Value;
                }
                else
                {
                    parameters[i].CurrentValue = parameters[i].DefaultValue;
                }
            }


            Disp_Parameters = ParametersToDisplay(parameters);

            parametrsGroup.GroupDescriptions.Remove(groupDescription);
            parametrsGroup.GroupDescriptions.Remove(SubgroupDescription);

            parametrsGroup = (CollectionView)CollectionViewSource.GetDefaultView(Disp_Parameters);

            parametrsGroup.GroupDescriptions.Add(groupDescription);
            parametrsGroup.GroupDescriptions.Add(SubgroupDescription);
            return true;
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
                        Console.WriteLine("====================Parameters sent done======================");
                        //WIFIcofig.ServerMessageSend = 
                        //BootReady = false;
                        break;
                    }
                }

            }
            //BootBtnEnabled = true;
            ConfigSendStop = false;
        }


       
        List<byte[]> ConfigSendList = new List<byte[]>();

        public static string ConfigStatus { get; set; }

        public static bool ConfigSendReady { get; set; }

        public static bool ConfigSendDone { get; set; }

        public static int ConfigPersentage { get; set; }

        public static int ConfigSentIndex { get; set; }

        public static bool ConfigSendStop { get; set; }

        public static bool bootContinue;

        public static int ConfigSentAckIndex { get; set; }

        
        static int Configchunks = 0;
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

        private void Save_ParaMetersToFile()
        {
            byte[] paraMeterBytes = new byte[parameters.Count * 4];
            //byte[] valuebytes = new byte [4]
            for (int i = 0; i < parameters.Count; i++)
            {
                //valuebytes = BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(parameters[i].CurrentValue),4));
                
                //Array.Copy(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(parameters[i].CurrentValue), 4)), 0, paraMeterBytes, i * 4, 4);
                Array.Copy(BitConverter.GetBytes(parameters[i].CurrentValue), 0, paraMeterBytes, i * 4, 4);
            }

            string hex = BitConverter.ToString(paraMeterBytes).Replace("-", string.Empty);
            string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\\\Saved Files\\Parameters" + "\\" + "Parameters.mer";
            File.WriteAllText(_savedFilesPath, hex);

            int fileChunck = 512;
            int bytesleft = 0;
            int ConfigfileSize = 0;
            bytesleft = ConfigfileSize = paraMeterBytes.Length;
            ConfigSendList.Clear();
            Configchunks = (int)Math.Round(ConfigfileSize / (double)fileChunck);
            int shifter = 0;
            for (int i = 0; i < Configchunks; i++)
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

        private void OpenParameterFile()
        {
            string _filename = "";
            byte[] _parameters = { 0 };
            string value = "";
            Microsoft.Win32.OpenFileDialog _openFileDialog = new Microsoft.Win32.OpenFileDialog();
            _openFileDialog.Filter = "Mernok Elektronik File (*.mer)|*.mer";

            if (_openFileDialog.ShowDialog() == true)
            {

                _filename = _openFileDialog.FileName;
            }

            using (StreamReader reader = new StreamReader(_filename))
            {
                value = reader.ReadToEnd();
            }
            _parameters = StringToByteArray(value);

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].CurrentValue = ((Int32)_parameters[(0 + (i * 4)) + 2]) + ((Int32)_parameters[(1 + (i * 4)) + 2] << 8) + ((Int32)_parameters[(2 + (i * 4)) + 2] << 16) + ((Int32)_parameters[(3 + (i * 4)) + 2] << 24);
            }

            Disp_Parameters = ParametersToDisplay(parameters);

            parametrsGroup.GroupDescriptions.Remove(groupDescription);
            parametrsGroup.GroupDescriptions.Remove(SubgroupDescription);

            parametrsGroup = (CollectionView)CollectionViewSource.GetDefaultView(Disp_Parameters);

            parametrsGroup.GroupDescriptions.Add(groupDescription);
            parametrsGroup.GroupDescriptions.Add(SubgroupDescription);
        }

        private void SaveParameterFile()
        {
            Microsoft.Win32.SaveFileDialog _saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            _saveFileDialog.DefaultExt = ".mer";
            _saveFileDialog.Filter = "Mernok Elektronik File (*.mer)|*.mer";
            string _filename = "Parameter.mer";
            _saveFileDialog.FileName = _filename.Remove(_filename.Length - 4, 4);
            _saveFileDialog.FilterIndex = 1;
            _saveFileDialog.RestoreDirectory = true;

            if (_saveFileDialog.ShowDialog() == true)
            {
                if (_saveFileDialog.FileName.Contains(".mer"))
                {
                    StreamWriter writer = new StreamWriter(_saveFileDialog.FileName);
                    int counter = 0;


                    byte[] paraMeterBytes = new byte[parameters.Count * 4];
                    byte[] valbytes = new byte[4];
                    Int32 valInt32 = new Int32();
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        valbytes = BitConverter.GetBytes(parameters[i].CurrentValue);
                        valInt32 = BitConverter.ToInt32(valbytes, 0);
                        if (i == 322)
                        {
                            Thread.Sleep(1);
                        }
                        //Array.Copy(BitConverter.GetBytes(BitConverter.ToInt32(valbytes, 0)), 0, paraMeterBytes, i * 4, 4);
                        Array.Copy(BitConverter.GetBytes(parameters[i].CurrentValue), 0, paraMeterBytes, i * 4, 4);
                    }

                    string hex = BitConverter.ToString(paraMeterBytes).Replace("-", string.Empty);
                    // write parameter file start bytes
                    writer.Write("5B26");

                    writer.Write(hex.ToCharArray());
                    //foreach (char b in hex)
                    //{
                    //    writer.Write(b);
                    //}

                    // write parameter file stop byte
                    writer.Write("5D");

                    // === dispose streamwrite ===
                    writer.Dispose();
                    // === close stramwrite ===
                    writer.Close();
                }
            }
        }

 

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static readonly Regex _regex = new Regex("[^0-9-]");

        private bool StringTestNum(string value)
        {
            return !_regex.IsMatch(value);
        }

        public static string IPAddressConditioner (string IPAddr)
        {
            string IP1 = "";
            string IP2 = "";
            string IP3 = "";
            string IP4 = "";
            string IP = "";
            int count = 0;
            Regex regexItem = new Regex("[^0-9.]");

            //first step is to make sure it is a valid IP address       
            // are there only numbers and full stops in the string
            // are there 3 full stops in the string
            // is the length of the IP address 15 or less

            if (((regexItem.IsMatch(IPAddr)) || ((count = IPAddr.Count(f => f == '.')) != 3)) || IPAddr.Length > 15)
            {
                return "000.000.000.000";
            }
          
            // if the string is in the correct format, check each sub-IP number and return a conditioned version

            IP1 = Convert.ToUInt16(IPAddr.Substring(0, IPAddr.IndexOf('.'))) > 255 ? "255" : Convert.ToUInt16(IPAddr.Substring(0, IPAddr.IndexOf('.'))).ToString("000");
            IPAddr = IPAddr.Remove(0, IPAddr.IndexOf('.')+1);
            IP2 = Convert.ToUInt16(IPAddr.Substring(0, IPAddr.IndexOf('.'))) > 255 ? "255" : Convert.ToUInt16(IPAddr.Substring(0, IPAddr.IndexOf('.'))).ToString("000");
            IPAddr = IPAddr.Remove(0, IPAddr.IndexOf('.')+1);
            IP3 = Convert.ToUInt16(IPAddr.Substring(0, IPAddr.IndexOf('.'))) > 255 ? "255" : Convert.ToUInt16(IPAddr.Substring(0, IPAddr.IndexOf('.'))).ToString("000");
            IPAddr = IPAddr.Remove(0, IPAddr.IndexOf('.')+1);
            IP4 = Convert.ToUInt16(IPAddr) > 255 ? "255" : Convert.ToUInt16(IPAddr).ToString("000");

            //IP = IP1 + "." + IP2 + "." + IP3 + "." + IP4;
            return IP1 + "." + IP2 + "." + IP3 + "." + IP4;
        }
       
      
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if ((string)ButtonBack.Content == "Back")
            {
                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                    GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&PX00]");
                //            else

                backBtner = true;
            }
            else
            {
                InfoDelay.Start();
                ButtonState(true);
                ParamsRequestStarted = false;
                ParamsSendStarted = false;
            }

        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            SureMessageVis = Visibility.Visible;
        }

        private static Thread ConfigureThread;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            ButtonState(false);
            Save_ParaMetersToFile();
            ConfigSendReady = false;
            ConfigSendStop = false;
            ParamsRequestStarted = false;
            //BootStart = true;
            ConfigSentIndex = 0;
            ConfigSentAckIndex = -1;
            updateDispatcherTimer.Start();
            if (ConfigureThread != null && ConfigureThread.IsAlive)
            {

            }
            else
            {
                ConfigureThread = new Thread(ConfigSendDo)
                {
                    IsBackground = true,
                    Name = "ConfigurationTransmitThread"
                };
                ConfigureThread.Start();
            }

            ConfigStatus = "Asking device to configure parameters...";
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&PP00]");

            updateDispatcherTimer.Start();
        }

        private void ButtonConfigRefresh_Click(object sender, RoutedEventArgs e)
        {
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&pP00]");
            StoredIndex = -1;
            ParamsRequestStarted = true;
            ParamsReceiveComplete = false;

            ParamsTransmitComplete = false;
            ParamsSendStarted = false;
            ButtonState(false);
            updateDispatcherTimer.Start();
            ParamReceiveProgress = 0;
        }

        void ButtonState(bool State)
        {
            if (State)
            {
                ButtonConfigRefresh.IsEnabled = true;
                SendFileButton.IsEnabled = true;
                OpenFileButton.IsEnabled = true;
                ButtonBack.Content = "Back";


            }
            else
            {
                ButtonConfigRefresh.IsEnabled = false;
                SendFileButton.IsEnabled = false;
                OpenFileButton.IsEnabled = false;
                ButtonBack.Content = "Cancel";
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            SureMessageVis = Visibility.Collapsed;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenParameterFile();
            SureMessageVis = Visibility.Collapsed;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveParameterFile();
            SureMessageVis = Visibility.Collapsed;
        }
     
        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ImageFilesView;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.AudioFilesView;
   
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

