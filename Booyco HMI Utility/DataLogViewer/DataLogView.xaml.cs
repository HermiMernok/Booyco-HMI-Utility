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
               
            }
            else
            {
                ProgressbarDataLogs.Value = e.ProgressPercentage;
                TextBlockProgressStatus.Text = "Upload (" + e.ProgressPercentage.ToString().PadLeft(3, '0') + "%)";
            }
        }

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog openFileDialog = new Microsoft.Win32.SaveFileDialog();

           
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
            
            if(ThreadsIsSelected())
            {
                ButtonMap.IsEnabled = true;
                ButtonDisplay.IsEnabled = true;
            }
            else
            {
                ButtonMap.IsEnabled = false;
                ButtonDisplay.IsEnabled = false;
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
                    if (item.EventID == 150 || item.EventID == 157)
                    {
                       

                        Event_Count++;
                    }
                    else if (item.EventID == 151 || item.EventID == 158)
                    {
                       
                        Event_Count++;
                    }
                    else if (item.EventID == 152 || item.EventID == 159)
                    {


                        Event_Count++;

                    }
                    else if (item.EventID == 153 || item.EventID == 160)
                    {
                        

                        Event_Count++;
                    }
                    else if (item.EventID == 154 || item.EventID == 161)
                    {
                      

                        Event_Count++;
                    }
                    else if (item.EventID == 155 || item.EventID == 162)
                    {
                        
                        Event_Count++;
                    }
                    else if (item.EventID == 156 || item.EventID == 163)
                    {
                        

                        Event_Count++;
                    }

                    if (Event_Count == 7)
                    {
                        Event_Count = 0;
                      
                        return true;

                    }


                }

            }

            return false;
   
        }

        private void  PlotThreads()
        {
          
            ProximityDetectionEvent TempEvent = new ProximityDetectionEvent();
            uint Event_Count = 0;

            foreach (LogEntry item in DataGridLogs.SelectedItems)
            {
                if (item != null)
                {
                    if (item.EventID == 150 || item.EventID == 157)
                    {
                        // TODO: Fix this two variables
                        TempEvent.DateTimeStamp = item.DateTime;
                        TempEvent.ThreatNumberStart = item.Number;
                        TempEvent.ThreatNumberStop = item.Number + 6;
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
                        TempEvent.ThreatLatitude = ((double)BitConverter.ToInt32(item.RawData, 0) * Math.Pow(10, -7));
                        TempEvent.ThreatLongitude = ((double)BitConverter.ToInt32(item.RawData, 4) * Math.Pow(10, -7));

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
                        TempEvent.UnitLatitude = ((double)BitConverter.ToInt32(item.RawData, 0) * Math.Pow(10, -7));
                        TempEvent.UnitLongitude = ((double)BitConverter.ToInt32(item.RawData, 4) * Math.Pow(10, -7));
                        Event_Count++;
                    }
                    else if (item.EventID == 156 || item.EventID == 163)
                    {
                        TempEvent.POILatitude = ((double)BitConverter.ToInt32(item.RawData, 0) * Math.Pow(10, -7));
                        TempEvent.POILongitude = ((double)BitConverter.ToInt32(item.RawData, 4) * Math.Pow(10, -7));

                        Event_Count++;
                    }

                    if (Event_Count == 7)
                    {
                        Event_Count = 0;
                        ProximityDetectionEventList.Add(TempEvent);
                        TempEvent = new ProximityDetectionEvent();
                       
                    }                


                }

            }

            foreach (ProximityDetectionEvent EventItem in ProximityDetectionEventList)
            {
                GlobalSharedData.PDSMapMarkers.Clear();

                string PDS_Event_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                                "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                                "Threat ID: 0x" + EventItem.PrimaryID.ToString("X") + "\n" +
                                                "Threat Kind: " + EventItem.ThreatTechnology.ToString() + "\n" +
                                                // "Threat Group: " + PDS_01_Group.ToString() + "\n" +
                                                "Threat Type: " + EventItem.ThreatType.ToString() + "\n" +
                                                "Threat Speed: " + EventItem.ThreatSpeed.ToString("##,#00.0") + " km/h\n" +
                                                "Threat Distance: " + EventItem.ThreatDistance.ToString() + " m\n" +
                                                "Threat Heading: " + EventItem.ThreatHeading.ToString() + " deg\n" +
                                                "Threat Accuracy: " + EventItem.ThreatHorizontalAccuracy.ToString() + " m\n" +
                                                "Threat Priority: " + EventItem.ThreatPriority.ToString() + "\n" +
                                                "Threat Latitude: " + EventItem.ThreatLatitude.ToString() + " deg\n" +
                                                "Threat Longitude: " + EventItem.ThreatLongitude.ToString() + " deg";

                string Unit_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                          "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                          "Unit Speed: " + EventItem.UnitSpeed.ToString("##,#00.0") + " km/h\n" +
                                          "Unit Heading: " + EventItem.UnitHeading.ToString("###,#.00") + " deg\n" +
                                          "Unit Accuracy: " + EventItem.UnitHorizontalAccuracy.ToString() + " m\n" +
                                          "Unit Latitude: " + EventItem.UnitLatitude.ToString() + " deg\n" +
                                          "Unit Longitude: " + EventItem.UnitLongitude.ToString() + " deg\n" +
                                          "Unit Presence Distance: " + EventItem.PresenceDistance.ToString() + " m\n" +
                                          "Unit Warning Distance: " + EventItem.WarningDistance.ToString() + " m\n" +
                                          "Unit Critical Distance: " + EventItem.CriticalDistance.ToString() + " m\n" +
                                          "Threat Zone: " + EventItem.ThreatDisplayZone.ToString() + "\n" +
                                          "Threat Display Width: " + EventItem.ThreatDisplayWidth.ToString() + "\n" +
                                          "Threat Sector: " + EventItem.ThreatDisplaySector.ToString();

                string POI_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                          "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                          "POI Distance (UTM Plot): " + EventItem.ThreatPOIUTMDistance.ToString("##,##00.00") + " m\n" +
                                          "POI Distance (Log): " + EventItem.ThreatPOILOGDistance.ToString() + " m\n" +
                                          "POI Latitude: " + EventItem.POILatitude.ToString() + " deg\n" +
                                          "POI Longitude: " + EventItem.POILongitude.ToString() + " deg";


                MarkerEntry PDSMarker1 = new MarkerEntry();
                PDSMarker1.MapMarker = new GMapMarker(new PointLatLng(EventItem.ThreatLatitude, EventItem.ThreatLongitude));
                MarkerEntry PDSMarker2 = new MarkerEntry();
                PDSMarker2.MapMarker = new GMapMarker(new PointLatLng(EventItem.UnitLatitude, EventItem.UnitLongitude));
                MarkerEntry PDSMarkerPOI = new MarkerEntry();
                PDSMarkerPOI.MapMarker = new GMapMarker(new PointLatLng(EventItem.POILongitude, EventItem.POILatitude));

                if (TempEvent.ThreatTechnology == 5)
                {
                    PDSMarker1.Heading = EventItem.ThreatHeading;
                    PDSMarker1.Zone = 0;
                    PDSMarker1.title = PDS_Event_Information;
                    PDSMarker1.Type = (int)MarkerType.Indicator;                
                }
                else
                {                
                    PDSMarker1.title = PDS_Event_Information;
                    PDSMarker1.Width = EventItem.ThreatDisplayWidth;
                    PDSMarker1.Height = EventItem.ThreatDisplayWidth;
             
                    PDSMarker1.Type = (int)MarkerType.Ellipse;                 
                }
                PDSMarker2.Heading = EventItem.UnitHeading;
                PDSMarker2.Zone = EventItem.ThreatDisplayZone;
                PDSMarker2.title = Unit_Information;     
                PDSMarker2.Type = (int)MarkerType.Indicator;
              
                PDSMarkerPOI.Zone = EventItem.ThreatDisplayZone;
                PDSMarkerPOI.title = POI_Information;
                PDSMarkerPOI.Type = (int)MarkerType.Point;
         
                GlobalSharedData.PDSMapMarkers.Add(PDSMarker1);
                GlobalSharedData.PDSMapMarkers.Add(PDSMarker2);
                extendedWindow.MapView.UpdateMapMarker();          
                
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
                DataGridLogs.Columns[5].Visibility = Visibility.Hidden;
            }
            else
            {
                DataGridLogs.Columns[5].Visibility = Visibility.Visible;
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
            DateTime _clearTime = new DateTime();

            List<LogEntry> _tempList = new List<LogEntry>();
            foreach (LogEntry item in DataGridLogs.SelectedItems)
            {
                _tempList.Add(item);
            }

            List<LogEntry> _sortedList = _tempList.OrderBy(a => a.Number).ToList();

                foreach (LogEntry item in _sortedList)
            {
                HMIDisplayEntry _tempHMIDisplayEntry = new HMIDisplayEntry();
                PDSThreatEvent _tempPDSThreatEvent = new PDSThreatEvent();

                if (item.EventID == 150)
                {
                    _tempPDSThreatEvent.ThreatBID = item.DataList.ElementAt(0);
                     _tempPDSThreatEvent.ThreatGroup = item.DataList.ElementAt(2);
                    _tempPDSThreatEvent.ThreatType = item.DataList.ElementAt(3);
                    _tempPDSThreatEvent.ThreatWidth = item.DataList.ElementAt(4);
                    _tempPDSThreatEvent.ThreatSector = item.DataList.ElementAt(5);
                    _tempPDSThreatEvent.ThreatZone = item.DataList.ElementAt(6);
                    _tempPDSThreatEvent.ThreatDistance = item.DataList.ElementAt(8);                   
                    _tempPDSThreatEvent.ThreatHeading = item.DataList.ElementAt(9);
                    _tempPDSThreatEvent.DateTime = item.DateTime;
                    HMIDisplayEntry _foundItem = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatBID == item.DataList.First());
                
                if(_foundItem != null)
                {
                        if (_foundItem.EndDateTime != _clearTime)
                        {
                            _tempHMIDisplayEntry.StartDateTime = item.DateTime;
                            _tempHMIDisplayEntry.ThreatBID = item.DataList.First();
                            _tempHMIDisplayEntry.PDSThreat.Add(_tempPDSThreatEvent);
                            GlobalSharedData.HMIDisplayList.Add(_tempHMIDisplayEntry);
                        }
                        else
                        {
                            _foundItem.PDSThreat.Add(_tempPDSThreatEvent);
                          
                        }
                }
                else
                {
                    _tempHMIDisplayEntry.StartDateTime = item.DateTime;
                    _tempHMIDisplayEntry.ThreatBID = item.DataList.First();
                    _tempHMIDisplayEntry.PDSThreat.Add(_tempPDSThreatEvent);
                        GlobalSharedData.HMIDisplayList.Add(_tempHMIDisplayEntry);
                }
               
                                        
                }
                if (item.EventID == 157)
                {
                    HMIDisplayEntry _foundItem = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatBID == item.DataList.First());
                    if(_foundItem != null)
                    {
                        _foundItem.EndDateTime = item.DateTime;
                    }
                }
            }
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.HMIDisplayView;
        }
    }
}
