using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for DataExtractor.xaml
    /// </summary>
    public partial class DataExtractorView : UserControl
    {
      
        // === Public Variables ===
        public const int DATALOG_RX_SIZE = 8192;
        public static bool Heartbeat = false;
        public static int DataIndex { get; set; }
        static bool _fileCreated = false;
        public static int TotalCount { get; set; }
        private uint SelectVID = 0;

        // === Static Variables ===
        static bool DatalogsBusy = false;
        static bool MovingLogs = false;
        static string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Saved Files";
        static string _newLogFilePath = "";
        static int StoredIndex = 0;
        static bool DataExtractorComplete = false;
        bool ConnectionLost = false;
        private DispatcherTimer updateDispatcherTimer;       
        private static int  DataLogProgress = 0;       
        private uint _heartBeatDelay = 0;
        static bool StartDataReceiving = false;
        /// <summary>
        /// DataExtractorView: The constructor function
        /// Setup required variables 
        /// </summary>
        public DataExtractorView()
        {           
            InitializeComponent();
            Label_StatusView.Content = "Waiting for user command..";
            updateDispatcherTimer = new DispatcherTimer();
            updateDispatcherTimer.Tick += new EventHandler(InfoUpdater);
            updateDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
          
            DataLogProgress = 0;
            DataIndex = 0;
            TotalCount = 0;
            Button_ViewLogs.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// InfoUpdater
        /// Update screen information based on set interval by means of a timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoUpdater(object sender, EventArgs e)
        {
            
            if (ConnectionLost)
            {
              
                TCPclientR _foundTCPClient = WiFiconfig.TCPclients.FirstOrDefault(t => t.VID == SelectVID);
                if (_foundTCPClient != null)
                {
                    WiFiconfig.SelectedIP = _foundTCPClient.IP;                  
                 
                    //Button_Extract.IsEnabled = true;
                    ConnectionLost = false;
                    WiFiconfig.BusyReconnecting = false;
                }
                else
                {                    
                   
                    //Button_Extract.IsEnabled = false;
                }
            }
            else
            {
                
                // === if device disconnect delete half complete log file and go back to view connections ===
                if (Visibility == Visibility.Visible && (WiFiconfig.clients.Count == 0 || WiFiconfig.clients.Where(t => t.Client.RemoteEndPoint.ToString() == WiFiconfig.SelectedIP).ToList().Count == 0))
                {
                    WiFiconfig.BusyReconnecting = true;
                    // WiFiconfig.ConnectionError = true;
                    //if (File.Exists(_newLogFilePath) && !DataExtractorComplete)
                    //{
                    //    File.Delete(_newLogFilePath);
                    //}
                    DataExtractorComplete = false;
                    Button_Extract.Content = "Extract";
                   // DatalogsBusy = false;
                   // Button_Back.Content = "Back";
                   // StoredIndex = -1;
                    ConnectionLost = true;
                    // Button_Back_Click(null, null);               
                }
                // === check if datalogs started, busy with extraction ===
                if (DatalogsBusy)
                {
                    // === check if datalogs are busy moving ===
                    if (MovingLogs)
                    {
                        ProgressBar_DataLogExtract.Value = DataLogProgress;
                        Label_ProgressStatusPercentage.Content = (DataLogProgress / 10).ToString() + "%";
                        Label_StatusView.Content = "Moving Logs to Flash";
                    }
                    // === else the datalogs are being received ===
                    else
                    {
                        ProgressBar_DataLogExtract.Value = DataLogProgress;
                        Label_ProgressStatusPercentage.Content = (DataLogProgress / 10).ToString() + "%";
                        Label_StatusView.Content = "Datalog packet " + DataIndex.ToString() + " of " + TotalCount.ToString() + "...";
                    }

                    // === check if heartbeat received ===
                    if (Heartbeat)
                    {
                       
                        if (StartDataReceiving)
                        {
                            _heartBeatDelay++;

                            if (_heartBeatDelay > 15)
                            {
                                GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LC00]");
                                // === clear heartbeat information and start over ===
                                Heartbeat = false;
                                _heartBeatDelay = 0;
                            
                                   Console.WriteLine("DataIndex: " + DataIndex.ToString());

                            }
                        }
                        else
                        {                           
                            _heartBeatDelay++;

                            if (_heartBeatDelay > 15)
                            {
                                Heartbeat = false;
                                _heartBeatDelay = 0;
                                GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LL00]");
                            }
                        }
                        
                    }
                }

                // === else no datalogs are being extracted ===
                else
                {
                    if (Heartbeat)
                    {
                        Button_Extract.IsEnabled = true;
                    }
                        // === if the extrection failed ===
                        if (!DataExtractorComplete)
                    {
                        
                        // == clear information, show failed message, and delete half downloaded log file ===
                        Label_ProgressStatusPercentage.Content = "";

                        if (File.Exists(_newLogFilePath))
                        {
                            Label_ProgressStatusPercentage.Content = "Process Cancelled...";
                            File.Delete(_newLogFilePath);
                        }
                    }
                    // === else the extraction was successful ===
                    else
                    {
                        // === update information and save filepath for file view ===
                        Label_ProgressStatusPercentage.Content = "File Completed...";
                        Button_ViewLogs.Visibility = Visibility.Visible;
                        GlobalSharedData.FilePath = _newLogFilePath;
                    }

                    // === infromation to be cleared regardless if the logs extraction status ===
                    DataLogProgress = 0;
                    DataIndex = 0;
                
                    Button_Back.Content = "Back";
                    _fileCreated = false;
                    StartDataReceiving = false;
                    ProgressBar_DataLogExtract.Value = 0;
                    Label_StatusView.Content = "Waiting for user command..";
                }
            }

            ConnectionInfoUpdate();
        }
         
        /// <summary>
        /// Button_Back_Click: Back Button click event
        /// Close view and open source(previous) view 
        /// Or
        /// Cancel datalog extraction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {   
            // === if datalogs are busy extracting ===
            if (DatalogsBusy)
            {
                // Cancel datalogs and change text from cancel to back
                DatalogsBusy = false;
                Button_Back.Content = "Back";
                GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LDs00]");
                StartDataReceiving = false;
                StoredIndex = -1;
               
            }
            // === else if datalogs are not busy extracting ===
            else            
            {
                // === Close view and open source(previous) view ===
                this.Visibility = Visibility.Collapsed;
                ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            }
        }

        /// <summary>
        /// Button_Extract_Click: Extract Button click event
        /// Start Extracting datalogs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Extract_Click(object sender, RoutedEventArgs e)
        {  
            // === Update information and buttons ===
            Button_Extract.IsEnabled = false;
            Button_Back.Content = "Cancel";
            DatalogsBusy = true;
            MovingLogs = false;
            StartDataReceiving = false;
            Button_ViewLogs.Visibility = Visibility.Hidden;

            // === clear heartbeat information ===
            Heartbeat = false;
            _heartBeatDelay = 0;
            StoredIndex = 0;
            
            // === Send start datalog informaiton ===
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LL00]");
         
            // === Check if log file is created ===
            if (!_fileCreated)
            {
                // === Create log file name ===
                _newLogFilePath = _savedFilesPath + "\\DataLog_BooycoPDS_" + WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID.ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".Mer";
                int Filecount = 1;

                // === append a number if the log file already exists ===
                while (File.Exists(_newLogFilePath))
                {
                    if (Filecount > 1)
                    {
                        _newLogFilePath.Remove(_newLogFilePath.Length - 1);
                        _newLogFilePath += Filecount;
                    }
                    else
                    {
                        _newLogFilePath += Filecount;
                    }

                    if (Filecount >= 10)
                    {
                        break;
                    }
                }

                // === Create Log file ===
                File.Create(_newLogFilePath).Dispose();
                _fileCreated = true;
            }
        }

         /// <summary>
         ///  DataExtractorParser
         ///  Parse the Wifi received information from the unit
         /// </summary>
         /// <param name="message"></param>
         /// <param name="endPoint"></param>
        public static void DataExtractorParser(byte[] message, EndPoint endPoint)
        {
            // === Check if device is busy moving logs ===
            if((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L') && (message[3] == 'k'))
            {               
                DataLogProgress = (50 * message[4]) / message[5];
                Console.WriteLine(message[4].ToString() + "-" + message[5].ToString() + "-" + DataLogProgress.ToString());
                if (!StartDataReceiving)
                {
                    MovingLogs = true;
                }
            }

            // === Check if device is ready to receive ===
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L') && (message[3] == 'a'))
            {
               
                MovingLogs = false;        
            }

            // === Check if device is ready to receive ===
            if ((message.Length >= 9) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L') && (message[3] == 'c'))
            {
              
                byte[] StoredIndexBytes = BitConverter.GetBytes(StoredIndex);
                byte[] Logchunk = Enumerable.Repeat((byte)0xFF, 10).ToArray();
                Logchunk[0] = (byte)'[';
                Logchunk[1] = (byte)'&';
                Logchunk[2] = (byte)'L';
                Logchunk[3] = (byte)'D';
                Logchunk[4] = (byte)'a';
                Logchunk[5] = StoredIndexBytes[0];
                Logchunk[6] = StoredIndexBytes[1];
                Logchunk[7] = 0;
                Logchunk[8] = 0;
                Logchunk[9] = (byte)']';

                GlobalSharedData.ServerMessageSend = Logchunk;
            }

            // === Check if device is ready to send datalog file ===
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L') && (message[3] == 'D'))
            {
                MovingLogs = false;
                StartDataReceiving = true;
                DataIndex =  BitConverter.ToUInt16(message, 4);               
                TotalCount= BitConverter.ToUInt16(message, 6);
                DataLogProgress = (DataIndex * 850) / TotalCount+50;
               
                // === check if datalog extraction has not started ===
                if (!DatalogsBusy)
                {                   
                    // === Send  stop request ===
                    GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LDs00]");
                }

                //else if (DataIndex < TotalCount && DataIndex > StoredIndex) 
                // === Check if received packet number is one more than the stored previous packet number ===
                if (DataIndex == (StoredIndex+1))
                {                
                    
                    using (var stream = new FileStream(_newLogFilePath, FileMode.Append))
                    {
                        stream.Write(message, 8, DATALOG_RX_SIZE);
                    }
                    
                    byte[] Logchunk = Enumerable.Repeat((byte)0xFF, 10).ToArray();
                    
                    Logchunk[0] = (byte)'[';
                    Logchunk[1] = (byte)'&';
                    Logchunk[2] = (byte)'L';
                    Logchunk[3] = (byte)'D';
                    Logchunk[4] = (byte)'a';
                    Logchunk[5] = message[4];
                    Logchunk[6] = message[5];
                    Logchunk[7] = 0;
                    Logchunk[8] = 0;
                    Logchunk[9] = (byte)']';

                    GlobalSharedData.ServerMessageSend = Logchunk;
                    Console.WriteLine("DataIndex: " + DataIndex.ToString());
                    StoredIndex = DataIndex;

                }
                // === Check if received packet number is the same as the total packet count ===
                else if (DataIndex == TotalCount)
                {
                    DatalogsBusy = false;

                    using (var stream = new FileStream(_newLogFilePath, FileMode.Append))
                    {
                        stream.Write(message, 8, DATALOG_RX_SIZE);
                    }
                    GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LDs00]");
                    DataExtractorComplete = true;
                    _fileCreated = false;
                }
                // === Check if the received packet number is the same as the stored previous packet number === 
                else if(DataIndex == StoredIndex)
                {

                    byte[] Logchunk = Enumerable.Repeat((byte)0xFF, 10).ToArray();
                    Logchunk[0] = (byte)'[';
                    Logchunk[1] = (byte)'&';
                    Logchunk[2] = (byte)'L';
                    Logchunk[3] = (byte)'D';
                    Logchunk[4] = (byte)'a';
                    Logchunk[5] = message[4];
                    Logchunk[6] = message[5];
                    Logchunk[7] = 0;
                    Logchunk[8] = 0;
                    Logchunk[9] = (byte)']';

                    GlobalSharedData.ServerMessageSend = Logchunk;
                    Console.WriteLine("DataIndex: " + DataIndex.ToString());
                   // StoredIndex = -1;
                }
            
                else
                {
                    
                    byte[] StoredIndexBytes = BitConverter.GetBytes(StoredIndex);
                    byte[] Logchunk = Enumerable.Repeat((byte)0xFF, 10).ToArray();
                    Logchunk[0] = (byte)'[';
                    Logchunk[1] = (byte)'&';
                    Logchunk[2] = (byte)'L';
                    Logchunk[3] = (byte)'D';
                    Logchunk[4] = (byte)'a';
                    Logchunk[5] = StoredIndexBytes[0];
                    Logchunk[6] = StoredIndexBytes[1];
                    Logchunk[7] = 0;
                    Logchunk[8] = 0;
                    Logchunk[9] = (byte)']';

                    GlobalSharedData.ServerMessageSend = Logchunk;
                }
            }           
        }

        /// <summary>
        /// UserControl_IsVisibleChanged: DataExtractorView isVisibleChanged event
        /// display/retreive information, start/stop timer according to visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // === Check if DataExtractorView is visible ===
            if (this.Visibility == Visibility.Visible)
            {
                WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
         
                Label_DeviceName.Content = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                Label_DeviceVID.Content = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                SelectVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                Label_Firmware.Content = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmRev + "." + WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmSubRev;
                Label_StatusView.Content = "Waiting for user command..";
                ProgressBar_DataLogExtract.Value = 0 ;
                Label_ProgressStatusPercentage.Content = "";
                StoredIndex = -1;
                _heartBeatDelay = 0;
                DataExtractorComplete = false;
                _fileCreated = false;
                updateDispatcherTimer.Start();
                ConnectionLost = false;
                StartDataReceiving = false;

            }
            // === else Dataextractor is not visible ===
            else
            {
                
                SelectVID = 0;
                DataExtractorComplete = false;
                Button_ViewLogs.Visibility = Visibility.Hidden;
                Button_Extract.Content = "Extract";
                updateDispatcherTimer.Stop();
            }
            DataLogProgress = 0;
            DataIndex = 0;
        }

        /// <summary>
        /// Button_ViewLogs_Click: ViewLogs Button event
        /// Open the Datalogview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            // === Clear information ===
            DataLogProgress = 0;
            DataIndex = 0;

            // === If datalogs were successfull ===
            if (DataExtractorComplete)
            {     
                // === Open Datalogview ===
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogView;
            }
        }

        void ConnectionInfoUpdate()
        {
            if(ConnectionLost)
            {
                UserControlSpinnerLoad_Disconnect.Visibility = Visibility.Visible;
                // Label_Lock.Visibility = Visibility.Visible;
                Label_DeviceConnection.Foreground = new SolidColorBrush(Colors.Red);
                Label_DeviceConnection.Content = "Disconnected";
            }
            else
            {
                Label_DeviceConnection.Content = "Active";
                Label_DeviceConnection.Foreground = new SolidColorBrush(Colors.Green);
                UserControlSpinnerLoad_Disconnect.Visibility = Visibility.Collapsed;
               // Label_Lock.Visibility = Visibility.Collapsed;
            }
        }
    }
}
