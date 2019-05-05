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
using System.Security.Principal;
using System.Management;
using System.Diagnostics;
using System.Reflection;

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
            // Check if user is NOT admin
            if (!IsRunningAsAdministrator())
            {
                // Setting up start info of the new process of the same application
                ProcessStartInfo processStartInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().CodeBase);

                // Using operating shell and setting the ProcessStartInfo.Verb to “runas” will let it run as admin
                processStartInfo.UseShellExecute = true;
                processStartInfo.Verb = "runas";

                // Start the application as new process
                Process.Start(processStartInfo);

                // Shut down the current (old) process
                Environment.Exit(Environment.ExitCode);
            }
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
        /// <summary>
        /// Function that check's if current user is in Aministrator role
        /// </summary>
        /// <returns></returns>
        public static bool IsRunningAsAdministrator()
        {
            // Get current Windows user
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

            // Get current Windows user principal
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);

            // Return TRUE if user is in role "Administrator"
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
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
                BootView.Visibility = DataLogView.Visibility = ParametersView.Visibility = WiFiView.Visibility = USBView.Visibility = BluetoothView.Visibility = DataLogView.Visibility = FileView.Visibility = Visibility.Collapsed;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.Startup;
                 
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.FileMenuView)
            {
                WiFiView.Visibility = USBView.Visibility = BluetoothView.Visibility = DataLogView.Visibility = Visibility.Collapsed;
                FileMenuView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.FileMenuView;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.WiFi)
            {
                USBView.Visibility = BluetoothView.Visibility = DataLogView.Visibility = FileMenuView.Visibility = Visibility.Collapsed;
                WiFiView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.WiFi;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.USB)
            {
                WiFiView.Visibility = BluetoothView.Visibility = DataLogView.Visibility = FileMenuView.Visibility = Visibility.Collapsed;
                USBView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.USB;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.File)
            {
                WiFiView.Visibility = USBView.Visibility = BluetoothView.Visibility = DataLogView.Visibility = Visibility.Collapsed;
                FileView.Visibility = Visibility.Visible;
                //ProgramFlow.SourseWindow = (int)ProgramFlowE.File;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.DataLogFileView)
            {
                WiFiView.Visibility = USBView.Visibility = BluetoothView.Visibility = DataLogView.Visibility = Visibility.Collapsed;
                DataLogFileView.Visibility = Visibility.Visible;
                //ProgramFlow.SourseWindow = (int)ProgramFlowE.File;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.ParameterFileView)
            {
                WiFiView.Visibility = USBView.Visibility = BluetoothView.Visibility = DataLogView.Visibility = Visibility.Collapsed;
                ParameterFileView.Visibility = Visibility.Visible;
                //ProgramFlow.SourseWindow = (int)ProgramFlowE.File;
            }
            
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Bluetooth)
            {
                WiFiView.Visibility = USBView.Visibility = DataLogView.Visibility = FileView.Visibility = Visibility.Collapsed;
                BluetoothView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.Bluetooth;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.GPRS)
            {
                WiFiView.Visibility = USBView.Visibility = BluetoothView.Visibility = DataLogView.Visibility = FileView.Visibility = Visibility.Collapsed;
                //GPRSView.Visibility = Visibility.Visible;
                ProgramFlow.SourseWindow = (int)ProgramFlowE.GPRS;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Bootload)
            {
                DataLogView.Visibility = ParametersView.Visibility = Visibility.Collapsed;
                BootView.Visibility = Visibility.Visible;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.ConfigureMenuView)
            {
                ConfigureMenuView.Visibility = Visibility.Visible;
                //ConfigView.Visibility = Visibility.Visible;
                AudioFilesView.Visibility = Visibility.Collapsed;
                ParametersView.Visibility = Visibility.Collapsed;
                ImageFilesView.Visibility = Visibility.Collapsed;
            }
            else if(ProgramFlow.ProgramWindow == (int)ProgramFlowE.ParametersView)
            {
                ParametersView.Visibility = Visibility.Visible;
                AudioFilesView.Visibility = Visibility.Collapsed;
                ConfigureMenuView.Visibility = Visibility.Collapsed;
                ImageFilesView.Visibility = Visibility.Collapsed;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.AudioFilesView)
            {
                AudioFilesView.Visibility = Visibility.Visible;
                ParametersView.Visibility = Visibility.Collapsed;
                ConfigureMenuView.Visibility = Visibility.Collapsed;
                ImageFilesView.Visibility = Visibility.Collapsed;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.ImageFilesView)
            {
                ImageFilesView.Visibility = Visibility.Visible;
                AudioFilesView.Visibility = Visibility.Collapsed;
                ParametersView.Visibility = Visibility.Collapsed;
                ConfigureMenuView.Visibility = Visibility.Collapsed;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.DataExtractorView)
            {                             
                DataExtractorView.Visibility = Visibility.Visible;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.DataLogView)
            {
                DataLogView.DisplayWindowMap();
                BootView.Visibility = ParametersView.Visibility = Visibility.Collapsed;
                MapView.Visibility = Visibility.Collapsed;
                HMIDisplayView.Visibility = Visibility.Collapsed;
                DataLogView.Visibility = Visibility.Visible;
                DataExtractorView.Visibility = Visibility.Collapsed;
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.Mapview)
            {
                MapView.Visibility = Visibility.Visible;                
            }
            else if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.HMIDisplayView)
            {
                HMIDisplayView.Visibility = Visibility.Visible;
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

            if (ProgramFlow.ProgramWindow != (int)ProgramFlowE.DataExtractorView && ProgramFlow.ProgramWindow != (int)ProgramFlowE.WiFi && ProgramFlow.ProgramWindow != (int)ProgramFlowE.ParametersView && ProgramFlow.ProgramWindow != (int)ProgramFlowE.Bootload)
            {
                HeartbeatCount = "";
                WiFiApStatus = "";
            }
            else
            {
                #endregion
                HeartbeatCount = GlobalSharedData.ServerStatus;
                WiFiApStatus = GlobalSharedData.WiFiApStatus;
            }

            if(ErrorView)
            {
                ErrorView = false;
                Error_messageView.Visibility = Visibility.Visible;
            }

        



            //else
            //    ProgrammingDone.Visibility = Visibility.Collapsed;



        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WiFiconfig.endAll = true;
            WiFiconfig.WirelessHotspot(null, null, false);
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
        public bool BusyView { get; private set; }
        private void StartUpView_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
