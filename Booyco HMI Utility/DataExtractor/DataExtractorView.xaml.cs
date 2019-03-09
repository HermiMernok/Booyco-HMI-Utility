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
        static bool _fileCreated = false;
        static string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Saved Files";
        private DispatcherTimer updateDispatcherTimer;
        static string _newLogFilePath = "";

        private static int  DataLogProgress = 0;

        public static bool DataExtractorDone { get; set; }
        
        public static int DataExtractorAckIndex { get; set; }

        public static int DataIndex { get; set; }
        public static int TotalCount { get; set; }

        static bool CancelCheck = false;


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

        }

        private void InfoUpdater(object sender, EventArgs e)
        {
            ProgressBar_DataLogExtract.Value = DataLogProgress;
            Label_ProgressStatusPercentage.Content = "Overall progress: " + (DataLogProgress/10).ToString() + "%";
            Label_StatusView.Content = "Datalog packet " + DataIndex.ToString() + " of " + TotalCount.ToString() + "...";

            if (CancelCheck)
            {
                Button_Extract.IsEnabled = false;
                Button_Back.Content = "Cancel";

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
                    Button_Extract.Content = "View Logs";
                    GlobalSharedData.FilePath = _newLogFilePath;
         
                }
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
            if (DataExtractorComplete)
            {
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogView;
            }
            else
            {
                updateDispatcherTimer.Start();
                //BootStatus = "Asking device to boot...";
                GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LL00]");
            }

          
        }

        static int  StoredIndex = -1;
        static bool DataExtractorComplete = false;
        public static void DataExtractorSendParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L') && (message[3] == 'a'))
            {
                int test = 0;
            }
                if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L') && (message[3] == 'D'))
            {
                DataIndex =  BitConverter.ToUInt16(message, 4);               
                TotalCount= BitConverter.ToUInt16(message, 6);

                DataLogProgress = (DataIndex * 1000) / TotalCount ;

           
                if (DataIndex == 1 && !_fileCreated)
                {
                    CancelCheck = true;
                 

                    _newLogFilePath = _savedFilesPath + "\\DataLog_BooycoPDS_"+ WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID.ToString() +"_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".Mer";
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

                        if(Filecount >= 10)
                        {
                            break;
                        }

                    }        
                        File.Create(_newLogFilePath).Dispose();
                    _fileCreated = true;
                }
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
                    StoredIndex = DataIndex;                
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
            }
            else
            {
                updateDispatcherTimer.Stop();
                DataExtractorComplete = false;
                Button_Extract.Content = "Extract";
            }
        }
    }
}
