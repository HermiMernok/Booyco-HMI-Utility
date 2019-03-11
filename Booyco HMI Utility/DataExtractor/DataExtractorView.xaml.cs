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
        public const int DATALOG_RX_SIZE = 8192;
        //public const int DATALOG_RX_SIZE = 4096;
        static bool _fileCreated = false;

        public static bool Heartbeat = false;
        static string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Saved Files";
        private DispatcherTimer updateDispatcherTimer;
        static string _newLogFilePath = "";

        private static int  DataLogProgress = 0;

        public static bool DataExtractorDone { get; set; }
        
        public static int DataExtractorAckIndex { get; set; }

        public static int DataIndex { get; set; }
        public static int TotalCount { get; set; }

        private int _heartBeatDelay = 0;

        static bool CancelCheck = false;
        static bool MovingLogs = false;

        public DataExtractorView()
        {
            InitializeComponent();
            Label_StatusView.Content = "Waiting instructions..";
            updateDispatcherTimer = new DispatcherTimer();
            updateDispatcherTimer.Tick += new EventHandler(InfoUpdater);
            updateDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
          
            DataLogProgress = 0;
            DataIndex = 0;
            TotalCount = 0;
            Button_ViewLogs.Visibility = Visibility.Hidden;

        }

        private void InfoUpdater(object sender, EventArgs e)
        {
            if (Visibility == Visibility.Visible && (WiFiconfig.clients.Count == 0 || WiFiconfig.clients.Where(t => t.Client.RemoteEndPoint.ToString() == WiFiconfig.SelectedIP).ToList().Count == 0))
            {
                WiFiconfig.ConnectionError = true;
                if (File.Exists(_newLogFilePath))
                {
                    File.Delete(_newLogFilePath);
                }
                Button_Back_Click(null, null);
               
            }
            if (MovingLogs)
            {
                Label_StatusView.Content = "Moving Logs to Flash";
             
            }
            else
            {
                ProgressBar_DataLogExtract.Value = DataLogProgress;

                Label_ProgressStatusPercentage.Content = "Overall progress: " + (DataLogProgress / 10).ToString() + "%";
                Label_StatusView.Content = "Datalog packet " + DataIndex.ToString() + " of " + TotalCount.ToString() + "...";
            }
         
            if (CancelCheck)
            {
                if (Heartbeat)
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
            else
            {
                if (!DataExtractorComplete)
                {


                    Label_ProgressStatusPercentage.Content = "Process Cancelled...";

                    if (File.Exists(_newLogFilePath))
                    {
                        File.Delete(_newLogFilePath);
                    }
                }
                else
                {
                    Label_ProgressStatusPercentage.Content = "File Completed...";
                    Button_ViewLogs.Visibility = Visibility.Visible;
                    GlobalSharedData.FilePath = _newLogFilePath;


                }
                DataLogProgress = 0;
                DataIndex = 0;
                Button_Extract.IsEnabled = true;
                Button_Back.Content = "Back";
                _fileCreated = false;
                ProgressBar_DataLogExtract.Value = 0;
                Label_StatusView.Content = "Waiting instructions..";
            }
           
        }
         


        

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
          
            if (CancelCheck)
            {
                CancelCheck = false;
                Button_Back.Content = "Back";
                StoredIndex = -1;
                //if (!DataExtractorComplete)
                //{


                //    Label_ProgressStatusPercentage.Content = "Process Cancelled...";

                //    if (File.Exists(_newLogFilePath))
                //    {
                //        File.Delete(_newLogFilePath);
                //    }
                //}

            }
            else
            {
                
                this.Visibility = Visibility.Collapsed;
                ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            }
        }
           

        private static Thread DataExtractorThread;
        private void Button_Extract_Click(object sender, RoutedEventArgs e)
        {
            Button_Extract.IsEnabled = false;
            Button_Back.Content = "Cancel";
            CancelCheck = true;
            MovingLogs = false;
            Button_ViewLogs.Visibility = Visibility.Hidden;           
          
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LL00]");
            Heartbeat = false;
            _heartBeatDelay = 0;
            StoredIndex = 0;

            if (!_fileCreated)
            {

                _newLogFilePath = _savedFilesPath + "\\DataLog_BooycoPDS_" + WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID.ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".Mer";
                int Filecount = 1;
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
                File.Create(_newLogFilePath).Dispose();
                _fileCreated = true;
            }

        }

        static int  StoredIndex = 0;
        static bool DataExtractorComplete = false;
        public static void DataExtractorSendParse(byte[] message, EndPoint endPoint)
        {
            if((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L') && (message[3] == 'k'))
            {            
                MovingLogs = true;
            }
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L') && (message[3] == 'a'))
            {
                MovingLogs = false;
                int test = 0;
            }
                if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L') && (message[3] == 'D'))
            {
                MovingLogs = false;
                DataIndex =  BitConverter.ToUInt16(message, 4);               
                TotalCount= BitConverter.ToUInt16(message, 6);

                DataLogProgress = (DataIndex * 1000) / TotalCount ;
           
               
                if(!CancelCheck)
                {                   
                    GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LDs00]");
                }

                else if (DataIndex < TotalCount && DataIndex > StoredIndex)
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
                    StoredIndex = -1;
                }
                else if (DataIndex == TotalCount)
                {
                    CancelCheck = false;
                
                    using (var stream = new FileStream(_newLogFilePath, FileMode.Append))
                    {
                        stream.Write(message, 8, DATALOG_RX_SIZE);
                    }
                    GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LDs00]");
                    DataExtractorComplete = true;
                    _fileCreated = false;
                
                }                   
                           
            }
            else
            {
                
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                Label_DeviceName.Content = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                Label_DeviceVID.Content = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                Label_Firmware.Content = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmRev + "." + WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmSubRev;
                Label_StatusView.Content = "Waiting instructions..";
                ProgressBar_DataLogExtract.Value = 0 ;
                Label_ProgressStatusPercentage.Content = "";
                StoredIndex = -1;
                _heartBeatDelay = 0;
                DataExtractorComplete = false;
                updateDispatcherTimer.Start();
            }
            else
            {
                updateDispatcherTimer.Stop();
                DataExtractorComplete = false;
                Button_ViewLogs.Visibility = Visibility.Hidden;
                Button_Extract.Content = "Extract";
                updateDispatcherTimer.Stop();
            }
            DataLogProgress = 0;
            DataIndex = 0;
        }

        private void Button_ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            DataLogProgress = 0;
            DataIndex = 0;
            if (DataExtractorComplete)
            {
                
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogView;
            }
        }
    }
}
