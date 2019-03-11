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
        public CollectionViewSource parametersGroup = new CollectionViewSource();
        GeneralFunctions generalFunctions;
        private static bool backBtner = false;

        private DispatcherTimer updateDispatcherTimer;

        private DispatcherTimer dispatcherTimer;

        static int StoredIndex = -1;
        static bool ParamsReceiveComplete = false;
        static bool ParamsRequestStarted = false;
        static bool ParamsTransmitComplete = false;
        static bool ParamsSendStarted = false;
        private static int ParamReceiveProgress = 0;
        private static int ParamTransmitProgress = 0;
        public static int DataIndex { get; set; }
        public static int TotalCount { get; set; }

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

            updateDispatcherTimer = new DispatcherTimer();
            updateDispatcherTimer.Tick += new EventHandler(ConfigReceiveParams);
            updateDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        private void ConfigReceiveParams(object sender, EventArgs e)
        {
            if (ParamsRequestStarted)
            {
                ConfigSendReady = false;
                ConfigSendDone = false;
                ProgressBar_Params.Value = ParamReceiveProgress;
                Label_ProgressStatusPercentage.Content = "Overall progress: " + (ParamReceiveProgress).ToString() + "%";
                Label_StatusView.Content = "Loading parameters from device: Packet " + DataIndex.ToString() + " of " + TotalCount.ToString() + "...";
                if (ParamsReceiveComplete)
                {
                    UpdateParametersFromDevice();
                    updateDispatcherTimer.Stop();
                    ParamsReceiveComplete = false;
                }
            }
            else if (ConfigSendReady)
            {
                ParamsRequestStarted = false;
                ParamsReceiveComplete = false;
                ProgressBar_Params.Value = ParamTransmitProgress;
                Label_ProgressStatusPercentage.Content = "Overall progress: " + (ParamTransmitProgress).ToString() + "%";
                Label_StatusView.Content = "Loading parameters to device: Packet " + ConfigSentIndex.ToString() + " of " + Configchunks.ToString() + "...";

                if (ConfigSendDone)
                {
                    ConfigSendDone = false;
                    updateDispatcherTimer.Stop();
                }

            }
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible) //when the view is opened
            {
                GetDefaultParametersFromFile();
                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                    WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;

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
                //ConfigSendReady = false;
            }

            if(backBtner)
            {
                backBtner = false;
                ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
                this.Visibility = Visibility.Collapsed;
                ConfigSendStop = true;
            }

            //if (bootchunks > 0 && !BootDone && BootFlashPersentage > 0)
            //{
            //    BootloadingProgress.Value = (BootSentIndex + BootFlashPersentage) / ((double)bootchunks + 100) * 1000;
            //    if (BootSentIndex > 0)
            //        BootFlashPersentage = 100;
            //}
            //else
            //    BootloadingProgress.Value = 0;

            //BootStatusView = BootStatus;
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
            
            //parametersGroup.Source = Disp_Parameters;                     
            //parametersGroup.GroupDescriptions.Add(groupDescription);
            //parametersGroup.GroupDescriptions.Add(SubgroupDescription);

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
            string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Saved Files" + "\\" + "Parameters.mer";
            File.WriteAllText(_savedFilesPath, hex);

            int fileChunck = 512;
            int bytesleft = 0;
            int ConfigfileSize = 0;
            bytesleft = ConfigfileSize = paraMeterBytes.Length;
            ConfigSendList.Clear();
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
            if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
               GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&PX00]");
            //            else

            backBtner = true;
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private static Thread ConfigureThread;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Save_ParaMetersToFile();

            //BootStart = true;
            ConfigSendReady = false;
            ConfigSendStop = false;
            ConfigSentIndex = 0;
            ConfigSentAckIndex = -1;
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

            ConfigStatus = "Asking device to configure parameters...";
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&PP00]");

            updateDispatcherTimer.Start();
            ConfigSendReady = true;
            ConfigSendDone = false;
            ParamsRequestStarted = false;
            ParamsReceiveComplete = false;
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
                ParametersDisplay tempPar = (ParametersDisplay)DGparameters.SelectedItem;

                var SortedIndex = tempPar.OriginIndx;
                               
                int DisplayIndex = DGparameters.SelectedIndex;
                //var sortedParameterList = parametersGroup.View.OfType<ParametersDisplay>().ToList();
                //var SortedIndex = sortedParameterList[DisplayIndex].OriginIndx;

                if (parameters[SortedIndex].CurrentValue > parameters[SortedIndex].MinimumValue)
                {
                    parameters[SortedIndex].CurrentValue--;
                }
                else if (parameters[SortedIndex].CurrentValue == parameters[SortedIndex].MinimumValue)
                {
                    parameters[SortedIndex].CurrentValue = parameters[SortedIndex].MaximumValue;
                }

                Disp_Parameters = ParametersToDisplay(parameters);

                //parametrsGroup.GroupDescriptions.Remove(groupDescription);
                //parametrsGroup.GroupDescriptions.Remove(SubgroupDescription);

                //parametrsGroup = (CollectionView)CollectionViewSource.GetDefaultView(Disp_Parameters);

                //parametrsGroup.GroupDescriptions.Add(groupDescription);
                //parametrsGroup.GroupDescriptions.Add(SubgroupDescription);
            }
        }

        private void max_Button_Click(object sender, RoutedEventArgs e)
        {

            if (DGparameters.SelectedIndex != -1)
            {
                ParametersDisplay tempPar = (ParametersDisplay)DGparameters.SelectedItem;
                var SortedIndex = tempPar.OriginIndx;

                //var sortedParameterList = parametersGroup.View.OfType<ParametersDisplay>().ToList();
                //var SortedIndex = sortedParameterList[DisplayIndex].OriginIndx;

                int DisplayIndex = DGparameters.SelectedIndex;
                               
                if (parameters[SortedIndex].CurrentValue < parameters[SortedIndex].MaximumValue)
                {
                    parameters[SortedIndex].CurrentValue++;
                }
                else if (parameters[SortedIndex].CurrentValue == parameters[SortedIndex].MaximumValue)
                {
                    parameters[SortedIndex].CurrentValue = parameters[SortedIndex].MinimumValue;
                }

                Disp_Parameters[SortedIndex] = DisplayParameterUpdate(parameters[SortedIndex], SortedIndex);

                ////Disp_Parameters = ParametersToDisplay(parameters);

                //parametrsGroup.GroupDescriptions.Remove(groupDescription);
                //parametrsGroup.GroupDescriptions.Remove(SubgroupDescription);

                //parametrsGroup = (CollectionView)CollectionViewSource.GetDefaultView(Disp_Parameters);

                //parametrsGroup.GroupDescriptions.Add(groupDescription);
                //parametrsGroup.GroupDescriptions.Add(SubgroupDescription);

                //DGparameters.SelectedIndex = DisplayIndex;
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
            //Maak die reg
            //if (DGparameters.SelectedIndex != -1 && parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].Ptype == 4)
            //{
            //    TextBox textBox = (TextBox)sender;
            //    textBox.Text = generalFunctions.StringConditioner(textBox.Text);
            //    textBox.SelectionStart = textBox.Text.Length;
            //    String str = textBox.Text;
            //    byte[] NameBytes = new byte[20];

            //    NameBytes = Encoding.ASCII.GetBytes(textBox.Text);
            //    for (int i = 19; i > 0; i--)
            //    {
            //        if((19-i) < NameBytes.Length)
            //            parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx-i].CurrentValue = NameBytes[19-i];
            //        else
            //            parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx - i].CurrentValue = (byte)' ';
            //    }

            //}
            //else if(DGparameters.SelectedIndex != -1 && parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].Ptype == 0)
            //{
            //    TextBox textBox = (TextBox)sender;
            //    textBox.Text = generalFunctions.StringNumConditioner(textBox.Text);
            //    textBox.SelectionStart = textBox.Text.Length;
            //    parameters[Disp_Parameters[DGparameters.SelectedIndex].OriginIndx].CurrentValue = Convert.ToInt32(Disp_Parameters[DGparameters.SelectedIndex].Value);
            //}
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

        #endregion

        #region Config Send properties
        List<byte[]> ConfigSendList = new List<byte[]>();

        public static string ConfigStatus { get; set; }

        public static bool ConfigSendReady { get; set; }

        public static bool ConfigSendDone { get; set; }

        public static int ConfigPersentage { get; set; }

        public static int ConfigSentIndex { get; set; }

        public static bool ConfigSendStop { get; set; }

        public static bool bootContinue;

        public static int ConfigSentAckIndex { get; set; }

        #endregion

        public static void ConfigSendParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'P'))
            {

                #region Configure ready to start
                if (message[3] == 'a' && message[6] == ']')
                {                   
                    ConfigSendReady = true;
                    ConfigSendStop = false;
                    ConfigSentIndex = 0;
                    ConfigSentAckIndex = -1;
                    Thread.Sleep(2);
                    ConfigStatus = "Device ready to configure...";
                    GlobalSharedData.ServerStatus = "Config ready message recieved";
                    GlobalSharedData.BroadCast = false;
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
                        //ConfigStatus = "Device receiving packet " + ConfigSentAckIndex.ToString() + " of " + bootchunks.ToString() + "...";
                        GlobalSharedData.ServerStatus = "Config acknowledgment message recieved";

                    }
                }
                #endregion

                #region Configure complete message
                if (message[3] == 's' && message[6] == ']')
                {
                    ConfigStatus = "Device config read done...";
                    ConfigSendDone = true;
                    ConfigSendStop = true;
                    //Thread.Sleep(20);
                    //GlobalSharedData.ServerMessageSend = WiFiconfig.HeartbeatMessage;
                    //GlobalSharedData.ServerStatus = "Config paramaters sent message recieved";
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
                        ConfigSendReady = true;
                    }
                    else
                    {
                        //ConfigSentIndex = BitConverter.ToUInt16(message, 4);
                        ConfigSentIndex--;
                        //ConfigSentAckIndex = BitConverter.ToUInt16(message, 4);
                        ConfigStatus = "Waiting for device, please be patient... " + ConfigSentAckIndex.ToString() + "...";
                        Console.WriteLine("Error at Index" + ConfigSentIndex.ToString() + " ACK Index: " + ConfigSentAckIndex.ToString());
                    }

                }
                #endregion

                #region Configure Exit message
                if (message[3] == 'x' && message[6] == ']')
                {
                    backBtner = true;
                }
                #endregion

                ParamTransmitProgress = (ConfigSentIndex * 100) / Configchunks;

            }
            else
            {
                
            }
        }

        public static byte[] ParamReceiveBytes = new byte[800 * 4];

        public static void ConfigReceiveParamsParse(byte[] message, EndPoint endPoint)
        {        
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'p') && (message[3] == 'a'))
            {
                int test = 0;
            }
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'p') && (message[3] == 'D'))
            {
                DataIndex = BitConverter.ToUInt16(message, 4);
                TotalCount = BitConverter.ToUInt16(message, 6);

                Array.Copy(message, 8, ParamReceiveBytes, (DataIndex-1) * 512, 512);

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
                if ((Value < parameters[i].MaximumValue) && (Value > parameters[i].MinimumValue))
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

        private void ConfigRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigSendReady = false;
            ConfigSendDone = false;
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&pP00]");
            StoredIndex = -1;
            ParamsRequestStarted = true;
            ParamsReceiveComplete = false;
            updateDispatcherTimer.Start();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                ParamsRequestStarted = false;
                ParamsReceiveComplete = false;
                ConfigSendReady = false;
                ConfigSendDone = false;
                Label_StatusView.Content = "Waiting for user command..";
                ProgressBar_Params.Value = 0;
                Label_ProgressStatusPercentage.Content = "";
            }
            else
            {
                updateDispatcherTimer.Stop();

            }
        }
    }
}

