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
using OfficeOpenXml;
using ProximityDetectionSystemInfo;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for DataExtractorView.xaml
    /// </summary>
    /// 
    public partial class DataLogView : UserControl
    {
        private RangeObservableCollection<LogEntry> DataLogs;
        private string logFilename = "";
        private static BackgroundWorker backgroundWorkerReadFile = new BackgroundWorker();
        private DataLogManagement dataLogManager = new DataLogManagement();
        private bool _dataLogIsExpanded = false;



       

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

        private ExtendedWindow extendedWindow= new ExtendedWindow();


        public DataLogView()
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
                ButtonSave.IsEnabled = true;
                ProgressbarDataLogs.Visibility = Visibility.Collapsed;
                this.ButtonSelectAll.Visibility = Visibility.Visible;
                this.ButtonToggleExpand.Visibility = Visibility.Visible;
                Grid_ProgressBar.Visibility = Visibility.Collapsed;

            }
            else
            {
       
                ProgressbarDataLogs.Value = e.ProgressPercentage;
                TextBlockProgressStatus.Text = "Upload (" + e.ProgressPercentage.ToString().PadLeft(3, '0') + "%)";
            }
        }

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog _saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            // == Default extension ===
            _saveFileDialog.DefaultExt = ".xlsx";
            // == filter types ===
            _saveFileDialog.Filter = "Excel File (*.xlsx)|*.xlsx";
            string _filename = logFilename.Split('\\').Last();
            _saveFileDialog.FileName = _filename.Remove(_filename.Length-4,4);
            _saveFileDialog.FilterIndex = 1;
            _saveFileDialog.RestoreDirectory = true;

            if (_saveFileDialog.ShowDialog() == true)
            {
                if (_saveFileDialog.FileName.Contains(".csv"))
                {
                    //if (!File.Exists(_saveFileDialog.FileName))
                    //{
                    //    File.Create(_saveFileDialog.FileName);
                    //}
                    // === open streamwrite to save file ===
                    StreamWriter writer = new StreamWriter(_saveFileDialog.FileName);
                    int counter = 0;

                    

                    // === write each entry from data log in to .csv file ===
                    foreach (LogEntry _logEntry in DataLogs)
                    {
                        counter++;

                        writer.WriteLine(_logEntry.DateTime.Date + ";" +
                            _logEntry.DateTime.TimeOfDay + ";" +
                            _logEntry.EventID + ";" +
                            _logEntry.EventName + ";" +
                            _logEntry.RawEntry
                           //Single_Log_Data._EventInformation.ToString()
                           );

                        // === dispose streamwrite ===
                        writer.Dispose();
                        // === close stramwrite ===
                        writer.Close();
                    }

                }
                    if (_saveFileDialog.FileName.Contains(".xlsx"))
                    {
                        using (var p = new ExcelPackage())
                        {


                            //A workbook must have at least on cell, so lets add one... 
                            var ws = p.Workbook.Worksheets.Add("MySheet");

                            var dataRange = ws.Cells["A1"].LoadFromCollection
                           (
                           from s in DataLogs
                           orderby s.Number, s.EventName
                           select s,
                          true, OfficeOpenXml.Table.TableStyles.Medium2);
                         
                            //To set values in the spreadsheet use the Cells indexer.

                            // === Header ===
                            ws.Cells[1, 1].Value = "No.";
                            ws.Cells[1, 2].Value = "Date";
                            ws.Cells[1, 3].Value = "Time ";
                            ws.Cells[1, 4].Value = "Event ID";
                            ws.Cells[1, 5].Value = "Event Name";
                            ws.Cells[1, 6].Value = "Event Description";
                            ws.Cells[1, 7].Value = "Raw Data Display";
                            ws.Cells[1, 8].Value = "Event Information";

                            int count = 2;
                            foreach (LogEntry _logEntry in DataLogs)
                            {
                                ws.Cells[count, 1].Value = _logEntry.Number;
                                ws.Cells[count, 2].Value = _logEntry.DateTime.Date.ToString();
                                ws.Cells[count, 3].Value = _logEntry.DateTime.TimeOfDay.ToString();
                                ws.Cells[count, 4].Value = _logEntry.EventID;
                                ws.Cells[count, 5].Value = _logEntry.EventName;
                                ws.Cells[count, 6].Value = _logEntry.EventInfo;
                                ws.Cells[count, 7].Value = _logEntry.RawEntry;                
                                count++;
                            }
                        dataRange.AutoFitColumns();

                        //Save the new workbook. We haven't specified the filename so use the Save as method.
                        p.SaveAs(new FileInfo(_saveFileDialog.FileName));
                        }
                    }
                

              
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
           
            ButtonSave.IsEnabled = false;
            dataLogManager.AbortRequest = true;
          
            DataLogs.Clear();
            this.Visibility = Visibility.Collapsed;
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
        }

        private void Datagrid_Logs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridLogs.SelectedItems.Count >= 1)
            {
               // ButtonMap.IsEnabled = true;
                ButtonDisplay.IsEnabled = true;
            }
            else
            {
               // ButtonMap.IsEnabled = false;
                ButtonDisplay.IsEnabled = false;
            }
            if (ThreadsIsSelected())
            {
                ButtonMap.IsEnabled = true;
               // ButtonDisplay.IsEnabled = true;
            }
            else
            {
                ButtonMap.IsEnabled = false;
               // ButtonDisplay.IsEnabled = false;
            }
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

        private bool ThreadsIsSelected()
        {
          
            uint Event_Count = 0;

            foreach (LogEntry item in DataGridLogs.SelectedItems)
            {
                if (item != null)
                {
                    if (item.EventID == 150 || item.EventID == 159)
                    {

                        Event_Count++;
                    }
                   

                }

            }

            if(Event_Count> 0)
            {
                return true;
            }

            return false;        
   
        }

        private void  PlotThreads()
        {
          
            ProximityDetectionEvent TempEvent = new ProximityDetectionEvent();
            uint Event_Count = 0;
            ProximityDetectionEventList = new RangeObservableCollection<ProximityDetectionEvent>();
            foreach (LogEntry item in DataGridLogs.SelectedItems)
            {

                if (item != null)
                {

                  
                    if ((item.EventID == 150 || item.EventID == 159) && ((BitConverter.ToInt32(item.RawData, 40))!= 0 && (BitConverter.ToInt32(item.RawData, 44))!=0))
                
                    {
                        // TODO: Fix this two variables
                        TempEvent.DateTimeStamp = item.DateTime;
                        TempEvent.ThreatNumberStart = item.Number;
                        TempEvent.ThreatNumberStop = item.Number + 6;
                        TempEvent.PrimaryID = BitConverter.ToUInt32(item.RawData, 0);
                        TempEvent.ThreatTechnology = item.RawData[4];
                        //uint PDS_01_Group = Map_Information1.RawDataEntry[5];
                        TempEvent.ThreatType = item.RawData[6];
                        //TempEvent.ThreatDisplayWidth = (UInt16)(item.RawData[7] * 10);

                        Event_Count++;
                
                        TempEvent.ThreatDisplaySector = item.RawData[8];
                      
                        if(item.EventID == 150)
                        {
                            TempEvent.ThreatDisplayZone = item.RawData[9];
                        }
                        else
                        {
                            TempEvent.ThreatDisplayZone = 0;
                        }
                        if(DataLogs.Count() == 0)
                        {

                        }
                        TempEvent.EventInfo = item.EventInfoList;
                        TempEvent.ThreatSpeed = double.Parse(item.DataList[7]);
                        TempEvent.ThreatDistance = ushort.Parse(item.DataList[8]);
                        TempEvent.ThreatHeading = double.Parse(item.DataList[9]);
                        Event_Count++;

                        TempEvent.ThreatLatitude = double.Parse(item.DataList[10]);
                        TempEvent.ThreatLongitude = double.Parse(item.DataList[11]);

                        Event_Count++;
                        TempEvent.ThreatHorizontalAccuracy = uint.Parse(item.DataList[12]);                    
                        TempEvent.ThreatPriority = uint.Parse(item.DataList[13]);
                        TempEvent.ThreatPOILOGDistance = double.Parse(item.DataList[14]);
                        TempEvent.CriticalDistance = ushort.Parse(item.DataList[16]);
                        TempEvent.WarningDistance = ushort.Parse(item.DataList[17]);
                        TempEvent.PresenceDistance = ushort.Parse(item.DataList[18]);                    

                        Event_Count++;
                  
                        TempEvent.UnitSpeed = double.Parse(item.DataList[10]); 
                        TempEvent.UnitHeading = ((double)BitConverter.ToUInt16(item.RawData, 34)) ;
                        TempEvent.UnitHorizontalAccuracy = item.RawData[36];
                        TempEvent.ThreatDisplayWidth = (UInt16)(item.RawData[38]);

                        Event_Count++;
                   
                        TempEvent.UnitLatitude = ((double)BitConverter.ToInt32(item.RawData, 40) * Math.Pow(10, -7));
                        TempEvent.UnitLongitude = ((double)BitConverter.ToInt32(item.RawData, 44) * Math.Pow(10, -7));
                        Event_Count++;
                    
                   
                        TempEvent.POILatitude = ((double)BitConverter.ToInt32(item.RawData, 48) * Math.Pow(10, -7));
                        TempEvent.POILongitude = ((double)BitConverter.ToInt32(item.RawData, 52) * Math.Pow(10, -7));

                        Event_Count++;

                        //Convert Geodetic decimal to UTM for Unit and POI
                        GeoUtility.GeoSystem.Geographic LatLon_Unit = new GeoUtility.GeoSystem.Geographic(TempEvent.UnitLongitude, TempEvent.UnitLatitude);
                        GeoUtility.GeoSystem.UTM UTM_Unit = (GeoUtility.GeoSystem.UTM)LatLon_Unit;
                        GeoUtility.GeoSystem.Geographic POI_LatLon = new GeoUtility.GeoSystem.Geographic(TempEvent.POILongitude, TempEvent.POILatitude);
                        GeoUtility.GeoSystem.UTM POI_UTM = (GeoUtility.GeoSystem.UTM)POI_LatLon;
                        //Calculate distance between unit and POI
                        TempEvent.ThreatPOIUTMDistance = Math.Sqrt((Math.Pow((UTM_Unit.East - POI_UTM.East), 2) + Math.Pow((UTM_Unit.North - POI_UTM.North), 2)));

                        //if (item.EventID == 150 || item.EventID == 157)
                        //{
                        //    // TODO: Fix this two variables
                        //    TempEvent.DateTimeStamp = item.DateTime;
                        //    TempEvent.ThreatNumberStart = item.Number;
                        //    TempEvent.ThreatNumberStop = item.Number + 6;
                        //    TempEvent.PrimaryID = BitConverter.ToUInt32(item.RawData, 0);
                        //    TempEvent.ThreatTechnology = item.RawData[4];
                        //    //uint PDS_01_Group = Map_Information1.RawDataEntry[5];
                        //    TempEvent.ThreatType = item.RawData[6];
                        //    TempEvent.ThreatDisplayWidth = (UInt16)(item.RawData[7] * 10);

                        //    Event_Count++;
                        //}
                        //else if (item.EventID == 151 || item.EventID == 158)
                        //{
                        //    TempEvent.ThreatDisplaySector = item.RawData[0];
                        //    TempEvent.ThreatDisplayZone = item.RawData[1];
                        //    TempEvent.ThreatSpeed = ((double)BitConverter.ToInt16(item.RawData, 2)) / 10.0;
                        //    TempEvent.ThreatDistance = BitConverter.ToUInt16(item.RawData, 4);
                        //    TempEvent.ThreatHeading = BitConverter.ToInt16(item.RawData, 6);
                        //    Event_Count++;
                        //}
                        //else if (item.EventID == 152 || item.EventID == 159)
                        //{
                        //    TempEvent.ThreatLatitude = ((double)BitConverter.ToInt32(item.RawData, 0) * Math.Pow(10, -7));
                        //    TempEvent.ThreatLongitude = ((double)BitConverter.ToInt32(item.RawData, 4) * Math.Pow(10, -7));

                        //    Event_Count++;

                        //}
                        //else if (item.EventID == 153 || item.EventID == 160)
                        //{
                        //    TempEvent.ThreatHorizontalAccuracy = item.RawData[0];
                        //    TempEvent.ThreatPriority = item.RawData[4];

                        //    Event_Count++;
                        //}
                        //else if (item.EventID == 154 || item.EventID == 161)
                        //{
                        //    TempEvent.UnitSpeed = ((double)BitConverter.ToUInt16(item.RawData, 0)) / 10.0;
                        //    TempEvent.UnitHeading = ((double)BitConverter.ToUInt16(item.RawData, 2)) / 100.00;
                        //    TempEvent.UnitHorizontalAccuracy = item.RawData[4];

                        //    Event_Count++;
                        //}
                        //else if (item.EventID == 155 || item.EventID == 162)
                        //{
                        //    TempEvent.UnitLatitude = ((double)BitConverter.ToInt32(item.RawData, 0) * Math.Pow(10, -7));
                        //    TempEvent.UnitLongitude = ((double)BitConverter.ToInt32(item.RawData, 4) * Math.Pow(10, -7));
                        //    Event_Count++;
                        //}
                        //else if (item.EventID == 156 || item.EventID == 163)
                        //{
                        //    TempEvent.POILatitude = ((double)BitConverter.ToInt32(item.RawData, 0) * Math.Pow(10, -7));
                        //    TempEvent.POILongitude = ((double)BitConverter.ToInt32(item.RawData, 4) * Math.Pow(10, -7));

                        //    Event_Count++;
                        //}

                        //if (Event_Count == 7)
                        //{
                        //    Event_Count = 0;
                        //    ProximityDetectionEventList.Add(TempEvent);
                        //    TempEvent = new ProximityDetectionEvent();

                        //}

                        Event_Count = 0;
                        ProximityDetectionEventList.Add(TempEvent);
                        TempEvent = new ProximityDetectionEvent();
                       
                    }                


                }

            }
            GlobalSharedData.PDSMapMarkers = new List<MarkerEntry>();
            double LastLat = 0;
            double LastLon = 0;
            foreach (ProximityDetectionEvent EventItem in ProximityDetectionEventList)
            {

                //if(TempEvent.ThreatTechnology == (int)Tech_Kind.Pulse_GPS || TempEvent.ThreatTechnology == (int)Tech_Kind_GPS)
                //{ 

                string PDS_Event_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                                "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                                EventItem.EventInfo[0] + "\n" +         //Thread BID
                                                EventItem.EventInfo[1] + "\n" +         //THreat Kind
                                                EventItem.EventInfo[2] + "\n" +         //Threat Group
                                                EventItem.EventInfo[3] + "\n" +         //Threat Type
                                                EventItem.EventInfo[4] + "\n" +         //Threat Cluster
                                                EventItem.EventInfo[5] + "\n" +         //Threat Sector
                                                EventItem.EventInfo[6] + "\n" +         //Threat Zone
                                                EventItem.EventInfo[7] + "\n" +         //Threat Speed
                                                EventItem.EventInfo[8] + "\n" +         //Threat Distance
                                                EventItem.EventInfo[9] + "\n" +         //Threat Heading
                                                EventItem.EventInfo[10] + "\n" +         //Threat Latitude
                                                EventItem.EventInfo[11] + "\n" +         //Threat Longitude
                                                EventItem.EventInfo[12] + "\n";         //Threat Acc

                string Unit_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                          "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                            EventItem.EventInfo[19] + "\n" +         //Speed
                                            EventItem.EventInfo[20] + "\n" +         //Heading
                                            EventItem.EventInfo[21] + "\n" +         //Accuracy
                                           // EventItem.EventInfo[22] + "\n" +        
                                           // EventItem.EventInfo[23] + "\n" +         
                                            EventItem.EventInfo[24] + "\n" +         //LAT
                                            EventItem.EventInfo[25] + "\n" +         //LON
                                            EventItem.EventInfo[29] + "\n";         //Scenario
                                           // EventItem.EventInfo[27] + "\n" +         //Scenario

                string POI_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                          "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                          "POI Distance (UTM Plot): " + EventItem.ThreatPOIUTMDistance.ToString("##,##00.00") + " m\n" +
                                          "POI Distance (Log): " + EventItem.ThreatPOILOGDistance.ToString() + " m\n" +
                                          "POI Latitude: " + EventItem.POILatitude.ToString() + " deg\n" +
                                          "POI Longitude: " + EventItem.POILongitude.ToString() + " deg" + "\n" +  
                                          EventItem.EventInfo[29] + "\n";        //Scenario;

                 MarkerEntry PDSMarker1 = new MarkerEntry();               
                MarkerEntry PDSMarker2 = new MarkerEntry();               
                MarkerEntry PDSMarkerPOI = new MarkerEntry();


                int LastGeofenceIndex = -1;

                if (EventItem.ThreatTechnology == (int)Tech_Kind.Pulse_GPS)
                {
                    PDSMarker1.Heading = EventItem.ThreatHeading;
                    PDSMarker1.Zone = 10;
                    PDSMarker1.title = PDS_Event_Information;
                    PDSMarker1.Type = (int)MarkerType.Indicator;                
                }
                else if(EventItem.ThreatTechnology == (int)Tech_Kind.GPS)
                {                
                    PDSMarker1.title = PDS_Event_Information;
                    PDSMarker1.Width = EventItem.ThreatDisplayWidth;
                    PDSMarker1.Height = EventItem.ThreatDisplayWidth;
             
                    PDSMarker1.Type = (int)MarkerType.Ellipse;          
                }
                else
                {
                    PDSMarker1.title = PDS_Event_Information;
                    PDSMarker1.Width = EventItem.ThreatDisplayWidth;
                    PDSMarker1.Height = EventItem.ThreatDisplayWidth;
                    PDSMarker1.Type = (int)MarkerType.Ellipse;
                }

                LastGeofenceIndex = GlobalSharedData.PDSMapMarkers.FindLastIndex(x => x.MapMarker.Position.Lat == EventItem.ThreatLatitude && x.MapMarker.Position.Lng == EventItem.ThreatLongitude && x.Type == (int)MarkerType.Ellipse);

             
                PDSMarker1.MapMarker = new GMapMarker(new PointLatLng(EventItem.ThreatLatitude, EventItem.ThreatLongitude));
                PDSMarker2.MapMarker = new GMapMarker(new PointLatLng(EventItem.UnitLatitude, EventItem.UnitLongitude));
                PDSMarkerPOI.MapMarker = new GMapMarker(new PointLatLng(EventItem.POILatitude, EventItem.POILongitude));

                PDSMarker2.Heading = EventItem.UnitHeading;
                PDSMarker2.Zone = EventItem.ThreatDisplayZone;
                PDSMarker2.title = Unit_Information;
                PDSMarker2.PresenceZoneSize = EventItem.PresenceDistance+1;
                PDSMarker2.WarningZoneSize = EventItem.WarningDistance+1;
                PDSMarker2.CriticalZoneSize = EventItem.CriticalDistance+1;
                PDSMarker2.Type = (int)MarkerType.Indicator;
              
                PDSMarkerPOI.Zone = EventItem.ThreatDisplayZone;
                PDSMarkerPOI.title = POI_Information;
                PDSMarkerPOI.Type = (int)MarkerType.Point;
                if (LastGeofenceIndex == -1)
                {
                    GlobalSharedData.PDSMapMarkers.Add(PDSMarker1);
                }
                GlobalSharedData.PDSMapMarkers.Add(PDSMarker2);
                GlobalSharedData.PDSMapMarkers.Add(PDSMarkerPOI);
                extendedWindow.MapView.StartLat = EventItem.UnitLatitude;
                extendedWindow.MapView.StartLon = EventItem.UnitLongitude;
               

            }

            extendedWindow.MapView.UpdateMapMarker();


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
            //if (row.DetailsVisibility == Visibility.Collapsed)
            //{
            //    DataLogIsExpanded = true;

            //  //  Expander_Expanded(sender, e);
            //}
            //else
            //{
            //    DataLogIsExpanded = false;
            // //   Expander_Collapsed(sender, e);
            //}
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

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
                Grid_ProgressBar.Visibility = Visibility.Visible;
                this.ButtonSelectAll.Visibility = Visibility.Collapsed;
                this.ButtonToggleExpand.Visibility = Visibility.Collapsed;
                ProgressbarDataLogs.Visibility = Visibility.Visible;
                dataLogManager.AbortRequest = false;
                if (GlobalSharedData.FilePath != "" && File.Exists(GlobalSharedData.FilePath))
                {
                    logFilename = GlobalSharedData.FilePath;
                    if (!backgroundWorkerReadFile.IsBusy)
                    {
                        backgroundWorkerReadFile.RunWorkerAsync();
                    }
                   
                }
            }
            else
            {
                TextBlockProgressStatus.Text = "";
                ProgressbarDataLogs.Value = 0;

            }
           
        }

        private void ButtonToggleExpanded_Click(object sender, RoutedEventArgs e)
        {
            //for (int i = 0; i < DataGridLogs.Items.Count; i++)
            //{
            //    DataGridRow row = (DataGridRow)DataGridLogs.ItemContainerGenerator
            //                                               .ContainerFromIndex(i);
            //    if (row != null)
            //    { 
            //    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            //}
            //}

            if (DataGridLogs.Columns[5].Visibility == Visibility.Visible)
            {
                DataGridLogs.Columns[5].Visibility = Visibility.Collapsed;
      
                ButtonToggleExpand.Content = "Expand";
            }
            else
            {
                DataGridLogs.Columns[5].Visibility = Visibility.Visible;
                ButtonToggleExpand.Content = "Collapse";
            }
           

         
        }

        private void PreviewKeyDown_Filter_Key(object sender, KeyEventArgs e)
        {          
                // == check if enter is pressed ===
                if (e.Key == Key.Enter)
                {
                    // === if enter is pressed filter data logs ===
                    //Button_Filter_Click(null, null);
                }
            
        }
        /// <summary>
        /// CheckComboBox mouse Enter event
        ///  - add border to checkcombobox when mouse enters the user control(CheckComboBox)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckComboBox_Events_Mouse_Enter(object sender, MouseEventArgs e)
        {
           // CheckComboBox_Events.BorderThickness = new Thickness(1.0);
        }

        /// <summary>
        /// CheckComboBox mouse exit event
        ///  - remove border to checkcombobox when mouse leaves the user control (CheckComboBox)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckComboBox_Events_Mouse_Leave(object sender, MouseEventArgs e)
        {
            //CheckComboBox_Events.BorderThickness = new Thickness(0.0);
        }
        /// <summary>
        /// Check if "Select All" entry is selected in CheckComboBox_Events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckComboBox_Events_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
    {
        string selectedItem = e.Item as string; //cast to the type in the ItemsSource
        if (selectedItem == "Select All" && e.IsSelected)
        {
           // Select_All_CheckComboBox(CheckComboBox_Events, true);
        }
        if (selectedItem == "Select All" && !e.IsSelected)
        {
            //Select_All_CheckComboBox(CheckComboBox_Events, false);
        }
    }
        /// <summary>
        /// This function is used to select/unslect all the entries in a CheckCombobox.
        /// </summary>
        /// <param name="ChekComboBox_Temp"></param>
        /// <param name="Checked"></param>
        private void Select_All_CheckComboBox(Xceed.Wpf.Toolkit.CheckComboBox CheckComboBox_Temp, bool Checked)
        {
            // === If checked is true ===
            if (Checked)
            {

                // === check each checkbox in the checkCombobox ===
                foreach (var item in CheckComboBox_Temp.Items)
                {
                    if (!CheckComboBox_Temp.SelectedItems.Contains(item))
                    {
                        CheckComboBox_Temp.SelectedItems.Add(item);
                    }
                }
            }

            // === if check is false ===
            else
            {
                // === uncheck each checkbox in the checkcombobox ===
                foreach (var item in CheckComboBox_Temp.Items)
                {
                    if (CheckComboBox_Temp.SelectedItems.Contains(item))
                    {
                        CheckComboBox_Temp.SelectedItems.Remove(item);
                    }
                }
            }
        }

    

        private void ButtonDisplay_Click(object sender, RoutedEventArgs e)
        {
           
            GlobalSharedData.HMIDisplayList.Clear();
            DateTime _clearTime = new DateTime(2100, 01, 01);

            // List<LogEntry> _tempList = new List<LogEntry>();
            // foreach (LogEntry item in DataGridLogs.SelectedItems)
            // {
            //     _tempList.Add(item);
            // }

            //GlobalSharedData.

            DateTime StartSelectedDateTime =new DateTime(2100,01,01);
            DateTime EndSelectedDateTime = new DateTime(1700, 01, 01);

            foreach (LogEntry item in DataGridLogs.SelectedItems)
           {                     
                if(item.DateTime < StartSelectedDateTime )
                {
                    StartSelectedDateTime = item.DateTime;
                }
                if(item.DateTime >EndSelectedDateTime)
                {
                    EndSelectedDateTime = item.DateTime;
                }
            }

            GlobalSharedData.EndDateTimeDatalog = EndSelectedDateTime;
            GlobalSharedData.StartDateTimeDatalog = StartSelectedDateTime;

               List<LogEntry> _sortedList = DataLogs.OrderBy(a => a.DateTime).ToList();

                foreach (LogEntry item in _sortedList)
            {
                HMIDisplayEntry _tempHMIDisplayEntry = new HMIDisplayEntry();
                PDSThreatEvent _tempPDSThreatEvent = new PDSThreatEvent();

                if (item.EventID == 150)
                {
                    _tempPDSThreatEvent.ThreatBIDHex = item.DataList.ElementAt(0);
                    _tempPDSThreatEvent.ThreatBID = uint.Parse(item.DataList.ElementAt(0).Remove(0,2), System.Globalization.NumberStyles.HexNumber).ToString();
                    _tempPDSThreatEvent.ThreatGroup = item.DataList.ElementAt(2);
                    _tempPDSThreatEvent.ThreatType = item.DataList.ElementAt(3);
                    _tempPDSThreatEvent.ThreatWidth = item.DataList.ElementAt(4);
                    _tempPDSThreatEvent.ThreatSector = item.DataList.ElementAt(5);
                    _tempPDSThreatEvent.ThreatZone = item.DataList.ElementAt(6);
                    _tempPDSThreatEvent.ThreatDistance = item.DataList.ElementAt(8);                   
                    _tempPDSThreatEvent.ThreatHeading = item.DataList.ElementAt(9);
                    _tempPDSThreatEvent.DateTime = item.DateTime;
               
                    HMIDisplayEntry _foundItem = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatBID == uint.Parse(item.DataList.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());

                    
                    if (_foundItem != null)
                {
                        if (_foundItem.EndDateTime != _clearTime)
                        {
                            _tempHMIDisplayEntry.StartDateTime = item.DateTime;
                            _tempHMIDisplayEntry.ThreatBID = uint.Parse(item.DataList.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                            _tempHMIDisplayEntry.ThreatPriority = item.DataList.ElementAt(15);
                            _tempHMIDisplayEntry.PDSThreat.Add(_tempPDSThreatEvent);
                            _tempHMIDisplayEntry.EndDateTime = new DateTime(2100, 01, 01);
                            GlobalSharedData.HMIDisplayList.Add(_tempHMIDisplayEntry);

                        }
                        else
                        {
                            _foundItem.ThreatPriority = item.DataList.ElementAt(15);
                            _foundItem.PDSThreat.Add(_tempPDSThreatEvent);
                          
                        }
                }
                else
                {
                    _tempHMIDisplayEntry.StartDateTime = item.DateTime;
                    _tempHMIDisplayEntry.ThreatBID = uint.Parse(item.DataList.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                    _tempHMIDisplayEntry.ThreatPriority = item.DataList.ElementAt(15);
                     _tempHMIDisplayEntry.PDSThreat.Add(_tempPDSThreatEvent);
                    _tempHMIDisplayEntry.EndDateTime = new DateTime(2100,01,01);
                    GlobalSharedData.HMIDisplayList.Add(_tempHMIDisplayEntry);
                }
               
                                        
                }
                if (item.EventID == 159)
                {

                    if (uint.Parse(item.DataList.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber) == 0)
                    {
                       
                        HMIDisplayEntry _foundItem1 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == item.DataList.ElementAt(15) );
            
                        if (_foundItem1 != null &&  _foundItem1.EndDateTime > item.DateTime)
                        {
                            _foundItem1.EndDateTime = item.DateTime;
                        }
              
                    }
                    else
                    {
                        HMIDisplayEntry _foundItem = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatBID == uint.Parse(item.DataList.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                        if (_foundItem != null)
                        {
                            _foundItem.EndDateTime = item.DateTime;
                        }
                    }
                }

                if(item.EventID == 2)
                {

                 
                    HMIDisplayEntry _foundItem1 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "1");

                    if (_foundItem1 != null && _foundItem1.EndDateTime > item.DateTime)
                    {
                        _foundItem1.EndDateTime = item.DateTime;
                    }

                    HMIDisplayEntry _foundItem2 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "2");

                    if (_foundItem2 != null && _foundItem1.EndDateTime > item.DateTime)
                    {
                        _foundItem2.EndDateTime = item.DateTime;
                    }

                    HMIDisplayEntry _foundItem3 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "3");

                    if (_foundItem3 != null && _foundItem1.EndDateTime > item.DateTime)
                    {
                        _foundItem3.EndDateTime = item.DateTime;
                    }

                    HMIDisplayEntry _foundItem4 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "4");

                    if (_foundItem4 != null && _foundItem1.EndDateTime > item.DateTime)
                    {
                        _foundItem4.EndDateTime = item.DateTime;
                    }

                    HMIDisplayEntry _foundItem5 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "5");

                    if (_foundItem5 != null && _foundItem1.EndDateTime > item.DateTime)
                    {
                        _foundItem5.EndDateTime = item.DateTime;
                    }
                }
            }
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.HMIDisplayView;
        }

        bool SelectAll = false;
        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (SelectAll)
            {
                ButtonSelectAll.Content = "Select All";
                DataGridLogs.UnselectAll();
                SelectAll = false;               
            }
            else
            {
                ButtonSelectAll.Content = "Unselect All";
                DataGridLogs.SelectAll();
                SelectAll = true;
            }
        }

        bool touchActive = false;
        // private void DataGridRow_TouchDown(object sender, TouchEventArgs e)
        private void DataGridRow_TouchDown(object sender, TouchEventArgs e)
        {
           
            DataGridRow row = (DataGridRow)sender;
            if (row.IsSelected)
            {
                touchActive = false;
            }
            else
            {
                touchActive = true;
            }
        }

        private void DataGridRow_MouseMove(object sender, MouseEventArgs e)
        {
            if (touchActive)
            {
                DataGridRow row = (DataGridRow)sender;
                if (row.IsSelected)
                {
                    row.IsSelected = true;
                }
            }
        }

        private void DataGridRow_TouchMove(object sender, TouchEventArgs e)
        {
          
                
         
        }


    }
}
