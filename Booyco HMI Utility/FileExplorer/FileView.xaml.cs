using Booyco_HMI_Utility.CustomObservableCollection;
using Booyco_HMI_Utility.FileExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for FileView.xaml
    /// </summary>
    public partial class FileView : UserControl
    {
        string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Saved Files";
        private RangeObservableCollection<FileEntry> FileList;

        public FileView()
        {
            InitializeComponent();     
            FileList = new RangeObservableCollection<FileEntry>();
            DataGridFiles.ItemsSource = FileList;
            ReadSavedFolder();
        }

        private void ButtonDataViewer_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Dataview;
        }
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
        }

        private void ButtonConfigViewer_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Confiure;
        }

        private void ReadSavedFolder()
        {

            if (Directory.Exists(_savedFilesPath))
            {
                DirectoryInfo d = new DirectoryInfo(_savedFilesPath);

                FileInfo[] Files = d.GetFiles("*.txt");
                string str = "";
                uint count = 0;
                foreach (FileInfo file in Files)
                {
                    string _type = "";
                    if(file.Name.Split('_')[0] == "DataLog")
                    {
                        _type = "DataLog";
                    }
                   
                    count++;
                    FileList.Add(new FileEntry
                    {
                        Number = count,
                        Name = file.Name,                     
                        Type = _type,
                         Path = file.FullName

                    });


                }
            }
            else
            {
                Directory.CreateDirectory(_savedFilesPath);
            }

        }

        private void DataGridFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid _dataGrid = (DataGrid)sender;
            if(FileList.ElementAt(_dataGrid.SelectedIndex).Type == "DataLog")
            {
                ButtonDataViewer.IsEnabled = true;
                ButtonConfigViewer.IsEnabled = false;
                GlobalSharedData.FilePath = FileList.ElementAt(_dataGrid.SelectedIndex).Path;
         
            }
            else if (FileList.ElementAt(_dataGrid.SelectedIndex).Type == "Parameter")
            {
                ButtonDataViewer.IsEnabled = false ;
                ButtonConfigViewer.IsEnabled = true;
                GlobalSharedData.FilePath = FileList.ElementAt(_dataGrid.SelectedIndex).Path;
            }
        }
    }
}
