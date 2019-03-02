using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DispatcherTimer dispatcherTimer;
      
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
            ProgramFlow.SourseWindow = (int)ProgramFlowE.Startup;

            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(WindowUpdateTimer);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            dispatcherTimer.Start();
        }

        static void CurrentDomain_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Console.WriteLine((e.Exception as Exception).Message);
        }

        private void WindowUpdateTimer(object sender, EventArgs e)
        {
            #region screens

            if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Startup)
            {
                BootView.Visibility = DataExtractorView.Visibility = ConfigView.Visibility = WiFiView.Visibility = USBView.Visibility = BluetoothView.Visibility = DataExtractorView.Visibility = FileView.Visibility = Visibility.Collapsed;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.Startup;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.WiFi)
            {
                USBView.Visibility = BluetoothView.Visibility = DataExtractorView.Visibility = FileView.Visibility = Visibility.Collapsed;
                WiFiView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.WiFi;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.USB)
            {
                WiFiView.Visibility = BluetoothView.Visibility = DataExtractorView.Visibility = FileView.Visibility = Visibility.Collapsed;
                USBView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.USB;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.File)
            {
                WiFiView.Visibility = USBView.Visibility = BluetoothView.Visibility = DataExtractorView.Visibility = Visibility.Collapsed;
                FileView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.File;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Bluetooth)
            {
                WiFiView.Visibility = USBView.Visibility = DataExtractorView.Visibility = FileView.Visibility = Visibility.Collapsed;
                BluetoothView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.Bluetooth;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.GPRS)
            {
                WiFiView.Visibility = USBView.Visibility = BluetoothView.Visibility = DataExtractorView.Visibility = FileView.Visibility = Visibility.Collapsed;
                //GPRSView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.GPRS;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Bootload)
            {
                DataExtractorView.Visibility = ConfigView.Visibility = Visibility.Collapsed;
                BootView.Visibility = Visibility.Visible;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Confiure)
            {
                DataExtractorView.Visibility = BootView.Visibility = Visibility.Collapsed;
                ConfigView.Visibility = Visibility.Visible;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Dataview)
            {
                DataExtractorView.DisplayWindowMap();
                BootView.Visibility = ConfigView.Visibility = Visibility.Collapsed;
                MapView.Visibility = Visibility.Collapsed;
                DataExtractorView.Visibility = Visibility.Visible;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Mapview)
            {
                MapView.Visibility = Visibility.Visible;
                
            }
            else
            {
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
            }

            if (Bootloader.BootDone)
            {
                ProgrammingDone.Visibility = Visibility.Visible;
                Bootloader.BootDone = false;
            }

            if(Bootloader.FileError)
            {
                Bootloader.FileError = false;
                ErrorView = true;
                Error_messageView.ErrorMessage = Bootloader.FileErrorMessage;
            }

            if(WiFiconfig.ConnectionError)
            {
                Error_messageView.ErrorMessage = "Connection Lost!";                
                Bootloader.BootDone = false;
                WiFiconfig.ConnectionError = false;
                ErrorView = true;
            }

            if(ErrorView)
            {
                ErrorView = false;
                Error_messageView.Visibility = Visibility.Visible;
            }



            //else
            //    ProgrammingDone.Visibility = Visibility.Collapsed;


            #endregion
            HeartbeatCount = GlobalSharedData.ServerStatus;
            WiFiApStatus = GlobalSharedData.WiFiApStatus;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WiFiconfig.endAll = true;

            var prc = new ProcManager();
            prc.KillByPort(13000);
            
            Environment.Exit(Environment.ExitCode);
        }


        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion

        private string _HeartbeatCount;

        public string HeartbeatCount
        {
            get { return _HeartbeatCount; }
            set { _HeartbeatCount = value; OnPropertyChanged("HeartbeatCount"); }
        }

        private string _WiFiApStatus;

        public string WiFiApStatus
        {
            get { return _WiFiApStatus; }
            set { _WiFiApStatus = value; OnPropertyChanged("WiFiApStatus"); }
        }

        public bool ErrorView { get; private set; }
    }
}
