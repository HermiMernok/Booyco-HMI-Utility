using Booyco_HMI_Utility.CustomObservableCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for AudioFilesView.xaml
    /// </summary>
    public partial class AudioFilesView : UserControl
    {
        private RangeObservableCollection<AudioEntry> AudioFileList;
        private uint SelectVID = 0;

        public AudioFilesView()
        {
            InitializeComponent();
            AudioFileList = new RangeObservableCollection<AudioEntry>();
            DataGridAudioFiles.AutoGenerateColumns = false;
            DataGridAudioFiles.ItemsSource = AudioFileList;
     
            ReadAudioFiles();
                
        }

        void ReadAudioFiles()
        {
            AudioFileList.Add(new AudioEntry
            {
                ID = 1,
                Name = "Test File",
                Progress = 50
            });
        }
        public static void AudioFileSendParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'A'))
            {
            }
            //    #region Configure ready to start
            //    if (message[3] == 'a' && message[6] == ']')
            //    {
            //        ConfigSendReady = true;
            //        //ConfigSentIndex = 0; 
            //        //ConfigSentAckIndex = -1;
            //        ParamsSendStarted = true;
            //        ConfigStatus = "Device ready to configure...";
            //        GlobalSharedData.ServerStatus = "Config ready message recieved";
            //        GlobalSharedData.BroadCast = false;
            //        WiFiconfig.SelectedIP = endPoint.ToString();
            //    }
            //    #endregion

            //    #region Configure next index
            //    if (message[3] == 'D')
            //    {
            //        if (message[4] == 'a' && message[9] == ']')
            //        {
            //            ConfigSentAckIndex = BitConverter.ToUInt16(message, 5);
            //            ConfigStatus = "Device receiving packet " + ConfigSentAckIndex.ToString() + " of " + Configchunks.ToString() + "...";
            //            GlobalSharedData.ServerStatus = "Config acknowledgment message recieved";

            //        }
            //    }
            //    #endregion

            //    #region Configure complete message
            //    if (message[3] == 's' && message[6] == ']')
            //    {
            //        ConfigStatus = "Device config read done...";
            //        ConfigSendDone = true;
            //        ParamsTransmitComplete = true;
            //        ConfigSendStop = true;
            //        ConfigSendReady = false;

            //        //Thread.Sleep(20);
            //        //GlobalSharedData.ServerMessageSend = WiFiconfig.HeartbeatMessage;
            //        //GlobalSharedData.ServerStatus = "Config paramaters sent message recieved";
            //    }
            //    #endregion

            //    #region Configure error message
            //    if (message[3] == 'e' && message[8] == ']')
            //    {
            //        if (BitConverter.ToUInt16(message, 4) == 0xFFFF)
            //        {
            //            ConfigSentIndex = 0;
            //            ConfigSentAckIndex = -1;
            //            ConfigStatus = "Waiting for device, please be patient... " + ConfigSentAckIndex.ToString() + "...";
            //            ConfigSendReady = true;
            //        }
            //        else
            //        {
            //            ConfigSentIndex = BitConverter.ToUInt16(message, 4);
            //            //ConfigSentIndex--;
            //            ConfigSentAckIndex = BitConverter.ToUInt16(message, 4);
            //            ConfigStatus = "Waiting for device, please be patient... " + ConfigSentAckIndex.ToString() + "...";
            //            Console.WriteLine("Error at Index" + ConfigSentIndex.ToString() + " ACK Index: " + ConfigSentAckIndex.ToString());
            //        }

            //    }
            //    #endregion

            //    #region Configure Exit message
            //    if (message[3] == 'x' && message[6] == ']')
            //    {
            //        backBtner = true;
            //    }
            //    #endregion
            //    Console.WriteLine("Packet Index:" + ConfigSentIndex.ToString() + " ACK Index: " + ConfigSentAckIndex.ToString());
            //}
            //else
            //{

            //}
        }
        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            // === Send start datalog informaiton ===
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&AA00]");
            
        }

        private void ButtonAppend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ConfigureMenuView;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {

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
                WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                SelectVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
            }
            else
            {
                SelectVID = 0;
            }
        }
    }
}
