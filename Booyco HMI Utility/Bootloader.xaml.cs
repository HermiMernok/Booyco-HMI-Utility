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

        public static string BootStatus { get; set; }

        public static int BootFlashPersentage { get; set; }

        public static bool BootReady { get; set; }

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
            InitializeComponent();
            BootFlashPersentage = 0;


            DataContext = this;               
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(InfoUpdater);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            dispatcherTimer.Start();
            
        }

        private void InfoUpdater(object sender, EventArgs e)
        {
            if (WiFiconfig.clients.Count > 0 && GlobalSharedData.SelectedDevice != -1 && GlobalSharedData.SelectedDevice < WiFiconfig.clients.Count)
            {
                DeviceName = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                DeviceVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                FirmwareRev = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmRev;
                WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                //DeviceName = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                //DeviceName = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
            }
            else if(this.Visibility == Visibility.Visible)
            {
                BtnBack_Click(null, null);
            }

            if(bootfile!=null && bootfile != null)
            {
                btnBootload.IsEnabled = true;
            }
            else
            {
                btnBootload.IsEnabled = false;
            }

            if (bootchunks > 0 && BootSentIndex > 1 && !BootDone)
                BootloadingProgress.Value = BootSentIndex / (double)bootchunks * 1000;
            else
                BootloadingProgress.Value = 0;

            FlashEraseProgress.Value = BootFlashPersentage;

            //BootStatusLbl.Content = BootStatus;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            this.Visibility = Visibility.Collapsed;
        }

        private void Bootload_Click(object sender, RoutedEventArgs e)
        {

            BootReady = false;
            BootSentIndex = 0;
            BootAckIndex = -1;

            Thread BootloaderThread = new Thread(BootloaderDo)
            {
                IsBackground = true
            };
            BootloaderThread.Start();
            BootStatus = "Asking device to boot...";
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&BB00]");
        }

        private void BootloaderDo()
        {
            while (true)
            {
                if (BootReady)
                {

                    if (BootSentIndex == 0 && BootAckIndex == -1)
                    {
                        Thread.Sleep(10);
                        GlobalSharedData.ServerMessageSend = BootFileList.ElementAt(BootSentIndex);
                        BootSentIndex++;
                    }

                    if (BootSentIndex < BootFileList.Count && BootAckIndex == BootSentIndex - 1)
                    {
                        Thread.Sleep(10);
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

        }

        public static void BootloaderParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&')) //
            {
                if (message[2] == 'B')
                {
                    if (message[3] == 'a' && message[6] == ']')
                    {
                        BootStatus = "Device ready to boot...";
                        GlobalSharedData.BroadCast = false;
                        BootReady = true;
                        WiFiconfig.SelectedIP = endPoint.ToString();
                    }

                    if (message[3] == 's' && message[6] == ']')
                    {
                        //done bootloading
                        BootStatus = "Device bootloading done...";
                        BootDone = true;
                        BootFlashPersentage = 0;
                        BootReady = false;
                        Thread.Sleep(20);
                        GlobalSharedData.ServerMessageSend = WiFiconfig.HeartbeatMessage;
                    }

                    if (message[3] == 'e' && message[8] == ']')
                    {
                        BootStatus = "Device bootloading packet error...";
                        BootSentIndex--;
                    }

                    if (message[3] == 'f' && message[7] == ']')
                    {
                        BootFlashPersentage = message[4];
                        BootReady = true;
                        BootStatus = "Device bootloading flash erase... " + BootFlashPersentage.ToString() + "%";
                    }

                }
                else if (message[2] == 'D')
                {

                    if (message[3] == 'a' && message[8] == ']')
                    {
                        bootContinue = true;
                        BootAckIndex = BitConverter.ToUInt16(message, 4);
                        BootStatus = "Device bootloading packet " + BootAckIndex.ToString() + " of " + bootchunks.ToString() + "...";

                    }
                }
            }
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                bootfilebytes = File.ReadAllBytes(openFileDialog.FileName);

            bootfile = openFileDialog.FileName;

            int fileNameStart = bootfile.LastIndexOf("\\") + 1;
            string fileSub = bootfile.Substring(fileNameStart, bootfile.Length - fileNameStart);

            File.WriteAllBytes(fileSub, bootfilebytes);

            if (bootfile.Contains("M-PFW-"))
            {
                int start = bootfile.IndexOf("M-PFW");
                string firm = bootfile.Substring(start+6, 3);
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
                bootchunk[2] = (byte)'D';
                bootchunk[3] = bytes[0];
                bootchunk[4] = bytes[1];
                bootchunk[5] = bytes2[0];
                bootchunk[6] = bytes2[1];

                if (bytesleft > fileChunck)
                    Array.Copy(bootfilebytes, shifter, bootchunk, 7, fileChunck);
                else if (bytesleft > 0)
                    Array.Copy(bootfilebytes, shifter, bootchunk, 7, bytesleft);

                bootchunk[519] = 0;
                bootchunk[520] = 0;
                bootchunk[521] = (byte)']';
                BootFileList.Add(bootchunk);
                shifter += fileChunck;
                bytesleft -= fileChunck;
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

    }
}
