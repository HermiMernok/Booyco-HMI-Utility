﻿using Booyco_HMI_Utility.CustomObservableCollection;
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


namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for DataExtractorView.xaml
    /// </summary>
    /// 
    public partial class DataExtractorView : UserControl
    {
        private RangeObservableCollection<LogEntry> DataLogs;
        private string logFilename = "";
        private static BackgroundWorker backgroundWorkerReadFile = new BackgroundWorker();
        private DataLogManagement dataLogManager = new DataLogManagement();





        public DataExtractorView()
        {
            InitializeComponent();
            DataLogs = new RangeObservableCollection<LogEntry>();
            DataGridLogs.AutoGenerateColumns = false;
            DataGridLogs.ItemsSource = DataLogs;
            DataGridLogs.IsReadOnly = true;
            dataLogManager.ReportProgressDelegate += backgroundWorkerReadFile.ReportProgress;
            backgroundWorkerReadFile.WorkerReportsProgress = true;
            backgroundWorkerReadFile.DoWork += new DoWorkEventHandler(ProcessLogFile);
            backgroundWorkerReadFile.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerProgressChanged);
        }

        /// <summary>
        /// 
        /// </summary>
        void OpenFile()
        {


        }
        private void ProcessLogFile(object sender, DoWorkEventArgs e)
        {
            dataLogManager.ReadFile(logFilename);

        }


        public void backgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {


            if (e.ProgressPercentage > 100)
            {
                DataLogs.Clear();
                DataLogs.AddRange(dataLogManager.TempList);
                dataLogManager.TempList.Clear();

            }
            else
            {
                ProgressbarDataLogs.Value = e.ProgressPercentage;
                TextBlockProgressStatus.Text = "Upload (" + e.ProgressPercentage.ToString().PadLeft(3, '0') + "%)";
            }
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.DefaultExt = "All files(*.*)";
            openFileDialog.Filter = "Log Fils (*.Mer) | *.Mer | Text Files (*.txt) | *.txt | All files(*.*) | *.*";

            if (openFileDialog.ShowDialog() == true)
            {
                logFilename = openFileDialog.FileName;

                // Start the background worker           

                if (!backgroundWorkerReadFile.IsBusy)
                {
                    backgroundWorkerReadFile.RunWorkerAsync();
                }
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.File;
        }

        private void Datagrid_Logs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ButtonMap_Click(object sender, RoutedEventArgs e)
        {
            //MapWindow MapWindow = new MapWindow();
            //MapWindow.Show();

            //Window window = new Window
            //{
            //    Title = "Booyco HMI Utility: Map",
            //    Content = new MapView(),
            //    Height = 800,
            //    Width = 1280
            //};



            //window.Show();
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Mapview;

        }

        public void DisplayWindowMap()
        {
            if (GlobalSharedData.ViewMode == true)
            {
                 Window window = new Window
                {
                    Title = "Booyco HMI Utility: Map",
                    Content = new MapView(),
                    Height = 800,
                    Width = 1280
                };

                window.Show();
                GlobalSharedData.ViewMode = false;


            }

        }

            
    }
}
