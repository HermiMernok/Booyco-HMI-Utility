using System;
using System.Collections.Generic;
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

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for DataExtractor.xaml
    /// </summary>
    public partial class DataExtractorView : UserControl
    {
        public static bool DataExtractorReady { get; set; }

        public static bool DataExtractorStop { get; set; }

        public static string DataExtractorStatus { get; set; }

        public static bool DataExtractorDone { get; set; }

        public static int DataExtractorSentIndex { get; set; }

        public static bool DataExtractorContinue;

        public static int DataExtractorAckIndex { get; set; }


        public DataExtractorView()
        {
            InitializeComponent();

        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
        }

    

        private static Thread DataExtractorThread;
        private void Button_Extract_Click(object sender, RoutedEventArgs e)
        {
                     
            //BootStatus = "Asking device to boot...";
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LL00]");
        }

        static int  StoredIndex = -1;
        static bool DataExtractorComplete = false;
        public static void DataExtractorSendParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'L'))
            {
                int DataIndex =  BitConverter.ToUInt16(message, 5);
                int TotalCount= BitConverter.ToUInt16(message, 7);
                //int DataIndex = message[5] message[4]
                if (DataIndex == StoredIndex+1 || StoredIndex == -1)
                {

                    GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LDa0000]");

                    if(TotalCount == DataIndex)
                    {
                        DataExtractorComplete = true;
                    }

                    StoredIndex = DataIndex;
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
            }
        }
    }
}
