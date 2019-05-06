using Booyco_HMI_Utility.CustomObservableCollection;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for AudioFilesView.xaml
    /// </summary>
    public partial class AudioFilesView : UserControl
    {
        string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Saved Files\\Audio";
        static private RangeObservableCollection<AudioEntry> AudioFileList;
        private uint SelectVID = 0;
        static private UInt16 TotalAudioFiles = 0;
        private static int ReceivedAudioFileNumber = 0;
        private static UInt16 CurrentAudioFileNumber = 1;
        private static UInt16 PreviousAudioFileNumber = 1;
        static private UInt16 TotalCount = 0;
        static bool SendingComplete = false;
        static bool SendingStarted = false;
        static int TotalSize = 522;
        static int DataSize = TotalSize - 14;
   

        DispatcherTimer updateDispatcherTimer;
        public AudioFilesView()
        {
            InitializeComponent();
            AudioFileList = new RangeObservableCollection<AudioEntry>();
            DataGridAudioFiles.AutoGenerateColumns = false;
            DataGridAudioFiles.ItemsSource = AudioFileList;
     
            ReadAudioFiles();
            StoreAudioFile(1);

            updateDispatcherTimer = new DispatcherTimer();
            updateDispatcherTimer.Tick += new EventHandler(InfoUpdater);
            updateDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        void InfoUpdater(object sender, EventArgs e)
        {
            if(SendingStarted)
            {
                ProgressBar_AudioFiles.Maximum = TotalAudioFiles * 100;
                if (TotalCount > 0)
                {
                    ProgressBar_AudioFiles.Value = (int)((DataIndex * 100) / TotalCount) + (CurrentAudioFileNumber - 1) * 100;
                }
            }
           
        }

        void ReadAudioFiles()
        {
            AudioFileList.Clear();
            if (Directory.Exists(_savedFilesPath))
            {
                DirectoryInfo d = new DirectoryInfo(_savedFilesPath);               
                FileInfo[] FilesWav = d.GetFiles("*.wav");                     

                ushort count = 0;
                foreach (FileInfo file in FilesWav)
                {
                     count++;

                    AudioFileList.Add(new AudioEntry
                    {
                        ID = count,
                        FileName = file.Name,
                        DateTimeCreated = file.CreationTime.ToString("yyyy-MM-dd HH-mm-ss"),
                        Path = file.FullName,
                        Size = file.Length,
                        Progress = 0
                    });                 
                }
                TotalAudioFiles = count;
            }
            else
            {
                Directory.CreateDirectory(_savedFilesPath);
            }
        }
        
        static bool ValidAudioSizes(RangeObservableCollection<AudioEntry> DeviceAudioFileList)
        {
            if(DeviceAudioFileList.Count == AudioFileList.Count)
            {
                int index = 0;
                foreach(AudioEntry item in DeviceAudioFileList)
                {                    
                    if(item.Size != AudioFileList.ElementAt(index).Size)
                    {
                        return false;
                    }
                    index++;
                }
                return true;
            }
            return false;
        }
        static UInt16 TotalFilesCount = 0;
        static UInt16 DataIndex = 0;
        static bool AudioFileRequestStarted = false;
        public static void AudioFileSendParse(byte[] message, EndPoint endPoint)
        {
            if (TotalCount > 0)
            {
               

                if(PreviousAudioFileNumber < CurrentAudioFileNumber)
                {
                    AudioFileList.ElementAt(PreviousAudioFileNumber - 1).Progress = 100;
                    PreviousAudioFileNumber = CurrentAudioFileNumber;
                }
                else
                {
                    AudioFileList.ElementAt(CurrentAudioFileNumber - 1).Progress = (int)((DataIndex * 100) / TotalCount);
                }
               
            }
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'A'))
            {
                // === Check if it is Ready packet. The device is ready ===             
                if ((message[3] == 'a') &&  (message[521] == ']'))
                {
                    AudioFileRequestStarted = true;
                    TotalFilesCount = BitConverter.ToUInt16(message, 4);

                    RangeObservableCollection<AudioEntry> DeviceAudioFileList = new RangeObservableCollection<AudioEntry>();

                    for (int i = 0; i < TotalFilesCount; i++)
                    {
                        DeviceAudioFileList.Add(new AudioEntry
                        {
                            Size = (long)BitConverter.ToUInt32(message, 6 + i * 4)
                        });
                    }

                    // ===TODO: do something with return value ===
                    ValidAudioSizes(DeviceAudioFileList);

                    SendAudioFile(CurrentAudioFileNumber);

                    Console.WriteLine("Audio - Received Packet ready");
                }
                               
                // === Check if it is an acknowdlegement packet. The device has received the previous packet, and is ready to receive next chunk ===
                else if ((message[3] == 'D') && (message[4] == 'a') && (message[11] == ']'))
                {
                    DataIndex = BitConverter.ToUInt16(message, 5);
                    DataIndex++;
                    ReceivedAudioFileNumber = BitConverter.ToUInt16(message, 7);
                    SendAudioFile(ReceivedAudioFileNumber);
                    Console.WriteLine("Audio - Ready to receive next chunk -" + DataIndex.ToString() + ":" + TotalCount.ToString() + "-" + CurrentAudioFileNumber.ToString());
                }

                // === Check if it is an error packet. The device request previous packet to be sent again ===
                else if((message[3] == 'e')  && (message[10] == ']'))
                {
                    DataIndex = BitConverter.ToUInt16(message, 4);
                    ReceivedAudioFileNumber = BitConverter.ToUInt16(message, 6);
                    SendAudioFile(ReceivedAudioFileNumber);
                }

                // === Check if it is a complete packet. The device has received all packets ===
                else if ((message[3] == 's') && (message[8] == ']'))
                {
                    ReceivedAudioFileNumber = BitConverter.ToUInt16(message, 4);
                    if (ReceivedAudioFileNumber + 1 < TotalAudioFiles)
                    {
                        SendAudioFile(ReceivedAudioFileNumber + 1);
                    }
                    else
                    {
                       //still busy    
                    }
                }
            }
          
        }
        static void SendAudioFile(int AudioFileNumber)
        {
            if (AudioFileNumber == 0)
            {
                AudioFileNumber = 1;
            }

            if (DataIndex > 60000)
            {
                DataIndex = 0;
            }
            if(DataIndex == TotalCount)
            {
                CurrentAudioFileNumber++;
                DataIndex = 0;
                TotalCount = 0;
            }
            if(CurrentAudioFileNumber == TotalAudioFiles)
            {
                SendingComplete = true;
            }
            if (!SendingComplete)
            {
                StoreAudioFile(CurrentAudioFileNumber);

                byte[] AudioFileChunk = Enumerable.Repeat((byte)0xFF, DataSize+14).ToArray();
                byte[] SentIndexArray = BitConverter.GetBytes(DataIndex);
                byte[] TotalCountArray = BitConverter.GetBytes(TotalCount);
                byte[] FileNumberArray = BitConverter.GetBytes(CurrentAudioFileNumber);
                byte[] FileTotalArray = BitConverter.GetBytes(TotalAudioFiles);

                AudioFileChunk[0] = (byte)'[';
                AudioFileChunk[1] = (byte)'&';
                AudioFileChunk[2] = (byte)'A';
                AudioFileChunk[3] = (byte)'D';
                AudioFileChunk[4] = SentIndexArray[0]; // === SentIndex ===
                AudioFileChunk[5] = SentIndexArray[1];
                AudioFileChunk[6] = TotalCountArray[0]; // === TotalCount ===
                AudioFileChunk[7] = SentIndexArray[1];
                AudioFileChunk[8] = FileNumberArray[0]; // === FileNumber ===
                AudioFileChunk[9] = FileNumberArray[1];
                AudioFileChunk[10] = FileTotalArray[0]; // === FileTotal ===
                AudioFileChunk[11] = FileTotalArray[1];
                Buffer.BlockCopy(CurrentAudioFileArray, DataSize * DataIndex, AudioFileChunk, 12, DataSize);

                AudioFileChunk[TotalSize - 1] = (byte)']';

                GlobalSharedData.ServerMessageSend = AudioFileChunk;
            }
        }

        static byte[] CurrentAudioFileArray = { 0 };
        static void StoreAudioFile(int FileNumber)
        {           
            AudioEntry _SelectedFile = AudioFileList.FirstOrDefault(t => t.ID == FileNumber);

            if (_SelectedFile != null)
            {
                // === Read datalog file ===
                //string _logInfoRaw = System.IO.File.ReadAllText(Log_Filename, Encoding.Default);          
                BinaryReader _breader = new BinaryReader(File.OpenRead(_SelectedFile.Path));
                CurrentAudioFileArray = _breader.ReadBytes((int)_SelectedFile.Size);
              
                TotalCount = (UInt16)(CurrentAudioFileArray.Length/ DataSize);
                _breader.Dispose();
                _breader.Close();
            }
        }
        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            // === Send start datalog informaiton ===      
            
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&AA00]");
            CurrentAudioFileNumber = 1;
            DataIndex = 0;
            ButtonNew.IsEnabled = false;
            SendingComplete = false;
            SendingStarted = true; 

        }

        private void ButtonAppend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonNew.IsEnabled)
            {
                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                {
                    ProgramFlow.ProgramWindow = (int)ProgramFlowE.ConfigureMenuView;
                }
                else
                {
                    ProgramFlow.ProgramWindow = (int)ProgramFlowE.FileMenuView;
                    this.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ButtonNew.IsEnabled = true;
            }


        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ImageFilesView;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ParametersView;  
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

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {              
                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                {
                    DataGridAudioFiles.Columns[2].Visibility = Visibility.Visible;
                    ButtonNew.Visibility = Visibility.Visible;
                    ButtonAppend.Visibility = Visibility.Visible;
                    Grid_Progressbar.Visibility = Visibility.Visible;
                    ButtonNext.Visibility = Visibility.Visible;
                    ButtonPrevious.Visibility = Visibility.Visible;
                    WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                    SelectVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                    updateDispatcherTimer.Start();
                    ReadAudioFiles();
                    ProgressBar_AudioFiles.Value = 0;
                }
                else
                {
                    DataGridAudioFiles.Columns[2].Visibility = Visibility.Collapsed;
                    ButtonNew.Visibility = Visibility.Collapsed;
                    ButtonAppend.Visibility = Visibility.Collapsed;
                    Grid_Progressbar.Visibility = Visibility.Collapsed;
                    ButtonNext.Visibility = Visibility.Collapsed;
                    ButtonPrevious.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                SelectVID = 0;
            }
        }
    }
}
