using System;
using System.Collections.Generic;
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
    /// Interaction logic for HMIDisplayView.xaml
    /// </summary>
    public partial class HMIDisplayView : UserControl
    {

        public bool CloseRequest = false;
        private DispatcherTimer dispatcherPlayTimer;
        private bool DisplpayPlay = false;

        public HMIDisplayView()
        {
            InitializeComponent();

            dispatcherPlayTimer = new DispatcherTimer();
            dispatcherPlayTimer.Tick += new EventHandler(PlayTimerTick);
            dispatcherPlayTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
   
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogView;
            CloseRequest = true;
        }



        private void ButtonNewWindow_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 25 ; i++)
            {
          
                Rectangle _rectangle = (Rectangle)this.FindName("Presence" + i);
                _rectangle.Opacity += 0.2;
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 25; i++)
            {
              
                Rectangle _rectangle = (Rectangle)this.FindName("Presence" + i);
                _rectangle.Opacity -= 0.2;
            }
        }
        private void PlayTimerTick(object sender, EventArgs e)
        {
            Slider_DateTime.Value += 5000000;
        }

            private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
              if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.HMIDisplayView)
            {
                //GlobalSharedData.HMIDisplayList.First().PDSThreat.First().ThreatZone;
                if (GlobalSharedData.HMIDisplayList.Count() != 0)
                {

                    PDSThreatOpacity(Convert.ToInt32(GlobalSharedData.HMIDisplayList.First().PDSThreat.First().ThreatZone), Convert.ToInt32(GlobalSharedData.HMIDisplayList.First().PDSThreat.First().ThreatSector), Convert.ToInt32(GlobalSharedData.HMIDisplayList.First().PDSThreat.First().ThreatWidth), 0.8);

                    DateTime _startTime = new DateTime(2100, 1, 1);
                    DateTime _endTime = new DateTime();
                    DateTime _clearTime = new DateTime();

                    foreach (HMIDisplayEntry item in GlobalSharedData.HMIDisplayList)
                    {


                        if (_clearTime == item.EndDateTime)
                        {

                            foreach (PDSThreatEvent tempPDSThreat in item.PDSThreat)
                            {
                                if (item.EndDateTime < tempPDSThreat.DateTime)
                                {
                                    item.EndDateTime = tempPDSThreat.DateTime;
                                }
                            }

                        }
                        if (_startTime.Ticks > item.StartDateTime.Ticks)
                        {
                            _startTime = item.StartDateTime;
                        }

                        if (_endTime.Ticks < item.EndDateTime.Ticks)
                        {
                            _endTime = item.EndDateTime;
                        }


                    }

                    Slider_DateTime.Minimum = _startTime.Ticks + 1;
                    TextBlock_StartDateTime.Text = _startTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt");


                    Slider_DateTime.Maximum = _endTime.Ticks + 1;
                    TextBlock_EndDateTime.Text = _endTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
                }
                

            }

            else
            {
                ClearClusters();
            }
        }

        void ClearClusters()
        {
            for (int i = 1; i < 25; i++)
            {

                Rectangle _rectangle = (Rectangle)this.FindName("Presence" + i);
                _rectangle.Opacity = 0.1;
            }
            for (int i = 1; i < 25; i++)
            {

                Rectangle _rectangle = (Rectangle)this.FindName("Warning" + i);
                _rectangle.Opacity = 0.1;
            }
            for (int i = 1; i < 25; i++)
            {

                Rectangle _rectangle = (Rectangle)this.FindName("Critical" + i);
                _rectangle.Opacity = 0.1;
            }
        }

        void PDSThreatOpacity(int _zone, int sector, int width, double opacity)
        {
            string _rectangleName ="";
            switch(_zone)
            {
                case 3:
                    _rectangleName = "Critical";
                    break;
                case 2:
                    _rectangleName = "Warning";
                    break;
                case 1:
                    _rectangleName = "Presence";
                    break;
            }

            ClusterOpacity(sector, width, opacity, _rectangleName);


        }

        void ClusterOpacity(int sector, int width, double opacity , string _rectangleName)
        {
            if(width > 360)
            {
                width = 360;
            }
            if(sector > 12)
           {
                sector = 12;
            }

            int numberSegements = width / 15;

            int startPos = sector * 2 - numberSegements / 2 + 1;

            if(startPos<0)
            {
                startPos = 24 + startPos;
            }

            for(int i = 0; i <numberSegements; i++)
            {
                int segement= 0;

                if(startPos+i>24)
                {
                    segement = startPos + i - 24;
                }
                else
                {
                    segement = startPos + i;
                }

                Rectangle _rectangle = (Rectangle)this.FindName(_rectangleName + segement);
                _rectangle.Opacity += opacity;

            }

        }

        private void Slider_DateTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ClearClusters();
            DateTime _dateTime = new DateTime((long)Slider_DateTime.Value);
            DateTime _enddateTime = new DateTime();
            TextBlock_SelectDateTime.Text = _dateTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt");

            int count = 0;
            foreach (HMIDisplayEntry item in GlobalSharedData.HMIDisplayList)
            {
                if (Convert.ToUInt32(item.ThreatBID) > 0)
                {

                    int firstindex = item.PDSThreat.FindLastIndex(p => p.DateTime < _dateTime);

                    // int lastindex = item.PDSThreat.FindLastIndex(p => p.DateTime < _dateTime);

                    if (firstindex == -1)
                    {
                        firstindex = item.PDSThreat.Count - 1;
                    }

                    //// if (firstindex == lastindex)
                    //  {
                    if (_dateTime <= item.EndDateTime && _dateTime >= item.StartDateTime)
                    {
                        TextBlock_Date.Text = item.PDSThreat.ElementAt(firstindex).DateTime.ToString("MM/dd/yyyy");
                        TextBlock_Time.Text = item.PDSThreat.ElementAt(firstindex).DateTime.ToString("hh:mm:ss");
                        count++;
                        PDSThreatOpacity(Convert.ToInt32(item.PDSThreat.ElementAt(firstindex).ThreatZone), Convert.ToInt32(item.PDSThreat.ElementAt(firstindex).ThreatSector), Convert.ToInt32(item.PDSThreat.ElementAt(firstindex).ThreatWidth), 0.8);
                        //  }
                    }

                    else if(_dateTime >= item.EndDateTime)
                    {
                        _enddateTime = item.EndDateTime;
                    }
                   
                }
            }

            if (count > 0)
            {
                TextBlock_Title.Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0)); 
                TextBlock_Title.Text = "PDS(1/" + count.ToString() + ") - Proximity Detection 01";

            }
            else
            {
                TextBlock_Time.Text = _enddateTime.ToString("hh:mm:ss");
                TextBlock_Title.Text = "";
            }
        }

        private void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            if (DisplpayPlay)
            {
                dispatcherPlayTimer.Stop();
                Button_Play.Content = "Play";
             
                DisplpayPlay = false;
            }
            else
            {
                dispatcherPlayTimer.Start();
                Button_Play.Content = "Pause";
                DisplpayPlay = true;
            }
        }


     
        private void Rectangle_Background_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    Rectangle _rectangle = (Rectangle)sender;
            //    Label_Count.Content = ((_rectangle.Opacity-0.1) / 0.2).ToString();
            //}
            //catch
            //{

            //}


        }
    }
}
