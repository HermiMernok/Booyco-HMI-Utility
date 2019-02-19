using Booyco_HMI_Utility.CustomObservableCollection;
using GMap.NET.WindowsPresentation;
using GMap.NET;
using ProximityDetectionSystemInfo;
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
using System.IO;
using Booyco_HMI_Utility.CustomMarker;

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
        private bool _dataLogIsExpanded;

        public bool DataLogIsExpanded
        {       
        
            get
            {
                return _dataLogIsExpanded;
            }
            set
            {
                _dataLogIsExpanded = value;
                OnPropertyChanged("DataLogIsExpanded");
            }
        
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ExtendedWindow extendedWindow = new ExtendedWindow();


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
            DataGridLogs.CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, SelectAll_Executed));

            DataLogIsExpanded = new bool();
            DataLogIsExpanded = false;
         
                 
        }

        int counter;
        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (counter++ % 2 == 0) //select on every other click
                dataGrid.SelectAll();
            else //and unselect on every other click
                dataGrid.UnselectAll();
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
            PlotThreads();

        }
        private RangeObservableCollection<ProximityDetectionEvent> ProximityDetectionEventList = new RangeObservableCollection<ProximityDetectionEvent>();

        private void PlotThreads()
        {
            ProximityDetectionEvent TempEvent = new ProximityDetectionEvent();
            uint Event_Count = 0;

            foreach (LogEntry item in DataGridLogs.SelectedItems)
            {
                if (item != null)
                {
                    if(item.EventID == 150 || item.EventID == 157)
                    {
                        // TODO: Fix this two variables
                        TempEvent.DateTimeStamp = item.DateTime;
                        TempEvent.ThreatNumber = item.Number;

                        TempEvent.PrimaryID = BitConverter.ToUInt32(item.RawData, 0);
                        TempEvent.ThreatTechnology = item.RawData[4];
                        //uint PDS_01_Group = Map_Information1.RawDataEntry[5];
                        TempEvent.ThreatType = item.RawData[6];
                        TempEvent.ThreatDisplayWidth = (UInt16)(item.RawData[7] * 10);

                        Event_Count++;
                    }
                    else if (item.EventID == 151 || item.EventID == 158)
                    {
                        TempEvent.ThreatDisplaySector = item.RawData[0];
                        TempEvent.ThreatDisplayZone = item.RawData[1];
                        TempEvent.ThreatSpeed = ((double)BitConverter.ToInt16(item.RawData, 2)) / 10.0;
                        TempEvent.ThreatDistance = BitConverter.ToUInt16(item.RawData, 4);
                        TempEvent.ThreatHeading = BitConverter.ToInt16(item.RawData, 6);
                        Event_Count++;
                    }
                    else if (item.EventID == 152 || item.EventID == 159)
                    {
                      

                        Event_Count++;

                    }
                    else if (item.EventID == 153 || item.EventID == 160)
                    {
                        TempEvent.ThreatHorizontalAccuracy = item.RawData[0];
                        TempEvent.ThreatPriority = item.RawData[4];

                        Event_Count++;
                    }
                    else if (item.EventID == 154 || item.EventID == 161)
                    {
                        TempEvent.UnitSpeed = ((double)BitConverter.ToUInt16(item.RawData, 0)) / 10.0;
                        TempEvent.UnitHeading = ((double)BitConverter.ToUInt16(item.RawData, 2)) / 100.00;
                        TempEvent.UnitHorizontalAccuracy = item.RawData[4];

                        Event_Count++;
                    }
                    else if (item.EventID == 155 || item.EventID == 162)
                    {
                        TempEvent.ThreatLatitude = ((double)BitConverter.ToInt32(item.RawData, 0) * Math.Pow(10, -7));
                        TempEvent.ThreatLongitude = ((double)BitConverter.ToInt32(item.RawData, 4) * Math.Pow(10, -7));
                        Event_Count++;
                    }
                    else if (item.EventID == 156 || item.EventID == 163)
                    {
                        //TempEvent.UnitLatitude = ((double)BitConverter.ToInt32(Log_Information.RawDataEntry, 0) * Math.Pow(10, -7));
                        //TempEvent.UnitLongitude = ((double)BitConverter.ToInt32(Log_Information.RawDataEntry, 4) * Math.Pow(10, -7));

                        Event_Count++;
                    }

                    if (Event_Count == 6)
                    {
                        Event_Count = 0;
                        ProximityDetectionEventList.Add(TempEvent);
                        TempEvent = new ProximityDetectionEvent();
                    }
                  


                }

            }

            foreach (ProximityDetectionEvent EventItem in ProximityDetectionEventList)
            {
                MarkerEntry PDSMarker1 = new MarkerEntry();
                PDSMarker1.MapMarker = new GMapMarker(new PointLatLng(EventItem.ThreatLatitude, EventItem.ThreatLongitude));
                MarkerEntry PDSMarker2 = new MarkerEntry();
                PDSMarker2.MapMarker = new GMapMarker(new PointLatLng(EventItem.UnitLatitude, EventItem.UnitLongitude));
      
                if (TempEvent.ThreatTechnology == 5)
                {                 
                   PDSMarker1.Type = 1;
                   // PDSMarker1.Shape = new CustomMarkerAssetType(MainApp, PDSMarker1, PDS_Event_Information);
                }
                else
                {
                    PDSMarker1.Type = 2;
                    //  PDSMarker1.Shape = new CustomMarkerEllipse(MainApp, PDSMarker1, PDS_Event_Information);
                    //PDSMarker1.Shape = new CircleVisual(PDSMarker1,Brushes.Red);
                }
                PDSMarker2.Type = 3;
              
                GlobalSharedData.PDSMapMarkers.Add(PDSMarker1);
                GlobalSharedData.PDSMapMarkers.Add(PDSMarker2);
                extendedWindow.MapView.UpdateMapMarker();          
                extendedWindow.MapView.MainMap.Position = new PointLatLng(TempEvent.UnitLatitude, TempEvent.UnitLongitude);
            }
            
        }

        public void DisplayWindowMap()
        {
            
            if(GlobalSharedData.ViewMode == true )
            {
              
                extendedWindow.Visibility = Visibility.Visible;
                GlobalSharedData.ViewMode = false;                
            }
            try
            {
             if(extendedWindow.MapView.CloseRequest)
                {
                    extendedWindow.Visibility = Visibility.Collapsed;
                    extendedWindow.MapView.CloseRequest = false;
                   // extendedWindow.Close();
                }
            }
            catch
            {

            }
        }
        private void RowDoubleClick(object sender, RoutedEventArgs e)
        {
            var row = (DataGridRow)sender;
            if (row.DetailsVisibility == Visibility.Collapsed)
            {
                DataLogIsExpanded = true;
             
              //  Expander_Expanded(sender, e);
            }
            else
            {
                DataLogIsExpanded = false;
             //   Expander_Collapsed(sender, e);
            }

                
        }
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.Visibility == Visibility.Visible)
            {
                if (GlobalSharedData.FilePath != "" && File.Exists(GlobalSharedData.FilePath))
                {
                    logFilename = GlobalSharedData.FilePath;
                    if (!backgroundWorkerReadFile.IsBusy)
                    {
                        backgroundWorkerReadFile.RunWorkerAsync();
                    }
                }
            }
           
        }
    }
}
