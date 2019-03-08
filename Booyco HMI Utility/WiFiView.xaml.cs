using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Threading;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for WiFiView.xaml
    /// </summary>
    public partial class WiFiView : UserControl , INotifyPropertyChanged
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

        WiFiconfig WiFiconfig;

        public WiFiView()
        {
            InitializeComponent();           
            DataContext = this;
            //WiFiconfig = new WiFiconfig();
        }

        private void ClientListUpdater(object sender, EventArgs e)
        {
            ServerStatusView = GlobalSharedData.ServerStatus;
            NetworkDevicesp = GlobalSharedData.NetworkDevices;
            TCPclients = WiFiconfig.ClientLsitChanged(TCPclients);
            if (WiFiconfig.clients != null)
            {
                if (WiFiconfig.clients.Count == 0)
                {
                    GlobalSharedData.SelectedDevice = -1;
                    btnEnabler = false;
                }
                else if (WiFiconfig.clients.Count > 0 && GlobalSharedData.SelectedDevice != -1)
                {
                    btnEnabler = true;
                }
            }
            if (DGTCPclientList.Items.Count == 1)
                GlobalSharedData.SelectedDevice = 0;
        }

        #region DisplayHandler
        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
        
        }

        private void BtnBootload_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Bootload;
           
        }

        private void BtnDatView_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataExtractorView;
           
    }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Confiure;

        }
        #endregion

        #region Properties
        private List<TCPclientR> _TCPclients;
        public List<TCPclientR> TCPclients
        {
            get
            {
                return _TCPclients;
            }
            set
            {
                _TCPclients = value;
                OnPropertyChanged("TCPclients");
            }
        }

        private List<NetworkDevice> _NetworkDevicesp;
        public List<NetworkDevice> NetworkDevicesp
        {
            get
            {
                return _NetworkDevicesp;
            }

            set
            {
                _NetworkDevicesp = value;
                OnPropertyChanged("NetworkDevicesp");
            }
        }

        private bool _btnEnabler;

        public bool btnEnabler
        {
            get { return _btnEnabler; }
            set { _btnEnabler = value; OnPropertyChanged("btnEnabler"); }
        }

        private string _ServerStatusView;

        public string ServerStatusView
        {
            get { return _ServerStatusView; }
            set { _ServerStatusView = value; OnPropertyChanged("ServerStatusView"); }
        }


        #endregion

        private void DGTCPclientList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DGTCPclientList.SelectedIndex != -1)
                GlobalSharedData.SelectedDevice = DGTCPclientList.SelectedIndex;
            else if (DGTCPclientList.Items.Count == 1)
                GlobalSharedData.SelectedDevice = 0;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.Visibility == Visibility.Visible)
            {
                WiFiconfig = new WiFiconfig();          
                WiFiconfig.ServerRun();
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(ClientListUpdater);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                dispatcherTimer.Start();
               
            }
            else
            {
                Bootloader.BootReady = Bootloader.BootDone = false;
                dispatcherTimer.Stop();
                WiFiconfig.ServerStop();
            }
        }

        private void DGTCPclientList_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (DGTCPclientList.SelectedIndex != -1)
                GlobalSharedData.SelectedDevice = DGTCPclientList.SelectedIndex;
        }
    }

}
