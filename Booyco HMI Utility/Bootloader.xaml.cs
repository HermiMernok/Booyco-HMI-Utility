using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
    /// Interaction logic for Bootloader.xaml
    /// </summary>
    public partial class Bootloader : UserControl, INotifyPropertyChanged
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
        private DispatcherTimer dispatcherTimer;

        List<byte[]> BootFileList = new List<byte[]>();

        public static bool BootStart { get; set; }

        public static int BootFlashPersentage { get; set; }

        public static bool BootReady { get; set; }

        public static bool BootStop { get; set; }

        public static string BootStatus { get; set; }

        public static bool BootDone { get; set; }

        public static int BootSentIndex { get; set; }

        public static bool bootContinue;

        public static int BootAckIndex { get; set; }

        byte[] bootfilebytes;
        int bootfileSize = 0;
        static int bootchunks = 0;
        int bytesleft = 0;

        public Bootloader()
        {
            DataContext = this;
            BootBtnEnabled = false;
            InitializeComponent();
            BootFlashPersentage = 0;
            BootStatusView = "Waiting instructions..";
            LicenseBool = false;
            BootStop = false;
        }

        private void InfoUpdater(object sender, EventArgs e)
        {
            if(this.Visibility == Visibility.Visible && WiFiconfig.clients.Count == 0)
            {
                WiFiconfig.ConnectionError = true;
                BtnBack_Click(null, null);
                BootReady = false;
                BootStart = false;
            }

            if (bootchunks > 0 && !BootDone && BootFlashPersentage>0)
            {
                BootloadingProgress.Value = (BootSentIndex+ BootFlashPersentage) / ((double)bootchunks+100) * 1000;
                if(BootSentIndex>0)
                    BootFlashPersentage = 100;
            }                
            else
                BootloadingProgress.Value = 0;

            //           FlashEraseProgress.Value = BootFlashPersentage;

            BootStatusView = BootStatus;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            this.Visibility = Visibility.Collapsed;
        }

        private static Thread BootloaderThread;
        private void Bootload_Click(object sender, RoutedEventArgs e)
        {
            BootStart = true;
            BootReady = false;
            BootSentIndex = 0;
            BootAckIndex = -1;
            byte[] bootmessage = new byte[522];
            if(BootloaderThread != null && BootloaderThread.IsAlive)
            {

            }
            else
            {
                BootloaderThread = new Thread(BootloaderDo)
                {
                    IsBackground = true,
                    Name = "BootloaderThread"
                };
                BootloaderThread.Start();
            }

            BootStatus = "Asking device to boot...";
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&BB00]");
        }

        private void BootloaderDo()
        {
            BootBtnEnabled = false;
            while (!WiFiconfig.endAll && !BootStop)
            {
                //Thread.Sleep(100);
                if (BootReady)
                {

                    if (BootSentIndex == 0 && BootAckIndex == -1)
                    {
                        GlobalSharedData.ServerMessageSend = BootFileList.ElementAt(BootSentIndex);
                        BootSentIndex++;
                    }

                    if (BootSentIndex < BootFileList.Count && BootAckIndex == BootSentIndex - 1)
                    {
                        GlobalSharedData.ServerMessageSend = BootFileList.ElementAt(BootSentIndex);
                        BootSentIndex++;
                    }

                    if (BootSentIndex == BootFileList.Count)
                    {
                        Console.WriteLine("====================Bootloading done======================");
                        //WIFIcofig.ServerMessageSend = 
                        //BootReady = false;
                        break;
                    }
                }

            }
            BootBtnEnabled = true;
            BootStop = false;

        }

        public static void BootloaderParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'B'))
            {

                #region Bootloading ready to start
                if (message[3] == 'a' && message[6] == ']')
                {
                    BootStatus = "Device ready to boot...";
                    GlobalSharedData.ServerStatus = "Boor ready message recieved";
                    GlobalSharedData.BroadCast = false;
                    BootReady = true;
                    WiFiconfig.SelectedIP = endPoint.ToString();
                }
                #endregion

                #region Bootload next index
                if (message[3] == 'D')
                {                        
                    if (message[4] == 'a' && message[9] == ']')
                    {
                        bootContinue = true;
                        BootAckIndex = BitConverter.ToUInt16(message, 5);
                        BootStatus = "Device bootloading packet " + BootAckIndex.ToString() + " of " + bootchunks.ToString() + "...";
                        GlobalSharedData.ServerStatus = "Boot acknowledgment message recieved";

                    }                       
                }
                #endregion

                #region Bootloading complete message
                if (message[3] == 's' && message[6] == ']')
                {
                    //done bootloading
                    BootStatus = "Device bootloading done...";
                    BootDone = true;                      
                    BootFlashPersentage = 0;
                    BootReady = false;
                    Thread.Sleep(20);
                    GlobalSharedData.ServerMessageSend = WiFiconfig.HeartbeatMessage;
                    GlobalSharedData.ServerStatus = "Boot acknowledgment message recieved";
                }
                #endregion

                #region Bootload error message
                if (message[3] == 'e' && message[8] == ']')
                {
                    if(BitConverter.ToUInt16(message, 4) == 0xFFFF)
                    {
                        BootSentIndex = 0;
                        BootAckIndex = -1;
                        BootStatus = "Waiting for device, please be patient... " + BootAckIndex.ToString() + "...";
                    }
                    else
                    {
                        BootSentIndex--;
                        BootStatus = "Waiting for device, please be patient... " + BootAckIndex.ToString() + "...";
                    }                        
                        
                }
                #endregion

                #region Bootload flash message persentage
                if (message[3] == 'f' && message[7] == ']')
                {
                    BootFlashPersentage = message[4];
                    BootStatus = "Device bootloading flash erase... " + BootFlashPersentage.ToString() + "%";
                }
                #endregion
            }
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                bootfilebytes = File.ReadAllBytes(openFileDialog.FileName);

            bootfile = openFileDialog.FileName;

            if(bootfile != "")
            {
                int fileNameStart = bootfile.LastIndexOf("\\") + 1;
                string fileSub = bootfile.Substring(fileNameStart, bootfile.Length - fileNameStart);

                //           File.WriteAllBytes(fileSub, bootfilebytes);

                if (bootfile.Contains("M-PFW-"))
                {
                    int start = bootfile.IndexOf("M-PFW");
                    string firm = bootfile.Substring(start + 6, 3);
                    SelectedFirm = Int16.Parse(firm);
                }



                //bootfilebytes = File.ReadAllBytes("D:\\Users\\NeilPretorius\\Desktop\\V14 2\\TITAN VISION - V14\\ME-VISION-L4-PFW\\Debug\\ME-VISION-L4-PFW.binary");

                //           txtEditor.Text = openFileDialog.FileName;

                bootfile = openFileDialog.FileName;

                int fileChunck = 512;

                bytesleft = bootfileSize = bootfilebytes.Length;
                bootchunks = (int)Math.Round(bootfileSize / (double)fileChunck);
                int shifter = 0;
                for (int i = 0; i <= bootchunks; i++)
                {
                    byte[] bootchunk = Enumerable.Repeat((byte)0xFF, 522).ToArray();
                    byte[] bytes = BitConverter.GetBytes(i);
                    byte[] bytes2 = BitConverter.GetBytes(bootchunks);
                    bootchunk[0] = (byte)'[';
                    bootchunk[1] = (byte)'&';
                    bootchunk[2] = (byte)'B';
                    bootchunk[3] = (byte)'D';
                    bootchunk[4] = bytes[0];
                    bootchunk[5] = bytes[1];
                    bootchunk[6] = bytes2[0];
                    bootchunk[7] = bytes2[1];

                    if (bytesleft > fileChunck)
                        Array.Copy(bootfilebytes, shifter, bootchunk, 8, fileChunck);
                    else if (bytesleft > 0)
                        Array.Copy(bootfilebytes, shifter, bootchunk, 8, bytesleft);

                    bootchunk[520] = 0;
                    bootchunk[521] = (byte)']';
                    BootFileList.Add(bootchunk);
                    shifter += fileChunck;
                    bytesleft -= fileChunck;
                }

                //btnBootload.IsEnabled = true;
                BootBtnEnabled = true;
            }
            else
            {
                //btnBootload.IsEnabled = false;
                BootBtnEnabled = false;
            }


        }

        private string _bootStatus = "";

        public string BootStatusView
        {
            get { return _bootStatus; }
            set
            {
                if (value != null)
                    _bootStatus = value;
                else
                {
                    _bootStatus = "Waiting for instructions...";
                }
                    OnPropertyChanged("BootStatusView");
            }
        }


        private string _Name;

        public string DeviceName
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged("DeviceName"); }
        }

        private int _FirmwareRev;

        public int FirmwareRev
        {
            get { return _FirmwareRev; }
            set { _FirmwareRev = value; OnPropertyChanged("FirmwareRev"); }
        }

        private int _FirmSub;

        public int FirmSub
        {
            get { return _FirmSub; }
            set { _FirmSub = value; OnPropertyChanged("FirmSub"); }
        }

        private int _VID;
        private string bootfile;

        public int DeviceVID
        {
            get { return _VID; }
            set { _VID = value; OnPropertyChanged("DeviceVID"); }
        }

        private int _SelectedFirm;

        public int SelectedFirm
        {
            get { return _SelectedFirm; }
            set { _SelectedFirm = value; OnPropertyChanged("SelectedFirm"); }
        }

        private bool _LicenseBool;

        public bool LicenseBool
        {
            get { return _LicenseBool; }
            set { _LicenseBool = value; OnPropertyChanged("LicenseBool"); }
        }

        private bool _BootBtnEnabled;

        public bool BootBtnEnabled
        {
            get { return _BootBtnEnabled; }
            set { _BootBtnEnabled = value; OnPropertyChanged("BootBtnEnabled"); }
        }


        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.Visibility == Visibility.Visible)
            {
                BootStatusView = BootStatus = "Waiting for instructions...";
                if (bootfile == null || bootfile == "")
                {
                    //btnBootload.IsEnabled = false;
                    BootBtnEnabled = false;                    
                }
                else
                {
                    //btnBootload.IsEnabled = true;
                    BootBtnEnabled = true;
                }

                DeviceName = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                DeviceVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                FirmwareRev = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmRev;
                WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                //DeviceName = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                //DeviceName = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(InfoUpdater);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                dispatcherTimer.Start();
                BootReady = false;
                BootStop = true;
                BootFlashPersentage = 0;
                BootSentIndex = 0;
                BootAckIndex = -1;
            }
            else
            {
                BootReady = false;
                BootStart = false;
                BootStop = true;
                dispatcherTimer.Stop();
                BootloadingProgress.Value = 0;
                BootStatusView = BootStatus = "Waiting for instructions...";
                BootFlashPersentage = 0;
                BootSentIndex = 0;
                BootAckIndex = -1;
            }
        }
    }
}
