using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    class ImageEntry:INotifyPropertyChanged
    {
        private UInt16 _ID;
        public UInt16 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
                OnPropertyChanged("ID");
            }
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
        }

        private UInt32 _Size;
        public UInt32 Size
        {
            get
            {
                return _Size;
            }
            set
            {
                _Size = value;
                OnPropertyChanged("Size");
            }
        }

        private UInt16 _Progress;
        public UInt16 Progress
        {
            get
            {
                return _Progress;
            }
            set
            {
                _Progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
