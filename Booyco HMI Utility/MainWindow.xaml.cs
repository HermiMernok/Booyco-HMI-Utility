﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer dispatcherTimer;
      
        public MainWindow()
        {
            InitializeComponent();

            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
            ProgramFlow.SourseWindow = (int)ProgramFlowE.Startup;

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(WindowUpdateTimer);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            dispatcherTimer.Start();
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
                BootView.Visibility = ConfigView.Visibility = Visibility.Collapsed;
                DataExtractorView.Visibility = Visibility.Visible;
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
            //else
            //    ProgrammingDone.Visibility = Visibility.Collapsed;


            #endregion
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }
    }
}
