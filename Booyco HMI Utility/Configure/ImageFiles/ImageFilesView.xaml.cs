using Booyco_BHU_Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
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
using static Booyco_BHU_Utility.FirmwareRevisionManagement;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for ImageFilesView.xaml
    /// </summary>
    public partial class ImageFilesView : UserControl
    {
        private uint SelectVID = 0;
        static private RangeObservableCollection<ImageEntry> ImageFileList;
        string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Saved Files\\Images";
        static private UInt16 TotalImageFiles = 0;
        List<FirmwareEntry> firmwareImageCountList = new List<FirmwareEntry>();
        FirmwareRevisionManagement FirmwareRevisionManager = new FirmwareRevisionManagement();


        public ImageFilesView()
        {
            InitializeComponent();
            ImageFileList = new RangeObservableCollection<ImageEntry>();

            DataGridImageFiles.AutoGenerateColumns = false;
            DataGridImageFiles.ItemsSource = ImageFileList;
            Label_StatusView.Content = "Waiting for user command..";
            Label_ProgressStatusPercentage.Content = "";
            firmwareImageCountList = FirmwareRevisionManager.ReadImageFirmwareRevision((int)EnumRevisionType.ImageRevision);
            ReadImageFiles();
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonAppend_Click(object sender, RoutedEventArgs e)
        {

        }


        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
            {
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.ConfigureMenuView;
            }
            else
            {
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.FileMenuView;
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.AudioFilesView;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ParametersView;
        }
                private void ButtonNext_MouseEnter(object sender, MouseEventArgs e)
        {
            RectangleArrowRight.Fill = new SolidColorBrush(Color.FromRgb(60, 6, 6));
            ImageParameter.Opacity = 1;
        }

        private void ButtonNext_MouseLeave(object sender, MouseEventArgs e)
        {
            RectangleArrowRight.Fill = new SolidColorBrush(Color.FromRgb(140, 9, 9));
            ImageParameter.Opacity = 0.6;
        }

        private void ButtonPrevious_MouseEnter(object sender, MouseEventArgs e)
        {
            RectangleArrowLeft.Fill = new SolidColorBrush(Color.FromRgb(60, 6, 6));
            ImagePicture.Opacity = 1;
        }

        private void ButtonPrevious_MouseLeave(object sender, MouseEventArgs e)
        {
            RectangleArrowLeft.Fill = new SolidColorBrush(Color.FromRgb(140, 9, 9));
            ImagePicture.Opacity = 0.6;
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                
                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                {
                    DataGridImageFiles.Columns[4].Visibility = Visibility.Visible;
                    ButtonNew.Visibility = Visibility.Visible;
                    ButtonAppend.Visibility = Visibility.Visible;
                    Grid_Progressbar.Visibility = Visibility.Visible;
                    ButtonNext.Visibility = Visibility.Visible;
                    ButtonPrevious.Visibility = Visibility.Visible;
                    WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                    SelectVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                    
                }
                else
                {
                    DataGridImageFiles.Columns[4].Visibility = Visibility.Collapsed;
                    ButtonNew.Visibility = Visibility.Collapsed;
                    ButtonAppend.Visibility = Visibility.Collapsed;
                    ButtonNext.Visibility = Visibility.Collapsed;
                    ButtonPrevious.Visibility = Visibility.Collapsed;
                    Grid_Progressbar.Visibility = Visibility.Collapsed;
                }

            }
            else
            {
               
            }

        }
        void ReadImageFiles()
        {
            ImageFileList.Clear();
            if (Directory.Exists(_savedFilesPath))
            {
                DirectoryInfo d = new DirectoryInfo(_savedFilesPath);
                FileInfo[] FilesBMP = d.GetFiles("*.bmp");
                ushort count = 0;

                int _firmwareNumber = 0;
                int _totalImages = 0;

                foreach (FileInfo file in FilesBMP)
                {
                    count++;

                    if (count > _totalImages)
                    {
                        for (int i = _firmwareNumber; i < firmwareImageCountList.Count(); i++)
                        {
                            _firmwareNumber = i + 1;
                            _totalImages = firmwareImageCountList.ElementAt(i).ImageCount;
                            if (count < _totalImages)
                            {
                                i = firmwareImageCountList.Count();
                            }
                        }
                    }

                    if (count <= _totalImages)
                    {
                        string selectedFileName = file.FullName;
                        BitmapImage bitmap = new BitmapImage();

                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(selectedFileName);                        
                        bitmap.EndInit();

                        var tb = new TransformedBitmap();                    
                        tb.BeginInit();
                        tb.Source = bitmap;
                        var transform = new ScaleTransform(1, -1, 0, 0);
                        tb.Transform = transform;
                        tb.EndInit();


                        ImageFileList.Add(new ImageEntry
                        {
                            ID = count,
                            FileName = file.Name,
                            DateTimeCreated = file.CreationTime.ToString("yyyy-MM-dd HH-mm-ss"),
                            Path = file.FullName,
                            Size = file.Length,
                            ImageSource = tb,
                            Progress = 0,
                            ProgressString = "",
                            FirmwareNumber = _firmwareNumber
                        });
                    }

                    
                }
                TotalImageFiles = count;
            }
            else
            {
                Directory.CreateDirectory(_savedFilesPath);
            }
        }

    }

       
    }
